using UnityEngine;
using System.Collections;

namespace PVR
{
    namespace Unity
    {
        [RequireComponent(typeof(Camera))]
        [RequireComponent(typeof(PVREyeOffset))]
        public class PVRSurface : MonoBehaviour
        {
            private RenderTexture _renderToTexture;
            private Camera _camera;
            private PVRPoseTracking _tracking;
            private PVREyeOffset _eyeOffset;
            private bool _headlocked = true;
            private int _layerId;

            pvrEyeType eye
            {
                get
                {
                    return (_eyeOffset.Eye == PVREyeOffset.Eyes.Left ? pvrEyeType.pvrEye_Left : pvrEyeType.pvrEye_Right);
                }
            }

            void EnsureRenderTarget()
            {
                if (_renderToTexture == null && PVRSession.instance.ready)
                {
                    pvrSizei size = PVRSession.instance.renderTargetSize;
                    _renderToTexture = new RenderTexture(size.w, size.h, 24, RenderTextureFormat.Default);
                    if (QualitySettings.antiAliasing > 0)
                    {
                        _renderToTexture.antiAliasing = QualitySettings.antiAliasing;
                    }
                    _camera.targetTexture = _renderToTexture;
                    _renderToTexture.Create();
                    PVRSession.RegisterColorBuffer(_renderToTexture.GetNativeTexturePtr());
                }
            }

            // Use this for initialization
            void Start()
            {
                _camera = gameObject.GetComponent<Camera>();
                if (_camera == null)
                {
                    Debug.LogError("[PVR-Unity] PVRSurface need Camera component.");
                    return;
                }
                
                _eyeOffset = gameObject.GetComponent<PVREyeOffset>();
                if (_eyeOffset == null)
                {
                    Debug.LogError("[PVR-Unity] PVRSurface need PVREyeOffset script component.");
                    return;
                }
                GameObject parent = GetParent.Get(this.gameObject);
                if (parent)
                {
                    _tracking = parent.GetComponent<PVRPoseTracking>();
                    _headlocked = (_tracking == null || (!_tracking.enabled));
                    _layerId = parent.GetInstanceID();
                    Camera parentCamera = parent.GetComponent<Camera>();
                    if (parentCamera)
                    {
                        _camera.CopyFrom(parentCamera);
                    }
                }

                EnsureRenderTarget();
            }

            // Update is called once per frame
            void Update()
            {
                if (PVRSession.instance.ready && _camera != null && _eyeOffset != null)
                {
                    EnsureRenderTarget();
                    if (_renderToTexture != null)
                    {
                        _renderToTexture.DiscardContents();
                    }

                    pvrFovPort _fovPort = PVRSession.instance.GetFovPort(eye);
                    Vector2 tanFov = new Vector2(Mathf.Max(_fovPort.RightTan, _fovPort.LeftTan), Mathf.Max(_fovPort.UpTan, _fovPort.DownTan));
                    _camera.aspect = tanFov.x / tanFov.y;
                    _camera.fieldOfView = 2.0f * Mathf.Atan(tanFov.y) * Mathf.Rad2Deg;
                }
            }

            void OnPreCull()
            {
                if (PVRSession.instance.ready && _camera != null && _eyeOffset != null && _renderToTexture != null)
                {
                    PVRSession.instance.SubmitEyeLayer(_layerId, eye, _renderToTexture.GetNativeTexturePtr(), _headlocked);

                    RenderTexture.active = _renderToTexture;
                    Rect _emptyViewport = new Rect(0, 0, _renderToTexture.width, _renderToTexture.height);
                    GL.Viewport(_emptyViewport);
                    GL.Clear(true, true, new Color(0, 0, 0, 0));
                    RenderTexture.active = null;
                }
            }

            void OnEnable()
            {
            }

            void OnDisable()
            {
            }

            void OnDestroy()
            {
                if (_renderToTexture != null)
                {
                    PVRSession.UnregisterColorBuffer(_renderToTexture.GetNativeTexturePtr());
                }
            }
        }
    }
}
