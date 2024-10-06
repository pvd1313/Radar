using System;
using Beast.Claw.Common;
using Beast.Claw.Serialization;
using Beast.Radar.Cinematic;
using Beast.Radar.World;

namespace Beast.Radar.Serialization
{
    [Serializable]
    public class BeastGameModel : IBeastSerializable
    {
        public int seed;
        public BeastTerrainData terrain;
        public BeastCameraState camera;
        
        public void Serialize(IBeastBinaryWriter stream)
        {
            stream.WriteInt32(seed);
            stream.Write(terrain);
            stream.Write(camera);
        }

        public void Deserialize(IBeastBinaryReader stream)
        {
            seed = stream.ReadInt32();
            stream.Read(ref camera);
            stream.Read(ref terrain);
        }

        public void CopyFrom(BeastGameModel source)
        {
            seed = source.seed;
            terrain = source.terrain;
            camera = source.camera;
        }
    }
}