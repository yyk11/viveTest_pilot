using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System;

public class control : MonoBehaviour {
	public string[] colNames;
	public string[] dirNames;
	public int dirid;
	public Color[] colors;
	public Color color;
	public enum m_state {Unstart, Putting, Finding, LogMaxsize, VarBallSize, VarDensity, Study_one, ShowBalls, Study1_Phase2,AdjustRange_Angle, Space_acquire, Shelf_Layout, Study2}; // study_one 自由放置， study1phase2 随机位置， adjustrange_angle 肩膀和右臂
	public m_state state;
	public GameObject head;
	public GameObject hand_left;
	public GameObject hand_right;
	public ObjectInHand objh_left;
	public ObjectInHand objh_right;

	public GameObject ball;
	public GameObject prt;
	public Quaternion rot;
	public Text canvas;
	private Text aim;
	public string text;
	public DataGet dataget;
	private httpReader httpserver;
	private string query;
	private Camera cam;



	public float ball_size = 1.0f;
	private Vector3 ball_scale = new Vector3(0.3f, 0.3f, 0.3f);
	private string text_instruction = "Press '3' to Study 1 Phase 1,\r\n Press '8' to feel techniques";


	void Start () {
		haptic_on = false;
		cam = GameObject.Find ("Camera (eye)").GetComponent<Camera>();
		state = m_state.Unstart;
		colors = new Color[]{ Color.red, Color.blue, Color.green, Color.yellow, Color.white, Color.black };
		colNames = new string[] {"red", "blue", "green", "yellow", "white", "black"};
		dirNames = new string[] {"up", "left", "right", "front", "back"};
		dirid = 0;
		canvas = GameObject.Find ("Text").GetComponent<Text> ();
		head = GameObject.Find ("Camera (eye)");
		hand_left = GameObject.Find ("Controller (left)");
		hand_right = GameObject.Find ("Controller (right)");
		objh_left = hand_left? hand_left.GetComponent<ObjectInHand>() : null;
		objh_right = hand_right? hand_right.GetComponent<ObjectInHand>() : null;
		dataget = GetComponent<DataGet> ();
		aim = GameObject.Find ("Aim").GetComponent<Text>();

		nums = new int[]{4, 6, 8, 9, 12};
		rot = new Quaternion (0, 0, 0, 0);
		finded = putNext = false;
		pf_v = new Vector3[nums.Length][];
		for (int i = 0; i < nums.Length; i++)
			pf_v [i] = new Vector3[2*nums [i]];

		vbs_v = new Vector3[vbs_num][];
		for (int i = 0; i < vbs_num; i++)
			vbs_v [i] = new Vector3[2*dirNames.Length];

		vbs_success = new bool[vbs_num][];
		for (int i = 0; i < vbs_num; i++)
			vbs_success [i] = new bool[dirNames.Length];

		vbs_sizes = new float[]{ 1.2f, 1.0f, 0.8f, 0.6f, 0.4f, 0.2f };
		numid = -1;
		canvas.text = text_instruction; 



		//get body data from the third controller
		//gameObject.AddComponent<httpReader>();
		httpserver = gameObject.GetComponent<httpReader>();
		httpserver.StartServer();
	}

	//for showing information to the user
	IEnumerator m_Wait(uint sec, string st){
		yield return new WaitForSeconds (sec);
		if(canvas.text == st) canvas.text = "";
	}
	public void setText(uint sec, string st){
		canvas.text = text = st;
		StartCoroutine (m_Wait(sec, st));
	}

	//for log max size
	private Vector3[] logmax_vs;
	public bool logmax_begin;
	public void logMaxSizeOnce(Vector3 pos){
		if(head){
			logmax_vs [dirid] = pos - head.transform.position;
		}
		else
			head = GameObject.Find ("Camera (eye)");
		if (++dirid >= dirNames.Length) {
			dataget.logData_1d ("user_max_size", logmax_vs, 5);
			state = m_state.Unstart;
			return;
		}
		setText (15, "try your best to stretch to " + dirNames [dirid] + "\nthen press");

	}
	public void logMaxSize(){
		if (++dirid >= dirNames.Length) {
			dataget.logData_1d ("user_max_size", logmax_vs, 5);
			state = m_state.Unstart;
			return;
		}
		setText (15, "try your best to stretch to " + dirNames [dirid] + "\nthen press");
		logmax_begin = true;
	}
	public void logClicked(Vector3 pos){
		Debug.Log ("press");
		if(head){
			logmax_vs [dirid] = pos - head.transform.position;
			logmax_begin = false;
		}
		else
			head = GameObject.Find ("Camera (eye)");
	}

	//for varible ball size
	public float[] vbs_sizes;
	private int vbs_num = 6;
	public Vector3[][] vbs_v;
	private bool[][] vbs_success;
	public int vbs_id = -1;
	public bool vbs_begin;
	public bool vbs_finished;
	public GameObject vbs_ball;

	private void varBallSize(){
		Debug.Log ("var");
		if (++dirid >= 5) {
			state = m_state.Unstart;
			if (vbs_id + 1 == vbs_num)
				canvas.text = "Press '1' to start MaxSize Section, Press '2' to start BallSize Section";
			else
				canvas.text =  "Please Press '2' to start next section with Ball Size " + vbs_sizes[vbs_id+1];
			return;
		}
		vbs_begin = vbs_finished = false;
		vbs_ball = GameObject.Instantiate (ball, prt.transform.position, rot, prt.transform);
		vbs_ball.GetComponent<Renderer> ().material.color = Color.green;
		vbs_ball.transform.localScale = ball_scale * ball_size;
		setText (10, "put it to " + dirNames [dirid] + " then try to get it");
	}
	public void clickaBall(Vector3 hand_pos, bool success)
	{
		if (vbs_finished == true) {
			vbs_finished = false;
			vbs_v [vbs_id][2 * dirid + 1] = hand_pos - head.transform.position;
			vbs_success [vbs_id][dirid] = success;
			Destroy (vbs_ball);
			varBallSize ();
		}
	}
	public void clickaBall(Vector3 hand_pos){
		if (vbs_finished == true) {
			vbs_finished = false;
			vbs_v [vbs_id][2 * dirid + 1] = hand_pos - head.transform.position;
			Destroy (vbs_ball);
			varBallSize ();
		}
	}

	//for putting and finding some ball
	public GameObject[] objects;
	public GameObject goalPrefab;
	public GameObject goal;
	public int objNum;
	public int id;
	public int numid;
	public int[] nums;
	public bool putNext;
	public bool finded;
	private Vector3[][] pf_v;

	public void ShowaBall(){
		color = colors[id% colors.Length];
		setText(6, "Put the " + colNames[id%colors.Length] + " ball\nthen press the touchpad");
		objects [id] = GameObject.Instantiate (ball, prt.transform.position, rot, prt.transform);
		objects [id].transform.localScale = ball_scale * ball_size;
		putNext = false;
		objects [id].GetComponent<Renderer> ().material.color = color;
	}
	public void FindaBall(){
		finded = false;
		color = colors[id%colors.Length];
		setText(6, "Find the " + colNames[id%colors.Length] + " ball");
	}
	public int FindNearBall(Vector3 pos){
		var minD = 10000f;
		var tmp = 0f;
		int idx = id;
		for (int i = id; i < objNum; i++) {
			tmp = (objects [i].transform.position - pos).magnitude;
			if (tmp < minD) {
				idx = i; minD = tmp;
			}
		}
		return idx;
	}
	public void FoundaBall(Vector3 pos){
		pf_v [numid] [2 * id] = objects [id].transform.position - head.transform.position;
		pf_v [numid] [2 * id + 1] = pos - head.transform.position;
	}

	/*public void AcquireBall(GameObject colliding, Vector3 position_controller)
	{
		Debug.Log (colliding == null);
		if (bubbling == false && colliding != null) {
			Debug.Log ("collide");
			colliding.transform.position = head.transform.position + head.transform.forward * 2;
			ball_old = colliding;
		} else if (bubbling == true) {
			int nearest = -1;
			float distance_nearest = 10000;
			for (int i = 0; i < num_ball; i++) {
				float distance = (position_controller - balls[i].transform.position).magnitude;
				if (distance < distance_nearest) {
					nearest = i;
					distance_nearest = distance;
				}
			}
			Debug.Log (distance_nearest);
			if (distance_nearest > threthold_bubble)
				return;
			ball_old = balls [nearest];
			ball_old.transform.position = head.transform.position + head.transform.forward * 2;
		}
		else {
			ball_old = null;
		}
	}*/

	/*
	private void Initiate_Lines()
	{

		if (balls_grid != null) {
			for (int i = 0; i < balls_grid.Length; i++)
				Destroy (balls_grid [i]);
		}
		GameObject flag = GameObject.Find ("Flag");
		flag.transform.position = head.transform.TransformPoint (new Vector3 (0, -1, 10));
		flag.transform.rotation = head.transform.rotation;
		flag.transform.Rotate (Vector3.up * 90, Space.Self);
		balls_grid = new GameObject[num_amount_one];
		for (int i = 0; i < num_z_level; i++) {
			for (int j = 0; j < num_d_level; j++) {

				for (int k = 0; k < num_theta_level; k++) {
					int index_space = i * num_d_level * num_theta_level + j * num_theta_level + k;
					GameObject ball_transparent = GameObject.Find ("Example");
					balls_grid[index_space] = GameObject.Instantiate(ball_transparent,head.transform.TransformPoint(positions_relative_one [index_space]+new Vector3(0,-0.15f,-0.1f)), ball_transparent.transform.rotation);
					//balls_grid[index_space] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					//balls_grid [index_space].AddComponent<Material> ();
					balls_grid[index_space].transform.localScale = scale_balls * 0.5f;
					balls_grid [index_space].name = index_space.ToString ();
					//balls_grid [index_space].GetComponent<Material>().color = new Color (1, 1, 1,0);

					//balls_grid[index_space].transform.position = positions_world_one [index_space]+new Vector3(0,-0.15f,-0.1f);
				}
			}
		}
	}
	*/


	// in rectangular region, discarded
	private void InitialBalls()
	{
		balls = new GameObject[num_ball];
		positions_ball_world = new Vector3[num_ball];

		for (int i = 0; i < num_ball; i++) {
			balls[i] = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			//xrange, yrange, zrange could be adjusted by data
			Vector3 position_new = new Vector3(0,0,0);
			position_new.y = UnityEngine.Random.Range(y_min, y_max);
			position_new.z = UnityEngine.Random.Range (z_min, z_max);
			if(i < num_ball / 2)
				position_new.x = UnityEngine.Random.Range (x_right_min, x_right_max);
			else
				position_new.x = UnityEngine.Random.Range (x_left_min, x_left_max);
			positions_ball_world[i] = head.transform.TransformPoint (position_new);
			balls [i].transform.position = positions_ball_world [i];
			balls [i].transform.localScale = scale_balls;
			balls [i].GetComponent<Renderer> ().material.color = colors_ball [i];
			balls [i].AddComponent<Rigidbody> ();
			balls [i].GetComponent<Rigidbody> ().useGravity = false;
			if (display_reference == true) {
				balls [i].transform.SetParent (head.transform);
			}
		}


	}


