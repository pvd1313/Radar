using Beast.Claw.Serialization;

namespace Beast.Radar.Player
{
    public class PlayerPreference : ISerializable
    {
        public bool isMaxFps;
        public bool isAutoPathfinding;
        public bool beginMovementOnSingleTap;
        public GraphyPreference graphy;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteBool(isMaxFps);
            stream.WriteBool(isAutoPathfinding);
            stream.WriteBool(beginMovementOnSingleTap);
            graphy.Serialize(stream);
        }

        public void Deserialize(IBinaryReader stream)
        {
            isMaxFps = stream.ReadBool();
            isAutoPathfinding = stream.ReadBool();
            beginMovementOnSingleTap = stream.ReadBool();
            graphy.Deserialize(stream);
        }
    }
}