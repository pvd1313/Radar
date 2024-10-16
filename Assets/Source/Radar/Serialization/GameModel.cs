using System;
using Beast.Claw.Serialization;
using Beast.Radar.Cinematic;
using Beast.Radar.Graphic.Terrain;

namespace Beast.Radar.Serialization
{
    [Serializable]
    public class GameModel : ISerializable
    {
        public TerrainData terrain;
        public CameraState camera;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.Write(terrain);
            stream.Write(camera);
        }

        public void Deserialize(IBinaryReader stream)
        {
            stream.Read(ref terrain);
            stream.Read(ref camera);
        }
    }
}