using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Graphic;
using UnityEngine;

namespace Beast.Radar.Sprite
{
    public class SpriteRenderConfig : ScriptableConfig
    {
        public RenderTextureData atlasTexture;
        public Shader renderShader;
        public float scalingReferencePPU;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}