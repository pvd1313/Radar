using System;
using System.Collections.Generic;
using Beast.Claw;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    [Serializable]
    public class TerrainAtlas : IValidatable
    {
        private static Dictionary<int, string> _indexToName;
        private static Dictionary<int, FloatBounds2D> _indexToBounds;
        
        public Texture2D mainTexture;
        public Texture2D colorTexture;
        public Texture2D configTexture;
        public Texture2D indexTexture;

#if UNITY_EDITOR
        public List<TerrainTileData> tiles;
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

#if UNITY_EDITOR
            if (tiles != null)
            {
                _indexToName ??= new Dictionary<int, string>();
                _indexToBounds ??= new Dictionary<int, FloatBounds2D>();
                _indexToName.Clear();
                _indexToBounds.Clear();
                
                foreach (TerrainTileData tile in tiles)
                {
                    if (!tile.IsValid(out string errorMessage))
                    {
                        return $"Tile '{tile.name}' has issues: {errorMessage}";
                    }

                    if (tile.texture.width != pixelsPerTile || tile.texture.height != pixelsPerTile)
                    {
                        return $"Tile '{tile.name}' has size '{tile.texture.Size()}' that does not equal to atlas PPT";
                    }

                    if (tile.index >= tileCount.x * tileCount.y)
                    {
                        return $"Tile '{tile.name}' index ({tile.index}) out of range {tileCount}";
                    }

                    if (_indexToName.TryGetValue(tile.index, out string existingTileName))
                    {
                        return $"Tile '{tile.name}' index ({tile.index}) conflicts with '{existingTileName}'";
                    }
                    
                    _indexToName[tile.index] = tile.name;

                    if (!tile.isBiomeTile)
                    {
                        continue;
                    }

                    FloatBounds2D bounds = tile.SpawnRangeAsBounds().Shrink(BMath.EPSILON);
                    
                    foreach ((int tileIndex, FloatBounds2D tileBounds) in _indexToBounds)
                    {
                        if (!tileBounds.Intersects(bounds))
                        {
                            continue;
                        }

                        return $"Tile '{tile.name}' spawn range overlaps tile '{_indexToName[tileIndex]}'";
                    }
                    
                    _indexToBounds[tile.index] = bounds;
                }
            }
#endif
            
            return null;
        }
    }
}