#define IGNORE_ISHITTESTVISIBLE_FALSE_ELEMENTS

using UnityEngine;
using Noesis;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

using LoadAction = UnityEngine.Rendering.RenderBufferLoadAction;
using StoreAction = UnityEngine.Rendering.RenderBufferStoreAction;

[AddComponentMenu("NoesisGUI/Noesis View")]
[HelpURL("https://www.noesisengine.com/docs")]
[DisallowMultipleComponent]
public class NoesisMonoRender: MonoBehaviour, ISerializationCallbackReceiver
{
    private static class Profiling
    {
        public static readonly CustomSampler UpdateSampler = CustomSampler.Create("Noesis.Update");
        public static readonly CustomSampler RenderOnScreenSampler = CustomSampler.Create("Noesis.RenderOnscreen");
        public static readonly string RegisterView = "Noesis.RegisterView";
        public static readonly string UnregisterView = "Noesis.UnregisterView";
        public static readonly string UpdateRenderTree = "Noesis.UpdateRenderTree";
        public static readonly string RenderOffScreen = "Noesis.RenderOffscreen";
        public static readonly string RenderOnScreen = "Noesis.RenderOnscreen";
        public static readonly string RenderTexture = "Noesis.RenderTexture";
    }

    public float TessellationMaxPixelError
    {
        set
        {
            if (_uiView != null)
            {
                _uiView.SetTessellationMaxPixelError(value);
            }

            _tessellationMaxPixelError = value;
        }

        get
        {
            if (_uiView != null)
            {
                return _uiView.GetTessellationMaxPixelError().Error;
            }

            return _tessellationMaxPixelError; 
        }
    }

    public RenderFlags RenderFlags
    {
        set
        {
            if (_uiView != null)
            {
                _uiView.SetFlags(value);
            }

            _renderFlags = value;
        }
        get
        {
            if (_uiView != null)
            {
                return _uiView.GetFlags();
            }

            return _renderFlags;
        }
    }

    public bool WorldSpace
    {
        get { return (RenderFlags & RenderFlags.DepthTesting) > 0; }
    }
    
    public bool DPIScale
    {
        set { _dpiScale = value; }
        get { return _dpiScale; }
    }
    
    public bool ContinuousRendering
    {
        set { _continuousRendering = value; }
        get { return _continuousRendering; }
    }
    
    public bool EnableExternalUpdate
    {
        set { _enableExternalUpdate = value; }
        get { return _enableExternalUpdate; }
    }
    
    public bool NeedsRendering
    {
        set { _needsRendering = value; }
        get { return _needsRendering; }
    }
    
    public bool EnableKeyboard
    {
        set { _enableKeyboard = value; }
        get { return _enableKeyboard; }
    }

    public bool EnableTouch
    {
        set { _enableTouch = value; }
        get { return _enableTouch; }
    }
    
    public bool EmulateTouch
    {
        set
        {
            if (_uiView != null)
            {
                _uiView.SetEmulateTouch(value);
            }

            _emulateTouch = value;
        }
        get { return _emulateTouch; }
    }

    public bool UseRealTimeClock
    {
        set { _useRealTimeClock = value; }
        get { return _useRealTimeClock; }
    }

    public bool ClearStencil
    {
        set { _clearStencil = value; }
        get { return _clearStencil; }
    }

    public FrameworkElement Content
    {
        get { return _uiView != null ? _uiView.Content : null; }
    }

    public bool IsRenderToTexture()
    {
        return !gameObject.TryGetComponent(out Camera _);
    }

    public event RenderingEventHandler Rendering
    {
        add
        {
            if (_uiView != null)
            {
                _uiView.Rendering += value;
            }
        }
        remove
        {
            if (_uiView != null)
            {
                _uiView.Rendering -= value;
            }
        }
    }
    
