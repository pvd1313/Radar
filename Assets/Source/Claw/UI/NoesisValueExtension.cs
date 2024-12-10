using System.Runtime.CompilerServices;

namespace Beast.Claw.UI
{
    public static class NoesisValueExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Noesis.Color ToNoesisColor(this UnityEngine.Color color)
        {
            return new Noesis.Color()
            {
                A = (byte)(color.a * 255),
                R = (byte)(color.r * 255),
                G = (byte)(color.g * 255),
                B = (byte)(color.b * 255),
            };
        }
    }
}