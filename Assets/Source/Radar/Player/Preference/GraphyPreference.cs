using Beast.Claw.Serialization;

namespace Beast.Radar.Player
{
    public struct GraphyPreference : ISerializable
    {
        public bool isShowGraphy;
        public bool isAudioEnabled;
        public bool isFpsEnabled;
        public bool isInformationEnabled;
        public bool isRamEnabled;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteBool(isShowGraphy);
            stream.WriteBool(isAudioEnabled);
            stream.WriteBool(isFpsEnabled);
            stream.WriteBool(isInformationEnabled);
            stream.WriteBool(isRamEnabled);
        }

        public void Deserialize(IBinaryReader stream)
        {
            isShowGraphy = stream.ReadBool();
            isAudioEnabled = stream.ReadBool();
            isFpsEnabled = stream.ReadBool();
            isInformationEnabled = stream.ReadBool();
            isRamEnabled = stream.ReadBool();
        }
    }
}