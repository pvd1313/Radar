using System;
using Beast.Claw.Serialization;
using Unity.Mathematics;

namespace Beast.Radar.Serialization
{
    public struct PlayerData : ISerializable
    {
        public float2 position;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteFloat2(position);
        }

        public void Deserialize(IBinaryReader stream)
        {
            position = stream.ReadFloat2();
        }
    }
}