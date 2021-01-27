using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System;

namespace PVR
{
    namespace Unity
    {
        public class PVRRenderLib
        {
            public const int RENDER_EVENT = 0;
            public const int SHUTDOWN_EVENT = 1;
            public const int UPDATE_RENDERINFO_EVENT = 2;
            public const int INIT_EVENT = 3;
            private const string PluginName = "pvrUnityRenderPlugin";
            private const CallingConvention call_conv= CallingConvention.StdCall;

            [UnmanagedFunctionPointer(CallingConvention.Winapi)]
            private delegate void DebugLog(string log);
            private static readonly DebugLog debugLog = DebugWrapper;
            private static readonly IntPtr functionPointer = Marshal.GetFunctionPointerForDelegate(debugLog);
            private static void DebugWrapper(string log) { Debug.Log(log); }


            [DllImport(PluginName, CallingConvention = call_conv)]
            private static extern IntPtr GetRenderEventFunc();

            [DllImport(PluginName, CallingConvention = call_conv)]
            private static extern void LinkDebugFromUnity([MarshalAs(UnmanagedType.FunctionPtr)]IntPtr debugCal);

            [DllImport(PluginName, CallingConvention = call_conv)]
            private static extern pvrResult RegisterColorBufferFromUnity(System.IntPtr texturePtr);
            [DllImport(PluginName, CallingConvention = call_conv)]
            private static extern void UnregisterColorBufferFromUnity(System.IntPtr texturePtr);

            [DllImport(PluginName, CallingConvention = call_conv)]
            private static extern void SubmitEyeLayerFromUnity(int layer_uid, int eye, System.IntPtr texturePtr, ref pvrPosef pose, double SensorSampleTime, bool headLocked);

            private bool _linkDebug = false; //causes crash on exit if true, only enable for debugging

            public bool InitRenderLib()
            {
                if (_linkDebug)
                {
                    //this will cause a crash when exiting the Unity editor or an application
                    //only use for debugging purposes, do not leave on for release.
                    LinkDebugFromUnity(functionPointer); // Hook our c++ plugin into Unity's console log.
                }

                return OpenRenderLib();
            }

            public void SubmitEyeLayer(int layer_uid, int eye, System.IntPtr texturePtr, ref pvrPosef pose, double SensorSampleTime, bool headLocked)
            {
                SubmitEyeLayerFromUnity(layer_uid, eye, texturePtr, ref pose, SensorSampleTime, headLocked);
            }

            //Call the Unity Rendering Plugin to initialize the RenderManager
            public bool OpenRenderLib()
            {
                GL.IssuePluginEvent(GetRenderEventFunction(), PVRRenderLib.INIT_EVENT);
                return true;
            }

            public void RegisterColorBuffer(IntPtr colorBuffer)
            {
                RegisterColorBufferFromUnity(colorBuffer);
            }

            public void UnregisterColorBuffer(IntPtr colorBuffer)
            {
                UnregisterColorBufferFromUnity(colorBuffer);
            }

            //Get a pointer to the plugin's rendering function
            public IntPtr GetRenderEventFunction()
            {
                return GetRenderEventFunc();
            }

            //Shutdown RenderManager and Dispose of the ClientContext we created for it
            public void CloseRenderLib()
            {
                GL.IssuePluginEvent(GetRenderEventFunction(), PVRRenderLib.SHUTDOWN_EVENT);
            }

            public static bool IsRenderLibSupported()
            {
                bool support = true;
                if (!SystemInfo.graphicsDeviceVersion.Contains("Direct3D 11"))
                {
                    Debug.LogError("[PVR-Unity] RenderLib not supported on " +
                        SystemInfo.graphicsDeviceVersion + ". Only Direct3D11 is currently supported.");
                    support = false;
                }

                if (!SystemInfo.supportsRenderTextures)
                {
                    Debug.LogError("[PVR-Unity] RenderLib not supported. RenderTexture (Unity Pro feature) is unavailable.");
                    support = false;
                }
                if (!IsUnityVersionSupported())
                {
                    Debug.LogError("[PVR-Unity] RenderLib not supported. Unity 5.4+ is required for RenderManager support.");
                    support = false;
                }
                return support;
            }

            //Unity 5.4+ is required as the plugin uses the native plugin interface introduced in Unity 5.4
            public static bool IsUnityVersionSupported()
            {
                bool support = true;
                try
                {
                    string version = new Regex(@"(\d+\.\d+)\..*").Replace(Application.unityVersion, "$1");
                    if (new Version(version) < new Version("5.4"))
                    {
                        support = false;
                    }
                }
                catch
                {
                    Debug.LogWarning("[PVR-Unity] Unable to determine Unity version from: " + Application.unityVersion);
                    support = false;
                }
                return support;
            }
        }
    }
}