	private void UpdateBody() // for body reference, discarded
	{
		if (local_http == true) {
			for (int i = 0; i < balls_space.Length; i++) {
				//rotation_body.x = 0;
				//rotation_body.z = 0;
				//rotation_body.w = 0;
				balls_space [i].transform.position = positions_space [i];

				balls_space [i].transform.position += position_body + shift_head_chest;
				balls_space [i].transform.RotateAround(position_body, Vector3.up, rotation_body.y*3f/Mathf.PI*180);
				balls_space [i].transform.rotation = rotation_old_space;
			}
		}
	}


	// pilot study, show balls in peripheral space, and eyes-free acquisitions, for display reference and world reference

	private int num_ball = 8;
	private Vector3[] positions_ball_world; // fixed in the world
	private Vector3[] positions_ball_head; // relative to the head
	private Quaternion[] rotations_ball;
	private GameObject[] balls;
	private bool bubbling = true;
	private bool display_reference = false;
	private Color[] colors_ball = {Color.black, Color.blue, Color.cyan, Color.gray, Color.green, Color.magenta, Color.red, Color.white, Color.yellow};
	private Vector3 scale_balls = new Vector3(0.1f,0.1f,0.1f);
	public GameObject[] ball_old; // object grabbed by two controllers
	public bool haptic_on = false;
	private float x_left_min = -0.8f;
	private float x_left_max = -0.25f;
	private float x_right_min = 0.25f;
	private float x_right_max = 0.85f;
	private float y_min = -1f;
	private float y_max = 0.6f;
	private float z_min = -0.8f;
	private float z_max = 0.5f;
	private Quaternion rotation_old_head;
	private enum reference {display, body, world};
	public Vector3 position_body; // only one controller is allowed, only for debug.
	public Quaternion rotation_body;
	public bool local_http = true;
	private Vector3 shift_head_chest = new Vector3(0,0.5f,0);

	private reference reference_now = reference.world; //三种不同的坐标系，display物体放到camera的transform下；body通过传输始终竖直的，但是左右跟身体，位置跟身体；world在用户点击后校准
	public int num_button_down = 0;





	//intial by center position, distance and rotation angle
	private float angle_high = 1.4361614f;
	private float angle_low = -1.6575767917f;
	private float angle_front = 0.7355142f;
	private float angle_back = -0.86153696f;
	private Vector3 position_center_right = new Vector3 (0.21f, -0.2393892f, -0.03254552f);
	private float distance_longest = 0.739983472043f;
	private string[] name_prefabs = { "Arrow00_prefab", "Axe00_prefab", "Knife00_prefab", "Mace00_prefab", "Staff00_prefab", "Bow00_prefab", "Sword00_prefab", "Shield00_prefab" };


	// initial balls(objects) in the ranges users could not see, by angles, based on shoulder position and arm length
	private void InitialBalls_angle()
	{
		
		balls = new GameObject[num_ball];
		positions_ball_world = new Vector3[num_ball];
		positions_ball_head = new Vector3[num_ball];
		rotations_ball = new Quaternion[num_ball];
		rotation_old_head = head.transform.rotation;
		for (int i = 0; i < num_ball; i++) {
			// create balls
			//balls[i] = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			//object models
			balls[i] = GameObject.Find(name_prefabs[i]);
			//xrange, yrange, zrange could be adjusted by data
			Vector3 position_new = position_center_right;
			float distance_now = UnityEngine.Random.Range (0.4f, 1f)*distance_longest;
			float alfa = UnityEngine.Random.Range (angle_back, angle_front);
			float beta = UnityEngine.Random.Range (angle_low, angle_high);
			Vector3 position_Toadd = new Vector3 ();
			position_Toadd.x = float.Parse((distance_now * System.Math.Cos (alfa) * System.Math.Cos (beta)).ToString());
			position_Toadd.y = float.Parse((distance_now * System.Math.Sin (beta)).ToString());
			position_Toadd.z = float.Parse((distance_now * System.Math.Sin (alfa) * System.Math.Cos (beta)).ToString());
			position_new += position_Toadd;
			if (i < num_ball / 2)
				position_new.x = - position_new.x;
			positions_ball_head[i] = position_new;
			positions_ball_world [i] = head.transform.TransformPoint(position_new);
			balls[i].transform.position = positions_ball_world[i];
			//Debug.Log (balls[i]);
			rotations_ball [i] = balls [i].transform.rotation;
			balls [i].transform.rotation = rotation_old_head; // test to keep rotation fixed to the head
			/*balls [i].transform.localScale = scale_balls;
			balls [i].GetComponent<Renderer> ().material.color = colors_ball [i];
			balls [i].AddComponent<Rigidbody> ();
			balls [i].GetComponent<Rigidbody> ().useGravity = false;
			*/if (display_reference == true) {
				balls [i].transform.SetParent (head.transform);
			}
		}

	}

	//adjust for different users, angle, log shoulder position and arm length
	private string[] name_task = {"right shoulder", "right most"};
	private int num_task = 0;
	private Vector3[] parameter_arm = new Vector3[2];
	public void TriggerAngle(Vector3 position)
	{
		if (num_task > 1)
			return;
		DateTime time_now = System.DateTime.Now;
		if (time_now.Subtract (time_trigger).TotalSeconds <= time_threthold)// reuse the time thethold for double click
			return;
		time_trigger = time_now;
		parameter_arm [num_task] = head.transform.InverseTransformPoint (position);
		switch (num_task) {
		case 0:
			position_center_right = head.transform.InverseTransformPoint (position);
			break;
		case 1:
			distance_longest = (head.transform.InverseTransformPoint(position) - position_center_right).magnitude;

			break;
		default:
			break;
		}
		num_task++;
		if(num_task == name_task.Length){
			canvas.text = "";
			dataget.logData_parameter ("parameter", parameter_arm);
			return;
		}
		canvas.text = name_task [num_task];
			
	}

	//calibrate for reference system, keep the relative positions of the balls fixed to the head position, apdate the positions referred to the world
	public void CalibrationReference()
	{
		rotation_old_head = head.transform.rotation;
		if (reference_now == reference.display) {
			for (int i = 0; i < balls.Length; i++) {
				positions_ball_world [i] = head.transform.TransformPoint (positions_ball_head [i]);
				balls [i].transform.position = positions_ball_world [i];
				balls [i].transform.rotation = rotation_old_head;
			}
		} else if (reference_now == reference.world) {
			for (int i = 0; i < balls_space.Length; i++) {
				positions_ball_world_space [i] = head.transform.TransformPoint (positions_space [i]);
				balls_space [i].transform.position = positions_ball_world_space [i];
				balls_space [i].transform.rotation = rotation_old_head;
			}
		}
	}






	private float threthold_bubble = 0.35f;

	public bool TriggerGet(GameObject colliding, GameObject mycontroller, Vector3 position_controller)
	{
		if (bubbling == false && colliding != null) {
			//Debug.Log ("collide");
			colliding.transform.position = mycontroller.transform.position;
			colliding.transform.SetParent(mycontroller.transform);
			ball_old[num_button_down-1] = colliding;
			return true;
		} else if (bubbling == true) {
			int nearest = -1;
			float distance_nearest = 10000;
			for (int i = 0; i < num_ball; i++) {
				float distance = (position_controller - balls[i].transform.position).magnitude;
				if (distance < distance_nearest) {
					nearest = i;
					distance_nearest = distance;
				}
			}
			//Debug.Log (distance_nearest);
			if (distance_nearest > threthold_bubble)
				return false;
			ball_old[num_button_down-1] = balls [nearest];
			ball_old[num_button_down-1].transform.position = mycontroller.transform.position;
			ball_old[num_button_down-1].transform.SetParent(mycontroller.transform);
			return true;
		}
		else {
			//ball_old = null;
			return false;
		}
	}



	public void ReleaseBall(GameObject objInHand)
	{
		string name_object = objInHand.name;
		//Debug.Log (name_object);
		int index = -1;
		for (int i = 0; i < name_prefabs.Length; i++) {
			if (name_object == name_prefabs [i]) {
				index = i;
				break;
			}
		}
		if (index != -1) {
			objInHand.transform.position = positions_ball_world [index];
			objInHand.transform.SetParent (null);
			objInHand.transform.rotation = rotation_old_head;
		}
		/*
		if (ball_old != null && rotation_old != null) {
			ball_old.transform.SetParent (null);
			ball_old.transform.position = position_old;
			ball_old.transform.rotation = rotation_old;
		}*/
	}

	 //study 2 pointing in the space



	private Vector3 position_center_space;
	private Vector3[] positions_space; // the positions relative to the head, keep unchanged during the process
	private Vector3[] positions_ball_world_space; // the positions in the world reference
	private Vector3[] positions_result_space;
	private GameObject ball_current;
	private GameObject[] balls_space;

	private float offset_y = 0.1f;
	private string[] name_prefab_space = {"throwingknife14_red","potion04_pink","axe1h13_grey","throwingaxe01_black","dagger14_green","dagger25_green","shield04_orange","dagger02_grey","dagger10_grey","dagger02_black","dagger15_yellow","throwingknife16_black","fork02_silver","saw02_gold","sword2h10_purple","throwingstar03_grey","sword2h15_purple","shovel05_silver","throwingknife15_yellow","throwingknife08_blue","wand02_yellow","throwingaxe16_yellow","dagger19_grey","dagger27_red","sword1h19_blue","hammer01_bronze","throwingknife01_pink","throwingaxe06_pink","sword1h03_green","shield24_blue","throwingstar16_grey","axe1h11_green","axe03_green","throwingknife13_grey","tong01_blue","bar01_silver","dagger25_red","knife01_blue","shovel05_iron","sword2h17_grey","dagger15_red","shield21_blue","key05_bronze","throwingknife12_yellow","throwingstar07_blue","sword2h10_orange","throwingknife03_blue","coin02_silver","blower01_iron","dagger06_red","dagger22_green","shovel01_silver","throwingstar15_blue","dagger23_green","throwingaxe03_white","shield14_yellow","hammer03_gold","book05_purple","hammer01_green","sword2h05_red","axe1h05_yellow","shield08_blue","axe1h02_green","ore02_green","seeds02_grey","sword2h11_black","key04_green","throwingaxe11_red","throwingstar05_orange","throwingknife16_grey","wand01_red","dagger13_green","throwingstar06_green","shield19_red","sword1h21_yellow","shield16_grey","gem05_green","axe2h11_beige","saw02_bronze","dagger22_blue","pickaxe03_bronze","sword1h02_green","sword1h21_white","dagger25_grey","dagger01_green","sword2h12_orange","sword1h13_grey","shield18_black","throwingknife07_blue","hook01_blue","axe02_silver","hammer02_bronze","throwingknife01_orange","shield07_blue","dagger26_yellow","thread03_green","shield09_yellow","sword2h01_green","sword1h13_yellow","dagger16_blue","shield19_yellow","dagger10_red","sword1h02_yellow","gem05_white","axe2h16_beige","pickaxe01_bronze","shield05_brown","bucket01_blue","throwingaxe11_brown","throwingknife13_green","sword1h09_red","sword1h12_red","plate01_iron","sword2h10_yellow","orb03_purple","throwingknife03_yellow","shovel02_silver","throwingstar08_red","potion03_red","sword1h11_blue","axe1h14_brown","sword1h03_grey","pickaxe02_gold","pickaxe01_gold","nails01_green","orb01_red","throwingstar02_green","throwingaxe01_green","axe1h10_yellow","ore01_silver","dagger01_black","sword1h06_red","dynamite02_red","dagger18_yellow","gem03_pink","axe01_bronze","gem05_pink","bar02_iron","sword1h11_green","wand05_red","axe1h07_purple","axe2h02_yellow","axe2h03_green","ink01_blue","tong02_bronze","orb02_yellow","sword2h08_blue","gem06_yellow","potion03_purple","axe04_blue","sword2h14_blue","bar01_bronze","shield12_grey","axe03_iron","coin03_silver","saw01_iron","sword1h20_purple","book02_orange","axe1h15_grey","throwingstar02_blue","dagger22_grey","fishingpole01_gold","throwingaxe04_yellow","throwingstar13_blue","shield08_brown","dagger23_red","dagger09_blue","blower01_wood","gem05_yellow","wood01","throwingknife01_green","shovel03_green","throwingaxe11_black","throwingstar07_red","sword2h19_green","sword1h04_blue","dynamite02_black","orb03_green","bar02_gold","bar02_blue"};
	private Quaternion rotation_old_space;
	private DateTime time_trigger;
	private float time_threthold = 0.1f; // by emprical tests
	public bool touch_and_see = false; // trigger if when user touches an object, we visualize in field of view
	private GameObject[] ball_visual_old;
	private Vector3[] positions_visual;
	private GameObject ball_reference;
	private GameObject[] balls_grid;

