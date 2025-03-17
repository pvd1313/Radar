using Beast.Claw.Math;
using Beast.Claw.Math.CCL;
using Beast.Radar.Terrain;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Beast.Radar.Navigation
{
    public struct TerrainMotionCostComputer : IMotionCostComputer
    {
        [NativeDisableContainerSafetyRestriction] public AveragePixelCountTexture<TerrainCell> terrain8KMTexture;
        [NativeDisableContainerSafetyRestriction] public TextureWithBlocks<TerrainCell> terrain1KMTexture;
        public bool use8KMTerrainTexture;
        
        public ByteArray16 cellBiomeCost;
        public ByteArray16 cellFeatureCost;

        public bool useNoiseTexture;
        [NativeDisableContainerSafetyRestriction] public NativeTexture<byte> noiseTexture;
        public FloatMinMax noiseStrength;
        public FloatMinMax noiseRange;
        
        public float ComputeCost(MotionCostInput input)
        {
            float cost0 = _getCellCost(input.motionStartPoint, input);
            float cost1 = _getCellCost(input.motionEndPoint, input);

            float distance = BMath.Distance(input.motionStartPoint, input.motionEndPoint);

            return (cost0 + cost1) * distance * 0.5f;
        }

        private float _getCellCost(int2 cellIndex, MotionCostInput motionInput)
        {
            TerrainCell cell = use8KMTerrainTexture
                ? terrain8KMTexture.GetPixel8KM(cellIndex)
                : terrain1KMTexture.GetPixel(cellIndex);

            float cost = cellBiomeCost.GetByte((byte)cell.Biome);

            cost = BMath.Min(cost, cell.GetFeatureMinCost(cellFeatureCost));

            cost = BMath.Max(1, cost);

            if (useNoiseTexture)
            {
                int2 delta0 = BMath.Abs(motionInput.pathStartPoint - cellIndex);
                int2 delta1 = BMath.Abs(motionInput.pathEndPoint - cellIndex);

                int max0 = BMath.Max(delta0.x, delta0.y);
                int max1 = BMath.Max(delta1.x, delta1.y);

                float junctionDistance = BMath.Min(max0, max1);

                junctionDistance = BMath.Clamp(junctionDistance, noiseRange.min, noiseRange.max);
                
                float noiseDistanceFactor = BMath.InverseLerp(noiseRange, junctionDistance);

                int2 noisePoint = cellIndex % noiseTexture.size;

                float noiseColor = noiseTexture.GetPixel(noisePoint.x, noisePoint.y) / 255f;
                
                cost *= BMath.Lerp(noiseStrength, noiseColor * noiseDistanceFactor);
            }

            return cost;
        }
    }
}