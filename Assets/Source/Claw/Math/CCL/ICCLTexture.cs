using Unity.Mathematics;

namespace Beast.Claw.Math.CCL
{
    public interface ICCLTexture
    {
        public int2 Size { get; }

        public bool TestPixel(int x, int y);
    }
}