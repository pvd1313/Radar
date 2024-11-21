using System.Runtime.CompilerServices;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Math;
using Beast.Claw.Validation;
using Unity.Mathematics;

namespace Beast.Claw.Graphic
{
    [ExtensionClass]
    public static class ScreenExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Width(this IScreen screen)
        {
            screen.ValidateNotNullThrowException(nameof(screen));

            return screen.Size.x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Height(this IScreen screen)
        {
            screen.ValidateNotNullThrowException(nameof(screen));

            return screen.Size.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ViewportDistance(this IScreen screen, float2 point0, float2 point1)
        {
            screen.ValidateNotNullThrowException(nameof(screen));

            float2 delta = point1 - point0;
            delta.x *= screen.Aspect;

            return delta.Magnitude();
        }
    }
}