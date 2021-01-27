using UnityEngine;

namespace PVR
{
    namespace Unity
    {
        /// <summary>
        /// </summary>
        public class Math
        {
            public static Vector3 ConvertPosition(pvrVector3f vec)
            {
                /// Convert to left-handed
                return new Vector3((float)vec.x, (float)vec.y, (float)-vec.z);
            }

            public static Vector2 ConvertPosition(pvrVector2f vec)
            {
                return new Vector2((float)vec.x, (float)vec.y);
            }

            public static Quaternion ConvertOrientation(pvrQuatf quat)
            {
                /// Wikipedia may say quaternions are not handed, but these needed modification.
                return new Quaternion(-(float)quat.x, -(float)quat.y, (float)quat.z, (float)quat.w);
            }

            public static Matrix4x4 ConvertPose(pvrPosef pose)
            {
                return Matrix4x4.TRS(Math.ConvertPosition(pose.Position), Math.ConvertOrientation(pose.Orientation), Vector3.zero);
            }

            public static Rect ConvertViewport(pvrViewPort viewport, pvrSizei surfaceDisplayDimensions)
            {
                //Unity expects normalized coordinates, not pixel coordinates
                return new Rect((float)viewport.x / (float)surfaceDisplayDimensions.w,
                        (float)viewport.y / (float)surfaceDisplayDimensions.h,
                        (float)viewport.width / (float)surfaceDisplayDimensions.w,
                        (float)viewport.height / (float)surfaceDisplayDimensions.h);
            }
            public static Rect ConvertViewportRenderLib(pvrViewPort viewport)
            {
                //Unity expects normalized coordinates, not pixel coordinates
                //@todo below assumes left and right eyes split the screen in half horizontally
                return new Rect(viewport.x / viewport.width, viewport.y / viewport.height, viewport.width / viewport.width, 1);
            }

            public static Matrix4x4 ConvertMatrix(pvrMatrix4f matrix)
            {
                Matrix4x4 matrix4x4 = new Matrix4x4();
                matrix4x4[0, 0] = matrix.M[0];
                matrix4x4[0, 1] = matrix.M[1];
                matrix4x4[0, 2] = matrix.M[2];
                matrix4x4[0, 3] = matrix.M[3];
                matrix4x4[1, 0] = matrix.M[4];
                matrix4x4[1, 1] = matrix.M[5];
                matrix4x4[1, 2] = matrix.M[6];
                matrix4x4[1, 3] = matrix.M[7];
                matrix4x4[2, 0] = matrix.M[8];
                matrix4x4[2, 1] = matrix.M[9];
                matrix4x4[2, 2] = matrix.M[10];
                matrix4x4[2, 3] = matrix.M[11];
                matrix4x4[3, 0] = matrix.M[12];
                matrix4x4[3, 1] = matrix.M[13];
                matrix4x4[3, 2] = matrix.M[14];
                matrix4x4[3, 3] = matrix.M[15];
                return matrix4x4/* * Matrix4x4.Scale(new Vector3(1, -1, 1))*/;
            }

            //Returns a Unity Matrix4x4 from the provided boundaries
            //from http://docs.unity3d.com/ScriptReference/Camera-projectionMatrix.html
            static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
            {
                float x = 2.0F * near / (right - left);
                float y = 2.0F * near / (top - bottom);
                float a = (right + left) / (right - left);
                float b = (top + bottom) / (top - bottom);
                float c = -(far + near) / (far - near);
                float d = -(2.0F * far * near) / (far - near);
                float e = -1.0F;
                Matrix4x4 m = new Matrix4x4();
                m[0, 0] = x;
                m[0, 1] = 0;
                m[0, 2] = a;
                m[0, 3] = 0;
                m[1, 0] = 0;
                m[1, 1] = y;
                m[1, 2] = b;
                m[1, 3] = 0;
                m[2, 0] = 0;
                m[2, 1] = 0;
                m[2, 2] = c;
                m[2, 3] = d;
                m[3, 0] = 0;
                m[3, 1] = 0;
                m[3, 2] = e;
                m[3, 3] = 0;
                return m;
            }
        }
    }
}
