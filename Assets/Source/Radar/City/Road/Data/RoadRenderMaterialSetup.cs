using UnityEngine;

namespace Beast.Radar.City
{
    public struct RoadRenderMaterialSetup
    {
        public float lineWidth;
        public Texture2D texture;
        public float textureAlpha;

        public void Apply(Material material)
        {
            material.SetFloat("_LineWidth", lineWidth);
            material.SetTexture("_Texture", texture);
            material.SetFloat("_TextureAlpha", textureAlpha);
        }
    }
}