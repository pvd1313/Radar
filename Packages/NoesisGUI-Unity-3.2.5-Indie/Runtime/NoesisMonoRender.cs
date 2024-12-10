using UnityEngine;
using Noesis;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

public class NoesisMonoRender : MonoBehaviour
{
    [DllImport(Library.Name)]
    private static extern void Noesis_UnityUpdate();

    private static class Profiling
    {
        public static readonly CustomSampler UpdateSampler = CustomSampler.Create("Noesis.Update");
        public static readonly string RegisterView = "Noesis.RegisterView";
        public static readonly string UnregisterView = "Noesis.UnregisterView";
        public static readonly string UpdateRenderTree = "Noesis.UpdateRenderTree";
        public static readonly string RenderOffScreen = "Noesis.RenderOffscreen";
        public static readonly string RenderOnScreen = "Noesis.RenderOnscreen";
        public static readonly string RenderTexture = "Noesis.RenderTexture";
    }

    [SerializeField] private float _tessellationMaxPixelError = TessellationMaxPixelError.MediumQuality.Error;
    [SerializeField] private RenderFlags _renderFlags = 0;
    [SerializeField] private bool _useRealTimeClock;
    [SerializeField] private bool _clearStencil;

    private NoesisView _uiView;
    private bool _needsRendering;

    private bool _updatePending = true;
    private int _viewSizeX;
    private int _viewSizeY;
    private float _viewScale;
    private Camera _camera;
    private CommandBuffer _commandBuffer;

    public NoesisView Initialize(Camera unityCamera, FrameworkElement element)
    {
        _commandBuffer = new CommandBuffer();
        _camera = unityCamera;

        Camera.onPreRender += _onPreRender;
        Camera.onPostRender += _onPostRender;

        _createView(element);

        return _uiView;
    }

    private void OnDestroy()
    {
        _destroyView();
    }

    private void LateUpdate()
    {
        Profiling.UpdateSampler.Begin();

        float t = Time.realtimeSinceStartup;

        _fetchSize();

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

    private void _fetchSize()
    {
        int sizeX = _camera.pixelWidth;
        int sizeY = _camera.pixelHeight;

        if (sizeX != _viewSizeX || sizeY != _viewSizeY)
        {
            _uiView.SetSize(sizeX, sizeY);
            _viewSizeX = sizeX;
            _viewSizeY = sizeY;
        }


        float scale;

        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            float diagonalPixels = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
            
            scale = diagonalPixels * 0.001f;
        }
        else
        {
            // float scale = (!WorldSpace && _dpiScale && Screen.dpi > 0.0f) ? Screen.dpi / 96.0f : 1.0f;
            scale = (Screen.dpi > 0.0f) ? Screen.dpi / 144.0f : 1.0f;
        }
        

        if (!Mathf.Approximately(scale, _viewScale))
        {
            _uiView.SetScale(scale);
            _viewScale = scale;
        }
    }

    private void _onPreRender(Camera cam)
    {
        // In case there are several cameras rendering to the same texture (Camera Stacking),
        // the camera rendered first (less depth) is the one that must apply our offscreen phase
        // to avoid inefficient Load/Store in Tiled architectures
        if (_updatePending && cam.targetTexture == _camera.targetTexture && cam.depth <= _camera.depth)
        {
            if (_uiView != null)
            {
                _commandBuffer.name = Profiling.RenderOffScreen;
                NoesisRenderer.RenderOffscreen(_uiView, _commandBuffer, false);

                Graphics.ExecuteCommandBuffer(_commandBuffer);
                _commandBuffer.Clear();

                GL.InvalidateState();
                _forceRestoreCameraRenderTarget();
            }

            _updatePending = false;
        }
    }

    private void _onPostRender(Camera cam)
    {
        _commandBuffer.name = Profiling.RenderOnScreen;

        NoesisRenderer.RenderOnscreen(_uiView, false, _commandBuffer, false, _clearStencil);

        Graphics.ExecuteCommandBuffer(_commandBuffer);

        _commandBuffer.Clear();

        GL.InvalidateState();

        _updatePending = true;
    }

    private static void _forceRestoreCameraRenderTarget()
    {
        // Unity should automatically restore the render target but sometimes (for example a scene without lights)
        // it doesn't. We use this hack to flush the active render target and force unity to set the camera RT afterward
        RenderTexture surface = RenderTexture.GetTemporary(1, 1);
        Graphics.SetRenderTarget(surface);
        RenderTexture.ReleaseTemporary(surface);
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
            _uiView.SetEmulateTouch(false);
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
}