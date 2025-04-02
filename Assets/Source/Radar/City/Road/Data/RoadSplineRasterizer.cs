using Beast.Radar.Graphic;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public struct RoadSplineRasterizer
    {
        public static void CollectSplineVariants(RoadSplineMap splineMap, NativeHashSet<long> result)
        {
            for (int i = 0; i < splineMap.roads.Length; i++)
            {
                RoadSpline road = splineMap.roads[i];

                for (int j = 0; j < road.pointCount - 1; j++)
                {
                    float2 point0 = splineMap.roadPoints[road.firstPointIndex + j];
                    float2 point1 = splineMap.roadPoints[road.firstPointIndex + j + 1];
                    float2 handle = splineMap.roadHandles[road.firstPointIndex + j];
                    
                    GetSplineRotationMask(
                        point0, 
                        point1, 
                        handle,
                        out long rotation0,
                        out long rotation90,
                        out long rotation180,
                        out long rotation270);

                    if (result.Contains(rotation0) ||
                        result.Contains(rotation90) ||
                        result.Contains(rotation180) ||
                        result.Contains(rotation270))
                    {
                        continue;
                    }

                    result.Add(rotation0);
                }
            }
        }

        public static void GetSplineRotationMask(
            float2 point0,
            float2 point1,
            float2 handle,
            out long rotation0,
            out long rotation90,
            out long rotation180,
            out long rotation270)
        {
            int2 p0 = BWorld.WorldToTileIndex(point0);
            int2 p1 = BWorld.WorldToTileIndex(point1);
            int2 h = BWorld.WorldToTileIndex(handle);

            int2 a = p1 - p0;
            int2 b = h - p0;
            rotation0 = _encode(a, b);

            a = _rotate90(a);
            b = _rotate90(b);
            rotation90 = _encode(a, b);

            a = _rotate90(a);
            b = _rotate90(b);
            rotation180 = _encode(a, b);

            a = _rotate90(a);
            b = _rotate90(b);
            rotation270 = _encode(a, b);
        }
        
        private static long _encode(int2 a, int2 b)
        {
            return ((long)a.x & 0xFFFF) << 48 |
                   ((long)a.y & 0xFFFF) << 32 |
                   ((long)b.x & 0xFFFF) << 16 |
                   ((long)b.y & 0xFFFF);
        }
        
        private static int2 _rotate90(int2 vector)
        {
            return new int2(vector.y * -1, vector.x);
        }
    }
}