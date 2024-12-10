using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Debug;
using Beast.Claw.Math;
using Beast.Claw.Reflection;
using Beast.Claw.Text;
using Beast.Claw.Validation;
using Beast.Radar.Terrain;

namespace Beast.Radar.Navigation
{
    [Serializable]
    public class CellMovementCostConfig : IValidatable
    {
        private static readonly HashSet<ETerrainBiome> ALL_BIOMES;
        private static readonly HashSet<ETerrainFeature> ALL_FEATURES;

        private static readonly HashSet<ETerrainBiome> BIOME_BUFFER;
        private static readonly HashSet<ETerrainFeature> FEATURE_BUFFER;
        
        public CellBiomeMovementCost[] biome;
        public CellFeatureMovementCost[] feature;

        static CellMovementCostConfig()
        {
            try
            {
                ALL_BIOMES = BEnum.GetValues<ETerrainBiome>().ToHashSet();
                ALL_BIOMES.Remove(ETerrainBiome.Empty);

                ALL_FEATURES = BEnum.GetValues<ETerrainFeature>().ToHashSet();
                ALL_FEATURES.Remove(ETerrainFeature.None);

                BIOME_BUFFER = new HashSet<ETerrainBiome>();
                FEATURE_BUFFER = new HashSet<ETerrainFeature>();
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }
        
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
                int index = BMath.IntFloorLog2((int)cost.feature);
                featureCost.SetByte(index, cost.cost);
            }
        }

        public string Validate()
        {
            BIOME_BUFFER.Clear();

            foreach (CellBiomeMovementCost biomeCost in biome)
            {
                if (!BIOME_BUFFER.Add(biomeCost.biome))
                {
                    return "Biome " + biomeCost.biome.Q() + " is duplicated";
                }

                if (!ALL_BIOMES.Contains(biomeCost.biome))
                {
                    return "Invalid biome: " + biomeCost.biome.Q();
                }
            }

            foreach (ETerrainBiome terrainBiome in ALL_BIOMES)
            {
                if (!BIOME_BUFFER.Contains(terrainBiome))
                {
                    return "Biome is missng: " + terrainBiome.Q();
                }
            }
            
            FEATURE_BUFFER.Clear();
            foreach (CellFeatureMovementCost featureCost in feature)   
            {
                if (!FEATURE_BUFFER.Add(featureCost.feature))
                {
                    return "Feature " + featureCost.feature.Q() + " is duplicated";
                }

                if (!ALL_FEATURES.Contains(featureCost.feature))
                {
                    return "Invalid feature: " + featureCost.feature.Q();
                }
            }

            foreach (ETerrainFeature terrainFeature in ALL_FEATURES)
            {
                if (!FEATURE_BUFFER.Contains(terrainFeature))
                {
                    return "Feature is missing: " + terrainFeature.Q();
                }
            }

            return null;
        }
    }
}