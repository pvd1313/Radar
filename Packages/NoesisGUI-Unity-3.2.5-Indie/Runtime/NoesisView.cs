#define IGNORE_ISHITTESTVISIBLE_FALSE_ELEMENTS

using UnityEngine;
using Noesis;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using UnityEngine.Profiling;
using UnityEngine.XR;

using LoadAction = UnityEngine.Rendering.RenderBufferLoadAction;
using StoreAction = UnityEngine.Rendering.RenderBufferStoreAction;

//[ExecuteInEditMode]
[AddComponentMenu("NoesisGUI/Noesis View")]
[HelpURL("https://www.noesisengine.com/docs")]
[DisallowMultipleComponent]
public class NoesisView: MonoBehaviour, ISerializationCallbackReceiver
{
    #region Public properties

    /// <summary>
    /// User interface definition XAML
    /// </summary>
    public NoesisXaml Xaml
    {
        set { _xaml = value; }
        get { return _xaml; }
    }

    /// <summary>
    /// The texture to render this View into
    /// </summary>
    public RenderTexture Texture
    {
        set { _texture = value; }
        get { return _texture; }
    }

    /// <summary>
    /// Tessellation curve tolerance in screen space. 'Medium Quality' is usually fine for PPAA (non-multisampled)
    /// while 'High Quality' is the recommended pixel error if you are rendering to a 8x multisampled surface
    /// </summary>
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

    /// <summary>
    /// Bit flags used for debug rendering purposes.
    /// </summary>
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

    /// <summary>
    /// When enabled, the UI is positioned in the world among other objects in the Scene
    /// </summary>
    public bool WorldSpace
    {
        get { return (RenderFlags & RenderFlags.DepthTesting) > 0; }
    }

    /// <summary>
    /// The projection matrix set to the view is used for determining the visual impact of nodes
    /// in the offscreen phase. The stereo matrices used for rendering in VR are slightly different.
    /// To account for this difference, it is recommended to apply a scale using this property.
    /// For non-VR this must be always 1. For VR, we recommend a value between 2 and 3.
    /// </summary>
    public float StereoOffscreenScaleFactor
    {
        set { _stereoScale = value; }
        get { return _stereoScale; }
    }

    /// <summary>
    /// When enabled, the view is scaled by the actual DPI of the screen or physical device running the application
    /// </summary>
    public bool DPIScale
    {
        set { _dpiScale = value; }
        get { return _dpiScale; }
    }

    /// <summary>
    /// When continuous rendering is disabled, rendering only happens when UI changes. For performance
    /// purposes and to save battery this is the default mode when rendering to texture. If not rendering
    /// to texture, this property is ignored. Use the property 'NeedsRendering' instead.
    /// </summary>
    public bool ContinuousRendering
    {
        set { _continuousRendering = value; }
        get { return _continuousRendering; }
    }

    /// <summary>
    /// When enabled, the view must be explicitly updated by calling 'ExternalUpdate()'.
    /// By default, the view is automatically updated during LateUpdate.
    /// </summary>
    public bool EnableExternalUpdate
    {
        set { _enableExternalUpdate = value; }
        get { return _enableExternalUpdate; }
    }

    /// <summary>
    /// After updating the view, this flag indicates if the GUI needs to be repainted.
    /// This flag can be used on manually painted cameras to optimize performance and save battery.
    /// </summary>
    public bool NeedsRendering
    {
        set { _needsRendering = value; }
        get { return _needsRendering; }
    }

    /// <summary>
    /// Enables keyboard input management.
    /// </summary>
    public bool EnableKeyboard
    {
        set { _enableKeyboard = value; }
        get { return _enableKeyboard; }
    }

    /// <summary>
    /// Enables mouse input management.
    /// </summary>
    public bool EnableMouse
    {
        set { _enableMouse = value; }
        get { return _enableMouse; }
    }

