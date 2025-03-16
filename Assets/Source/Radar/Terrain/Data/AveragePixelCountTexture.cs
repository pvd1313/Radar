using System;
using Beast.Claw.Common;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Profiling;

namespace Beast.Radar.Terrain
{
    /// <summary>
    /// This texture contains info of most appeared pixel in NxN block.
    /// </summary>
    public struct AveragePixelCountTexture<TValue> : IDisposable where TValue : unmanaged
    {
        public int cellSize;
        public int cellCountX;
        public NativeArray<TValue> texture;

        public readonly TValue GetPixelClamped(int2 worldPosition)
        {
            int2 cellPosition = BMath.FloorToInt(worldPosition.Div(cellSize));
            cellPosition.x = BMath.Clamp(cellPosition.x, 0, cellCountX - 1);
            cellPosition.y = BMath.Clamp(cellPosition.y, 0, cellCountX - 1);

            return texture[cellPosition.y * cellCountX + cellPosition.x];
        }

        public readonly TValue GetPixel(int2 worldPosition)
        {
            int2 cellPosition = BMath.FloorToInt(worldPosition.Div(cellSize));
            
            return texture[cellPosition.y * cellCountX + cellPosition.x];
        }

        public readonly int2 FloorToCellWorldPosition(int2 worldPosition)
        {
            return BMath.FloorToInt(worldPosition.Div(cellSize)) * cellSize;
        }
        
        public readonly int2 WorldPositionToCell(float2 worldPosition)
        {
            return BMath.FloorToInt(worldPosition / cellSize);
        }
        
        public readonly float2 GetCellCenterWorld(int2 cellIndex8KM)
        {
            return (float2)cellIndex8KM * cellSize + cellSize * 0.5f;
        }

        public static AveragePixelCountTexture<TerrainCell> FromTextureWithBlocks(TextureWithBlocks<TerrainCell> texture, int cellSize)
        {
            if (Build.IS_DEVELOPMENT)
            {
                if ((cellSize <= 1) || (!BMath.IsPowerOf2(cellSize)) || (cellSize > 16))
                {
                    throw new ArgumentException("Invalid cellSize: " + cellSize);
                }
            }

            int cellCountX = texture.blockCountX * (texture.blockSize / cellSize);
            
            Profiler.BeginSample("AllocateCellArray");
            CreateAveragePixelCountTextureJob job = new()
            {
                outputCellSize = cellSize,
                sourceTexture = texture,
                outputTexture = BNative.PermArray<TerrainCell>(cellCountX * cellCountX)
            };
            Profiler.EndSample();
            
            Profiler.BeginSample("CreateAveragePixelCountTextureJob.Execute()");
            job.Schedule(texture.blockCountX * texture.blockCountX, 16).Complete();
            Profiler.EndSample();

            return new AveragePixelCountTexture<TerrainCell>
            {
                cellSize = cellSize,
                texture = job.outputTexture,
                cellCountX = cellCountX
            };
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