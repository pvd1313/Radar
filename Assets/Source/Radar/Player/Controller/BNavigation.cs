using System.Runtime.CompilerServices;
using Beast.Claw.Attributes.Annotation;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.Player
{
    [StaticUtility]
    public static class BNavigation
    {
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