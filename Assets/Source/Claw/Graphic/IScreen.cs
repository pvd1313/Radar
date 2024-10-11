using Unity.Mathematics;

namespace Beast.Claw.Graphic
{
    public interface IScreen
    {
        public int2 Size { get; }
        public float Aspect { get; }
        public int Width { get; }
        public int Height { get; }
    }
}