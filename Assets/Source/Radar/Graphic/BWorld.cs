using System.Runtime.CompilerServices;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Math;
using Unity.Mathematics;

namespace Beast.Radar.Graphic
{
    [ConstantGroup, StaticUtility]
    public static class BWorld
    {
        public const int TILES_PER_CELL = 100;
        
        public const float DESTINATION_REACHED_DISTANCE = 1f / TILES_PER_CELL * 0.5f;

        /// <para>0.131 -> 13</para> 
        /// <para>1.0 -> 100</para>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 WorldToTileIndex(float2 worldPosition)
        {
            return BMath.FloorToInt(worldPosition * TILES_PER_CELL);
        }

        /// <para>13 -> 0.13</para>
        /// <para>100 -> 1.0</para>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 TileIndexToWorld(int2 tileIndex)
        {
            return tileIndex / TILES_PER_CELL;
        }

        public static float2 SnapToTileGridCenter(float2 worldPosition)
        {
            float2 tileSize = 1f / TILES_PER_CELL;

            worldPosition = BMath.Floor(worldPosition * TILES_PER_CELL) / TILES_PER_CELL;

            return worldPosition + tileSize * 0.5f;
        }
    }
}