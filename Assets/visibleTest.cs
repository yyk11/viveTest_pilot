using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class visibleTest : MonoBehaviour {
	public bool visible = false;
	// Use this for initialization
	void Start () {
		
	}
	void OnBecameVisible()
	{
		Debug.Log ("v");
		visible = true;
	}

	void OnBecameInvisible()
	{
		Debug.Log ("i");
		visible = false;
	}

	// Update is called once per frame
	void Update () {

	}
}
