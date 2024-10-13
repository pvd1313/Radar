using System;
using Beast.Claw;
using Beast.Claw.Common;
using Beast.Claw.Serialization;
using Unity.Mathematics;

namespace Beast.Radar.Graphic.Terrain
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
        
        public bool IsValid(out string errorMessage)
        {
            errorMessage = null;
            
            if (seed01 < 0 || seed01 > 1)
            {
                errorMessage = $"Invalid seed ({seed01})";
                return false;
            }

            if (size.x < 0 || size.x > 4096)
            {
                errorMessage = $"Invalid size ({size})";
                return false;
            }

            if (biomeScale99 <= 0)
            {
                errorMessage = $"Invalid biome scale ({biomeScale99}";
                return false;
            }

            if (waterFlood01 < 0 || waterFlood01 > 1)
            {
                errorMessage = $"Invalid water flood ({waterFlood01})";
                return false;
            }

            return true;
        }
    }
}