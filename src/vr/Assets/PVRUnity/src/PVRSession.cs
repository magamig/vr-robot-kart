using UnityEngine;
using System.Collections;
using System;

namespace PVR
{
    namespace Unity
    {
        public class PVRSession : MonoBehaviour
        {
            public enum TrackingOrig
            {
                eye = 0,
                floor,
            }
            public TrackingOrig trackingOrig;

            private static PVRSession _instance;
            private static PVRRenderLib _renderlib;
            private static bool _dllFixed = false;
            private PVRClient _clientObject;
            private bool _ready = false;
            private string _errMsg;
            private IEnumerator _endOfFrameCoroutine;
            private pvrHmdStatus _HmdStatus = new pvrHmdStatus();
            private pvrTrackingState _PoseState = new pvrTrackingState();
            private pvrPosef[] _eyePoses = new pvrPosef[2];
            private pvrSizei _renderTargetSize = new pvrSizei();
            private pvrMatrix4f[] _projMatrix = new pvrMatrix4f[2];
            private pvrEyeRenderInfo[] _renderInfo = new pvrEyeRenderInfo[2];
            private pvrPosef[] _hmdToEyePose = new pvrPosef[2];
            private pvrInputState _InputState = new pvrInputState();
            public static PVRSession instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = GameObject.FindObjectOfType<PVRSession>();
                        if (_instance == null)
                        {
                            Debug.LogError("[PVR-Unity] Error: You need the PVRSession prefab in your game!!");
                        }
                        else
                        {
                            DontDestroyOnLoad(_instance.gameObject);
                        }
                    }
                    return _instance;
                }
            }

            public bool ready
            {
                get {
                    return _ready;
                }
            }

            public string errorMsg
            {
                get
                {
                    return _errMsg;
                }
            }

            public PVRClient client
            {
                get
                {
                    return _clientObject;
                }
            }

            public pvrPoseStatef headPoseState
            {
                get
                {
                    return _PoseState.HeadPose;
                }
            }
            public pvrPoseStatef[] handsPoseState
            {
                get
                {
                    return _PoseState.HandPoses;
                }
            }

            public pvrPosef[] eyePose
            {
                get
                {
                    return _eyePoses;
                }
            }

            public pvrSizei renderTargetSize
            {
                get
                {
                    return _renderTargetSize;
                }
            }

            public pvrPosef[] hmdToEyePose
            {
                get
                {
                    return _hmdToEyePose;
                }
            }
            
            public pvrInputState inputState
            {
                get
                {
                    return _InputState;
                }
            }

            public pvrTrackingOrigin trackingOrigin
            {
                get
                {
                    pvrTrackingOrigin origin = pvrTrackingOrigin.pvrTrackingOrigin_EyeLevel;
                    if (ready)
                    {
                        _clientObject.getTrackingOriginType(ref origin);
                    }
                    return origin;
                }
                set
                {
                    if (ready)
                    {
                        _clientObject.setTrackingOriginType(value);
                    }
                }
            }
            public void recenterTrackingOrigin()
            {
                if (ready)
                {
                    _clientObject.recenterTrackingOrigin();
                }
            }

            public void triggerHapticPulse(int hand, float intensity)
            {
                if (ready)
                {
                    pvrTrackedDeviceType device = ((hand == 0) ? pvrTrackedDeviceType.pvrTrackedDevice_LeftController : pvrTrackedDeviceType.pvrTrackedDevice_RightController);
                    _clientObject.triggerHapticPulse(device, intensity);
                }
            }

            public pvrMatrix4f GetEyeProjectionMatrix(pvrEyeType eye)
            {
                return _projMatrix[eye == pvrEyeType.pvrEye_Left ? 0 : 1];
            }

            public pvrFovPort GetFovPort(pvrEyeType eye)
            {
                return _renderInfo[eye == pvrEyeType.pvrEye_Left ? 0 : 1].Fov;
            }

            public static void RegisterColorBuffer(IntPtr colorBuffer)
            {
                if (_renderlib != null)
                {
                    _renderlib.RegisterColorBuffer(colorBuffer);
                }
            }

            public static void UnregisterColorBuffer(IntPtr colorBuffer)
            {
                if (_renderlib != null)
                {
                    _renderlib.UnregisterColorBuffer(colorBuffer);
                }
            }
            public void SubmitEyeLayer(int layer_uid, pvrEyeType eye, IntPtr colorBuffer, bool headLocked)
            {
                if (_renderlib != null)
                {
                    _renderlib.SubmitEyeLayer(layer_uid, (int)eye, colorBuffer, ref eyePose[(int)eye], headPoseState.TimeInSeconds, headLocked);
                }
            }

            public bool IsButtonInPress(int hand, PVR.pvrButton btn)
            {
                if ((_InputState.HandButtons[hand] & (uint)btn) != 0)
                {
                    return true;
                }
                return false;
            }

            public bool IsButtonInTouch(int hand, PVR.pvrButton btn)
            {
                if ((_InputState.HandTouches[hand] & (uint)btn) != 0)
                {
                    return true;
                }
                return false;
            }

            public pvrVector2f GetButtonAxis(int hand, PVR.pvrButton btn)
            {
                pvrVector2f axis = new pvrVector2f();
                switch (btn)
                {
                    case pvrButton.pvrButton_Grip:
                        axis.x = _InputState.Grip[hand];
                        break;
                    case pvrButton.pvrButton_Trigger:
                        axis.x = _InputState.Trigger[hand];
                        break;
                    case pvrButton.pvrButton_JoyStick:
                        axis.x = _InputState.JoyStick[hand].x;
                        axis.y = _InputState.JoyStick[hand].y;
                        break;
                    case pvrButton.pvrButton_TouchPad:
                        axis.x = _InputState.TouchPad[hand].x;
                        axis.y = _InputState.TouchPad[hand].y;
                        break;
                    default:
                        axis.x = IsButtonInPress(hand, btn) ? 1.0f : 0.0f;
                        break;
                }
                return axis;
            }

            public bool isDeviceConnected(pvrTrackedDeviceType device)
            {
                uint devices = 0;
                _clientObject.getConnectedDevices(ref devices);
                return ((devices & (uint)device) != 0);
            }

            void Awake()
            {
                //if an instance of this singleton does not exist, set the instance to this object and make it persist
                if (_instance == null)
                {
                    _instance = this;
                    DontDestroyOnLoad(this);
                }
                else
                {
                    //if an instance of this singleton already exists, destroy this one
                    if (_instance != this)
                    {
                        Destroy(this.gameObject);
                    }
                }
            }

            // Use this for initialization
            void Start()
            {
                if (!_dllFixed)
                {
                    DLLSearchPathFixer.fix();
                    _dllFixed = true;
                }

                if (_clientObject == null)
                {
                    Debug.Log("[PVR-Unity] Starting");
                    _clientObject = new PVR.PVRClient();
                    if (!_clientObject.init())
                    {
                        Debug.LogError("[PVR-Unity] PVR Session init failed. Start PVR Server and restart the application.");
                        _errMsg = "Failed to init PVR Session.";
                        _clientObject = null;
                        return;
                    }
                }
                if (_renderlib == null)
                {
                    if (!PVRRenderLib.IsRenderLibSupported())
                    {
                        _errMsg = "PVR render lib not support.";
                        return;
                    }
                    _renderlib = new PVRRenderLib();
                    if (!_renderlib.InitRenderLib())
                    {
                        Debug.LogError("[PVR-Unity] PVR RenderLib init failed.");
                        _errMsg = "Failed to init PVR render lib.";
                        return;
                    }
                }

                if (trackingOrig == TrackingOrig.floor)
                {
                    _clientObject.setTrackingOriginType(pvrTrackingOrigin.pvrTrackingOrigin_FloorLevel);
                }
                else
                {
                    _clientObject.setTrackingOriginType(pvrTrackingOrigin.pvrTrackingOrigin_EyeLevel);
                }

                Screen.sleepTimeout = SleepTimeout.NeverSleep;
                pvrDisplayInfo info = new pvrDisplayInfo();
                if (_clientObject.getEyeDisplayInfo(pvrEyeType.pvrEye_Left, ref info) != pvrResult.pvr_success)
                {
                    Debug.LogError("[PVR-Unity] getHmdDisplayInfo failed.");
                    return;
                }
                Application.targetFrameRate = (Int32)info.refresh_rate;

                pvrEyeRenderInfo renderInfo = new pvrEyeRenderInfo();
                if (_clientObject.getEyeRenderInfo(pvrEyeType.pvrEye_Left, ref renderInfo) != pvrResult.pvr_success)
                {
                    Debug.LogError("[PVR-Unity] getEyeRenderInfo failed.");
                    return;
                }
                if (_clientObject.getFovTextureSize(pvrEyeType.pvrEye_Left, renderInfo.Fov, 1.0f, ref _renderTargetSize) != pvrResult.pvr_success)
                {
                    Debug.LogError("[PVR-Unity] getFovTextureSize failed.");
                    return;
                }
                _ready = true;
                UpdateRenderInfo();
                UpdateHmdPose();
                UpdateInputState();
            }

            void OnEnable()
            {
                if (_endOfFrameCoroutine == null)
                {
                    _endOfFrameCoroutine = EndOfFrame();
                }

                StartCoroutine(_endOfFrameCoroutine);
            }

            bool CheckSessionStatus()
            {
                if (_clientObject == null)
                {
                    return false;
                }
                pvrResult ret = _clientObject.getHmdStatus(ref _HmdStatus);
                if (ret != pvrResult.pvr_success || _HmdStatus.ShouldQuit == 1)
                {
                    if (ret != pvrResult.pvr_success)
                    {
                        _errMsg = "lost connect.";
                        Debug.Log("[PVR-Unity] lost connect with service, request application to quit");
                    }
                    else
                    {
                        Debug.Log("[PVR-Unity] service request application to quit");
                    }
                    OnDisable();
                    Stop();
                    Application.Quit();
                    return false;
                }

                if (_HmdStatus.ServiceReady == 0)
                {
                    _errMsg = "service not ready.";
                    return false;
                }

                if (_HmdStatus.HmdPresent == 0)
                {
                    _errMsg = "hmd not present.";
                    return false;
                }

                if (_HmdStatus.DisplayLost == 1)
                {
                    _errMsg = "display lost.";
                    return false;
                }

                return true;
            }

            void UpdateHmdPose()
            {
                if (_ready)
                {
                    double display_time = _clientObject.getPredictedDisplayTime(0);
                    if (_clientObject.getTrackingState(display_time, ref _PoseState) != pvrResult.pvr_success)
                    {
                        Debug.LogError("[PVR-Unity] call getTrackingState failed.");
                        return;
                    }

                    if (_PoseState.HeadPose.StatusFlags != 0)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            _clientObject.calcEyePoses(_PoseState.HeadPose.ThePose, _hmdToEyePose, _eyePoses);
                        }
                    }
                    
                }
            }

            void UpdateInputState()
            {
                if (_ready)
                {
                    if (_clientObject.getInputState(ref _InputState) != pvrResult.pvr_success)
                    {
                        Debug.LogError("[PVR-Unity] call getInputState failed.");
                        return;
                    }
                }
            }

            private void UpdateRenderInfo()
            {
                for (int i = 0; i < 2; i++)
                {
                    _clientObject.getEyeRenderInfo(i == 0 ? pvrEyeType.pvrEye_Left : pvrEyeType.pvrEye_Right, ref _renderInfo[i]);
                    _clientObject.Matrix4f_Projection(_renderInfo[i].Fov, 0.01f, 10000.0f, true, ref _projMatrix[i]);
                    _hmdToEyePose[i] = _renderInfo[i].HmdToEyePose;
                }
            }

            // Update is called once per frame
            void Update()
            {
                _ready = CheckSessionStatus();
                if (ready)
                {
                    UpdateRenderInfo();

                }
            }

            void LateUpdate()
            {

            }

            IEnumerator EndOfFrame()
            {
                while (true)
                {
                    yield return new WaitForEndOfFrame();
                    if (ready)
                    {
                        // Issue a RenderEvent, which copies Unity RenderTextures to RenderManager buffers
                        GL.IssuePluginEvent(_renderlib.GetRenderEventFunction(), PVRRenderLib.RENDER_EVENT);

                        UpdateHmdPose();
                        UpdateInputState();
                    }
                }
            }

            void Stop()
            {
                // Only stop the main instance, since it is the only one that
                // ever actually starts-up.
                if (this == instance)
                {
                    if (null != _clientObject)
                    {
                        Debug.Log("[PVR-Unity] Shutting down PVR.");
                        _clientObject.Dispose();
                        _clientObject = null;
                    }
                    if (null != _renderlib)
                    {
                        _renderlib.CloseRenderLib();
                        _renderlib = null;
                    }
                }
            }

            void OnDisable()
            {
                StopCoroutine(_endOfFrameCoroutine);
            }

            void OnDestroy()
            {
                Stop();
            }

            void OnApplicationQuit()
            {
            }
        }

    }
}

