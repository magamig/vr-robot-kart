using UnityEngine;
using System.Collections;
using PVR.Unity;
using System;

public class RecenterTest : MonoBehaviour, PVR.Unity.IPVRInputEventTarget {

    public void OnAxisChange(PVRAxisEventData data)
    {
    }

    public void OnButtonPress(PVRButtonEventData data)
    {
        if (data.hand == 0 && data.btn == PVR.pvrButton.pvrButton_Trigger)
        {
            PVR.Unity.PVRSession.instance.recenterTrackingOrigin();
        }
    }

    public void OnButtonRelease(PVRButtonEventData data)
    {

    }

    public void OnButtonTouch(PVRButtonEventData data)
    {

    }

    public void OnButtonUntouch(PVRButtonEventData data)
    {

    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
