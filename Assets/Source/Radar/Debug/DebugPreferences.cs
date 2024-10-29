using Beast.Claw.Serialization;

namespace Beast.Radar.Debug
{
    public class DebugPreferences : ISerializable
    {
        public bool showLogConsole = true;
        public bool showFPSCounter = true;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteBool(showLogConsole);
            stream.WriteBool(showFPSCounter);
        }

        public void Deserialize(IBinaryReader stream)
        {
            showLogConsole = stream.ReadBool();
            showFPSCounter = stream.ReadBool();
        }
    }
}