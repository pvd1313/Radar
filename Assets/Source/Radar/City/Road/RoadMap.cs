using System;
using Beast.Claw.Math;
using Beast.Claw.Serialization;
using Unity.Collections;

namespace Beast.Radar.City
{
    public struct RoadMap : IDisposable, ISerializable
    {
        public NativeArray<RoadInfo> roads;
        public NativeArray<ushort2> roadPoints;

        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteNativeArrayAndLength(roads);
            stream.WriteNativeArrayAndLength(roadPoints);
        }

        public void Deserialize(IBinaryReader stream)
        {
            roads = stream.ReadNativeArrayAndLength<RoadInfo>(Allocator.Persistent);
            roadPoints = stream.ReadNativeArrayAndLength<ushort2>(Allocator.Persistent);
        }
        
        public void Dispose()
        {
            if (roads.IsCreated)
            {
                roads.Dispose();
            }

            if (roadPoints.IsCreated)
            {
                roadPoints.Dispose();
            }
        }
    }
}