    /// <summary>
    /// Enables touch input management.
    /// </summary>
    public bool EnableTouch
    {
        set { _enableTouch = value; }
        get { return _enableTouch; }
    }

    /// <summary>
    /// The initial delay (in seconds) between an initial button action and a repeated action.
    /// </summary>
    public float ActionsRepeatDelay
    {
        set { _actionsRepeatDelay = value; }
        get { return _actionsRepeatDelay; }
    }

    /// <summary>
    /// The speed (in seconds) that the button action repeats itself once repeating.
    /// </summary>
    public float ActionsRepeatRate
    {
        set { _actionsRepeatRate = value; }
        get { return _actionsRepeatRate; }
    }

    /// <summary>
    /// Transform representing the real world origin for tracking devices
    /// </summary>
    public UnityEngine.Transform XRTrackingOrigin
    {
        set { _xrTrackingOrigin = value; }
        get { return _xrTrackingOrigin; }
    }

    /// <summary>
    /// Emulate touch input with mouse.
    /// </summary>
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

    /// <summary>
    /// When enabled, UI is updated using Time.realtimeSinceStartup.
    /// </summary>
    public bool UseRealTimeClock
    {
        set { _useRealTimeClock = value; }
        get { return _useRealTimeClock; }
    }

    /// <summary>
    /// When enabled, then stencil buffer is cleared before rendering this view
    /// </summary>
    public bool ClearStencil
    {
        set { _clearStencil = value; }
        get { return _clearStencil; }
    }

    /// <summary>
    /// Gets the root of the loaded Xaml.
    /// </summary>
    /// <returns>Root element.</returns>
    public FrameworkElement Content
    {
        get { return _uiView != null ? _uiView.Content : null; }
    }

    /// <summary>
    /// Indicates if this component is rendering UI to a RenderTexture.
    /// </summary>
    /// <returns></returns>
    public bool IsRenderToTexture()
    {
        return !gameObject.TryGetComponent(out Camera _);
    }

    #endregion

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

    public ViewStats GetStats()
    {
        if (_uiView != null)
        {
            return _uiView.GetStats();
        }

        return new ViewStats();
    }

