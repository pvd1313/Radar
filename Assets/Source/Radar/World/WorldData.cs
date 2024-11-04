using System;
using Beast.Claw.Serialization;
using Beast.Radar.Terrain;

namespace Beast.Radar.World
{
    [Serializable]
    public struct WorldData : ISerializable
    {
        public Guid guid;
        public TerrainData terrain;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteGuid(guid);
            stream.Write(terrain);
        }

        public void Deserialize(IBinaryReader stream)
        {
            guid = stream.ReadGuid();
            stream.Read(ref terrain);
        }
    }
}