	private int num_move;


	private void InitialParameters(){
		ball_visual_old = new GameObject[2];
		ball_old = new GameObject[2];
		touch_and_see = false;
		rotation_old_space = head.transform.rotation;
		positions_visual = new Vector3[2];
		positions_visual [0] =  new Vector3 (0.2f, 0, 0.5f);
		positions_visual [1] = new Vector3 (-0.2f, 0, 0.5f);
	}

	// arrange balls(objects) in the layout of a big shelf in front of the users
	//inverse relative input absolute output
	private void InitialPositionSpace()
	{
		

		InitialParameters ();
		GameObject.Find ("Camera (eye)").GetComponent<Camera> ().fieldOfView = 110.0f;

		zs = new float[num_z_level];
		fis = new float[num_z_level];
		thetas = new float[num_theta_level];
		ds = new float[num_d_level];

		positions_space = new Vector3[num_z_level * num_d_level * num_theta_level];
		positions_ball_world_space = new Vector3[num_z_level * num_d_level * num_theta_level];
		balls_space = new GameObject[num_z_level * num_d_level * num_theta_level];
		positions_result_space = new Vector3[num_z_level * num_d_level * num_theta_level];

		for (int i = 0; i < num_z_level; i++) {
			zs [i] = distance_low + (distance_high - distance_low) * i / num_z_level;
			fis [i] = theta_lowest + (theta_highest - theta_lowest) * i / num_z_level;
		}

		for (int i = 0; i < num_d_level; i++) {
			ds [i] = distance_near + (distance_far - distance_near) * i / num_d_level;
		}

		for (int i = 0; i < num_theta_level; i++) {
			thetas [i] = theta_least + (theta_most - theta_least) * i / num_theta_level;
		}

		SetBallsPosition (false);
	
	}

	private void SetBallsPosition(bool height_fi)
	{
		if (height_fi) {
			for (int i = 0; i < num_z_level; i++) {
				for (int j = 0; j < num_d_level; j++) {
					for (int k = 0; k < num_theta_level; k++) {
						int index_space = i * num_d_level * num_theta_level + j * num_theta_level + k;
						positions_space [index_space].x = ds [j] * Mathf.Sin (thetas [k]);
						positions_space [index_space].y = zs [i] - offset_y;
						positions_space [index_space].z = ds [j] * Mathf.Cos (thetas [k]);
						//balls
						//balls_space[index_space] = GameObject.CreatePrimitive (PrimitiveType.Sphere);
						//objects
						balls_space [index_space] = GameObject.Find (name_prefab_space [index_space]);
						positions_ball_world_space [index_space] = head.transform.TransformPoint (positions_space [index_space]);
						balls_space [index_space].transform.position = positions_ball_world_space [index_space];
						balls_space [index_space].transform.rotation = rotation_old_space;
						balls_space [index_space].transform.localScale = scale_balls * 1.5f;
						balls_space [index_space].GetComponent<Renderer> ().material.color = colors_ball [i];
						balls_space [index_space].AddComponent<Rigidbody> ();
						balls_space [index_space].GetComponent<Rigidbody> ().useGravity = false;
						balls_space [index_space].transform.SetParent (null);
						if (reference_now == reference.display)
							balls_space [index_space].transform.SetParent (head.transform);
					}
				}
			}
		} else {
			for (int i = 0; i < num_z_level; i++) {
				for (int j = 0; j < num_d_level; j++) {
					for (int k = 0; k < num_theta_level; k++) {
						int index_space = i * num_d_level * num_theta_level + j * num_theta_level + k;
						positions_space [index_space].x = ds [j] * Mathf.Cos(fis[i]) * Mathf.Sin (thetas [k]);
						positions_space [index_space].y = ds [j] * Mathf.Sin(fis[i])  - offset_y;
						positions_space [index_space].z = ds [j] * Mathf.Cos(fis[i]) * Mathf.Cos (thetas [k]);
						//balls
						//balls_space[index_space] = GameObject.CreatePrimitive (PrimitiveType.Sphere);
						//objects
						balls_space [index_space] = GameObject.Find (name_prefab_space [index_space]);
						positions_ball_world_space [index_space] = head.transform.TransformPoint (positions_space [index_space]);
						balls_space [index_space].transform.position = positions_ball_world_space [index_space];
						balls_space [index_space].transform.rotation = rotation_old_space;
						balls_space [index_space].transform.localScale = scale_balls * 1.5f;
						balls_space [index_space].GetComponent<Renderer> ().material.color = colors_ball [i];
						balls_space [index_space].AddComponent<Rigidbody> ();
						balls_space [index_space].GetComponent<Rigidbody> ().useGravity = false;
						if (reference_now == reference.display)
							balls_space [index_space].transform.SetParent (head.transform);
					}
				}
			}
		}
	}
	//stand still, the reference is world
	//different trigger because of different object sets and positions, could be merged
	public bool TriggerSpace(GameObject colliding, GameObject mycontroller, Vector3 position_controller)
	{
		Debug.Log (mycontroller.GetInstanceID ());
		GameObject[] balls_state = balls_space;
		if (num_button_down == 2) {//double down to triger mode switch, only can be triggerred in space state
			DateTime time_now = System.DateTime.Now;
			Debug.Log (time_now.Subtract (time_trigger).TotalSeconds);
			if (time_now.Subtract (time_trigger).TotalSeconds <= time_threthold) {
				//	break;
				ReleaseBallSpace (ball_old [0]);//set the object grabbed by the first controller free
				if (reference_now == reference.display) {
					ball_reference = GameObject.CreatePrimitive (PrimitiveType.Sphere);
					ball_reference.transform.localScale = new Vector3 (0.001f, 0.001f, 0.001f);
					ball_reference.transform.position = head.transform.position - new Vector3(0,0.001f,0.001f);
					ball_reference.transform.rotation = head.transform.rotation;
					for (int i = 0; i < balls_space.Length; i++) {
						//balls_space [i].transform.SetParent (null);
						balls_space [i].transform.SetParent (ball_reference.transform);
					}
				}
				if (reference_now == reference.world) {
					CalibrationReference ();
				}
				return false;
			}
		}
		time_trigger = System.DateTime.Now;
		Debug.Log (balls_state.Length);
		if (bubbling == false && colliding != null) {
			Debug.Log ("collide");
			colliding.transform.position = mycontroller.transform.position;
			colliding.transform.SetParent(mycontroller.transform);
			ball_old[num_button_down-1] = colliding;
			return true;
		} else if (bubbling == true) {
			//Debug.Log ("try bubble");
			int nearest = -1;
			float distance_nearest = 10000;
		
			for (int i = 0; i < balls_state.Length; i++) {
				float distance = (position_controller - balls_state[i].transform.position).magnitude;
				if (reference_now == reference.display)
					distance = (position_controller - head.transform.TransformPoint (positions_space [i])).magnitude;
				if (distance < distance_nearest) {
					nearest = i;
					distance_nearest = distance;
				}
			}
			//Debug.Log (distance_nearest);
			//Debug.Log (name_prefab_space [nearest]);
			if (distance_nearest > threthold_bubble)
				return false;
			//Debug.Log (num_button_down);
			//Debug.Log (ball_old.Length);
			ball_old[num_button_down-1] = balls_state [nearest];
			ball_old[num_button_down-1].transform.position = mycontroller.transform.position; // trigger colliding detection and set ball_old to colliding & objInHand
			ball_old[num_button_down-1].transform.SetParent(mycontroller.transform);

			return true;
		}
		else {
			ball_old[num_button_down-1] = null;
			return false;
		}
	}


	public void ReleaseBallSpace(GameObject objInHand) // release the ball in different states
	{
		if (objInHand == null)
			return;
		// state == m_state.Space_acquire
		string name_object = objInHand.name;
		//Debug.Log (name_object);
		int index = -1;
		for (int i = 0; i < name_prefab_space.Length; i++) {
			if (name_object == name_prefab_space [i]) {
				index = i;
				break;
			}
		}
		//Debug.Log (index);
		if (index != -1) {
			objInHand.transform.position = positions_ball_world_space [index];
			objInHand.transform.SetParent (null);
			if(index != 0)
				objInHand.transform.rotation = balls_space[0].transform.rotation;
			else
				objInHand.transform.rotation = balls_space[1].transform.rotation;
		}
		if (reference_now == reference.display) {
			for (int i = 0; i < balls_space.Length; i++) {
				balls_space [i].transform.position = head.transform.TransformPoint (positions_space [i]);
				balls_space [i].transform.SetParent (head.transform);
			}
			if (ball_reference != null)
				ball_reference = null;
		}

	}

