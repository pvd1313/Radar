using System;
using Beast.Claw;
using Beast.Claw.Serialization;
using Unity.Mathematics;

namespace Beast.Radar.Terrain
{
    [Serializable]
    public struct TerrainData : ISerializable, IValidatable
    {
        public float seed01;
        public int2 size;
        public float biomeScale99;
        public float waterFlood01;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteFloat(seed01);
            stream.WriteInt2(size);
            stream.WriteFloat(biomeScale99);
            stream.WriteFloat(waterFlood01);
        }

        public void Deserialize(IBinaryReader stream)
        {
            seed01 = seed01 = stream.ReadFloat();
            size = stream.ReadInt2();
            biomeScale99 = stream.ReadFloat();
            waterFlood01 = stream.ReadFloat();
        }
        
        public string Validate()
        {
            if (seed01 < 0 || seed01 > 1)
            {
                return $"Invalid seed ({seed01})";
            }

            if (size.x < 0 || size.x > 4096)
            {
                return $"Invalid size ({size})";
            }

            if (biomeScale99 <= 0)
            {
                return $"Invalid biome scale ({biomeScale99}";
            }

            if (waterFlood01 < 0 || waterFlood01 > 1)
            {
                return $"Invalid water flood ({waterFlood01})";
            }

            return null;
        }
    }
}