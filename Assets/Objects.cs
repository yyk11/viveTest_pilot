using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objects : MonoBehaviour {

	// Use this for initialization
	private GameObject head;
	public Vector3 pos = new Vector3 (-0.9f, 2.35f, -3.3f);
	public Vector3 repos = new Vector3 (0f, 0f, 0.46f);
	void Start () {
		if(head = GameObject.Find ("Camera (eye)"))
			transform.position = head.transform.position + repos;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.F)) {
			Debug.Log ("fix");
			if (head) {
				transform.position = head.transform.position + repos;
			}
			else
				head = GameObject.Find ("Camera (eye)");
		}
		if (Input.GetKey (KeyCode.G)) {
			Debug.Log ("here");
			repos = transform.position - head.transform.position;
		}
		if (Input.GetKey (KeyCode.H))
			transform.position = pos;
	}
}
