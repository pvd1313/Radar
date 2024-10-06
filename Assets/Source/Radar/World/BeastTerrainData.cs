using System;
using Beast.Claw.Common;
using Beast.Claw.Serialization;
using Unity.Mathematics;

namespace Beast.Radar.World
{
    [Serializable]
    public struct BeastTerrainData : IBeastSerializable
    {
        public int2 size;
        public float biomeScale99;
        public float waterFlood01;
        
        public void Serialize(IBeastBinaryWriter stream)
        {
            stream.WriteInt2(size);
            stream.WriteFloat(biomeScale99);
            stream.WriteFloat(waterFlood01);
        }

        public void Deserialize(IBeastBinaryReader stream)
        {
            size = stream.ReadInt2();
            biomeScale99 = stream.ReadFloat();
            waterFlood01 = stream.ReadFloat();
        }
    }
}