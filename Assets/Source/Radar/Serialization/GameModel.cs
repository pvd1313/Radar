using System;
using Beast.Claw.Serialization;
using Beast.Radar.Cinematic;
using Beast.Radar.Player;
using Beast.Radar.Terrain;

namespace Beast.Radar.Serialization
{
    [Serializable]
    public class GameModel : ISerializable
    {
        public Guid world;
        public CameraState camera;
        public PlayerData player;
        public ulong timeMinutes;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteGuid(world);
            stream.Write(camera);
            stream.Write(player);
            stream.WriteUInt64(timeMinutes);
        }

        public void Deserialize(IBinaryReader stream)
        {
            world = stream.ReadGuid();
            stream.Read(ref camera);
            stream.Read(ref player);
            timeMinutes = stream.ReadUInt64();
        }
    }
}