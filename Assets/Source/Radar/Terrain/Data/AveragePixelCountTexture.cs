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
    public struct AveragePixelCountTexture<TValue> : IDisposable where TValue : unmanaged
    {
        public int cellSize;
        public int cellCountX;
        public NativeArray<TValue> texture;

        public TValue GetPixelClamped(int2 worldPosition)
        {
            int2 cellPosition = BMath.FloorToInt(worldPosition.Div(cellSize));
            cellPosition.x = BMath.Clamp(cellPosition.x, 0, cellCountX - 1);
            cellPosition.y = BMath.Clamp(cellPosition.y, 0, cellCountX - 1);

            return texture[cellPosition.y * cellCountX + cellPosition.x];
        }

        public TValue GetPixel(int2 worldPosition)
        {
            int2 cellPosition = BMath.FloorToInt(worldPosition.Div(cellSize));
            
            return texture[cellPosition.y * cellCountX + cellPosition.x];
        }

        public int2 FloorToCellWorldPosition(int2 worldPosition)
        {
            return BMath.FloorToInt(worldPosition.Div(cellSize)) * cellSize;
        }
        
        public int2 WorldPositionToCell(float2 worldPosition)
        {
            return BMath.FloorToInt(worldPosition / cellSize);
        }
        
        public int2 CellToWorldPoint(int2 cell)
        {
            return BMath.FloorToInt(cell.Mul(cellSize));
        }

        public static AveragePixelCountTexture<EBiomeName> FromTextureWithBlocks(TextureWithBlocks<EBiomeName> texture, int cellSize)
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
                outputTexture = Native.PermArray<EBiomeName>(cellCountX * cellCountX)
            };
            Profiler.EndSample();
            
            Profiler.BeginSample("CreateAveragePixelCountTextureJob.Execute()");
            job.Schedule(texture.blockCountX * texture.blockCountX, 16).Complete();
            Profiler.EndSample();

            return new AveragePixelCountTexture<EBiomeName>
            {
                cellSize = cellSize,
                texture = job.outputTexture,
                cellCountX = cellCountX
            };
        }

        public void Dispose()
        {
            if (!texture.IsCreated)
            {
                return;
            }
            
            texture.Dispose();
        }
    }
}