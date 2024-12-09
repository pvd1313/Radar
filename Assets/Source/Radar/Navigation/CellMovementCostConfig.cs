using System;
using Beast.Radar.Terrain;

namespace Beast.Radar.Navigation
{
    [Serializable]
    public class CellMovementCostConfig
    {
        public CellBiomeMovementCost[] biome;
        public CellFeatureMovementCost[] feature;

        public void ToByteArray(out ByteArray16 biomeCost, out ByteArray16 featureCost)
        {
            biomeCost = default;
            
            foreach (CellBiomeMovementCost cost in biome)
            {
                biomeCost.SetByte((byte)cost.biome, cost.cost);
            }

            featureCost = default;

            foreach (CellFeatureMovementCost cost in feature)
            {
                featureCost.SetByte((byte)cost.feature, cost.cost);
            }
        }
    }
}