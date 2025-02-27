using Unity.Mathematics;

namespace Beast.Claw.Graphic
{
    public interface IScreen
    {
        public int2 Size { get; }
        public float Aspect { get; }
        public float PPU { get; }
    }
}