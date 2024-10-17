using System;
using System.Collections.Generic;
using Beast.Claw;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    [Serializable]
    public class TerrainAtlas : IValidatable
    {
        private static HashSet<int> _indexHash;
        
        public Texture2D mainTexture;
        public Texture2D colorTexture;
        public Texture2D configTexture;

#if UNITY_EDITOR
        public List<TerrainTileSprite> sprites;
#endif
        
        public int2 tileCount;
        public int pixelsPerTile;

        public string Validate()
        {
            if (tileCount.x <= 0 || tileCount.y <= 0)
            {
                return $"Invalid tile count {tileCount}";
            }

            if (pixelsPerTile <= 0)
            {
                return $"Invalid atlas PPU {pixelsPerTile}";
            }

            float atlasWidth = tileCount.x * pixelsPerTile;
            if (!BMath.IsPowerOf2(atlasWidth))
            {
                return $"Atlas width ({atlasWidth}) is NPOT";
            }

            float atlasHeight = tileCount.y * pixelsPerTile;
            if (!BMath.IsPowerOf2(atlasHeight))
            {
                return $"Atlas height ({atlasHeight}) is NPOT";
            }

#if DEVELOPMENT
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

                    if (sprite.texture.width != pixelsPerTile || sprite.texture.height != pixelsPerTile)
                    {
                        return $"Sprite '{sprite.name}' has size '{sprite.texture.Size()}' that does not equal to atlas PPT";
                    }

                    if (sprite.index >= tileCount.x * tileCount.y)
                    {
                        return $"Sprite '{sprite.name}' index ({sprite.index}) out of range {tileCount}";
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