	public void VisualTouch(GameObject colliding,GameObject mycontroller,Vector3 position_controller)
	{
		//Debug.Log ("visual");
		int index = (mycontroller.GetInstanceID() == 19966?0:1);
		GameObject ball_to_visual = null;
		GameObject[] balls_state = balls_space;
		if (bubbling == false && colliding != null) {
			ball_to_visual = colliding;
		} else if (bubbling == true && balls_space != null) {
			//Debug.Log ("try bubble");
			int nearest = -1;
			float distance_nearest = 10000;
			for (int i = 0; i < balls_state.Length; i++) {
				float distance = (position_controller - head.transform.TransformPoint(positions_space[i])).magnitude;
				if (reference_now == reference.world)
					distance = (position_controller - positions_ball_world_space[i]).magnitude;
				if (distance < distance_nearest) {
					nearest = i;
					distance_nearest = distance;
				}
			}
			//Debug.Log (distance_nearest);
			//Debug.Log (name_prefab_space [nearest]);
			if (distance_nearest <= threthold_bubble)
				ball_to_visual = balls_state [nearest];
		}
		if (ball_visual_old == null)
			ball_visual_old = new GameObject[2];
		if (ball_to_visual != null) {
			if (ball_visual_old [index] != null) {
				if (ball_visual_old [index].name == ball_to_visual.name)
					return;
				Destroy (ball_visual_old [index]);
				ball_visual_old [index] = GameObject.Instantiate (ball_to_visual, head.transform.TransformPoint(positions_visual [index]), ball_to_visual.transform.rotation);
				//ball_visual_old [index].transform.SetParent (head.transform);
			} else {
				ball_visual_old [index] = GameObject.Instantiate (ball_to_visual, head.transform.TransformPoint(positions_visual [index]), ball_to_visual.transform.rotation);
				//ball_visual_old [index].transform.SetParent (head.transform);
			}

				
		} else if (ball_visual_old [index] != null) {
			Destroy (ball_visual_old [index]);
			ball_visual_old [index] = null;
		}
		return;
			
	}


	//study 1
	private GameObject ball_target;
	private GameObject ball_reset;
	private GameObject ball_check;
	private Vector3 position_show;
	private int num_amount_one; // amount of tasks in study 1
	private int num_now_one = 0; // the number of trials completed, increase once triggerred, order_target[num_now_one]is the index of the target position
	private Vector3[] positions_world_one;
	private Vector3[] positions_relative_one;
	private Vector3[] positions_result;
	private int[] orders_move_targets;
	private Vector3[] positions_body;
	private Quaternion[] rotations_body;
	private Vector3[] positions_head;
	private Quaternion[] rotations_head;
	private Vector3[] positions_correct;
	private bool[] visible_result;
	private int[] order_target;
	private enum state_study1
	{
		appear,
		move,
		acquire,
		reset
	} 
	private state_study1 appear_move_acquire ; // target appear, move the direction, acquire the target
	private Vector3 position_reset;
	private Vector3 position_check;
	//private Vector3[] positions_reset_relative = {new Vector3(0,0,0.5f), new Vector3(0.25f,0,0.433f), new Vector3(0.433f,0,0.25f),new Vector3(0.5f,0,0)};
	private int num_reset = 0;
	private Vector3 position_marker;
	private GameObject ball_marker;
	private LineRenderer line;

	//for get ready
	private Vector3[] positions_ready_relative = {new Vector3(0.5f, 0, 0), new Vector3(0.65f, 0, 0), new Vector3(0.8f,0,0), new Vector3(0.32f,0,0.383f), new Vector3(0.416f, 0, 0.498f), new Vector3(0.512f,0,0.613f), new Vector3(0.2f, 0.2f,0.2f), new Vector3(0.26f,0.26f,0.26f), new Vector3(0.32f,0.32f,0.32f) };
	private GameObject[] balls_ready;
	private float[] zs;
	private float[] thetas;
	private float[] fis;
	private float[] ds;
	private int num_z_level = 5;
	private int num_d_level = 1;
	private int num_theta_level = 12;
	private int num_move_level = 12;
	private float distance_high = 0.7f;
	private float distance_low = -0.7f;
	private float distance_near = 0.65f;
	private float distance_far = 0.65f;
	private float distance_move = 0.5f;
	private float theta_least = - Mathf.PI;
	private float theta_most = Mathf.PI;
	private float theta_highest = Mathf.PI/3;
	private float theta_lowest = -Mathf.PI/3;
	private float theta_move_least = -Mathf.PI;
	private float theta_move_most = Mathf.PI;
	private Vector3 position_offset = new Vector3 (0.0f, -0.15f, -0.10f);
	private float[] angles_move;
	private Vector3 position_first_head;
	private int[] order_move;
	private string[] text_horizon_study1 = { "南","西南","西南", "西", "西北", "西北", "北", "东北", "东北", "东", "东南","东南" };
	private string[] text_vertical_study1 = { "下下", "下", "中", "上", "上上" };
	private string[] text_move_study1 = { "左左", "左", "右", "右右" };
	private string[] text_flags = { "北", "东", "南", "西" };
	private Vector3[] positions_flag = {
		new Vector3 (0, 0, -50),
		new Vector3 (-25, 0, -43.3f),
		new Vector3 (-43.3f, 0, -25),
		new Vector3 (-50, 0, 0),
		new Vector3 (-43.3f, 0, 25),
		new Vector3 (-25, 0, 43.3f),
		new Vector3 (0, 0, 50),
		new Vector3 (25, 0, 43.3f),
		new Vector3 (43.3f, 0, 25),
		new Vector3 (50, 0, 0),
		new Vector3 (43.3f, 0, -25),
		new Vector3 (25, 0, -43.3f),


	};
	private Color[] colors_flag = { new Color(0,0,1), new Color(0.5f,0,0.866f), new Color(0.866f,0,0.5f), new Color(1,0,0), new Color(0.866f,0.5f,0), new Color(0.5f,0.866f,0),new Color(0,1,0), new Color(0.5f,1,0.866f), new Color(0.866f,1,0.5f),new Color(1,1,0), new Color(1,0.866f,0.5f), new Color(1,0.5f,0.866f), };
	private int[] index_check;
	private int num_check = 10;
	GameObject controller_left;
	bool[] have_index;
	bool timer_on = false;
	int timer_tick_count;
	int timer_tick_amount;
	
	private void Initiate_lines()
	{
		line = gameObject.AddComponent<LineRenderer> ();
		GameObject plane = GameObject.Find ("Plane");
		Vector3 start_line = cam.transform.position + position_offset;
		Vector3 end_line = new Vector3 ();
		start_line.y = plane.transform.position.y+0.01f;
		start_line += Vector3.back * 50;
		//line.material = new Material (Shader.Find ("Particles/Additive"));
		//line.startColor = Color.blue;
		//line.endColor = Color.blue;
		line.material.SetColor ("_TintColor",new Color(1,0,0,1));
		//line.SetColors(Color.black, Color.black);
		line.startWidth = 0.1f;
		line.endWidth = 0.1f;
		line.SetPosition (0, start_line);
		line.SetPosition (1, start_line + Vector3.forward*100);
		line = GameObject.Find ("Line").AddComponent<LineRenderer> ();
		start_line.y = plane.transform.position.y+0.01f;
		start_line += Vector3.left * 50 - Vector3.back*50;
		line.material.SetColor ("_TintColor",new Color(0,1,0,1));
		line.startWidth = 0.1f;
		line.endWidth = 0.1f;
		line.SetPosition (0, start_line);
		line.SetPosition (1, start_line + Vector3.right*100);

		GameObject lg = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		lg.transform.localScale = scale_balls * 0.001f;
		line = lg.AddComponent<LineRenderer> ();

		start_line.y = plane.transform.position.y+0.01f;
		start_line += Vector3.back*4 - Vector3.left*46;
		end_line = start_line + Vector3.forward * 8;
		line.startWidth = 0.1f;
		line.endWidth = 0.1f;
		line.SetPosition (0, start_line);
		line.SetPosition (1, end_line);
		lg = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		lg.transform.localScale = scale_balls * 0.001f;
		line = lg.AddComponent<LineRenderer> ();

		start_line.y = plane.transform.position.y+0.01f;
		start_line = end_line;
		end_line += Vector3.right*8;
		line.startWidth = 0.1f;
		line.endWidth = 0.1f;
		line.SetPosition (0, start_line);
		line.SetPosition (1, end_line);
		lg = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		lg.transform.localScale = scale_balls * 0.001f;
		line = lg.AddComponent<LineRenderer> ();

		start_line.y = plane.transform.position.y+0.01f;
		start_line = end_line;
		end_line += Vector3.back*8;
		line.startWidth = 0.1f;
		line.endWidth = 0.1f;
		line.SetPosition (0, start_line);
		line.SetPosition (1, end_line);
		lg = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		lg.transform.localScale = scale_balls * 0.001f;
		line = lg.AddComponent<LineRenderer> ();

		start_line.y = plane.transform.position.y+0.01f;
		start_line = end_line;
		end_line += Vector3.left*8;
		line.startWidth = 0.1f;
		line.endWidth = 0.1f;
		line.SetPosition (0, start_line);
		line.SetPosition (1, end_line);

	}
	private void Show_Text(){
		
		if (appear_move_acquire == state_study1.acquire)
			aim.text = "G";
		else if (appear_move_acquire == state_study1.appear)
			aim.text = "A";
		else if (appear_move_acquire == state_study1.move)
			aim.text = "M";
		else
			aim.text = "C";
	}

	private void Destroy_Study1Phase2()
	{
		if (ball_target != null)
			Destroy (ball_target);
		if (ball_reset != null)
			Destroy (ball_reset);
		if (ball_marker != null)
			Destroy (ball_marker);
		if (ball_reference != null)
			Destroy (ball_reference);
		zs = null;
		fis = null;
		thetas = null;
		ds = null;
		num_amount_one = 0;
		num_now_one = 0;
		num_reset = 0;
		num_move = 0;

		positions_world_one = null;
		positions_relative_one = null;
		positions_result = null;
		positions_correct = null;
		angles_move = null;
		order_target = null;
		visible_result = null;
		
	}
	private float[] angles_side = { Mathf.PI / 4, Mathf.PI * 55 / 180 };
	private float distance_side = 10f;
	private GameObject[] plane_angles;
	private float height_plane = 3;

	private void Debug_Seen()
	{
		Camera cam = head.GetComponent<Camera> ();
		Debug.Log (cam.WorldToViewportPoint (ball_target.transform.position));
	}

