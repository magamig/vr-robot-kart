using PVR;
using PVR.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PVRControllerManager : MonoBehaviour {

    public GameObject left, right;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (PVRSession.instance.ready)
        {
            if (left != null)
            {
                if (PVRSession.instance.isDeviceConnected(pvrTrackedDeviceType.pvrTrackedDevice_LeftController))
                {
                    pvrPoseStatef poseState = PVRSession.instance.handsPoseState[0];
                    if (poseState.StatusFlags != 0)
                    {
                        left.SetActive(true);
                    }
                    else
                    {
                        left.SetActive(false);
                    }
                }
                else
                {
                    left.SetActive(false);
                }
                
            }
            if (right != null)
            {
                if (PVRSession.instance.isDeviceConnected(pvrTrackedDeviceType.pvrTrackedDevice_RightController))
                {
                    pvrPoseStatef poseState = PVRSession.instance.handsPoseState[1];
                    if (poseState.StatusFlags != 0)
                    {
                        right.SetActive(true);
                    }
                    else
                    {
                        right.SetActive(false);
                    }
                }
                else
                {
                    right.SetActive(false);
                }
            }
        }
        else
        {
            if (left != null)
            {
                left.SetActive(false);
            }
            if (right != null)
            {
                right.SetActive(false);
            }
        }
    }
}
