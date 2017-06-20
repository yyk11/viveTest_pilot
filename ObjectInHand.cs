using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInHand : MonoBehaviour {

	private SteamVR_TrackedObject hand;
	private SteamVR_Controller.Device controller;
	public GameObject collidingObj;
	public GameObject objInHand;
	public control con;
	public bool putted;
	int tmp;

	void Start () {
		hand = GetComponent<SteamVR_TrackedObject> ();
		if ((int)hand.index >= 0)
			controller = SteamVR_Controller.Input ((int)hand.index);
		con = GameObject.Find ("control").GetComponent<control>();
		putted = false;
	}

	private void SetCollidingObj(Collider col){
		if (collidingObj || !col.GetComponent<Rigidbody> ())
			return;
		collidingObj = col.gameObject;
	}

	public void OnTriggerEnter(Collider col){ SetCollidingObj (col); }
	public void OnTriggerStay(Collider col){ SetCollidingObj (col); }
	public void OnTriggerExit(Collider col){ collidingObj = null; }

	private void GrabObj(){
		if (objInHand)
			return;
		objInHand = collidingObj;
		collidingObj = null;
		FixedJoint joint = gameObject.AddComponent<FixedJoint> ();
		joint.breakForce = joint.breakTorque = 20000;
		joint.connectedBody = objInHand.GetComponent<Rigidbody> ();
		Debug.Log ("grab " + objInHand.name);
	}

	public void ReleaseObj(){
		if (GetComponent<FixedJoint> ()) {
			GetComponent<FixedJoint> ().connectedBody = null;
			Destroy (GetComponent<FixedJoint> ());
			putted = true;
		}
		Debug.Log ("release ");
		objInHand = null;
	}
	
	// Update is called once per frame
	void Update () {
		if (controller != null) {
			switch((int)con.state){
			case 0:
				break;
			case 1:
				if (controller.GetHairTriggerDown () && collidingObj)
					GrabObj ();
				if (controller.GetHairTriggerUp () && objInHand)
					ReleaseObj ();
				if (controller.GetPressDown (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad) && putted==true) {
					con.putNext = true;
				}
				break;
			case 2:
				tmp = con.FindNearBall (GetComponent<Collider> ().transform.position);
				collidingObj = con.objects[tmp];
				if (controller.GetHairTriggerDown () && collidingObj){
					if (collidingObj != con.objects [con.id]) {
						controller.TriggerHapticPulse ();
						con.setText (5, "Sorry\nit's the " + con.colNames [tmp] + " one!");
						con.FoundaBall (gameObject.transform.position);
						Debug.Log ("destroy it");
						ReleaseObj ();
						Destroy (con.objects [con.id++]);
						con.finded = true;
					} else {
						con.FoundaBall (gameObject.transform.position);
						GrabObj ();
					}
					return;
				}
				if (controller.GetHairTriggerUp () && objInHand)
					ReleaseObj ();				
				break;
			case 3:
				if (controller.GetHairTriggerDown ()) {
					con.logMaxSizeOnce (gameObject.transform.position);
					/*if (!con.logmax_begin)
						con.logMaxSize ();
					else
						con.logClicked (gameObject.transform.position);*/
				}
				break;
			case 4:
				if (!con.vbs_finished) {
					if (controller.GetHairTriggerDown () && collidingObj)
						GrabObj ();
					if (controller.GetHairTriggerUp () && objInHand)
						ReleaseObj ();
					if (controller.GetPressDown (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
						con.vbs_begin = true;
				} else {
					if (controller.GetHairTriggerDown ()) {
						Debug.Log ("down");
						if (collidingObj)
							con.clickaBall (gameObject.transform.position, true);
						else
							con.clickaBall (gameObject.transform.position, false);
					}
				}
				break;
			default:
				break;
			}
		}
	}
}