    [SerializeField] private bool _isPPAAEnabled = true;
    [SerializeField] private float _tessellationMaxPixelError = Noesis.TessellationMaxPixelError.MediumQuality.Error;
    [SerializeField] private RenderFlags _renderFlags = 0;
    [SerializeField] private bool _dpiScale = true;
    [SerializeField] private bool _continuousRendering = true;
    [SerializeField] private bool _enableExternalUpdate = false;
    [SerializeField] private bool _enableKeyboard = true;
    [SerializeField] private bool _enableTouch = true;
    [UnityEngine.Serialization.FormerlySerializedAs("_enableGamepad")]
    [SerializeField] private bool _emulateTouch = false;
    [SerializeField] private bool _useRealTimeClock = false;
    [SerializeField] private bool _clearStencil = false;
    [SerializeField] private UnityEngine.Transform _xrTrackingOrigin;
    
    private NoesisView _uiView;
    private bool _needsRendering = false;
    private float _stereoScale = 1.0f;
    
    private bool _updatePending = true;
    private bool _visible = true;
    private int _viewSizeX;
    private int _viewSizeY;
    private float _viewScale;
    private float _viewStereoScale = 1.0f;
    private Camera _camera;
    private CommandBuffer _commandBuffer;
    private Vector3 _mousePos;
    private int _activeDisplay = 0;
    
#if ENABLE_UGUI_PACKAGE
    UnityEngine.EventSystems.PointerEventData _pointerData;
#endif
    
#if !UNITY_EDITOR && UNITY_STANDALONE_OSX
    private static int lastFrame;
    private static Noesis.Key lastKeyDown;
#endif

    public ViewStats GetStats()
    {
        if (_uiView != null)
        {
            return _uiView.GetStats();
        }

        return new ViewStats();
    }

    private void Reset()
    {
        _isPPAAEnabled = true;
        _tessellationMaxPixelError = Noesis.TessellationMaxPixelError.MediumQuality.Error;
        _renderFlags = 0;
        _dpiScale = true;
        _enableExternalUpdate = false;
        _enableKeyboard = true;
        _enableTouch = true;
        _emulateTouch = false;
        _useRealTimeClock = false;
        _clearStencil = false;
    }

    public NoesisView Initialize(Camera camera, FrameworkElement element)
    {
        _commandBuffer = new CommandBuffer();
        _camera = camera;
        
        useGUILayout = false;
        
        Camera.onPreRender += _onPreRender;
        Camera.onPostRender += _onPostRender;
        
        _createView(element);

        return _uiView;
    }

    private void OnDestroy()
    {
        _destroyView();
    }

    private void UpdateSize()
    {
        int sizeX = _camera.pixelWidth;
        int sizeY = _camera.pixelHeight;

        if (sizeX != _viewSizeX || sizeY != _viewSizeY)
        {
            _uiView.SetSize(sizeX, sizeY);
            _viewSizeX = sizeX;
            _viewSizeY = sizeY;
        }

        // TODO : #R92 Adjust here scaling.
        // float scale = (!WorldSpace && _dpiScale && Screen.dpi > 0.0f) ? Screen.dpi / 96.0f : 1.0f;
        float scale = (!WorldSpace && _dpiScale && Screen.dpi > 0.0f) ? Screen.dpi / 144.0f : 1.0f;

        if (scale != _viewScale)
        {
            _uiView.SetScale(scale);
            _viewScale = scale;
        }

        if (_stereoScale != _viewStereoScale)
        {
            _uiView.SetStereoOffscreenScaleFactor(_stereoScale);
            _viewStereoScale = _stereoScale;
        }
    }

    private void LateUpdate()
    {
        if (!_enableExternalUpdate)
        {
            UpdateInternal();
        }
    }

    public void ExternalUpdate()
    {
        Debug.Assert(_enableExternalUpdate, "Calling ExternalUpdate() with EnableExternalUpdate disabled", this);
        UpdateInternal();
    }

