using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEngine.UI;

public class DataGet : MonoBehaviour {

	public int total;
	public string dir = "C:/Users/PCG-527/Desktop/viveTest_pilot/data/";
	private string tmpdir;
	public FileStream file;
	public StreamWriter fstream;
	public FileStream file_angle;
	public StreamWriter fstream_angle; 

	// Use this for initialization
	void Start () {
/*		file = new FileStream("C:/Users/PCG-527/Desktop/test.txt", FileMode.Create);
		fstream = new StreamWriter(file);
		fstream.Write ("come on" + "\r\n" + "go on");
		fstream.Close ();*/
	}

	public void logData_1d (string foldname, Vector3[] data, int num) {
		
		Debug.Log ("log data as a 1D-array");
		tmpdir = dir + foldname + '/';
		total = int.Parse(System.IO.File.ReadAllText(tmpdir+"total.txt")) + 1;
		System.IO.File.WriteAllText(tmpdir+"total.txt", total.ToString());

		file = new FileStream(tmpdir+total.ToString()+".txt", FileMode.Append);
		fstream = new StreamWriter(file);

		for(int i = 0; i < num; i++)
			fstream.Write (data[i].x.ToString () + " " + data[i].y.ToString () + " " + data[i].z.ToString () + "\r\n");

		fstream.Close ();
	}


	// add succeed or not
	public void logData_2d(string foldname, Vector3[][] data, string[] caption, bool[][] success){

		Debug.Log ("log data as a 2D-array");
		tmpdir = dir + foldname + '/';
		total = int.Parse(System.IO.File.ReadAllText(tmpdir+"total.txt")) + 1;
		System.IO.File.WriteAllText(tmpdir+"total.txt", total.ToString());

		file = new FileStream(tmpdir+total.ToString()+".txt", FileMode.Append);
		fstream = new StreamWriter(file);

		for (int i = 0; i < data.Length; i++) {
			fstream.Write ((caption.Length > 0 ? caption [i] : i.ToString ()) + ":\r\n");
			for(int j = 0; j < data[i].Length; j++)
				fstream.Write (data [i][j].x.ToString () + " " + data [i][j].y.ToString () + " " + data [i][j].z.ToString () + "\r\n");
			for(int j = 0; j < success[i].Length;j++)
				fstream.Write(success[i][j].ToString() + " ");
			fstream.Write ("\r\n");
		}

		fstream.Close ();

	}

	public void logData_2d(string foldname, Vector3[][] data, string[] caption){
		
		Debug.Log ("log data as a 2D-array");
		tmpdir = dir + foldname + '/';
		total = int.Parse(System.IO.File.ReadAllText(tmpdir+"total.txt")) + 1;
		System.IO.File.WriteAllText(tmpdir+"total.txt", total.ToString());

		file = new FileStream(tmpdir+total.ToString()+".txt", FileMode.Append);
		fstream = new StreamWriter(file);

		for (int i = 0; i < data.Length; i++) {
			fstream.Write ((caption.Length > 0 ? caption [i] : i.ToString ()) + ":\r\n");
			for(int j = 0; j < data[i].Length; j++)
				fstream.Write (data [i][j].x.ToString () + " " + data [i][j].y.ToString () + " " + data [i][j].z.ToString () + "\r\n");
			fstream.Write ("\r\n");
		}
		fstream.Close ();

	}

	public void logData_3d(string foldname, Vector3[] data, string[] caption, int ver, int hor, int dis)
	{
		Debug.Log ("log data as 3D-array");
		tmpdir = dir + foldname + '/';
		total = int.Parse(System.IO.File.ReadAllText(tmpdir+"total.txt")) + 1;
		System.IO.File.WriteAllText(tmpdir+"total.txt", total.ToString());

		file = new FileStream(tmpdir+total.ToString()+".txt", FileMode.Append);
		fstream = new StreamWriter(file);

		for (int i = 0; i < dis; i++) {
			for (int j = 0; j < 　hor; j++) {
				fstream.Write (caption[i * hor + j] + ":\r\n");
				for (int k = 0; k < ver; k++) {
					fstream.Write(data[i*hor*ver + j * ver + k].x.ToString() + " " + data[i*hor*ver + j * ver +k].y.ToString() + " " + data[i*hor*ver + j * ver +k].z.ToString() + "\r\n");
				}
						fstream.Write("\r\n");
			}
		}
		fstream.Close();

	}

	public void logData_parameter(string foldname, Vector3[] parameters)
	{
		Debug.Log ("log parameters");
		tmpdir = dir + foldname + '/';
		total = int.Parse (System.IO.File.ReadAllText (tmpdir + "total.txt")) + 1;
		System.IO.File.WriteAllText (tmpdir + "total.txt", total.ToString ());

		file = new FileStream (tmpdir + total.ToString () + ".txt", FileMode.Append);
		fstream = new StreamWriter (file);
		for (int i = 0; i < parameters.Length; i++) {
			fstream.Write (parameters [i].x + " " + parameters [i].y + " " + parameters [i].z + " " + "\r\n");
		}
		fstream.Close ();
	}

