using Beast.Claw.Attributes.Annotation;

namespace Beast.Claw.Graphic
{
    [ConstantGroup]
    public static class RenderZOrder
    {
        public const float MAX = -0.99f;
        
        public const float GIZMO = -0.9f;
        public const float SPRITE = -0.4f;
        public const float TERRAIN_ROAD = -0.3f;
        public const float TERRAIN_TILE_RENDER = -0.2f;
        public const float TERRAIN_COLOR_RENDER = -0.1f;
    }
}