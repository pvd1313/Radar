using System;
using Unity.Mathematics;

namespace Beast.Radar.Navigation
{
    public static class NeighbourOffsets
    {
        public const int COUNT = 8;
        
        public static int2 Get(int index)
        {
            switch (index)
            {
                case 0: return new int2(-1, 0);
                case 1: return new int2(1, 0);
                case 2: return new int2(0, 1);
                case 3: return new int2(0, -1);
                case 4: return new int2(-1, -1);        
                case 5: return new int2(-1, 1);
                case 6: return new int2(1, -1);
                case 7: return new int2(1, 1);
                
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}