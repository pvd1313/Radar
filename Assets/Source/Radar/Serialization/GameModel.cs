using System;
using Beast.Claw.Serialization;
using Beast.Radar.Cinematic;
using Beast.Radar.Player;
using Beast.Radar.Terrain;
using Beast.Radar.World;

namespace Beast.Radar.Serialization
{
    [Serializable]
    public class GameModel : ISerializable
    {
        public TerrainData terrain;
        public CameraState camera;
        public PlayerData player;
        public WorldData world;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.Write(terrain);
            stream.Write(camera);
            stream.Write(player);
            stream.Write(world);
        }

        public void Deserialize(IBinaryReader stream)
        {
            stream.Read(ref terrain);
            stream.Read(ref camera);
            stream.Read(ref player);
            stream.Read(ref world);
        }
    }
}