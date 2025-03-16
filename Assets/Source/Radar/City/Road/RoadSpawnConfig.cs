using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Math;
using Beast.Radar.Navigation;
using UnityEngine;

namespace Beast.Radar.City
{
    public class RoadSpawnConfig : ScriptableConfig
    {
        public CellMovementCostConfig buildCost;
        public ERoadSpawnPriority spawnPriority;
        public int maxSegmentLength;
        public Texture2D noiseTexture;
        public FloatMinMax noiseStrength;
        public FloatMinMax noiseRange;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}