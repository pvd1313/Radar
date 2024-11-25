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
    }
}