using System;
using Beast.Claw.Graphic;
using Beast.Radar.Terrain;
using UnityEngine;

namespace Beast.Radar.City
{
    public struct RoadRenderMaterialSetup
    {
        public float lineWidth;
        public Texture2D texture;
        public ERoadShaderMode shaderMode;
        public ETerrainBiome biomeIndex;
        public float textureAlpha;

        public void Apply(Material material)
        {
            material.SetFloat("_LineWidth", lineWidth);

            switch (shaderMode)
            {
                case ERoadShaderMode.Texture:
                    material.SetTexture("_Texture", texture);
                    material.SetFloat("_TextureAlpha", textureAlpha);
                    material.SetKeyword("MODE_TEXTURE", true);
                    material.SetKeyword("MODE_BIOME", false);
                    break;
                
                case ERoadShaderMode.Biome:
                    material.SetFloat("_BiomeIndex", (int)biomeIndex);
                    material.SetKeyword("MODE_TEXTURE", false);
                    material.SetKeyword("MODE_BIOME", true);
                    break;
                
                default:
                    throw new ArgumentException("Invalid shader mode");
            }
            
            material.SetTexture("_Texture", texture);
        }
    }
}