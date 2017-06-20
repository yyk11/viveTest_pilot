using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class control : MonoBehaviour {
	public string[] colNames;
	public string[] dirNames;
	public int dirid;
	public Color[] colors;
	public Color color;
	public enum m_state {Unstart, Putting, Finding, LogMaxsize, VarBallSize, VarDensity};
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
	public string text;
	public DataGet dataget;

	public float ball_size = 1.0f;
	private Vector3 ball_scale = new Vector3(0.3f, 0.3f, 0.3f);

	void Start () {
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
		canvas.text = "Press '1' to start MaxSize Section, Press '2' to start BallSize Section";
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
					dataget.logData_2d ("ball_size", vbs_v, cap);
					//vbs_id = -1;
					//return;
					vbs_id = 0;

				}

				ball_size = vbs_sizes[vbs_id];
				dirid = -1;
				state = m_state.VarBallSize;
				varBallSize ();
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
		default:
			break;
		}
	}
}
