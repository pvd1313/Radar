using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Math;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    [StaticUtility]
    public static class RoadUtil
    {
        public static ushort2 SelectJunctionForCell(int2 cellIndex, int cellSize)
        {
            int2 junction = cellIndex * cellSize;
            
            junction += cellSize / 2 - 1;

            return (ushort2)junction;
        }
    }
}