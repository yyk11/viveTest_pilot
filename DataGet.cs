using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEngine.UI;

public class DataGet : MonoBehaviour {

	public int total;
	public string dir = "C:/Users/PCG-527/Desktop/viveTest/data/";
	private string tmpdir;
	public FileStream file;
	public StreamWriter fstream;
	private string filename = "";

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
	
	// Update is called once per frame
	void Update () {
		/*if (Input.GetKey (KeyCode.Return)) {
			Text inputfile = GameObject.Find ("FileName").GetComponent<Text> ();
			filename = inputfile.text;
			Debug.Log (filename);
		}*/
	}
}