	private void Initiate_FOV()
	{
		//Camera cam = GameObject.Find("Camera (eye)").GetComponent<Camera>();
		//cam.fieldOfView = 60.0f;
		//head.GetComponent<Camera> ().aspect = 16.0f / 10.0f;

		plane_angles = new GameObject[2];
		plane_angles [0] = GameObject.CreatePrimitive (PrimitiveType.Cube);
		plane_angles [0].transform.position = head.transform.TransformPoint (new Vector3 (0.01f, 0, 0.01f));
		plane_angles [0].transform.rotation = head.transform.rotation;
		plane_angles [0].transform.Rotate (Vector3.forward*90);
		plane_angles [0].transform.localScale= new Vector3(0.01f,0.01f,0.01f);
		plane_angles [0].GetComponent<Renderer> ().material.color = Color.black;
		plane_angles [0].transform.SetParent (head.transform);


	}
	private void Initiate_Balls() // target, reset, marker, reference
	{
		controller_left = GameObject.Find ("Controller (left)");
		SteamVR_Controller.Device controller = controller_left.GetComponent<ObjectInHand>().controller;
		if(controller.index != 3)
			controller_left = GameObject.Find ("Controller (right)");
		position_marker = head.transform.position;
		position_marker.y = 0.5f;
		ball_marker = GameObject.CreatePrimitive (PrimitiveType.Cube);
		ball_marker.transform.position = position_marker;
		ball_marker.GetComponent<Renderer> ().material.color = Color.red;
		ball_marker.transform.localScale = new Vector3(0.2f,0.01f,0.2f);
		ball_marker.transform.rotation = head.transform.rotation;
		//ball_target = GameObject.Find("Ball_target");
		ball_target = GameObject.CreatePrimitive(PrimitiveType.Sphere);

		ball_target.transform.localScale = scale_balls*0.3f;
		ball_target.AddComponent<Rigidbody> ();
		ball_target.GetComponent<Rigidbody> ().useGravity = false;
		ball_target.GetComponent<Renderer> ().material.color = Color.black;
		ball_target.name = "Ball_Target";

		ball_reset = GameObject.CreatePrimitive (PrimitiveType.Cube);
		ball_reset.transform.localScale = scale_balls * 0.4f;
		ball_reset.AddComponent<Rigidbody> ();
		ball_reset.GetComponent<Rigidbody> ().useGravity = false;
		ball_reset.GetComponent<Renderer> ().material.color = Color.gray;
		ball_reset.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeAll;
		ball_reset.name = "Ball_Reset";
		ball_reset.transform.position = position_reset;
		ball_check = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		ball_check.transform.localScale = scale_balls * 0.01f;
		ball_check.AddComponent<Rigidbody> ();
		ball_check.GetComponent<Rigidbody> ().useGravity = false;
		ball_check.GetComponent<Renderer> ().material.color = Color.gray;
		ball_check.name = "Ball_Check";
		ball_check.transform.position = position_check;
		ball_check.transform.LookAt (position_first_head);
		ball_reset.transform.LookAt (position_first_head);
		ball_reset.transform.SetParent (ball_check.transform);
		for (int i = 0; i < positions_flag.Length; i++) {
			GameObject ball_flag = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			ball_flag.GetComponent<Renderer> ().material.color = colors_flag [i];
			ball_flag.transform.position = cam.transform.position + position_offset + positions_flag [i];
			GameObject text3d = new GameObject ();
			text3d.AddComponent <TextMesh>();
			text3d.GetComponent<TextMesh> ().text = text_horizon_study1 [i];
			text3d.GetComponent<TextMesh> ().color = Color.black;
			text3d.GetComponent<TextMesh> ().font = canvas.font;
			text3d.GetComponent<TextMesh> ().characterSize = 0.2f;
			text3d.transform.position = cam.transform.position + position_offset + positions_flag[i]/10;
			text3d.transform.LookAt (ball_flag.transform.position);
		}
	}





	private  void Initiate_Positions()
	{
		position_first_head = cam.transform.position+position_offset;
		zs = new float[num_z_level];
		fis = new float[num_z_level];
		thetas = new float[num_theta_level];
		ds = new float[num_d_level];
		num_amount_one = num_z_level * num_d_level * num_theta_level;

		positions_world_one = new Vector3[num_amount_one];
		positions_relative_one = new Vector3[num_amount_one];
	    positions_correct = new Vector3[num_amount_one * num_move_level];
		orders_move_targets = new int[num_amount_one * num_move_level];
		positions_result = new Vector3[num_amount_one*num_move_level];
		positions_body = new Vector3[num_amount_one*num_move_level*2];
		rotations_body = new Quaternion[num_amount_one * num_move_level*2];
		positions_head = new Vector3[num_amount_one*num_move_level*2];
		rotations_head = new Quaternion[num_amount_one * num_move_level*2];
		order_target = new int[num_amount_one];
		visible_result = new bool[num_amount_one * num_move_level];



		for (int i = 0; i < num_z_level; i++) {
			zs [i] = distance_low + (distance_high - distance_low) * i / num_z_level;
			fis [i] = theta_lowest + (theta_highest - theta_lowest) * i / (num_z_level-1);
		}

		for (int i = 0; i < num_d_level; i++) {
			ds [i] = distance_near + (distance_far - distance_near) * i / num_d_level;
		}

		for (int i = 0; i < num_theta_level; i++) {
			thetas [i] = theta_least + (theta_most - theta_least) * i / (num_theta_level);
			Debug.Log (thetas [i]);
		}

		for (int i = 0; i < num_z_level; i++) {
			for (int j = 0; j < num_d_level; j++) {
				for (int k = 0; k < num_theta_level; k++) {
					int index_space = i * num_d_level * num_theta_level + j * num_theta_level + k;
					Vector3 position_tmp = new Vector3 ();
					position_tmp.x = ds [j] * Mathf.Cos(fis[i])*Mathf.Sin (thetas [k]);
					position_tmp.y = ds [j] * Mathf.Sin(fis[i]);
					position_tmp.z = ds [j] * Mathf.Cos(fis[i])*Mathf.Cos (thetas [k]);

					positions_relative_one [index_space] = position_tmp+position_offset;
					//positions_world_one [index_space] = cam.transform.TransformPoint (position_tmp+position_offset);
					positions_world_one[index_space] = cam.transform.position+ positions_relative_one[index_space];
					//GameObject ball_temp = GameObject.CreatePrimitive (PrimitiveType.Sphere);
					//ball_temp.transform.position = positions_world_one [index_space];
					//ball_temp.transform.localScale = scale_balls * 0.05f;
					//ball_temp.transform.RotateAround (cam.transform.position, Vector3.up, 15f);
					//positions_world_one [index_space] = ball_temp.transform.position;
					//positions_relative_one [index_space] = cam.transform.InverseTransformPoint (ball_temp.transform.position);
					order_target [index_space] = index_space;
				}
			}
		}
		int index_center = (num_z_level / 2) * num_theta_level + num_theta_level / 2;
		Vector3 position_reset_relative = positions_relative_one [index_center] + new Vector3 (0,0.15f, 0f);
		position_reset = cam.transform.position + position_reset_relative;
		Vector3 position_check_relative = positions_relative_one [index_center] + new Vector3 (0,0.15f, 0.1f);
		position_check = cam.transform.position + position_check_relative;
		int n = order_target.Length;
		while (n > 1) {
			n--;
			int k = UnityEngine.Random.Range (0, n);
			int tmp = order_target [k];
			order_target [k] = order_target [n];
			order_target [n] = tmp;
		}
	}
	private void Destroy_Get_Ready()
	{
		if (balls_ready == null)
			return;
		for (int i = 0; i < balls_ready.Length; i++)
			Destroy (balls_ready [i]);
		balls_ready = null;
	}
	private void Initiate_Get_Ready()
	{
		Destroy_Get_Ready ();
		balls_ready = new GameObject[positions_ready_relative.Length];
		haptic_on = false;
		canvas.text = "";
		for (int i = 0; i < balls_ready.Length; i++) {
			balls_ready [i] = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			balls_ready [i].transform.localScale = scale_balls * 0.3f;
			balls_ready [i].transform.position = head.transform.TransformPoint (positions_ready_relative [i]);
			balls_ready [i].GetComponent<Renderer> ().material.color = Color.red;
		}
	}

