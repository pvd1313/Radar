using System;
using Beast.Claw;
using Beast.Claw.Common;
using Beast.Claw.Graphic;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainPreviewTexture : IDisposable, IValidatable
    {
        public RenderTexture fbm;
        public RenderTexture biome;
        public RenderTexture color;

        public string Validate()
        {
            if (biome == null)
            {
                return "biomeTexture is null";
            }

            if (color == null)
            {
                return "outputTexture is null";
            }

            if (fbm == null)
            {
                return "fbm is null";
            }

            return null;
        }
        
        public void Dispose()
        {
            BGraphic.TryDispose(ref fbm);
            BGraphic.TryDispose(ref biome);
            BGraphic.TryDispose(ref color);
        }
    }
}