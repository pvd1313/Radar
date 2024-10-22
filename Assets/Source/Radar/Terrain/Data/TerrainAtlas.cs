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
        private static Dictionary<int, string> _indexToName;
        private static Dictionary<int, Bounds> _indexToBounds;
        
        public Texture2D mainTexture;
        public Texture2D colorTexture;
        public Texture2D configTexture;

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
                _indexToBounds ??= new Dictionary<int, Bounds>();
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

                    FloatMinMax water = tile.waterRange;
                    FloatMinMax forest = tile.forestRange;

                    float epsilon = BMath.EPSILON;

                    Vector3 center = new (water.GetCenter(), forest.GetCenter(), 0);
                    Vector3 size = new(water.Shrink(epsilon).GetRange(), forest.Shrink(epsilon).GetRange(), 1);

                    Bounds bounds = new(center, size);

                    foreach ((int tileIndex, Bounds tileBounds) in _indexToBounds)
                    {
                        if (!tileBounds.Intersects(bounds))
                        {
                            continue;
                        }

                        return $"Tile '{tile.name}' spawn range overlaps tile '{_indexToName[tileIndex]}'";
                    }
                    
                    _indexToName[tile.index] = tile.name;
                    _indexToBounds[tile.index] = bounds;
                }
            }
#endif
            
            return null;
        }
    }
}