	private void Initiate_Study1_Phase2()
	{
		
		bubbling = false;
		haptic_on = false;
		touch_and_see = false;
		appear_move_acquire = state_study1.appear;
		Destroy_Get_Ready ();
		Destroy_Study1Phase2 ();
		Show_Text ();
		Initiate_Positions ();
		Initiate_Balls ();
		Initiate_lines ();

		have_index = new bool[horizon_rotate_level * vertical_rotate_level];
		for (int i = 0; i < have_index.Length; i++)
			have_index [i] = false;
		for(int i = 0;i < num_check;i++)
		{
			int new_index = UnityEngine.Random.Range(0,have_index.Length);
			while(have_index[new_index] == true)
				new_index = UnityEngine.Random.Range(0,have_index.Length);
			have_index [new_index] = true;
			Debug.Log (new_index);
		}

		dataget.initiate_log ("time", "angle");

		if (reference_now == reference.display) {
			ball_reference = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			ball_reference.transform.localScale = new Vector3 (0.001f, 0.001f, 0.001f);
			ball_reference.transform.position = head.transform.position - new Vector3(0,0.001f,0.001f);
			ball_reference.transform.rotation = head.transform.rotation;
			//ball_target.transform.SetParent (ball_reference.transform);
		}
		ball_target.transform.position = new Vector3 (100, 100, -100);
		//ball_target.transform.position = positions_world_one [order_target[num_now_one]];
		int index_now = order_target [num_now_one];
		int index_vertical = index_now / num_d_level / num_theta_level;
		int index_horizon = index_now % num_theta_level;
		string text_canvas = num_now_one.ToString ();
		text_canvas += "\n" + text_horizon_study1 [index_horizon] + text_vertical_study1 [index_vertical];

		canvas.text = text_canvas;
	}
	//nothing about release for study 1 phase 2
	public void TriggerStudy1Phase2(GameObject colliding, GameObject mycontroller, Vector3 position_controller)
	{
		//avoid quick two trigger
		Debug.Log (appear_move_acquire);
		if (time_trigger == null) {
			time_trigger = System.DateTime.Now;
		} else {
			DateTime time_now = System.DateTime.Now;
			if (time_now.Subtract (time_trigger).TotalSeconds <= time_threthold)// reuse the time thethold for double click
				return;
			time_trigger = time_now;
		}

		string text_time = DateTime.Now.ToString ();
		Debug.Log (text_time);
		dataget.log_time (order_target [num_now_one].ToString (), appear_move_acquire.ToString (), text_time, mycontroller.GetInstanceID().ToString());

		// reset state
		if (appear_move_acquire == state_study1.appear) {
			if (colliding != null && colliding.name == ball_reset.name) {
				num_move = 0;
				int index_theta = order_target [num_now_one] % num_theta_level;
				Debug.Log (index_theta);
				Debug.Log (text_horizon_study1 [index_theta]);
				float value_theta = theta_least + index_theta * (theta_most - theta_least) / (num_theta_level - 1);
				order_move = new int[num_move_level];
				for (int i = 0; i < order_move.Length; i++) {
					order_move [i] = i;
				}
				int n = order_move.Length;
				while (n > 1) {
					n--;
					int k = UnityEngine.Random.Range (0, n);
					int tmp = order_move [k];
					order_move [k] = order_move [n];
					order_move [n] = tmp;
				}
				Debug.Log (ball_target == null);
				if (reference_now == reference.display)  // in display reference, we create target relative to head, only rotate, log positions_relative_one, results log inverse
					ball_target.transform.position = head.transform.TransformPoint (positions_relative_one [order_target [num_now_one]]);
				else
					ball_target.transform.position = positions_world_one [order_target [num_now_one]];
				ball_target.transform.SetParent (null);
				ball_target.transform.localScale = scale_balls * 0.3f;
				appear_move_acquire = state_study1.move;


			}
		} else if (appear_move_acquire == state_study1.move) {
			if (num_move < num_move_level) {
				float y_old = ball_check.transform.position.y;
				int index_middle = num_z_level / 2 * num_theta_level + order_move [num_move];
				float x_new = positions_world_one [index_middle].x;
				float z_new = positions_world_one [index_middle].z+0.1f;
				ball_check.transform.position = new Vector3 (x_new, y_old, z_new);
				ball_check.transform.LookAt (position_first_head);
				ball_target.transform.localScale = scale_balls * 0.0001f;
			}
			int move_beta = order_move [num_move]-6;
			if (move_beta < 0)
				move_beta += 12;
			string move_instruction = "";
			if (move_beta <= 6)
				move_instruction = "右转" + (move_beta * 30).ToString ();
			else
				move_instruction = "左转" + (360 - move_beta * 30).ToString ();
			canvas.text = move_instruction;
			//canvas.text = text_horizon_study1 [order_move [num_move]];
			appear_move_acquire = state_study1.reset;
		} else if (appear_move_acquire == state_study1.reset) {
			positions_body [order_target [num_now_one] * num_move_level*2 + num_move*2] = controller_left.transform.position;
			rotations_body [order_target [num_now_one] * num_move_level*2 + num_move*2] = controller_left.transform.rotation;
			positions_head [order_target [num_now_one] * num_move_level*2 + num_move*2] = cam.transform.position;
			rotations_head [order_target [num_now_one] * num_move_level*2 + num_move*2] = cam.transform.rotation;
			//ball_reset.transform.localScale = scale_balls * 0.2f;
			//ball_check.transform.localScale = scale_balls * 0.2f;
			appear_move_acquire = state_study1.acquire;
			ball_reset.GetComponent<Renderer> ().material.color = Color.gray;
			if (have_index [num_now_one] == true && num_move == num_move_level / 2) {
				timer_tick_count = 0;
				timer_tick_amount = UnityEngine.Random.Range (60, 240);
				timer_on = true;
			}
		}else if (appear_move_acquire == state_study1.acquire){
			
			if (reference_now == reference.world) {
				Vector3 position_v = cam.WorldToViewportPoint (ball_target.transform.position);
				bool visible_new = false;
				if (position_v.x > 0 && position_v.x < 1 && position_v.y > 0 && position_v.y < 1 && position_v.z > 0 && position_v.z < 1 && position_v.x + position_v.y + position_v.z >= 1)
					visible_new = true;
				orders_move_targets [order_target [num_now_one] * num_move_level + num_move] = order_move [num_move];
				positions_result [order_target [num_now_one]*num_move_level+num_move] = position_controller;
				positions_correct [order_target [num_now_one] * num_move_level + num_move] = positions_world_one [order_target [num_now_one]];
				positions_body [order_target [num_now_one] * num_move_level*2 + num_move*2+1] = controller_left.transform.position;
				rotations_body [order_target [num_now_one] * num_move_level*2 + num_move*2+1] = controller_left.transform.rotation;
				positions_head [order_target [num_now_one] * num_move_level*2 + num_move*2+1] = cam.transform.position;
				rotations_head [order_target [num_now_one] * num_move_level*2 + num_move*2+1] = cam.transform.rotation;
				visible_result [order_target [num_now_one] * num_move_level + num_move] = visible_new;
				float y_old = ball_check.transform.position.y;
				int index_center = num_z_level / 2 * num_theta_level + num_theta_level/2;
				float x_new = positions_world_one [index_center].x;
				float z_new = positions_world_one [index_center].z;
				ball_check.transform.position = new Vector3 (x_new, y_old, z_new);
				ball_check.transform.LookAt (position_first_head);
				//ball_check.transform.localScale = scale_balls * 0.4f;
				//ball_reset.transform.localScale = scale_balls * 0.4f;
				if (num_move < num_move_level - 1) {
					num_move++;
					float yold = ball_check.transform.position.y;
					int index_middle = num_z_level / 2 * num_theta_level + order_move [num_move];
					float xnew = positions_world_one [index_middle].x;
					float znew = positions_world_one [index_middle].z;
					ball_check.transform.position = new Vector3 (xnew, yold, znew);
					ball_check.transform.LookAt (position_first_head);
					int index_relative = (order_move [num_move] - order_move [num_move - 1] + 6) % num_move_level;
					if (index_relative < 0)
						index_relative += 12;
					//canvas.text = text_horizon_study1 [index_relative];
					int move_beta = order_move[num_move] - order_move[num_move-1];
					if (move_beta < 0)
						move_beta += 12;
					string move_instruction = "";
					if (move_beta <= 6)
						move_instruction = "右转" + (move_beta * 30).ToString ();
					else
						move_instruction = "左转" + (360 - move_beta * 30).ToString ();
					canvas.text = move_instruction;
					//canvas.text = text_horizon_study1[order_move[num_move]];
					appear_move_acquire = state_study1.reset;
					Show_Text ();
					return;
				} else {
					num_move = 0;
				}
				num_now_one++;



				if (num_now_one == positions_world_one.Length) {
					dataget.logData_Study1_Phase2 ("Study1Phase2", positions_correct, positions_result, visible_result, positions_body, rotations_body,positions_head,rotations_head,orders_move_targets);
					state = m_state.Unstart;
					canvas.text = "Done, Thanks";
				} else {
					int index_now = order_target [num_now_one];
					int index_vertical = index_now / num_theta_level;
					int index_horizon = index_now % num_theta_level;
					string text_canvas = num_now_one.ToString ();
					text_canvas += "\n" + text_horizon_study1 [index_horizon] + text_vertical_study1 [index_vertical];

					canvas.text = text_canvas;
				}
			}
			appear_move_acquire = state_study1.appear;
			ball_target.transform.position = new Vector3 (100, 100, -100);

		}
		Show_Text ();




	}

	private int num_ball_put = 30;
	private int num_now_put = -1;
	private bool put_get = true;
	private List<Vector3> positions_ball_put;
	private List<GameObject> balls_put;
	private List<GameObject> text3ds_put;
	private List<int> text_tests;
	private int num_test_now;
	private List<bool> result_same;
	private List<Vector3> result_offset;
	private Quaternion rotation_head_study1;
	private Vector3 position_text_offset = new Vector3(-0.02f,0.05f,0.47f);
	private Color[] color_ball_study1 = {
		Color.black,
		Color.blue,
		Color.red,
		Color.cyan,
		Color.gray,
		Color.green,
		Color.magenta,
		Color.white,
		Color.yellow
	};
	private float length_grid = 0.2f;
	private const float length_grid_old = 0.2f;
	private int num_edge_grid = 5;
	private Vector3 position_grid;
	private Vector3 position_zero;
	private int horizon_rotate_level = 12;
	private int vertical_rotate_level = 5;
	private float vertical_low = -Mathf.PI / 3;
	private float vertical_high = Mathf.PI/3;
	private float[] horizon_angles;
	private float[] vertical_angles;
	private int num_rotate;
	private Vector3[] position_first;
	private float y_offset;
	private float z_offset;
	private float x_offset;
	private string[] text_vertical = { "中","上","下","上","下" };
	private string[] text_horizon = { "前","右前","右前","右","右后","右后","后","左后","左后","左","左前","左前" };

	private void CreateNewTarget()
	{
		num_now_put++;
		GameObject text3d = new GameObject ();
		text3d.AddComponent <TextMesh>();
		text3d.GetComponent<TextMesh> ().text = num_now_put.ToString ();
		text3d.GetComponent<TextMesh> ().color = Color.black;
		text3d.GetComponent<TextMesh> ().font = canvas.font;
		text3d.GetComponent<TextMesh> ().characterSize = 0.05f;
		text3d.transform.position = position_text_offset;
		text3d.transform.rotation = rotation_head_study1;
		text3ds_put.Add (text3d);
		balls_put.Add(GameObject.CreatePrimitive (PrimitiveType.Sphere));
		balls_put [num_now_put].name = num_now_put.ToString ();
		balls_put [num_now_put].transform.position = position_show;
		balls_put [num_now_put].transform.localScale = scale_balls*0.8f;
		balls_put [num_now_put].AddComponent<Rigidbody> ();
		balls_put [num_now_put].GetComponent<Rigidbody> ().useGravity = false;
		balls_put [num_now_put].GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeAll;
		Color color_new;
		if (num_now_put < color_ball_study1.Length)
			color_new = color_ball_study1 [num_now_put];
		else
			color_new = new Color (UnityEngine.Random.Range (0f, 1f), UnityEngine.Random.Range (0f, 1f), UnityEngine.Random.Range (0f, 1f));
		balls_put [num_now_put].GetComponent<Renderer> ().material.color = color_new;
		text3ds_put [num_now_put].transform.SetParent (balls_put [num_now_put].transform);

	}
	private int center;
	private GameObject ball_front1;
	private GameObject ball_front2;
	private void Initiate_Rect()
	{
		center = num_edge_grid * num_edge_grid / 2;
		num_rotate = 0;
		y_offset = -0.20f;
		z_offset = -0.12f;
		x_offset = 0.04f;
		horizon_angles = new float[horizon_rotate_level];
		vertical_angles = new float[vertical_rotate_level];
		position_first = new Vector3[horizon_rotate_level * vertical_rotate_level];
		for (int i = 0; i < horizon_rotate_level; i++) {
			horizon_angles [i] = i * Mathf.PI*2 / horizon_rotate_level;
		}
		vertical_angles [0] = 0;
		for (int i = 1; i <= vertical_rotate_level/2; i++) {
			vertical_angles [2*i] = vertical_low * i * 2 / (vertical_rotate_level-1);
			vertical_angles [2 * i - 1] = vertical_high * i * 2 / (vertical_rotate_level-1);
		}
		for (int i = 0; i < vertical_rotate_level; i++) {
			for (int j = 0; j < horizon_rotate_level; j++) {
				float y_new = Mathf.Sin (vertical_angles [i]) * distance_far;
				float x_new = Mathf.Cos (vertical_angles [i]) * Mathf.Sin (horizon_angles [j]) * distance_far;
				float z_new = Mathf.Cos (vertical_angles [i]) * Mathf.Cos (horizon_angles [j]) * distance_far;
				position_first [i * horizon_rotate_level + j] = cam.transform.TransformPoint( new Vector3 (x_new, y_new, z_new) + new Vector3 (x_offset, y_offset, z_offset));
			}
		}
		balls_grid = new GameObject[num_edge_grid * num_edge_grid];
		position_zero = cam.transform.position;
		position_grid = new Vector3 (-length_grid/2, length_grid/2, distance_far);
		for (int i = 0; i < num_edge_grid; i++) {
			for (int j = 0; j < num_edge_grid; j++) {
				int index_now = i * num_edge_grid + j;
				balls_grid [index_now] = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				balls_grid [index_now].transform.localScale = scale_balls * 0.3f;
				Vector3 position_item = cam.transform.InverseTransformPoint (position_first [0]) + new Vector3 (i * length_grid / num_edge_grid - length_grid / 2, j * length_grid / num_edge_grid - length_grid / 2, 0);
				balls_grid [index_now].transform.position = cam.transform.TransformPoint(position_item);
				balls_grid [index_now].AddComponent<Rigidbody> ();
				balls_grid [index_now].GetComponent<Rigidbody> ().useGravity = false;
				balls_grid [index_now].GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeAll;
				balls_grid [index_now].name = index_now.ToString ();
			}
		}
		balls_grid [center].transform.LookAt (cam.transform.position);
		for (int i = 0; i < balls_grid.Length; i++) {
			if(i != center)
				balls_grid [i].transform.SetParent (balls_grid [center].transform);
		}
		ball_front1 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		Vector3 position_front1 = cam.transform.InverseTransformPoint (position_first [0]) + new Vector3 (0, 0.22f, 2f);
		ball_front1.transform.localScale = scale_balls * 0.6f;
		ball_front1.GetComponent<Renderer> ().material.color = Color.red;
		ball_front1.transform.position = cam.transform.TransformPoint (position_front1);
		ball_front2 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		Vector3 position_front2 = cam.transform.InverseTransformPoint (position_first [0]) + new Vector3 (0, 0.22f, 3f);
		ball_front2.transform.localScale = scale_balls * 0.6f;
		ball_front2.GetComponent<Renderer> ().material.color = Color.green;
		ball_front2.transform.position = cam.transform.TransformPoint (position_front2);
	}
	private void Initiate_Study1_Phase1()
	{
		Initiate_order ();
		Initiate_Positions ();
		//Initiate_Lines ();
		Initiate_Rect();
		if(text3ds_put != null)
			text3ds_put.Clear ();
		if(balls_put != null)
			balls_put.Clear ();
		if(positions_ball_put != null)
			positions_ball_put.Clear ();
		result_edge_length = new float[horizon_rotate_level*vertical_rotate_level];
		result_comfortable = new int[horizon_rotate_level*vertical_rotate_level];
		visible_result = new bool[horizon_rotate_level * vertical_rotate_level];
		for (int i = 0; i < result_comfortable.Length; i++) {
			result_comfortable [i] = -1;
		}
		num_ball_put = 30;
		num_now_put = -1;
		position_show = head.transform.TransformPoint (new Vector3(0,0,0.5f));
		position_text_offset = head.transform.TransformPoint (new Vector3(-0.02f,0.05f,0.47f));
		rotation_head_study1 = head.transform.rotation;
		ball_old = new GameObject[2];
		positions_ball_put = new List<Vector3>();
		balls_put = new List<GameObject>();
		text3ds_put = new List<GameObject> ();
		result_same = new List<bool> ();
		result_offset = new List<Vector3> ();
		bubbling = true;

		//CreateNewTarget ();


	}

