using Beast.Claw.Serialization;

namespace Beast.Radar.Player
{
    public class PlayerPreferences : ISerializable
    {
        public bool isMaxFps;
        public bool isAutoPathfinding;
        public bool isSingleTapMovement;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteBool(isMaxFps);
            stream.WriteBool(isAutoPathfinding);
            stream.WriteBool(isSingleTapMovement);
        }

        public void Deserialize(IBinaryReader stream)
        {
            isMaxFps = stream.ReadBool();
            isAutoPathfinding = stream.ReadBool();
            isSingleTapMovement = stream.ReadBool();
        }
    }
}