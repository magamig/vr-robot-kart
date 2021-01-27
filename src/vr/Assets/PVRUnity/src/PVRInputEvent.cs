using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace PVR
{
    namespace Unity
    {
        public class PVRButtonEventData : BaseEventData
        {
            private int _hand;
            private pvrButton _btn;

            public int hand
            {
                get
                {
                    return _hand;
                }
            }

            public pvrButton btn
            {
                get
                {
                    return _btn;
                }
            }

            public PVRButtonEventData(EventSystem eventSystem, int hand, pvrButton btn)
                :base(eventSystem)
            {
                _btn = btn;
                _hand = hand;
            }
        }

        public class PVRAxisEventData : BaseEventData
        {
            private int _hand;
            private pvrButton _btn;
            private pvrVector2f _axis;

            public int hand
            {
                get
                {
                    return _hand;
                }
            }

            public pvrButton btn
            {
                get
                {
                    return _btn;
                }
            }

            public float x
            {
                get
                {
                    return _axis.x;
                }
            }

            public float y
            {
                get
                {
                    return _axis.y;
                }
            }

            public PVRAxisEventData(EventSystem eventSystem, int hand, pvrButton btn, pvrVector2f axis)
                : base(eventSystem)
            {
                _btn = btn;
                _axis = axis;
                _hand = hand;
            }
        }

        public interface IPVRInputEventTarget : IEventSystemHandler
        {
            void OnButtonPress(PVRButtonEventData data);
            void OnButtonRelease(PVRButtonEventData data);
            void OnButtonTouch(PVRButtonEventData data);
            void OnButtonUntouch(PVRButtonEventData data);
            void OnAxisChange(PVRAxisEventData data);
        }

        public class PVRInputEvent : MonoBehaviour
        {
            private pvrInputState _PrevInputState = new pvrInputState();
            private PVR.pvrButton[] _Buttons = { PVR.pvrButton.pvrButton_ApplicationMenu,
                                PVR.pvrButton.pvrButton_System, PVR.pvrButton.pvrButton_Trigger,
                                PVR.pvrButton.pvrButton_TouchPad, PVR.pvrButton.pvrButton_Grip, PVR.pvrButton.pvrButton_JoyStick };
            // Use this for initialization
            void Start()
            {
                _PrevInputState.Grip = new float[2];
                _PrevInputState.HandButtons = new uint[2];
                _PrevInputState.HandTouches = new uint[2];
                _PrevInputState.JoyStick = new pvrVector2f[2];
                _PrevInputState.TouchPad = new pvrVector2f[2];
                _PrevInputState.Trigger = new float[2];
            }

            // Update is called once per frame
            void Update()
            {
                if (PVRSession.instance.ready) {
                    pvrInputState inputState = PVRSession.instance.inputState;
                    
                    for (int hand = 0; hand < 2; hand++)
                    {
                        uint oldButtons = _PrevInputState.HandButtons[hand];
                        uint oldTouches = _PrevInputState.HandTouches[hand];
                        if (inputState.HandButtons[hand] != oldButtons)
                        {
                            for (int i = 0; i < _Buttons.Length; i++)
                            {
                                bool currPress = ((inputState.HandButtons[hand] & (uint)_Buttons[i]) != 0);
                                bool oldPress = ((oldButtons & (uint)_Buttons[i]) != 0);
                                if (currPress != oldPress)
                                {
                                    if (currPress)
                                    {
                                        //Debug.LogFormat("[PVR-Unity] Button {0} is just pressed.", _Buttons[i]);
                                        ExecuteEvents.Execute<IPVRInputEventTarget>(gameObject, new PVRButtonEventData(EventSystem.current, hand, _Buttons[i]), (x,y)=>x.OnButtonPress((PVRButtonEventData)y));
                                    }
                                    else
                                    {
                                        //Debug.LogFormat("[PVR-Unity] Button {0} is just released.", _Buttons[i]);
                                        ExecuteEvents.Execute<IPVRInputEventTarget>(gameObject, new PVRButtonEventData(EventSystem.current, hand, _Buttons[i]), (x, y) => x.OnButtonRelease((PVRButtonEventData)y));
                                    }
                                }

                                bool currTouch = ((inputState.HandTouches[hand] & (uint)_Buttons[i]) != 0);
                                bool oldTouch = ((oldTouches & (uint)_Buttons[i]) != 0);
                                if (currTouch != oldTouch)
                                {
                                    if (currTouch)
                                    {
                                        ExecuteEvents.Execute<IPVRInputEventTarget>(gameObject, new PVRButtonEventData(EventSystem.current, hand, _Buttons[i]), (x, y) => x.OnButtonTouch((PVRButtonEventData)y));
                                    }
                                    else
                                    {
                                        ExecuteEvents.Execute<IPVRInputEventTarget>(gameObject, new PVRButtonEventData(EventSystem.current, hand, _Buttons[i]), (x, y) => x.OnButtonUntouch((PVRButtonEventData)y));
                                    }
                                }
                                pvrVector2f oldAxis = new pvrVector2f();
                                switch (_Buttons[i])
                                {
                                    case pvrButton.pvrButton_Grip:
                                        oldAxis.x = _PrevInputState.Grip[hand];
                                        break;
                                    case pvrButton.pvrButton_Trigger:
                                        oldAxis.x = _PrevInputState.Trigger[hand];
                                        break;
                                    case pvrButton.pvrButton_TouchPad:
                                        oldAxis.x = _PrevInputState.TouchPad[hand].x;
                                        oldAxis.y = _PrevInputState.TouchPad[hand].y;
                                        break;
                                    case pvrButton.pvrButton_JoyStick:
                                        oldAxis.x = _PrevInputState.JoyStick[hand].x;
                                        oldAxis.y = _PrevInputState.JoyStick[hand].y;
                                        break;
                                    default:
                                        oldAxis.x = oldPress ? 1.0f : 0.0f;
                                        break;
                                }
                                pvrVector2f newAxis = PVRSession.instance.GetButtonAxis(hand, _Buttons[i]);
                                if (newAxis.x != oldAxis.x || newAxis.y != oldAxis.y)
                                {
                                    ExecuteEvents.Execute<IPVRInputEventTarget>(gameObject, new PVRAxisEventData(EventSystem.current, hand, _Buttons[i], newAxis), (x, y) => x.OnAxisChange((PVRAxisEventData)y));
                                }
                            }
                        }

                        _PrevInputState.HandButtons[hand] = inputState.HandButtons[hand];
                        _PrevInputState.HandTouches[hand] = inputState.HandTouches[hand];
                        _PrevInputState.Grip[hand] = inputState.Grip[hand];
                        _PrevInputState.JoyStick[hand] = inputState.JoyStick[hand];
                        _PrevInputState.Trigger[hand] = inputState.Trigger[hand];
                        _PrevInputState.TouchPad[hand] = inputState.TouchPad[hand];
                    }
                    _PrevInputState.TimeInSeconds = inputState.TimeInSeconds;
                }
            }
        }

    }
}

