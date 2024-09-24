using UnityEngine;

namespace Beast.Radar.CG
{
    public static class ShaderProperty
    {
        public static class WorldMapFlood
        {
            public static readonly int Seed01 = Shader.PropertyToID("_Seed01");
            public static readonly int Flood01 = Shader.PropertyToID("_Flood01");
            public static readonly int Scale99 = Shader.PropertyToID("_Scale99");
        }
    }
}