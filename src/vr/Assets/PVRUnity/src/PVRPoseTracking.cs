using UnityEngine;
using System.Collections;

namespace PVR
{
    namespace Unity
    {
        public class PVRPoseTracking : MonoBehaviour
        {
            public enum TrackPart
            {
                Head = 0,
                LeftHand,
                RightHand,
            }
            public TrackPart tracking;
            // Use this for initialization
            void Start()
            {

            }

            // Update is called once per frame
            void Update()
            {
                if (PVRSession.instance.ready)
                {
                    uint StatusFlags = 0;
                    pvrVector3f position;
                    pvrQuatf rotation;

                    if (tracking == TrackPart.LeftHand)
                    {
                        pvrPoseStatef poseState = PVRSession.instance.handsPoseState[0];
                        StatusFlags = poseState.StatusFlags;
                        position = poseState.ThePose.Position;
                        rotation = poseState.ThePose.Orientation;
                    }
                    else if (tracking == TrackPart.RightHand)
                    {
                        pvrPoseStatef poseState = PVRSession.instance.handsPoseState[1];
                        StatusFlags = poseState.StatusFlags;
                        position = poseState.ThePose.Position;
                        rotation = poseState.ThePose.Orientation;
                    }
                    else
                    {
                        pvrPoseStatef poseState = PVRSession.instance.headPoseState;
                        StatusFlags = poseState.StatusFlags;
                        position = poseState.ThePose.Position;
                        rotation = poseState.ThePose.Orientation;
                    }
                    if (StatusFlags != 0)
                    {
                        transform.localPosition = Math.ConvertPosition(position);
                        transform.localRotation = Math.ConvertOrientation(rotation);
                    }
                }
                
            }
        }

    }
}

