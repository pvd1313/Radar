using Beast.Claw.Attributes.Annotation;

namespace Beast.Claw.Input
{
    [ConstantGroup]
    public static class AxisType
    {
        public const int NONE = 0;
        public const int RAW_TOUCH = 1;
        public const int DRAG = 2;
        public const int ZOOM = 3;
    }
}