using System;
using Beast.Claw.Common;
using Beast.Claw.Serialization;
using Beast.Radar.Cinematic;
using Beast.Radar.World;

namespace Beast.Radar.Serialization
{
    [Serializable]
    public class GameModel : ISerializable
    {
        public int seed;
        public TerrainData terrain;
        public CameraState camera;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteInt32(seed);
            stream.Write(terrain);
            stream.Write(camera);
        }

        public void Deserialize(IBinaryReader stream)
        {
            seed = stream.ReadInt32();
            stream.Read(ref terrain);
            stream.Read(ref camera);
        }

        public void CopyFrom(GameModel source)
        {
            seed = source.seed;
            terrain = source.terrain;
            camera = source.camera;
        }
    }
}