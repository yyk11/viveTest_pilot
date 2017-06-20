using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

public class httpReader : MonoBehaviour {

	private HttpListener httpListener;
	public Vector3 position_body = new Vector3();
	public Quaternion rotation_body = new Quaternion();
	public string query;
	// Use this for initialization
	void Start () {
		
	}

	public void StartServer()
	{
		httpListener = new HttpListener();

		httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
		httpListener.Prefixes.Add("http://127.0.0.1:8000/");

		httpListener.Start();
		new Thread(new ThreadStart(delegate
			{
				while (true)
				{
					HttpListenerContext httpListenerContext = httpListener.GetContext();
					query = httpListenerContext.Request.Url.ToString();
					string[] numbers = query.Split('/')[3].Split(' ');
					Debug.Log(query);
					httpListenerContext.Response.StatusCode = 200;
					//position_body = new Vector3(float.Parse(numbers[0]), float.Parse(numbers[1]),float.Parse(numbers[2]));
					//rotation_body = new Quaternion(float.Parse(numbers[3]),float.Parse(numbers[4]),float.Parse(numbers[5]),float.Parse(numbers[6]));
					using (StreamWriter writer = new StreamWriter(httpListenerContext.Response.OutputStream))
					{
						//need to be changed
						writer.WriteLine("0");

					}
				}
			})).Start();
	}


	// Update is called once per frame
	void Update () {
		
	}
}
