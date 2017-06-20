using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {

	public control con;
	private GameObject lh;
	private GameObject rh;
	public ObjectInHand oih_l;
	public ObjectInHand oih_r;

	void Start(){
		con = GameObject.Find ("control").GetComponent<control>();
		lh = GameObject.Find ("Controller (left)");
		rh = GameObject.Find ("Controller (right)");
		if (lh)
			oih_l = lh.GetComponent<ObjectInHand> ();
		if (rh)
			oih_r = rh.GetComponent<ObjectInHand> ();
	}

	void OnTriggerEnter(Collider col){
		if (col.gameObject == con.objects [con.id]) {
			con.id++;
			con.finded = true;
			Debug.Log ("goal destroy it");
			if (oih_l && oih_l.objInHand)
				oih_l.ReleaseObj ();
			if (oih_r && oih_r.objInHand)
				oih_r.ReleaseObj ();
			Destroy (col.gameObject);
		}
	}

	void Update () {
		
	}
}
