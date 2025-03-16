using System.Runtime.CompilerServices;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Math;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.Navigation
{
    [StaticUtility]
    public static class BNavigation
    {
        public static Bounds2D CreateSearchBounds(NavigationPathSearchBounds bounds)
        {
            Bounds2D searchBounds = Bounds2D.FromTwoPoints(bounds.startCell, bounds.finishCell);
            
            float searchRangeX = searchBounds.x.GetRange();
            float searchRangeY = searchBounds.y.GetRange();
            
            float deltaRangeHalf = BMath.Ceil((searchRangeY - searchRangeX) * 0.5f);
            
            // This will make search bounds to be a square.
            if (deltaRangeHalf > 0)
            {
                searchBounds.x = searchBounds.x.Expand(deltaRangeHalf);
            }
            else
            {
                searchBounds.y = searchBounds.y.Expand(deltaRangeHalf * -1);
            }
            
            float km8Radius = bounds.cell8KMSize * 0.5f;
            if (bounds.include8KMWorldPoints.Length > 0)
            {
                for (int i = 0; i < bounds.include8KMWorldPoints.Length; i++)
                {
                    float2 point8KM = bounds.include8KMWorldPoints[i];
                    Bounds2D pointBounds = Bounds2D.FromPointWithRadius(point8KM, km8Radius);
                    searchBounds = searchBounds.EnsureContains(pointBounds);
                }
            }
            
            Bounds2D textureBounds = new (0, bounds.terrainSize - 1);
            searchBounds = searchBounds.EnsureInside(textureBounds);

            return searchBounds;
        }
        
        public static void CalcPathNodeTypeCount(NativeList<int2> path, out int diagonalCount, out int straightCount)
        {
            diagonalCount = 0;
            straightCount = 0;
            
            if (path.Length < 2)
            {
                return;
            }
            
            int2 prevPosition = path[0];
            
            for (int i = 1; i < path.Length; i++)
            {
                int2 position = path[i];

                if (IsStraight(position, prevPosition))
                {
                    straightCount++;
                }
                else
                {
                    diagonalCount++;
                }

                prevPosition = position;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsStraight(int2 positionA, int2 positionB)
        {
            int2 delta = positionB - positionA;

            if (delta.x == 0 || delta.y == 0)
            {
                return true;
            }

            return false;
        }
    }
}