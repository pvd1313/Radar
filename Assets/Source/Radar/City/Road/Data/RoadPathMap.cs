using System;
using Beast.Claw.Common;
using Beast.Claw.Math;
using Beast.Claw.Serialization;
using Unity.Collections;

namespace Beast.Radar.City
{
    public struct RoadPathMap : IDisposable, ISerializable
    {
        public NativeArray<RoadPath> roads;
        public NativeArray<ushort2> roadPoints;

        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteNativeArrayAndLength(roads);
            stream.WriteNativeArrayAndLength(roadPoints);
        }

        public void Deserialize(IBinaryReader stream)
        {
            roads = stream.ReadNativeArrayAndLength<RoadPath>(Allocator.Persistent);
            roadPoints = stream.ReadNativeArrayAndLength<ushort2>(Allocator.Persistent);
        }
        
        public void Dispose()
        {
            roads.DisposeIfCreated();
            roadPoints.DisposeIfCreated();
        }
    }
}