	public void logData_space(string foldname, Vector3[] positions)
	{
		Debug.Log ("log space positions");
		tmpdir = dir + foldname + '/';
		total = int.Parse (System.IO.File.ReadAllText (tmpdir + "total.txt")) + 1;
		System.IO.File.WriteAllText (tmpdir + "total.txt", total.ToString ());

		file = new FileStream (tmpdir + total.ToString () + ".txt", FileMode.Append);
		fstream = new StreamWriter (file);
		for (int i = 0; i < positions.Length; i++) {
			fstream.Write (positions [i].x + " " + positions [i].y + " " + positions [i].z + " " + "\r\n");
		}
		fstream.Close ();
	}


	public void logData_Study1_Phase2(string foldname, Vector3[] positions_target, Vector3[] positions_result, bool[] visible_result, Vector3[] positions_body, Quaternion[] rotations_body, Vector3[] positions_head, Quaternion[] rotations_head, int[] orders_move)
	{
		Debug.Log ("log study1 phase2 results");
		tmpdir = dir + foldname + '/';
		total = int.Parse (System.IO.File.ReadAllText (tmpdir + "total.txt")) + 1;
		System.IO.File.WriteAllText (tmpdir + "total.txt", total.ToString ());

		file = new FileStream (tmpdir + total.ToString () + ".txt", FileMode.Append);
		fstream = new StreamWriter (file);
		fstream.Write ("target positions:\r\n");
		for (int i = 0; i < positions_target.Length; i++) {
			fstream.Write (positions_target [i].x + " " + positions_target [i].y + " " + positions_target [i].z + " " + "\r\n");
		}
		fstream.Write ("result positions:\r\n");
		for (int i = 0; i < positions_result.Length; i++) {
			fstream.Write (positions_result [i].x + " " + positions_result [i].y + " " + positions_result [i].z + " " + "\r\n");
		}
		fstream.Write ("result visibles:\r\n");
		for (int i = 0; i < positions_result.Length; i++) {
			fstream.Write (visible_result[i] + "\r\n");
		}
		fstream.Write ("body positions:\r\n");
		for (int i = 0; i < positions_result.Length; i++) {
			fstream.Write (positions_body [i].x + " " + positions_body [i].y + " " + positions_body [i].z + " " + "\r\n");
		}
		fstream.Write ("body rotations:\r\n");
		for (int i = 0; i < positions_result.Length; i++) {
			fstream.Write (rotations_body [i].x + " " + rotations_body [i].y + " " + rotations_body [i].z + " " + rotations_body[i].w + " \r\n");
		}
		fstream.Write ("head positions:\r\n");
		for (int i = 0; i < positions_result.Length; i++) {
			fstream.Write (positions_head [i].x + " " + positions_head [i].y + " " + positions_head [i].z + " " + "\r\n");
		}
		fstream.Write ("head rotations:\r\n");
		for (int i = 0; i < positions_result.Length; i++) {
			fstream.Write (rotations_head [i].x + " " + rotations_head [i].y + " " + rotations_head [i].z + " " + rotations_head[i].w + " \r\n");
		}
		fstream.Write ("move orders:\r\n");
		for (int i = 0; i < positions_result.Length; i++) {
			fstream.Write (orders_move[i].ToString() + " \r\n");
		}
		fstream.Close ();
	}
	public void initiate_log(string foldname, string foldname_angle)
	{
		Debug.Log ("initiate log");
		tmpdir = dir + foldname + '/';
		total = int.Parse (System.IO.File.ReadAllText (tmpdir + "total.txt")) + 1;
		System.IO.File.WriteAllText (tmpdir + "total.txt", total.ToString ());
		if (fstream != null)
			fstream.Close ();
		file = new FileStream (tmpdir + total.ToString () + ".txt", FileMode.Append);
		fstream = new StreamWriter (file);
		tmpdir = dir + foldname_angle + '/';
		total = int.Parse (System.IO.File.ReadAllText (tmpdir + "total.txt")) + 1;
		System.IO.File.WriteAllText (tmpdir + "total.txt", total.ToString ());
		if (fstream_angle != null)
			fstream_angle.Close ();
		file_angle = new FileStream (tmpdir + total.ToString () + ".txt", FileMode.Append);
		fstream_angle = new StreamWriter (file_angle);

	}
	public void log_time(string index, string state, string time, string id)
	{
		fstream.Write (index + " " + state + " " + time + " " + id + "\n");
		fstream.Flush ();
	}
	public void log_angle(string angle){
		fstream_angle.WriteLine (angle);
		fstream_angle.Flush ();
	}
	public void log_study2(string foldname, Vector3[] centers, float[] edge_length, int[] comfortable, bool[] visible)
	{
		Debug.Log ("log study2");
		tmpdir = dir + foldname + '/';
		total = int.Parse (System.IO.File.ReadAllText (tmpdir + "total.txt")) + 1;
		System.IO.File.WriteAllText (tmpdir + "total.txt", total.ToString ());
		if (fstream != null)
			fstream.Close ();
		file = new FileStream (tmpdir + total.ToString () + ".txt", FileMode.Append);
		fstream = new StreamWriter (file);
		for (int i = 0; i < edge_length.Length; i++) {
			fstream.WriteLine (edge_length [i].ToString () + " " + comfortable [i].ToString () + " " + centers[i].ToString() + " " + visible[i].ToString());
		}
		fstream.Close ();
	}
	// Update is called once per frame
	void Update () {
		/*if (Input.GetKey (KeyCode.Return)) {
			Text inputfile = GameObject.Find ("FileName").GetComponent<Text> ();
			filename = inputfile.text;
			Debug.Log (filename);
		}*/
	}
}
