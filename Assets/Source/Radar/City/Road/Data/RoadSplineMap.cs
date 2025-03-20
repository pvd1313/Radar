using System;
using Beast.Claw.Common;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public struct RoadSplineMap : IDisposable
    {
        public NativeArray<RoadSpline> roads;
        public NativeArray<float2> roadPoints;
        public NativeArray<float2> roadHandles;

        public void Dispose()
        {
            roads.DisposeIfCreated();
            roadPoints.DisposeIfCreated();
            roadHandles.DisposeIfCreated();
        }
    }
}