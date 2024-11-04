using System;
using Beast.Radar.Terrain;
using UnityEngine;

namespace Beast.Radar.Player
{
    [Serializable]
    public struct TileSlowdown
    {
        public EBiomeName biome;
        [Range(0, 1)] public float speedMultiplier;
    }
}