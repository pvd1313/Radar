using Beast.Claw.Math;
using Beast.Claw.Serialization;

namespace Beast.Radar.City
{
    public struct RoadInfo : ISerializable
    {
        public ushort startPointIndex;
        public ushort pointCount;
        public Bounds2D bounds;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteUInt16(startPointIndex);
            stream.WriteUInt16(pointCount);
            stream.Write(bounds);
        }

        public void Deserialize(IBinaryReader stream)
        {
            startPointIndex = stream.ReadUInt16();
            pointCount = stream.ReadUInt16();
            stream.Read(ref bounds);
        }
    }
}