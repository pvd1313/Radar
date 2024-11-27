using System;
using Beast.Claw.Graphic;
using Noesis;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Beast.Radar.UI
{
    public class RenderTextureBrush
    {
        public ImageBrush Brush => _brush;
        
        private readonly RenderTexture _unityRenderTexture;
        private readonly bool _isVulkan;
        
        private byte[] _vulkanByteArray;
        private ImageBrush _brush;
        private bool _hasActiveGPURequest;

        public RenderTextureBrush(RenderTexture unityRenderTexture)
        {
            _unityRenderTexture = unityRenderTexture;
            _brush = new ImageBrush();
            _isVulkan = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan;

            if (_isVulkan)
            {
                int byteSize = _unityRenderTexture.width * _unityRenderTexture.height * 3;

                _vulkanByteArray = new byte[byteSize];
            }
            else
            {
                _brush.ImageSource = new TextureSource(_unityRenderTexture);
            }
        }

        public void Blit()
        {
            if (!_isVulkan || _hasActiveGPURequest)
            {
                return;
            }
            
            Profiler.BeginSample("AsyncGPUReadback.Request");

            _hasActiveGPURequest = true;

            AsyncGPUReadback.Request(_unityRenderTexture, 0, UnityEngine.TextureFormat.RGB24, request =>
            {
                if (!_hasActiveGPURequest)
                {
                    return;
                }

                try
                {
                    _vulkanBlitRenderTexture(request.GetData<byte>().AsSpan());
                }
                finally
                {
                    _hasActiveGPURequest = false;
                }
            });
            
            Profiler.EndSample();
        }

        private void _vulkanBlitRenderTexture(Span<byte> sourceSpan)
        {
            Profiler.BeginSample("RenderTextureBrush._vulkanBlitRenderTexture()");
            
            Span<byte> targetSpan = _vulkanByteArray.AsSpan();
            
            Profiler.BeginSample("Copy bytes & Reverse Y");
            int2 textureSize = _unityRenderTexture.Size();
            int strideSize = textureSize.x * 3;

            for (int y = 0; y < textureSize.y; y++)
            {
                int sourceIndex = (textureSize.y - y - 1) * strideSize;
                int targetIndex = y * strideSize;

                Span<byte> from = sourceSpan.Slice(sourceIndex, strideSize);
                Span<byte> to = targetSpan.Slice(targetIndex, strideSize);

                from.CopyTo(to);
            }
            Profiler.EndSample();
            
            Profiler.BeginSample("Create BitmapSource");
            _brush.ImageSource?.Dispose();
            _brush.ImageSource = BitmapSource.Create(
                _unityRenderTexture.width, 
                _unityRenderTexture.height, 
                1, 
                1,
                _vulkanByteArray, 
                3 * _unityRenderTexture.width, 
                BitmapSource.Format.RGB8);
            Profiler.EndSample();
            
            Profiler.EndSample();
        }
        
        public void Dispose()
        {
            _vulkanByteArray = null;

            _brush.ImageSource?.Dispose();
            _brush.ImageSource = null;

            _hasActiveGPURequest = false;
            
            _brush.Dispose();
            _brush = null;
        }
    }
}