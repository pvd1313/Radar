using Unity.Mathematics;

namespace Beast.Claw.Graphic
{
    public interface IScreen
    {
        public int2 WidthHeight { get; }
        public float Aspect { get; }
        public float Width { get; }
        public float Height { get; }
    }
}