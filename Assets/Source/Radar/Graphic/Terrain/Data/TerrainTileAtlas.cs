using System;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic.Terrain
{
    [Serializable]
    public class TerrainTileAtlas
    {
        public Texture2D atlasTexture;
        public float renderYCrop;
        public int2 tileCount;
        public float2 tileBlendCenter;
        public float astlasPPU;
    }
}