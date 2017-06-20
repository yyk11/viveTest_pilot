using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour {

	public int speed = 3;
	public GameObject collidingObj;
	public GameObject objInHand;
	// Use this for initialization

	void Start () {

	}	

	private void SetCollidingObj(Collider col){
		if (collidingObj || !col.GetComponent<Rigidbody> ())
			return;
		collidingObj = col.gameObject;
	}

	public void OnTriggerEnter(Collider col){
		SetCollidingObj (col);
	}
	public void OnTriggerStay(Collider col){
		SetCollidingObj (col);
	}
	public void OnTriggerExit(Collider col){
		collidingObj = null;
	}

	private void GrabObj(){
		objInHand = collidingObj;
		collidingObj = null;
		FixedJoint joint = gameObject.AddComponent<FixedJoint> ();
		joint.breakForce = joint.breakTorque = 20000;
		joint.connectedBody = objInHand.GetComponent<Rigidbody> ();
		Debug.Log ("grab " + objInHand.name);
	}

	private void ReleaseObj(){
		if (GetComponent<FixedJoint> ()) {
			GetComponent<FixedJoint> ().connectedBody = null;
			Destroy (GetComponent<FixedJoint> ());
		}
		Debug.Log ("release " + objInHand.name);
		objInHand = null;
	}

	// Update is called once per frame
	void Update () {
		transform.Translate (speed * Time.deltaTime * Input.GetAxis ("Mouse X"), 0, speed * Time.deltaTime * Input.GetAxis ("Mouse Y"));
		if (Input.GetMouseButtonDown(0) && collidingObj)
			GrabObj ();
		if (Input.GetMouseButtonUp(0) && objInHand)
			ReleaseObj ();
	}
}
