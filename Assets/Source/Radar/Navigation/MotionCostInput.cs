using Unity.Mathematics;

namespace Beast.Radar.Navigation
{
    public struct MotionCostInput
    {
        public int2 motionStartPoint;
        public int2 motionEndPoint;
        public int2 pathStartPoint;
        public int2 pathEndPoint;
    }
}