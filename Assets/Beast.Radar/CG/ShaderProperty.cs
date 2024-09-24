using UnityEngine;

namespace Beast.Radar.CG
{
    public static class ShaderProperty
    {
        public static class WorldMapFlood
        {
            public static readonly int _Seed01 = Shader.PropertyToID("_Seed01");
            public static readonly int _Flood01 = Shader.PropertyToID("_Flood01");
            public static readonly int _Scale99 = Shader.PropertyToID("_Scale99");
        }

        public static class WorldMapRender
        {
            public static readonly int _ScreenAspect = Shader.PropertyToID("_ScreenAspect");
            public static readonly int _WorldTexture = Shader.PropertyToID("_WorldTexture");
            public static readonly int _WorldTextureSize = Shader.PropertyToID("_WorldTextureSize");
            public static readonly int _ViewDistance = Shader.PropertyToID("_ViewDistance");
            public static readonly int _CameraPosition = Shader.PropertyToID("_CameraPosition");
        }
    }
}