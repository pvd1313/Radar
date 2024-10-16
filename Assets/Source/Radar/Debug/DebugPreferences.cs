using Beast.Claw.Serialization;

namespace Beast.Radar.Debug
{
    public class DebugPreferences : ISerializable
    {
        public bool isShowLog;
        public bool isShowFps;
        
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