    private static Matrix4 NoesisMatrix(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, float w, float h)
    {
        float hw = 0.5f * w;
        float hh = 0.5f * h;

        Matrix4x4 noesisMatrix;

        if (SystemInfo.usesReversedZBuffer)
        {
            noesisMatrix = new Matrix4x4
            (
                new UnityEngine.Vector4(hw,  0,  0,    0),
                new UnityEngine.Vector4(0, -hh,  0,    0),
                new UnityEngine.Vector4(0,   0, -0.5f, 0),
                new UnityEngine.Vector4(hw, hh,  0.5f, 1)
            );
        }
        else
        {
            noesisMatrix = new Matrix4x4
            (
                new UnityEngine.Vector4(hw,  0, 0,    0),
                new UnityEngine.Vector4(0, -hh, 0,    0),
                new UnityEngine.Vector4(0,   0, 0.5f, 0),
                new UnityEngine.Vector4(hw, hh, 0.5f, 1)
            );
        }

        Matrix4x4 _ = noesisMatrix * projectionMatrix * viewMatrix;

        return new Matrix4
        (
            _.m00, _.m10, _.m20, _.m30,
            _.m01, _.m11, _.m21, _.m31,
            _.m02, _.m12, _.m22, _.m32,
            _.m03, _.m13, _.m23, _.m33
        );
    }

    private static Matrix4 CameraMatrix(Camera camera)
    {
        return NoesisMatrix(camera.worldToCameraMatrix, camera.projectionMatrix, camera.pixelWidth, camera.pixelHeight);
    }

    private void UpdateInternal()
    {
        if (_uiView != null && _visible)
        {
            Profiling.UpdateSampler.Begin();

            if (_camera != null && WorldSpace)
            {
                _uiView.SetProjectionMatrix(CameraMatrix(_camera));
            }

            float t = Time.realtimeSinceStartup;

            UpdateSize();

            NoesisUnity.IME.Update(_uiView);
            NoesisUnity.TouchKeyboard.Update();

            Noesis_UnityUpdate();
            _needsRendering = _uiView.Update(_useRealTimeClock ? t : Time.time);

            Profiling.UpdateSampler.End();

            if (_needsRendering)
            {
                _commandBuffer.name = Profiling.UpdateRenderTree;
                NoesisRenderer.UpdateRenderTree(_uiView, _commandBuffer);

                Graphics.ExecuteCommandBuffer(_commandBuffer);
                _commandBuffer.Clear();
            }
        }
    }

    private void _onPreRender(Camera cam)
    {
        if (_camera != null)
        {
            // In case there are several cameras rendering to the same texture (Camera Stacking),
            // the camera rendered first (less depth) is the one that must apply our offscreen phase
            // to avoid inefficient Load/Store in Tiled architectures
            if (_updatePending && cam.targetTexture == _camera.targetTexture && cam.depth <= _camera.depth)
            {
                if (_uiView != null && _visible)
                {
                    _commandBuffer.name = Profiling.RenderOffScreen;
                    NoesisRenderer.RenderOffscreen(_uiView, _commandBuffer, false);

                    Graphics.ExecuteCommandBuffer(_commandBuffer);
                    _commandBuffer.Clear();

                    GL.InvalidateState();
                    ForceRestoreCameraRenderTarget();
                }

                _updatePending = false;
            }
        }
    }

    private void ForceRestoreCameraRenderTarget()
    {
        // Unity should automatically restore the render target but sometimes (for example a scene without lights)
        // it doesn't. We use this hack to flush the active render target and force unity to set the camera RT afterward
        RenderTexture surface = RenderTexture.GetTemporary(1,1);
        Graphics.SetRenderTarget(surface);
        RenderTexture.ReleaseTemporary(surface);
    }

