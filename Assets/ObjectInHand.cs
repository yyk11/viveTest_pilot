using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInHand : MonoBehaviour
{

    private SteamVR_TrackedObject hand;
    public SteamVR_Controller.Device controller;
    public GameObject collidingObj;
    public GameObject objInHand;
    public control con;
    public bool putted;
    int tmp;

    void Start()
    {
        hand = GetComponent<SteamVR_TrackedObject>();
        if ((int)hand.index >= 0)
            controller = SteamVR_Controller.Input((int)hand.index);
        con = GameObject.Find("control").GetComponent<control>();
        putted = false;
    }

    private void SetCollidingObj(Collider col)
    {
        if (collidingObj || !col.GetComponent<Rigidbody>())
            return;
        collidingObj = col.gameObject;
    }

    public void OnTriggerEnter(Collider col)
    {
        if (objInHand)
            return;
        SetCollidingObj(col);
        if (con.haptic_on)
            controller.TriggerHapticPulse(500);
    }
    public void OnTriggerStay(Collider col)
    {
        if (objInHand)
            return;
        SetCollidingObj(col);
        if (con.haptic_on)
            controller.TriggerHapticPulse(500);
    }
    public void OnTriggerExit(Collider col) { collidingObj = null; }

    private void GrabObj()
    {
        if (objInHand)
            return;
        objInHand = collidingObj;
        collidingObj = null;
        FixedJoint joint = gameObject.AddComponent<FixedJoint>();
        joint.breakForce = joint.breakTorque = 20000;
        joint.connectedBody = objInHand.GetComponent<Rigidbody>();
        Debug.Log("grab " + objInHand.name);
    }

    public void ReleaseObj()
    {
        if (GetComponent<FixedJoint>())
        {
            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());
            putted = true;
        }
        Debug.Log("release ");
        objInHand = null;
    }

    // Update is called once per frame
    void Update()
    {
		//Debug.Log ((int)con.state);
        if (con.touch_and_see == true)
            con.VisualTouch(collidingObj, gameObject, gameObject.transform.position);
        if (controller != null)
        {
            switch ((int)con.state)
            {
                case 0:
                    break;
                case 1:
                    if (controller.GetHairTriggerDown() && collidingObj)
                        GrabObj();
                    if (controller.GetHairTriggerUp() && objInHand)
                        ReleaseObj();
                    if (controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad) && putted == true)
                    {
                        con.putNext = true;
                    }
                    break;
                case 2:
                    tmp = con.FindNearBall(GetComponent<Collider>().transform.position);
                    collidingObj = con.objects[tmp];
                    if (controller.GetHairTriggerDown() && collidingObj)
                    {
                        if (collidingObj != con.objects[con.id])
                        {
                            controller.TriggerHapticPulse();
                            con.setText(5, "Sorry\nit's the " + con.colNames[tmp] + " one!");
                            con.FoundaBall(gameObject.transform.position);
                            Debug.Log("destroy it");
                            ReleaseObj();
                            Destroy(con.objects[con.id++]);
                            con.finded = true;
                        }
                        else
                        {
                            con.FoundaBall(gameObject.transform.position);
                            GrabObj();
                        }
                        return;
                    }
                    if (controller.GetHairTriggerUp() && objInHand)
                        ReleaseObj();
                    break;
                case 3:
                    if (controller.GetHairTriggerDown())
                    {
                        con.logMaxSizeOnce(gameObject.transform.position);
                        /*if (!con.logmax_begin)
                            con.logMaxSize ();
                        else
                            con.logClicked (gameObject.transform.position);*/
                    }
                    break;
                case 4:
                    if (!con.vbs_finished)
                    {
                        if (controller.GetHairTriggerDown() && collidingObj)
                            GrabObj();
                        if (controller.GetHairTriggerUp() && objInHand)
                            ReleaseObj();
                        if (controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
                            con.vbs_begin = true;
                    }
                    else
                    {
                        if (controller.GetHairTriggerDown())
                        {
                            Debug.Log("down");
                            if (collidingObj)
                                con.clickaBall(gameObject.transform.position, true);
                            else
                                con.clickaBall(gameObject.transform.position, false);
                        }
                    }
                    break;
                case 7: // showballs 
                    if (controller.GetHairTriggerDown())
                    {
                        //con.AcquireBall (collidingObj, gameObject.transform.position);
                        bool result = con.TriggerGet(collidingObj, gameObject, gameObject.transform.position);
                        if (result)
                        {
                            objInHand = con.ball_old[con.num_button_down - 1];
                        }
                        else
                        {
                            objInHand = null;
                        }
                    }
                    if (controller.GetHairTriggerUp() && objInHand)
                        con.ReleaseBall(objInHand);
                    if (controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
                        con.CalibrationReference();
                    break;
                case 8: // study 1 phase 2
                    if (controller.GetHairTriggerDown())
                    {
                        con.TriggerStudy1Phase2(collidingObj, gameObject, gameObject.transform.position);
                    }
                    break;
                case 9: // adjust range angle
                    if (controller.GetHairTriggerDown())
                    {
                        con.TriggerAngle(gameObject.transform.position);
                    }
                    break;
                case 6: // study1 phase2
                    if (controller.GetHairTriggerDown())
                    {
                        //con.AcquireBall (collidingObj, gameObject.transform.position);
                        con.num_button_down += 1;
                        Debug.Log(con.num_button_down);
                        bool result = con.TriggerStudy1(collidingObj, gameObject, gameObject.transform.position);
                        if (result)
                        {
                            objInHand = con.ball_old[con.num_button_down - 1];
                        }
                        else
                        {
                            objInHand = null;
                        }
                    }
                    if (controller.GetHairTriggerUp())
                    {
                        con.ReleaseBallStudy1(objInHand);
                        if (con.num_button_down > 0)
                            con.num_button_down -= 1;
                        return;
                        if (objInHand)
                            con.ReleaseBallStudy1(objInHand);
                        objInHand = null;

                        Debug.Log(con.num_button_down);
                    }
                    if (con.local_http == true)
                    {
                        con.position_body = gameObject.transform.position;
                        con.rotation_body = gameObject.transform.rotation;
                        //Debug.Log (gameObject.transform.eulerAngles);
                    }
                    //if (controller.GetPressDown (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
                    //	con.CalibrationReference ();
                    break;
                case 10: // shelf interaction(space)
                    if (controller.GetHairTriggerDown())
                    {
                        //con.AcquireBall (collidingObj, gameObject.transform.position);
                        con.num_button_down += 1;
                        Debug.Log(con.num_button_down);
                        bool result = con.TriggerSpace(collidingObj, gameObject, gameObject.transform.position);
                        if (result)
                        {
                            objInHand = con.ball_old[con.num_button_down - 1];
                        }
                        else
                        {
                            objInHand = null;
                        }
                    }
                    if (controller.GetHairTriggerUp())
                    {
                        if (objInHand)
                            con.ReleaseBallSpace(objInHand);
                        objInHand = null;
                        if (con.num_button_down > 0)
                            con.num_button_down -= 1;
                        Debug.Log(con.num_button_down);
                    }
                    if (con.local_http == true)
                    {
                        con.position_body = gameObject.transform.position;
                        con.rotation_body = gameObject.transform.rotation;
                        //Debug.Log (gameObject.transform.eulerAngles);
                    }
                    //if (controller.GetPressDown (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
                    //	con.CalibrationReference ();
                    break;
                case 13: // study3
                    if (controller.GetHairTriggerDown())
                    {
                        con.TriggerStudy3(collidingObj, gameObject, gameObject.transform.position);
                    }
                    if (controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
                    {
                        con.CheckSecondTask();
                    }
                    break;
                case 14: // study3 pilot
                    if (controller.GetHairTriggerDown())
                    {
						objInHand = collidingObj;
                        con.TriggerPilot_Stduy3(collidingObj, gameObject, gameObject.transform.position);
                    }
                    if (controller.GetHairTriggerUp())
                    {
						Debug.Log ("hairup_pilot");
                        if (objInHand)
                            con.ReleasePilot_Study3(objInHand);
					objInHand = null;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
