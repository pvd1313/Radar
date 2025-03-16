namespace Beast.Radar.Navigation
{
    public interface IMotionCostComputer
    {
        public float ComputeCost(MotionCostInput input);
    }
}