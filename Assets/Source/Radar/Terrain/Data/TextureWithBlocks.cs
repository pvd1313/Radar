using System;
using System.Runtime.CompilerServices;
using Beast.Claw;
using Beast.Claw.Common;
using Beast.Claw.Graphic;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Beast.Claw.Validation;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Beast.Radar.Terrain
{
    /// <summary>
    /// This texture's array is packed into NxN blocks to improve CPU reading speed.
    /// </summary>
    public struct TextureWithBlocks<TValue> where TValue : unmanaged
    {
        public NativeArray<TValue> texture;
        public int blockSize;
        public int blockCountX;
        
        public static TextureWithBlocks<EBiomeName> FromBiomeTexture(Texture2D biomeTexture, int blockSize)
        {
            biomeTexture.ValidateNotNullThrowException(nameof(biomeTexture));
            
            if (Build.IS_DEVELOPMENT)
            {
                if (blockSize <= 0 || !BMath.IsPowerOf2(blockSize))
                {
                    throw new ArgumentException("Invalid block size: " + blockSize);
                }
                
                if ((biomeTexture.width != biomeTexture.height || !BMath.IsPowerOf2(biomeTexture.width)))
                {
                    throw new AggregateException($"Invalid texture size: {biomeTexture.Size()}");
                }
            }

            int blockCountX = biomeTexture.width / blockSize;
            
            Profiler.BeginSample("AllocateNativeArray");
            NativeArray<EBiomeName> textureWithBlocks = Native.PermArray<EBiomeName>(biomeTexture.width * biomeTexture.width);
            Profiler.EndSample();
            
            CreateTextureBlocksJob<EBiomeName, BiomeTextureConverter> job = new()
            {
                inputTexture = biomeTexture.GetRawTextureData<short>(),
                inputTextureSize = biomeTexture.width,
                outputBlockSize = blockSize,
                outputTextureBlocks = textureWithBlocks,
            };
            
            Profiler.BeginSample("TextureToBlocksJob.Execute()");
            job.Schedule(blockCountX * blockCountX, 32).Complete();
            Profiler.EndSample();

            return new TextureWithBlocks<EBiomeName>
            {
                texture = job.outputTextureBlocks,
                blockSize = blockSize,
                blockCountX = blockCountX
            };
        }

        public TValue GetPixelClamped(int2 worldPosition)
        {
            texture.ValidateIsCreated();
            
            worldPosition.x = BMath.Clamp(worldPosition.x, 0, blockSize * blockCountX - 1);
            worldPosition.y = BMath.Clamp(worldPosition.y, 0, blockSize * blockCountX - 1);
            
            int blockOffset = GetBlockOffset(worldPosition);
            int innerOffset = GetInnerOffset(worldPosition);

            return texture[blockOffset + innerOffset];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBlockOffset(int2 absolutePosition)
        {
            int2 blockXY = absolutePosition / blockSize;
            
            return (blockXY.y * blockCountX + blockXY.x) * (blockSize * blockSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInnerOffset(int2 absolutePosition)
        {
            int2 innerXY = absolutePosition % blockSize;
            return innerXY.y * blockSize + innerXY.x;
        }

        public void Dispose()
        {
            if (texture.IsCreated)
            {
                texture.Dispose();
            }
        }
    }
}