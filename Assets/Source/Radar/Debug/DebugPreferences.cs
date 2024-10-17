using Beast.Claw.Serialization;

namespace Beast.Radar.Debug
{
    public class DebugPreferences : ISerializable
    {
        public bool isShowLog = true;
        public bool isShowFps = true;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteBool(isShowLog);
            stream.WriteBool(isShowFps);
        }

        public void Deserialize(IBinaryReader stream)
        {
            isShowLog = stream.ReadBool();
            isShowFps = stream.ReadBool();
        }
    }
}