using System;
using Beast.Claw;
using Beast.Claw.Math;
using Beast.Claw.Serialization;
using Unity.Mathematics;

namespace Beast.Radar.Terrain
{
    [Serializable]
    public struct TerrainData : ISerializable, IValidatable
    {
        public float seed01;
        public int2 size;
        public float biomeScale01;
        public float water01;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteFloat(seed01);
            stream.WriteInt2(size);
            stream.WriteFloat(biomeScale01);
            stream.WriteFloat(water01);
        }

        public void Deserialize(IBinaryReader stream)
        {
            seed01 = seed01 = stream.ReadFloat();
            size = stream.ReadInt2();
            biomeScale01 = stream.ReadFloat();
            water01 = stream.ReadFloat();
        }
        
        public string Validate()
        {
            if (seed01 < 0 || seed01 > 1)
            {
                return "Invalid seed: " + seed01;
            }

            if (size.x <= 0 || size.y <= 0 || size.y > 4096 || size.x > 4096)
            {
                return "Invalid size: " + size;
            }

            if (!BMath.IsPowerOf2(size.x) || !BMath.IsPowerOf2(size.y))
            {
                return "NPOT size: " + size;
            }

            if (biomeScale01 < 0 || biomeScale01 > 1)
            {
                return "Invalid biome scale: " + biomeScale01;
            }

            if (water01 < 0 || water01 > 1)
            {
                return "Invalid water: " + water01;
            }

            return null;
        }
    }
}