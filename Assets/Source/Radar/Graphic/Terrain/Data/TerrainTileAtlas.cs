using System;
using System.Collections.Generic;
using Beast.Claw;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic.Terrain
{
    [Serializable]
    public class TerrainTileAtlas : IValidatable
    {
        private static HashSet<int> _indexHash;
        
        public Texture2D mainTexture;

#if BEAST_DEVELOPMENT
        public List<TerrainTileSprite> sprites;
#endif
        
        public float renderYCrop;
        public int2 tileCount;
        public float2 tileBlendCenter;
        public int atlasPPU;

        public string Validate()
        {
            if (tileCount.x <= 0 || tileCount.y <= 0)
            {
                return $"Invalid tile count {tileCount}";
            }

            if (atlasPPU <= 0)
            {
                return $"Invalid atlas PPU {atlasPPU}";
            }

#if BEAST_DEVELOPMENT
            if (sprites != null)
            {
                _indexHash ??= new HashSet<int>();
                _indexHash.Clear();
                
                foreach (TerrainTileSprite sprite in sprites)
                {
                    if (!sprite.IsValid(out string errorMessage))
                    {
                        return $"Sprite '{sprite.name}' has issues: {errorMessage}";
                    }

                    if (sprite.texture.width != atlasPPU || sprite.texture.height != atlasPPU)
                    {
                        return $"Sprite '{sprite.name}' has size that does not equal to atlas PPU";
                    }

                    if (sprite.index >= tileCount.x * tileCount.y)
                    {
                        return $"Sprite '{sprite.name}' has invalid index {sprite.index}";
                    }

                    if (!_indexHash.Add(sprite.index))
                    {
                        return $"Sprite '{sprite.name}' has duplicated index";
                    }
                }
            }
#endif
            
            return null;
        }
    }
}