	private void Initiate_order()
	{
		order_rotate = new int[horizon_rotate_level * vertical_rotate_level];
		for (int i = 0; i < order_rotate.Length; i++)
			order_rotate [i] = i;
		int n = order_rotate.Length;
		while (n > 2) {
			n--;
			int k = UnityEngine.Random.Range (1, n);
			int tmp = order_rotate [k];
			order_rotate [k] = order_rotate [n];
			order_rotate [n] = tmp;
		}

	}

	private Vector3 position_old_study2;
	private GameObject ball_colliding;
	private float[] result_edge_length;
	private int[] result_comfortable;
	private int[] order_rotate;
	public bool TriggerStudy1(GameObject colliding, GameObject mycontroller, Vector3 position_controller)
	{
		if (bubbling == true) {
			//Debug.Log ("try bubble");
			int nearest = -1;
			float distance_nearest = 10000;
			for (int i = 0; i < balls_grid.Length; i++) {
				float distance = (position_controller - balls_grid [i].transform.position).magnitude;
				if (distance < distance_nearest) {
					nearest = i;
					distance_nearest = distance;
				}
			}
			if (distance_nearest < threthold_bubble) {
				/*
				position_old_study2 = balls_grid[nearest].transform.position; 	
				balls_grid[nearest].transform.position = mycontroller.transform.position;
				balls_grid[nearest].transform.SetParent(mycontroller.transform);
				*/
				ball_colliding = balls_grid [nearest];
				ball_colliding.GetComponent<Renderer> ().material.color = Color.red;
			}
			return true;
		}
		return false;
		if (colliding != null) {
			
			position_old_study2 = colliding.transform.position; 	
			colliding.transform.position = mycontroller.transform.position;
			colliding.transform.SetParent(mycontroller.transform);
			ball_old[num_button_down-1] = colliding;

			return true;
		}
		return false;

		GameObject[] balls_state = balls_put.ToArray();
		bubbling = !put_get;
		if (bubbling == false && colliding != null) {
			/*
			colliding.transform.position = mycontroller.transform.position;
			colliding.transform.SetParent(mycontroller.transform);
			ball_old[num_button_down-1] = colliding;
			*/
			if (colliding.GetComponent<Renderer> ().material.color.a == 1) {
				
				Vector3 position_destroy = colliding.transform.position;
				Quaternion rotation_destroy = colliding.transform.rotation;
				GameObject ball_example = GameObject.Find ("Example");
				GameObject ball_new = GameObject.Instantiate (ball_example, position_destroy, rotation_destroy);
				ball_new.transform.localScale = scale_balls * 0.5f;
				int id = int.Parse (colliding.name);
				ball_new.name = id.ToString ();
				//colliding = ball_new;
				Destroy (colliding);
				balls_grid [id] = ball_new;
				return true;
			}
			num_now_put++;
			Debug.Log ("Change Color");
			Color color_new;
			if (num_now_put < color_ball_study1.Length)
				color_new = color_ball_study1 [num_now_put];
			else
				color_new = new Color (UnityEngine.Random.Range (0f, 1f), UnityEngine.Random.Range (0f, 1f), UnityEngine.Random.Range (0f, 1f),1);
			colliding.GetComponent<Renderer> ().material.color = color_new;
			return true;
		} else if (bubbling == true) {
			//Debug.Log ("try bubble");
			int nearest = -1;
			float distance_nearest = 10000;
			for (int i = 0; i < balls_state.Length; i++) {
				float distance = (position_controller - balls_state[i].transform.position).magnitude;
				if (distance < distance_nearest) {
					nearest = i;
					distance_nearest = distance;
				}
			}

			if (put_get == false && num_test_now < text_tests.Count-1) {
				result_same.Add (balls_put[nearest].name == text_tests [num_test_now].ToString());
				Debug.Log (result_same [num_test_now]);
				result_offset.Add (balls_state [nearest].transform.position - position_controller);
				//Debug.Log (result_offset [num_test_now]);
				num_test_now++;
				canvas.text = text_tests [num_test_now].ToString();
				return false;
			}

			//Debug.Log (distance_nearest);
			//Debug.Log (name_prefab_space [nearest]);
			if (distance_nearest > threthold_bubble)
				return false;
			//Debug.Log (num_button_down);
			//Debug.Log (ball_old.Length);

			ball_old[num_button_down-1] = balls_state [nearest];
			ball_old[num_button_down-1].transform.position = mycontroller.transform.position; // trigger colliding detection and set ball_old to colliding & objInHand
			ball_old[num_button_down-1].transform.SetParent(mycontroller.transform);

			return true;
		}
		else {
			ball_old[num_button_down-1] = null;
			return false;
		}
	}


	public void ReleaseBallStudy1(GameObject objInHand) // release the ball in different states
	{
		if (ball_colliding == null)
			return;
		ball_colliding.GetComponent<Renderer> ().material.color = Color.white;
		ball_colliding = null;
		return;
		ball_colliding.transform.position = position_old_study2;
		if (int.Parse (ball_colliding.name) == center)
			ball_colliding.transform.SetParent (null);
		else
			ball_colliding.transform.SetParent (balls_grid [center].transform);
		ball_colliding.transform.LookAt (position_zero);
		ball_colliding = null;
		return;
		if (num_now_put >= num_ball_put)
			return;


		if (num_now_put < num_ball_put-1 && objInHand.name == num_now_put.ToString()) {
			CreateNewTarget ();
		}

	}


	private float u_least;
	private float u_most;
	private float v_least;
	private float v_most;
	private int u_level;
	private int v_level;
	private GameObject[] balls_midu;
	private Vector3[] positions_midu;

	private void InitiateUV()
	{
		u_least = 0;
		u_most = 1;
		v_least = 0;
		v_most = 1;
		u_level = 20;
		v_level = 20;
	}

	private void VisualBalls_midu()
	{
		int amount_midu = u_level * v_level;
		balls_midu = new GameObject[amount_midu];
		for (int i = 0; i < u_level; i++) {
			for (int j = 0; j < v_level; j++) {
				
				//float u_now = u_least + (u_level - i) * (u_most - u_least) / u_level;
				//float v_now = v_least + (v_level - j) * (v_most - v_least) / v_level;
				float u_now = UnityEngine.Random.Range(0f,1f);
				float v_now = UnityEngine.Random.Range(0f,1f);
				float theta = 2 * Mathf.PI * u_now;
				float phi = Mathf.Acos (2 * v_now - 1);

				Vector3 position_now = new Vector3 (Mathf.Sin (theta) * Mathf.Sin (phi), Mathf.Cos (phi), Mathf.Cos (theta) * Mathf.Sin (phi)) * 0.3f;
				//Debug.Log (position_now.x.ToString() + " " + position_now.y.ToString() + " " + position_now.z.ToString());
				balls_midu [i * u_level + j] = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				balls_midu [i * u_level + j].transform.localScale = scale_balls * 0.3f;
				balls_midu [i * u_level + j].transform.position = head.transform.TransformPoint(position_now);
			}
		}
	}

