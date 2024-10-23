using System;
using Beast.Claw;
using Beast.Claw.Mathematics;
using Beast.Claw.Serialization;
using Unity.Mathematics;

namespace Beast.Radar.Terrain
{
    [Serializable]
    public struct TerrainData : ISerializable, IValidatable
    {
        public int seed;
        public int2 terrainSize;
        public float biomeSize01;
        public float water01;
        public float forest01;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteInt32(seed);
            stream.WriteInt2(terrainSize);
            stream.WriteFloat(biomeSize01);
            stream.WriteFloat(water01);
            stream.WriteFloat(forest01);
        }

        public void Deserialize(IBinaryReader stream)
        {
            seed = stream.ReadInt32();
            terrainSize = stream.ReadInt2();
            biomeSize01 = stream.ReadFloat();
            water01 = stream.ReadFloat();
            forest01 = stream.ReadFloat();
        }
        
        public string Validate()
        {
            if (terrainSize.x <= 0 || terrainSize.y <= 0 || terrainSize.y > 4096 || terrainSize.x > 4096)
            {
                return "Invalid size: " + terrainSize;
            }

            if (!BMath.IsPowerOf2(terrainSize.x) || !BMath.IsPowerOf2(terrainSize.y))
            {
                return "NPOT size: " + terrainSize;
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