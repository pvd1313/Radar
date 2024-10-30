using Beast.Claw.Serialization;
using Unity.Mathematics;

namespace Beast.Radar.Player
{
    public struct PlayerGameData : ISerializable
    {
        public int2 position;

        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteInt2(position);
        }

        public void Deserialize(IBinaryReader stream)
        {
            position = stream.ReadInt2();
        }
    }
}