	void Update () {
		switch(state){
		case m_state.Unstart:
			if (Input.GetKey (KeyCode.Space)) {
				if (++numid == nums.Length) {
					string[] cap = new string[pf_v.Length];
					for (int i = 0; i < pf_v.Length; i++)
						cap [i] = nums [i].ToString ();
					dataget.logData_2d ("density", pf_v, cap);
					return;
				}
				objNum = nums [numid];
				objects = new GameObject[objNum];
				id = 0;
				state = m_state.Putting;
				ShowaBall ();
			}
			if (Input.GetKey (KeyCode.Alpha1)) {
				Debug.Log ("1");
				dirid = -1;
				logmax_vs = new Vector3[5];
				state = m_state.LogMaxsize;
				logMaxSize ();
			}
			if (Input.GetKey (KeyCode.Alpha2)) {
				if (++vbs_id == vbs_num) {
					string[] cap = new string[vbs_num];
					for (int i = 0; i < vbs_num; i++)
						cap [i] = vbs_sizes [i].ToString ();
					dataget.logData_2d ("ball_size", vbs_v, cap, vbs_success);
					//vbs_id = -1;
					//return;
					vbs_id = 0;

				}

				ball_size = vbs_sizes [vbs_id];
				dirid = -1;
				state = m_state.VarBallSize;
				varBallSize ();
			}
			if (Input.GetKeyDown (KeyCode.Alpha3)) {
				Debug.Log ("Press 3");
				state = m_state.Study_one;
				//to do
				canvas.text = "";
				Initiate_Study1_Phase1 ();

			}

			if (Input.GetKeyDown (KeyCode.Alpha4)) {
				InitialBalls ();
				state = m_state.ShowBalls;
				canvas.text = "";
			}

			if (Input.GetKeyDown (KeyCode.Alpha5)) {
				Debug.Log ("Study1 Phase2");
				state = m_state.Study1_Phase2;
				Initiate_Study1_Phase2 ();

			}
			if (Input.GetKeyDown (KeyCode.Alpha6)) {//weapons around
				InitialBalls_angle ();
				state = m_state.ShowBalls;
				canvas.text = "";
			}
			if (Input.GetKeyDown (KeyCode.Alpha7)) {//calibration for shoulder position and arm length
				Debug.Log ("Press 7");
				state = m_state.AdjustRange_Angle;
				canvas.text = name_task [num_task];
				Destroy_Get_Ready ();

			}
			if (Input.GetKeyDown (KeyCode.Alpha8)) {//space
				Debug.Log ("Press 8");

				state = m_state.Space_acquire;
				InitialPositionSpace ();
				canvas.text = "";

			}
			if (Input.GetKeyDown (KeyCode.Alpha9)) {
				Initiate_FOV ();
				Initiate_Get_Ready ();
			}
			if (Input.GetKeyDown (KeyCode.S)) {
				Debug.Log ("visual");
				InitiateUV ();
				VisualBalls_midu ();
			}
			break;
		case m_state.Putting:
			if (putNext == true) {
				if (++id >= objNum) {
					putNext = false;
					state = m_state.Finding;
					id = 0;
					goal = GameObject.Instantiate (goalPrefab, prt.transform.position, rot, prt.transform);
					FindaBall ();
					return;
				}
				ShowaBall ();
			}
			break;
		case m_state.Finding:
			if (finded == true) {
				if (id >= objNum) {
					Destroy (goal);
					setText(5, "Congratulations!");
					state = m_state.Unstart;
					return;
				}
				FindaBall ();
			}
			break;
		case m_state.LogMaxsize:
			break;
		case m_state.VarBallSize:
			if (vbs_begin) {
				vbs_begin = false;
				vbs_finished = true;
				vbs_v [vbs_id][2*dirid] = vbs_ball.transform.position - head.transform.position;
			}
			break;
		case m_state.Study_one:
			//to do
			if (Input.GetKeyDown (KeyCode.Space)) {
				Vector3 position_v = cam.WorldToViewportPoint (balls_grid[center].transform.position);
				bool visible_new = false;
				if (position_v.x > 0 && position_v.x < 1 && position_v.y > 0 && position_v.y < 1 && position_v.z > 0 && position_v.z < 1 && position_v.x + position_v.y + position_v.z >= 1)
					visible_new = true;
				visible_result [order_rotate[num_rotate]] = visible_new;
				result_edge_length [order_rotate[num_rotate]] = length_grid;
				for (int i = 0; i < num_edge_grid; i++) {
					for (int j = 0; j < num_edge_grid; j++) {
						int index_now = i * num_edge_grid + j;
						balls_grid [index_now].transform.position = balls_grid [center].transform.position + (balls_grid [index_now].transform.position - balls_grid [center].transform.position) / length_grid * length_grid_old;
					}
				}
				length_grid = length_grid_old;
				num_rotate++;
				if (num_rotate < horizon_rotate_level * vertical_rotate_level) {
					balls_grid [center].transform.position = position_first [order_rotate[num_rotate]];
					balls_grid [center].transform.LookAt (position_zero);
					int index_horizon = order_rotate[num_rotate] % horizon_rotate_level;
					int index_vertical = order_rotate[num_rotate] / horizon_rotate_level;
					//canvas.text = "V: " + (vertical_angles [index_vertical] / Mathf.PI*180).ToString () + "\n" + "H: " + (horizon_angles [index_horizon] / Mathf.PI*180).ToString (); 
					canvas.text = "V: " + text_vertical[index_vertical] + "\n" + "H: " + text_horizon[index_horizon];
					Debug.Log (num_rotate);
				} else {
					canvas.text = "Done, thank you!";
					dataget.log_study2 ("study2", position_first, result_edge_length, result_comfortable, visible_result);
					//state = m_state.Unstart;
					return;
				}
			}

			if (Input.GetKeyDown (KeyCode.I)) {
				length_grid += 0.05f;
				for (int i = 0; i < num_edge_grid; i++) {
					for (int j = 0; j < num_edge_grid; j++) {
						balls_grid [i * num_edge_grid + j].transform.position = balls_grid [center].transform.position + (balls_grid [i * num_edge_grid + j].transform.position - balls_grid [center].transform.position) / (length_grid - 0.05f) * length_grid;

					}
				}
			}
			if (Input.GetKeyDown (KeyCode.D)) {
				length_grid -= 0.05f;
				for (int i = 0; i < num_edge_grid; i++) {
					for (int j = 0; j < num_edge_grid; j++) {
						balls_grid [i * num_edge_grid + j].transform.position = balls_grid [center].transform.position + (balls_grid [i * num_edge_grid + j].transform.position - balls_grid [center].transform.position) / (length_grid + 0.05f) * length_grid;

					}
				}
			}
			if (Input.GetKeyDown (KeyCode.Alpha1)) {
				result_comfortable [order_rotate[num_rotate]] = 1;
			}
			if (Input.GetKeyDown (KeyCode.Alpha2)) {
				result_comfortable [order_rotate[num_rotate]] = 2;
			}
			if (Input.GetKeyDown (KeyCode.Alpha3)) {
				result_comfortable [order_rotate[num_rotate]] = 3;
			}
			if (Input.GetKeyDown (KeyCode.Alpha4)) {
				result_comfortable [order_rotate[num_rotate]] = 4;
			}
			if (Input.GetKeyDown (KeyCode.Alpha5)) {
				result_comfortable [order_rotate[num_rotate]] = 5;
			}
			if (Input.GetKeyDown (KeyCode.R)) {
				if (balls_put.Count > 1) {
					Vector3[] position_tostore = new Vector3[balls_put.Count - 1];
					for (int i = 0; i < balls_put.Count - 1; i++)
						position_tostore [i] = head.transform.InverseTransformPoint (balls_put [i].transform.position);
					//dataget.log_study2 ("study2", position_tostore);
				}
				for (int i = 0; i < balls_put.Count; i++) {
					Destroy (balls_put [i]);
				}

				Initiate_Study1_Phase1 ();
				//canvas.text = text_instruction;
				//state = m_state.Unstart;

			}
			if (Input.GetKeyDown (KeyCode.G)) {
				put_get = false;
				text_tests = new List<int> ();
				for (int i = 0; i < balls_put.Count; i++)
					text_tests.Add (i);
				int n = text_tests.Count;
				while (n > 1) {
					n--;
					int k = UnityEngine.Random.Range (0, n + 1);
					int value = text_tests [k];
					text_tests [k] = text_tests [n];
					text_tests [n] = value;
				}
				text_tests.Add (-1);
				num_test_now = 0;
				canvas.text = text_tests [num_test_now].ToString ();
			}
			if (Input.GetKeyDown (KeyCode.A)) {
				put_get = true;
				Debug.Log (balls_put.Count);
				if (balls_put.Count == num_ball_put) {
					num_ball_put += 5;
					CreateNewTarget ();
				}
			}

			break;
		case m_state.ShowBalls://weapons around
			if (Input.GetKeyDown (KeyCode.Space)) {
				if (display_reference == false) {
					display_reference = true;
					for (int i = 0; i < num_ball; i++) {
						balls [i].transform.SetParent (head.transform);
					}
				} else {
					display_reference = false;
					for (int i = 0; i < num_ball; i++) {
						balls [i].transform.SetParent (null);
					}
				}
			}
			if (Input.GetKeyDown (KeyCode.B)) {
				Debug.Log ("bubble");
				bubbling = !bubbling;
			}
			if (Input.GetKeyDown (KeyCode.H)) {
				haptic_on = !haptic_on;
			}
			if (Input.GetKeyDown (KeyCode.R)) {
				Debug.Log ("Return");
				for (int i = 0; i < num_ball; i++) {
					Destroy (balls [i]);
				}
				balls = new GameObject[num_ball];
				display_reference = false;
				bubbling = true;
				//position_old = null;
				state = m_state.Unstart;
			}
			if (Input.GetKeyDown (KeyCode.A)) {
				Debug.Log ("Adjust Range");
				for (int i = 0; i < num_ball; i++) {
					Destroy (balls [i]);
				}
				balls = new GameObject[num_ball];
				display_reference = false;
				bubbling = true;
				//position_old = null;

				//diffrent adjust strategy
				//state = m_state.AdjustRange;
				//num_now = 0;
				//canvas.text = texts_limit [num_now];
				state = m_state.AdjustRange_Angle;
				num_task = 0;
				canvas.text = name_task [num_task];
			}
			break;
		case m_state.Study1_Phase2:
			if (timer_on == true) {
				timer_tick_count++;
				if (timer_tick_count >= timer_tick_amount) {
					timer_tick_count = 0;
					timer_tick_amount = -1;
					timer_on = false;
					ball_reset.GetComponent<Renderer> ().material.color = Color.magenta;
				}
			}
			if (Input.GetKeyDown (KeyCode.P)) { // redo previous trial
				if (num_now_one > 0) {
					appear_move_acquire = state_study1.appear;
					num_now_one--;
					ball_target.transform.position = positions_world_one [order_target [num_now_one]];
					canvas.text = num_now_one.ToString ();
				}
			}
			if (Input.GetKeyDown (KeyCode.R)) {
				Initiate_Study1_Phase2 ();
			}
			if (Input.GetKeyDown (KeyCode.W)) {
				reference_now = reference.world;
			}
			if (Input.GetKeyDown (KeyCode.D)) {
				reference_now = reference.display;
			}
			if (Input.GetKeyDown (KeyCode.Space)) {
				int index_theta = order_target [num_now_one] % num_theta_level;
				float value_theta = theta_least + (num_theta_level - index_theta) * (theta_most - theta_least) / num_theta_level;
				float theta_move = theta_move_least + (num_move_level - num_move) * (theta_move_most - theta_move_least) / num_move_level - value_theta ;
				Debug.Log (theta_move);
				dataget.log_angle (theta_move.ToString ());
			}
			//line.SetPosition (1, head.transform.position);
			if (ball_reference != null)
				ball_reference.transform.position = head.transform.position;
			break;
		case m_state.AdjustRange_Angle:
			if (Input.GetKeyDown (KeyCode.A) || Input.GetKeyDown (KeyCode.Alpha6)) {
				InitialBalls_angle ();
				num_task = 0;
				state = m_state.ShowBalls;
				canvas.text = "";
			}
			if (Input.GetKeyDown (KeyCode.Alpha5)) {
				distance_far = distance_longest;
				state = m_state.Study1_Phase2;
				Initiate_Study1_Phase2 ();
			}
			break;
		case m_state.Space_acquire:
			if (Input.GetKeyDown (KeyCode.D)) {
				reference_now = reference.display;
				InitialPositionSpace ();
			}
			if (Input.GetKeyDown (KeyCode.W)) {
				reference_now = reference.world;
				InitialPositionSpace ();
			}
			if (Input.GetKeyDown (KeyCode.B)) {
				reference_now = reference.body;
				InitialPositionSpace ();
			}
			switch (reference_now) {
			case reference.body:
				UpdateBody ();
				break;
			default:
				break;
			}
			if (ball_reference != null) {
				ball_reference.transform.position = head.transform.position - new Vector3(0.0f,0.001f,0.001f);
			}
			break;
		default:
			break;
		}
	}
}
