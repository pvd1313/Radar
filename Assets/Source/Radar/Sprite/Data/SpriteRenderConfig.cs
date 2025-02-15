using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Graphic;
using UnityEngine;

namespace Beast.Radar.Sprite
{
    public class SpriteRenderConfig : ScriptableConfig
    {
        public Texture2DParameters atlasTexture;
        public int entityTextureMaxSize;
        public Shader renderShader;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}