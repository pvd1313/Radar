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

        private Texture2D _vulkanTexture2D;
        private byte[] _vulkanByteArray;

        private readonly RenderTexture _unityRenderTexture;
        
        private ImageBrush _brush;

        private readonly bool _isVulkan;

        public RenderTextureBrush(RenderTexture unityRenderTexture)
        {
            _unityRenderTexture = unityRenderTexture;
            _brush = new ImageBrush();
            _isVulkan = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan;

            if (_isVulkan)
            {
                _vulkanTexture2D = BGraphic.CreateTexture2D(this, new Texture2DParameters
                {
                    isLinear = true,
                    createInitialized = false,
                    filterMode = FilterMode.Point,
                    format = UnityEngine.TextureFormat.RGB24,
                    mipChain = false,
                    size = _unityRenderTexture.Size(),
                    wrapMode = TextureWrapMode.Clamp
                });

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
            if (!_isVulkan)
            {
                return;
            }

            Profiler.BeginSample("RenderTextureBrush.Blit()");
            
            Profiler.BeginSample("BlitRenderTextureTo2D");
            BGraphic.BlitRenderTextureTo2D(_unityRenderTexture, _vulkanTexture2D);
            Profiler.EndSample();

            Span<byte> sourceSpan = _vulkanTexture2D.GetRawTextureData<byte>().AsSpan();
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
            BGraphic.TryDispose(ref _vulkanTexture2D);
            _vulkanByteArray = null;

            _brush.ImageSource?.Dispose();
            _brush.ImageSource = null;
            
            _brush.Dispose();
            _brush = null;
        }
    }
}