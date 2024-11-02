using System;
using Beast.Claw.Common;
using Beast.Claw.Jobs;
using Beast.Claw.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Profiling;

namespace Beast.Radar.Terrain
{
    /// <summary>
    /// This texture contains info of most appeared pixel in NxN block.
    /// </summary>
    public struct AveragePixelCountTexture : IDisposable
    {
        public bool isCreated;
        public int cellSize;
        public int cellCountX;
        public NativeArray<byte> texture;

        public byte GetPixelClamped(int2 worldPosition)
        {
            int2 cellPosition = BMath.FloorToInt(worldPosition.Divide(cellSize));
            cellPosition.x = BMath.Clamp(cellPosition.x, 0, cellCountX - 1);
            cellPosition.y = BMath.Clamp(cellPosition.y, 0, cellCountX - 1);

            return texture[cellPosition.y * cellCountX + cellPosition.x];
        }

        public static AveragePixelCountTexture FromTextureWithBlocks(TextureWithBlocks texture, int cellSize)
        {
            if (Build.IS_DEVELOPMENT && (cellSize <= 1 || !BMath.IsPowerOf2(cellSize)) || cellSize > 16)
            {
                throw new ArgumentException("Invalid cellSize: " + cellSize);
            }

            int cellCountX = texture.blockCountX * (texture.blockSize / cellSize);
            
            Profiler.BeginSample("AllocateCellArray");
            CreateAveragePixelCountTextureJob job = new()
            {
                outputCellSize = cellSize,
                sourceTexture = texture,
                outputTexture = NativeMemory.CreatePermArray<byte>(cellCountX * cellCountX)
            };
            Profiler.EndSample();
            
            Profiler.BeginSample("CreateAveragePixelCountTextureJob.Execute()");
            job.Schedule(texture.blockCountX * texture.blockCountX, 16).Complete();
            Profiler.EndSample();

            return new AveragePixelCountTexture
            {
                isCreated = true,
                cellSize = cellSize,
                texture = job.outputTexture,
                cellCountX = cellCountX
            };
        }

        public void Dispose()
        {
            if (!isCreated)
            {
                return;
            }

            isCreated = false;
            texture.Dispose();
        }
    }
}