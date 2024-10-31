using System;
using Beast.Claw;
using Beast.Claw.Common;
using Beast.Claw.Jobs;
using Beast.Claw.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Beast.Radar.Terrain
{
    public struct TextureWithBlocks
    {
        private NativeArray<byte> _texture;
        private bool _isCreated;
        private int _blockSize;
        private int _blockCountX;

        public byte GetPixelClamped(int2 position)
        {
            if (!_isCreated)
            {
                throw new InvalidOperationException(nameof(TextureWithBlocks) + " was disposed or was not initialized");
            }
            
            position.x = BMath.Clamp(position.x, 0, _blockSize * _blockCountX - 1);
            position.y = BMath.Clamp(position.y, 0, _blockSize * _blockCountX - 1);

            int2 blockXY = position / _blockSize;
            int blockSizeSqr = _blockSize * _blockSize;
            int blockOffset = blockXY.y * _blockCountX * blockSizeSqr + blockXY.x * blockSizeSqr;
            int2 innerXY = position % _blockSize;
            int innerOffset = innerXY.y * _blockSize + innerXY.x;

            return _texture[blockOffset + innerOffset];
        }

        public static TextureWithBlocks FromBiomeTexture(Texture2D biomeTexture, int blockSize)
        {
            biomeTexture.ValidateNotNullThrowException(nameof(biomeTexture));
            
            if (Build.IS_DEVELOPMENT && blockSize <= 0 || !BMath.IsPowerOf2(blockSize))
            {
                throw new ArgumentException("Invalid block size: " + blockSize);
            }

            int blockCountX = biomeTexture.width / blockSize;
            
            Profiler.BeginSample("AllocateNativeArray");
            NativeArray<byte> textureWithBlocks = NativeMemory.CreatePermArray<byte>(biomeTexture.width * biomeTexture.width);
            Profiler.EndSample();
            
            TextureToBlocksJob job = new()
            {
                inputTexture = biomeTexture.GetRawTextureData<short>(),
                inputTextureSize = biomeTexture.width,
                outputBlockSize = blockSize,
                outputTextureBlocks = textureWithBlocks,
            };
            
            Profiler.BeginSample("TextureToBlocksJob.Execute()");
            job.Schedule(blockCountX * blockCountX, 32).Complete();
            Profiler.EndSample();

            return new TextureWithBlocks
            {
                _isCreated = true,
                _texture = job.outputTextureBlocks,
                _blockSize = blockSize,
                _blockCountX = blockCountX
            };
        }

        public void Dispose()
        {
            if (!_isCreated)
            {
                return;
            }
            
            _texture.Dispose();
            _isCreated = false;
        }
    }
}