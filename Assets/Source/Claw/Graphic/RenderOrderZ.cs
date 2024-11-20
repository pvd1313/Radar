using Beast.Claw.Attributes.Annotation;

namespace Beast.Claw.Graphic
{
    [ConstantGroup]
    public static class RenderOrderZ
    {
        public const float MAX = -0.99f;
        public const float GIZMO = -0.9f;
        public const float TERRAIN_TILE_RENDER = -0.2f;
        public const float TERRAIN_COLOR_RENDER = -0.1f;
    }
}