    #region Keyboard input events
    /// <summary>
    /// Notifies Renderer that a key was pressed.
    /// </summary>
    /// <param name="key">Key identifier.</param>
    public bool KeyDown(Key key)
    {
        if (_uiView != null)
        {
            return _uiView.KeyDown(key);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that a key was released.
    /// </summary>
    /// <param name="key">Key identifier.</param>
    public bool KeyUp(Key key)
    {
        if (_uiView != null)
        {
            return _uiView.KeyUp(key);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that a key was translated to the corresponding character.
    /// </summary>
    /// <param name="ch">Unicode character value.</param>
    public bool Char(uint ch)
    {
        if (_uiView != null)
        {
            return _uiView.Char(ch);
        }

        return false;
    }
    #endregion

    #region Mouse input events
    /// <summary>
    /// Notifies Renderer that mouse was moved. The mouse position is specified in renderer
    /// surface pixel coordinates.
    /// </summary>
    /// <param name="x">Mouse x-coordinate.</param>
    /// <param name="y">Mouse y-coordinate.</param>
    public bool MouseMove(int x, int y)
    {
        if (_uiView != null)
        {
            return _uiView.MouseMove(x, y);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that a mouse button was pressed. The mouse position is specified in
    /// renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Mouse x-coordinate.</param>
    /// <param name="y">Mouse y-coordinate.</param>
    /// <param name="button">Indicates which button was pressed.</param>
    public bool MouseButtonDown(int x, int y, MouseButton button)
    {
        if (_uiView != null)
        {
            return _uiView.MouseButtonDown(x, y, button);
        }

        return false;
    }

    /// Notifies Renderer that a mouse button was released. The mouse position is specified in
    /// renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Mouse x-coordinate.</param>
    /// <param name="y">Mouse y-coordinate.</param>
    /// <param name="button">Indicates which button was released.</param>
    public bool MouseButtonUp(int x, int y, MouseButton button)
    {
        if (_uiView != null)
        {
            return _uiView.MouseButtonUp(x, y, button);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer of a mouse button double click. The mouse position is specified in
    /// renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Mouse x-coordinate.</param>
    /// <param name="y">Mouse y-coordinate.</param>
    /// <param name="button">Indicates which button was pressed.</param>
    public bool MouseDoubleClick(int x, int y, MouseButton button)
    {
        if (_uiView != null)
        {
            return _uiView.MouseDoubleClick(x, y, button);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that mouse wheel was rotated. The mouse position is specified in
    /// renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Mouse x-coordinate.</param>
    /// <param name="y">Mouse y-coordinate.</param>
    /// <param name="wheelRotation">Indicates the amount mouse wheel has changed.</param>
    public bool MouseWheel(int x, int y, int wheelRotation)
    {
        if (_uiView != null)
        {
            return _uiView.MouseWheel(x, y, wheelRotation);
        }

        return false;
    }
    #endregion

    #region Touch input events
    /// <summary>
    /// Notifies Renderer that a finger is moving on the screen. The finger position is
    /// specified in renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Finger x-coordinate.</param>
    /// <param name="y">Finger y-coordinate.</param>
    /// <param name="touchId">Finger identifier.</param>
    public bool TouchMove(int x, int y, uint touchId)
    {
        if (_uiView != null)
        {
            return _uiView.TouchMove(x, y, touchId);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that a finger touches the screen. The finger position is
    /// specified in renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Finger x-coordinate.</param>
    /// <param name="y">Finger y-coordinate.</param>
    /// <param name="touchId">Finger identifier.</param>
    public bool TouchDown(int x, int y, uint touchId)
    {
        if (_uiView != null)
        {
            return _uiView.TouchDown(x, y, touchId);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that a finger is raised off the screen. The finger position is
    /// specified in renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Finger x-coordinate.</param>
    /// <param name="y">Finger y-coordinate.</param>
    /// <param name="touchId">Finger identifier.</param>
    public bool TouchUp(int x, int y, uint touchId)
    {
        if (_uiView != null)
        {
            return _uiView.TouchUp(x, y, touchId);
        }

        return false;
    }
    #endregion

    /// <summary>
    /// Loads the user interface specified in the XAML property
    /// </summary>
    public void LoadXaml(bool force)
    {
        if (force)
        {
            DestroyView();
        }

        if (_xaml != null && _uiView == null)
        {
            object obj = _xaml.Load();

            if (obj != null)
            {
                FrameworkElement content = obj as FrameworkElement;

                if (content != null)
                {
                    CreateView(content);
                }
                else
                {
                    Debug.LogError($"{_xaml.uri}: Root type '{obj.GetType().Name}' does not inherit from 'FrameworkElement'");
                }
            }
        }
    }

    /// <summary>
    /// Called once when component is attached to GameObject for the first time
    /// </summary>
    void Reset()
    {
        _isPPAAEnabled = true;
        _tessellationMaxPixelError = Noesis.TessellationMaxPixelError.MediumQuality.Error;
        _renderFlags = 0;
        _dpiScale = true;
        _continuousRendering = gameObject.TryGetComponent(out Camera _);
        _enableExternalUpdate = false;
        _enableKeyboard = true;
        _enableMouse = true;
        _enableTouch = true;
        _enableActions = false;
        _emulateTouch = false;
        _useRealTimeClock = false;
        _clearStencil = false;
        _actionsRepeatDelay = 0.5f;
        _actionsRepeatRate = 0.1f;
    }

    void Start()
    {
        // https://forum.unity.com/threads/gc-collect-in-guiutility-begingui.642229/
        // Avoid OnGUI GC Allocations
        useGUILayout = false;

      #if !ENABLE_INPUT_SYSTEM
        if (_enableActions)
        {
            Debug.LogWarning("Actions enabled requires 'Active Input Handling' set to 'New' or 'Both' in Player Settings");
            _enableActions = false;
        }
      #endif
    }

    private CommandBuffer _commands;

    private void EnsureCommandBuffer()
    {
        if (_commands == null)
        {
            _commands = new CommandBuffer();
        }
    }

    void Awake()
    {
        EnsureCommandBuffer();
    }

    private Camera _camera;

    void OnEnable()
    {
        EnsureCommandBuffer();
        TryGetComponent<Camera>(out _camera);

        LoadXaml(false);

        Camera.onPreRender += PreRender;
    }

    void OnDisable()
    {
        Camera.onPreRender -= PreRender;
    }

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

    void OnDestroy()
    {
        DestroyView();
    }

#if ENABLE_UGUI_PACKAGE
    UnityEngine.EventSystems.PointerEventData _pointerData;
#endif

    private Vector2 ProjectPointer(float x, float y)
    {
        if (_camera != null)
        {
            return new Vector2(x, Screen.height - y);
        }
        else if (_texture != null)
        {
            // Project using texture coordinates

          #if ENABLE_UGUI_PACKAGE
            // First try with Unity UI RawImage objects
            UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;

            if (eventSystem != null && eventSystem.IsPointerOverGameObject())
            {
                Vector2 pos = new Vector2(x, y);

                if (_pointerData == null)
                {
                    _pointerData = new UnityEngine.EventSystems.PointerEventData(eventSystem)
                    {
                        pointerId = 0,
                        position = pos
                    };
                }
                else
                {
                    _pointerData.Reset();
                }

                _pointerData.delta = pos - _pointerData.position;
                _pointerData.position = pos;

                if (TryGetComponent(out RectTransform rect))
                {
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,
                        _pointerData.position, _pointerData.pressEventCamera, out pos))
                    {
                        Vector2 pivot = new Vector2(
                            rect.pivot.x * rect.rect.width,
                            rect.pivot.y * rect.rect.height);

                        float texCoordX = (pos.x + pivot.x) / rect.rect.width;
                        float texCoordY = (pos.y + pivot.y) / rect.rect.height;

                        float localX = _texture.width * texCoordX;
                        float localY = _texture.height * (1.0f - texCoordY);
                        return new Vector2(localX, localY);
                    }
                }
            }
          #endif

            // NOTE: A MeshCollider must be attached to the target to obtain valid
            // texture coordinates, otherwise Hit Testing won't work

            Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    float localX = _texture.width * hit.textureCoord.x;
                    float localY = _texture.height * (1.0f - hit.textureCoord.y);
                    return new Vector2(localX, localY);
                }
            }

            return new Vector2(-1, -1);
        }

        return Vector2.zero;
    }

    private Vector3 _mousePos;
    private int _activeDisplay = 0;

    private static bool HasMouse()
    {
        return Input.mousePresent;
    }

    private static bool IsCursorVisible()
    {
        return UnityEngine.Cursor.visible && UnityEngine.Cursor.lockState != CursorLockMode.Locked;
    }

    private Vector3 MousePosition()
    {
        Vector3 mousePosition = Input.mousePosition;

        Vector3 p = Display.RelativeMouseAt(mousePosition);

        if (p == Vector3.zero)
        {
            return mousePosition;
        }

        _activeDisplay = (int)p.z;
        return p;
    }

    private void UpdateMouse()
    {
        if (HasMouse() && IsCursorVisible())
        {
            Vector3 mousePos = MousePosition();

            // mouse move
            if ((_camera == null || _activeDisplay == _camera.targetDisplay) && _mousePos != mousePos)
            {
                _mousePos = mousePos;

                Vector2 mouse = ProjectPointer(_mousePos.x, _mousePos.y);
                _uiView.MouseMove((int)mouse.x, (int)mouse.y);
            }
        }
    }

    private void UpdateTouch()
    {
        for (int i = 0; i < Input.touchCount; i++) 
        {
            Touch touch = Input.GetTouch(i);
            Vector2 pos = ProjectPointer(touch.position.x, touch.position.y);
            TouchPhase phase = touch.phase;

            if (phase == TouchPhase.Began)
            {
                _uiView.TouchDown((int)pos.x, (int)pos.y, (uint)touch.fingerId);
            }
            else if (phase == TouchPhase.Moved || phase == TouchPhase.Stationary)
            {
                _uiView.TouchMove((int)pos.x, (int)pos.y, (uint)touch.fingerId);
            }
            else
            {
                _uiView.TouchUp((int)pos.x, (int)pos.y, (uint)touch.fingerId);
            }
        }
    }

    private void UpdateInputs(float t)
    {
        if (_enableMouse)
        {
            UpdateMouse();
        }

        if (_enableTouch)
        {
            UpdateTouch();
        }
    }

    private int _viewSizeX;
    private int _viewSizeY;
    private float _viewScale;
    private float _viewStereoScale = 1.0f;

    private void UpdateSize()
    {
        int sizeX = 0;
        int sizeY = 0;

        if (_camera != null)
        {
            sizeX = _camera.pixelWidth;
            sizeY = _camera.pixelHeight;
        }
        else if (_texture != null)
        {
            sizeX = _texture.width;
            sizeY = _texture.height;
        }

        if (sizeX != _viewSizeX || sizeY != _viewSizeY)
        {
            _uiView.SetSize(sizeX, sizeY);
            _viewSizeX = sizeX;
            _viewSizeY = sizeY;
        }

        float scale = (!WorldSpace && _dpiScale && Screen.dpi > 0.0f) ? Screen.dpi / 96.0f : 1.0f;

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

    private bool _visible = true;

    void LateUpdate()
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

    private static Matrix4 CameraStereoMatrix(Camera camera, Camera.StereoscopicEye eye)
    {
        Matrix4x4 viewMatrix = camera.GetStereoViewMatrix(eye);
        Matrix4x4 projectionMatrix = camera.GetStereoProjectionMatrix(eye);

        return NoesisMatrix(viewMatrix, projectionMatrix, camera.pixelWidth, camera.pixelHeight);
    }

    private static Matrix4 CameraActiveStereoMatrix(Camera camera)
    {
        Camera.StereoscopicEye eye;

        if (camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
        {
            eye = Camera.StereoscopicEye.Left;
        }
        else
        {
            Debug.Assert(camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right);
            eye = Camera.StereoscopicEye.Right;
        }

        return CameraStereoMatrix(camera, eye);
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
            UpdateInputs(t);

            NoesisUnity.IME.Update(_uiView);
            NoesisUnity.TouchKeyboard.Update();

            Noesis_UnityUpdate();
            _needsRendering = _uiView.Update(_useRealTimeClock ? t : Time.time);

            Profiling.UpdateSampler.End();

            if (_needsRendering)
            {
                _commands.name = Profiling.UpdateRenderTree;
                NoesisRenderer.UpdateRenderTree(_uiView, _commands);

                Graphics.ExecuteCommandBuffer(_commands);
                _commands.Clear();
            }

            if (_camera == null && _texture != null)
            {
                if (_continuousRendering || _needsRendering)
                {
                    _commands.name = Profiling.RenderTexture;
                    NoesisRenderer.RenderOffscreen(_uiView, _commands, false);
                    _commands.SetRenderTarget(_texture, LoadAction.DontCare, StoreAction.Store, LoadAction.DontCare, StoreAction.DontCare);
                    _commands.ClearRenderTarget(true, true, UnityEngine.Color.clear, 0.0f);
                    NoesisRenderer.RenderOnscreen(_uiView, !IsGL(), _commands, false, _clearStencil);

                    Graphics.ExecuteCommandBuffer(_commands);
                    _commands.Clear();

                    GL.InvalidateState();
                    _texture.DiscardContents(false, true);
                }
            }
        }
    }

    void OnBecameInvisible()
    {
        if (_uiView != null && _texture != null)
        {
            _visible = false;
        }
    }

    void OnBecameVisible()
    {
        if (_uiView != null && _texture != null)
        {
            _visible = true;
        }
    }

    private void RenderOffscreen(CommandBuffer commands)
    {
        NoesisRenderer.RenderOffscreen(_uiView, _commands, false);
    }

    private void RenderOnScreen(bool flipY, CommandBuffer commands)
    {
        if (_camera != null && _camera.stereoEnabled)
        {
          #if (ENABLE_VR_MODULE && ENABLE_VR)
            if (XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.MultiPass)
            {
                NoesisRenderer.RenderOnscreen(_uiView, CameraActiveStereoMatrix(_camera),
                    flipY, commands, false, _clearStencil);
            }
            else
            {
                NoesisRenderer.RenderOnscreen(_uiView, CameraMatrix(_camera),
                    CameraStereoMatrix(_camera, Camera.StereoscopicEye.Left),
                    CameraStereoMatrix(_camera, Camera.StereoscopicEye.Right),
                    flipY, commands, false, _clearStencil);
            }
          #endif
        }
        else
        {
            NoesisRenderer.RenderOnscreen(_uiView, flipY, commands, false, _clearStencil);
        }
    }

    private bool _updatePending = true;

    private void PreRender(Camera cam)
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
                    _commands.name = Profiling.RenderOffScreen;
                    RenderOffscreen(_commands);

                    Graphics.ExecuteCommandBuffer(_commands);
                    _commands.Clear();

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

    private void OnPostRender()
    {
        if (_uiView != null && _visible)
        {
            _commands.name = Profiling.RenderOnScreen;
            RenderOnScreen(FlipRender(), _commands);

            Graphics.ExecuteCommandBuffer(_commands);
            _commands.Clear();

            GL.InvalidateState();
            _updatePending = true;
        }
    }

    private EventModifiers _modifiers = 0;

    private void ProcessModifierKey(EventModifiers modifiers, EventModifiers delta, EventModifiers flag, Key key)
    {
        if ((delta & flag) > 0)
        {
            if ((modifiers & flag) > 0)
            {
                _uiView.KeyDown(key);
            }
            else
            {
                _uiView.KeyUp(key);
            }
        }
    }

    private bool HitTest(float x, float y)
    {
        Visual root = (Visual)VisualTreeHelper.GetRoot(_uiView.Content);
        Point p = root.PointFromScreen(new Point(x, y));

        // Comment this define at the top of the file if you want to use the old behavior
      #if IGNORE_ISHITTESTVISIBLE_FALSE_ELEMENTS
        return Noesis_UnityHitTest(BaseComponent.getCPtr(root), ref p);
      #else
        return VisualTreeHelper.HitTest(root, p).VisualHit != null;
      #endif
    }

#if !UNITY_EDITOR && UNITY_STANDALONE_OSX
    private static int lastFrame;
    private static Noesis.Key lastKeyDown;
#endif

    private bool MouseEmulated()
    { 
        return Input.simulateMouseWithTouches && Input.touchCount > 0;
    }

    private void ProcessEvent(Event ev, bool enableKeyboard, bool enableMouse)
    {
        // Process keyboard modifiers
        if (enableKeyboard)
        {
            EventModifiers delta = ev.modifiers ^ _modifiers;
            if (delta > 0)
            {
                _modifiers = ev.modifiers;

                ProcessModifierKey(ev.modifiers, delta, EventModifiers.Shift, Key.LeftShift);
                ProcessModifierKey(ev.modifiers, delta, EventModifiers.Control, Key.LeftCtrl);
                ProcessModifierKey(ev.modifiers, delta, EventModifiers.Command, Key.LeftCtrl);
                ProcessModifierKey(ev.modifiers, delta, EventModifiers.Alt, Key.LeftAlt);
            }
        }

        switch (ev.type)
        {
            case EventType.MouseDown:
            {
                if (enableMouse && IsCursorVisible())
                {
                    Vector2 mouse = ProjectPointer(ev.mousePosition.x, Screen.height - ev.mousePosition.y);

                    if (HitTest(mouse.x, mouse.y))
                    {
                        ev.Use();
                    }

                    if (!MouseEmulated())
                    {
                        if (ev.clickCount == 1)
                        {
                            _uiView.MouseButtonDown((int)mouse.x, (int)mouse.y, (MouseButton)ev.button);
                        }
                        else
                        {
                            _uiView.MouseDoubleClick((int)mouse.x, (int)mouse.y, (MouseButton)ev.button);
                        }
                    }
                }
                break;
            }
            case EventType.MouseUp:
            {
                if (enableMouse && IsCursorVisible())
                {
                    Vector2 mouse = ProjectPointer(ev.mousePosition.x, Screen.height - ev.mousePosition.y);

                    if (HitTest(mouse.x, mouse.y))
                    {
                        ev.Use();
                    }

                    if (!MouseEmulated())
                    {
                        _uiView.MouseButtonUp((int)mouse.x, (int)mouse.y, (MouseButton)ev.button);
                    }
                }
                break;
            }
            case EventType.ScrollWheel:
            {
                if (enableMouse && IsCursorVisible())
                {
                    Vector2 mouse = ProjectPointer(ev.mousePosition.x, Screen.height - ev.mousePosition.y);

                    if (ev.delta.y != 0.0f)
                    {
                        _uiView.MouseWheel((int)mouse.x, (int)mouse.y, -(int)(ev.delta.y * 40.0f));
                    }

                    if (ev.delta.x != 0.0f)
                    {
                        _uiView.MouseHWheel((int)mouse.x, (int)mouse.y, (int)(ev.delta.x * 40.0f));
                    }
                }
                break;
            }
            case EventType.KeyDown:
            {
                if (enableKeyboard)
                {
                    // Don't process key when IME composition is being used
                    if (ev.keyCode != KeyCode.None && NoesisUnity.IME.compositionString == "")
                    {
                        Key noesisKeyCode = NoesisKeyCodes.Convert(ev.keyCode);
                        if (noesisKeyCode != Key.None)
                        {
                          #if !UNITY_EDITOR && UNITY_STANDALONE_OSX
                            // In OSX Standalone, CMD + key always sends two KeyDown events for the key.
                            // This seems to be a bug in Unity. 
                            if (!ev.command || lastFrame != Time.frameCount || lastKeyDown != noesisKeyCode)
                            {
                                lastFrame = Time.frameCount;
                                lastKeyDown = noesisKeyCode;
                          #endif
                                _uiView.KeyDown(noesisKeyCode);
                          #if !UNITY_EDITOR && UNITY_STANDALONE_OSX
                            }
                          #endif
                        }
                    }

                    if (ev.character != 0)
                    {
                        // Filter out character events when CTRL is down
                        bool isControl = (_modifiers & EventModifiers.Control) != 0 || (_modifiers & EventModifiers.Command) != 0;
                        bool isAlt = (_modifiers & EventModifiers.Alt) != 0;
                        bool filter = isControl && !isAlt;

                        if (!filter)
                        {
                          #if !UNITY_EDITOR && UNITY_STANDALONE_LINUX
                            // It seems that linux is sending KeySyms instead of Unicode points
                            // https://github.com/substack/node-keysym/blob/master/data/keysyms.txt
                            ev.character = NoesisKeyCodes.KeySymToUnicode(ev.character);
                          #endif
                            _uiView.Char((uint)ev.character);
                        }
                    }

                }
                break;
            }
            case EventType.KeyUp:
            {
                // Don't process key when IME composition is being used
                if (enableKeyboard)
                {
                    if (ev.keyCode != KeyCode.None && NoesisUnity.IME.compositionString == "")
                    {
                        Key noesisKeyCode = NoesisKeyCodes.Convert(ev.keyCode);
                        if (noesisKeyCode != Key.None)
                        {
                            _uiView.KeyUp(noesisKeyCode);
                        }
                    }
                }
                break;
            }
        }
    }

    void OnGUI()
    {
        if (_uiView != null && (_camera == null || _activeDisplay == _camera.targetDisplay))
        {
            if (_camera)
            {
                UnityEngine.GUI.depth = -(int)_camera.depth;
            }

            ProcessEvent(Event.current, _enableKeyboard, _enableMouse);
        }
    }

    void OnApplicationFocus(bool focused)
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

    private void CreateView(FrameworkElement content)
    {
        if (_uiView == null)
        {
            // Send settings for the internal device, created by the first view
            NoesisRenderer.SetRenderSettings();

            _viewSizeX = 0;
            _viewSizeY = 0;
            _viewScale = 1.0f;

            _uiView = new View(content);
            _uiView.SetTessellationMaxPixelError(_tessellationMaxPixelError);
            _uiView.SetEmulateTouch(_emulateTouch);
            _uiView.SetFlags(_renderFlags);

            _commands.name = Profiling.RegisterView;
            NoesisRenderer.RegisterView(_uiView, _commands);
            Graphics.ExecuteCommandBuffer(_commands);
            _commands.Clear();

          #if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += DestroyView;
          #endif
        }
    }

    private void DestroyView()
    {
        if (_uiView != null)
        {
            _commands.name = Profiling.UnregisterView;
            NoesisRenderer.UnregisterView(_uiView, _commands);
            Graphics.ExecuteCommandBuffer(_commands);
            _commands.Clear();

            _uiView = null;
        }
    }

    public void OnBeforeSerialize() {}

    public void OnAfterDeserialize()
    {
        // (3.0) PPAA flag is now in view render flags 
        if (_isPPAAEnabled)
        {
            _renderFlags |= RenderFlags.PPAA;
            _isPPAAEnabled = false;
        }
    }

    private View _uiView;
    private bool _needsRendering = false;
    private float _stereoScale = 1.0f;

    [SerializeField] private NoesisXaml _xaml;
    [SerializeField] private RenderTexture _texture;

    [SerializeField] private bool _isPPAAEnabled = true;
    [SerializeField] private float _tessellationMaxPixelError = Noesis.TessellationMaxPixelError.MediumQuality.Error;
    [SerializeField] private RenderFlags _renderFlags = 0;
    [SerializeField] private bool _dpiScale = true;
    [SerializeField] private bool _continuousRendering = true;
    [SerializeField] private bool _enableExternalUpdate = false;
    [SerializeField] private bool _enableKeyboard = true;
    [SerializeField] private bool _enableMouse = true;
    [SerializeField] private bool _enableTouch = true;
    [UnityEngine.Serialization.FormerlySerializedAs("_enableGamepad")]
    [SerializeField] private bool _enableActions = false;
    [SerializeField] private bool _emulateTouch = false;
    [SerializeField] private bool _useRealTimeClock = false;
    [SerializeField] private bool _clearStencil = false;

    [UnityEngine.Serialization.FormerlySerializedAs("_gamepadRepeatDelay")]
    [SerializeField] private float _actionsRepeatDelay = 0.5f;

    [UnityEngine.Serialization.FormerlySerializedAs("_gamepadRepeatRate")]
    [SerializeField] private float _actionsRepeatRate = 0.1f;

    [SerializeField] private UnityEngine.Transform _xrTrackingOrigin;

    [DllImport(Library.Name)]
    private static extern void Noesis_UnityUpdate();

    [DllImport(Library.Name)]
    private static extern bool Noesis_UnityHitTest(HandleRef root, ref Point point);
}