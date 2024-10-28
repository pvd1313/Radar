using Beast.Claw.Serialization;

namespace Beast.Radar.World
{
    public struct WorldData: ISerializable
    {
        public ulong minutes;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteUInt64(minutes);
        }

        public void Deserialize(IBinaryReader stream)
        {
            minutes = stream.ReadUInt64();
        }
    }
}