    private static bool IsGL()
    {
        var type = SystemInfo.graphicsDeviceType;

      #if UNITY_2023_1_OR_NEWER
        return type == GraphicsDeviceType.OpenGLES3 || type == GraphicsDeviceType.OpenGLCore;
      #else
        return type == GraphicsDeviceType.OpenGLES2 || type == GraphicsDeviceType.OpenGLES3
            || type == GraphicsDeviceType.OpenGLCore;
      #endif
    }

    private bool IsEyeTexture(RenderTexture texture)
    {
        // In VR the Swap Chain is named 'XR Texture[#]' (before Unity 2020 it was 'RTDeviceEyeTextureArray')
        return texture.name.StartsWith("XR Texture");
    }

    private bool FlipRender()
    {
        // In D3D when Unity is rendering to an intermediate texture instead of the back buffer, we need to vertically flip the output
        // Note that camera.activeTexture should only be checked from OnPostRender
        if (!IsGL())
        {
          #if ENABLE_VR && ENABLE_XR_MODULE
            return _camera.activeTexture != null && !IsEyeTexture(_camera.activeTexture);
          #else
            return _camera.activeTexture != null;
          #endif
        }

        return false;
    }

    private void _onPostRender(Camera cam)
    {
        if (_uiView != null && _visible)
        {
            _commandBuffer.name = Profiling.RenderOnScreen;
            bool flipY = FlipRender();
            NoesisRenderer.RenderOnscreen(_uiView, flipY, _commandBuffer, false, _clearStencil);

            Graphics.ExecuteCommandBuffer(_commandBuffer);
            _commandBuffer.Clear();

            GL.InvalidateState();
            _updatePending = true;
        }
    }

    private void OnGUI()
    {
        if (_uiView != null && (_camera == null || _activeDisplay == _camera.targetDisplay))
        {
            if (_camera)
            {
                UnityEngine.GUI.depth = -(int)_camera.depth;
            }

            // ProcessEvent(Event.current, _enableKeyboard, false);
        }
    }

    private void OnApplicationFocus(bool focused)
    {
        if (_uiView != null)
        {
            if (NoesisUnity.TouchKeyboard.keyboard == null)
            {
                if (focused)
                {
                    _uiView.Activate();
                }
                else
                {
                    _uiView.Deactivate();
                }
            }
        }
    }

    private void _createView(FrameworkElement content)
    {
        if (_uiView == null)
        {
            // Send settings for the internal device, created by the first view
            NoesisRenderer.SetRenderSettings();

            _viewSizeX = 0;
            _viewSizeY = 0;
            _viewScale = 1.0f;

            _uiView = new NoesisView(content);
            _uiView.SetTessellationMaxPixelError(_tessellationMaxPixelError);
            _uiView.SetEmulateTouch(_emulateTouch);
            _uiView.SetFlags(_renderFlags);

            _commandBuffer.name = Profiling.RegisterView;
            NoesisRenderer.RegisterView(_uiView, _commandBuffer);
            Graphics.ExecuteCommandBuffer(_commandBuffer);
            _commandBuffer.Clear();
        }
    }

    private void _destroyView()
    {
        Camera.onPreRender -= _onPreRender;
        Camera.onPostRender -= _onPostRender;
        
        if (_uiView != null)
        {
            _commandBuffer.name = Profiling.UnregisterView;
            NoesisRenderer.UnregisterView(_uiView, _commandBuffer);
            Graphics.ExecuteCommandBuffer(_commandBuffer);
            _commandBuffer.Clear();

            _uiView = null;
        }
    }

    public void OnBeforeSerialize()
    {
        
    }

    public void OnAfterDeserialize()
    {
        // (3.0) PPAA flag is now in view render flags 
        if (_isPPAAEnabled)
        {
            _renderFlags |= RenderFlags.PPAA;
            _isPPAAEnabled = false;
        }
    }

    [DllImport(Library.Name)]
    private static extern void Noesis_UnityUpdate();

    [DllImport(Library.Name)]
    private static extern bool Noesis_UnityHitTest(HandleRef root, ref Point point);
}