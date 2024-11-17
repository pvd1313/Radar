using System;
using Beast.Claw;
using Beast.Claw.Common;
using Beast.Claw.Math;
using Beast.Claw.Serialization;

namespace Beast.Radar.Terrain
{
    [Serializable]
    public struct TerrainData : ISerializable, IValidatable
    {
        public int seed;
        public int terrainSize;
        public float biomeSize01;
        public float water01;
        public float forest01;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteInt32(seed);
            stream.WriteInt32(terrainSize);
            stream.WriteFloat(biomeSize01);
            stream.WriteFloat(water01);
            stream.WriteFloat(forest01);
        }

        public void Deserialize(IBinaryReader stream)
        {
            seed = stream.ReadInt32();
            terrainSize = stream.ReadInt32();
            biomeSize01 = stream.ReadFloat();
            water01 = stream.ReadFloat();
            forest01 = stream.ReadFloat();
        }

        public bool IsSame(TerrainData other)
        {
            return seed == other.seed &&
                   terrainSize == other.terrainSize &&
                   BMath.Approximately(biomeSize01, other.biomeSize01) &&
                   BMath.Approximately(water01, other.water01) &&
                   BMath.Approximately(forest01, other.forest01);
        }

        public override string ToString()
        {
            return
                $"(seed: {seed}; terrainSize: {terrainSize}; biomeSize: {biomeSize01}; water: {water01}; forest: {forest01})";
        }

        public string Validate()
        {
            if (terrainSize <= 0 || terrainSize > 4096)
            {
                return "Invalid terrain size: " + terrainSize;
            }

            if (!BMath.IsPowerOf2(terrainSize))
            {
                return "NPOT terrain size: " + terrainSize;
            }

            if (biomeSize01 < 0 || biomeSize01 > 1)
            {
                return "Invalid biome size: " + biomeSize01;
            }

            if (water01 < 0 || water01 > 1)
            {
                return "Invalid water: " + water01;
            }
            
            if (forest01 < 0 || forest01 > 1)
            {
                return "Invalid forest: " + water01;
            }

            return null;
        }
    }
}