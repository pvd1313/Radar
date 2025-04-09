using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Radar.Terrain;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Editor.Atlas
{
    [Serializable]
    public class TextureBaker : Controller
    {
        private const int INDEX_MAP_SIZE = 512;
        
        public Texture2D BakeMainTexture(TerrainAtlas atlas)
        {
            int2 textureSize = atlas.tileCount * atlas.pixelsPerTile;
            
            RenderTexture renderTexture = BGraphic.CreateTempRenderTexture(this, new RenderTextureData
            {
                size = textureSize,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                antiAliasing = 1,
                readWrite = RenderTextureReadWrite.Linear,
                format = RenderTextureFormat.ARGB32,
            });

            foreach (TerrainTileData tile in atlas.tiles)
            {
                int xOffset = tile.index % atlas.tileCount.x * atlas.pixelsPerTile;
                int yOffset = tile.index / atlas.tileCount.x * atlas.pixelsPerTile;
                
                BGraphic.DrawTexture2DToRenderTexture(new Texture2DToRenderTexture
                {
                    targetTexturePixelOffset = new float2(xOffset, yOffset),
                    sourceTexture = tile.texture,
                    targetTexture = renderTexture
                });
            }

            Texture2D resultTexture = BGraphic.CreateTexture2D(this, new Texture2DParameters
            {
                filterMode = FilterMode.Point,
                size = textureSize,
                wrapMode = TextureWrapMode.Clamp,
                isLinear = true,
                mipChain = false,
                createInitialized = false,
                format = TextureFormat.RGB24,
            });

            BGraphic.BlitRenderTextureTo2D(renderTexture, resultTexture);
            
            BGraphic.Dispose(renderTexture);

            return resultTexture;
        }

        public Texture2D BakeColorTexture(TerrainAtlas atlas)
        {
            Texture2D resultTexture = BGraphic.CreateTexture2D(this, new Texture2DParameters
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                size = atlas.tileCount,
                format = TextureFormat.RGB24,
                isLinear = true,
                mipChain = false,
            });

            _fillWithColor(resultTexture, Color.black);

            foreach (TerrainTileData sprite in atlas.tiles)
            {
                int x = sprite.index % resultTexture.width;
                int y = sprite.index / resultTexture.width;

                resultTexture.SetPixel(x, y, sprite.averageColor);
            }

            resultTexture.Apply();

            return resultTexture;
        }

        public Texture2D BakeConfigTexture(TerrainAtlas atlas)
        {
            Texture2D resultTexture = BGraphic.CreateTexture2D(this, new Texture2DParameters
            {
                filterMode = FilterMode.Point,
                size = atlas.tileCount,
                wrapMode = TextureWrapMode.Clamp,
                isLinear = true,
                format = TextureFormat.RG16,
                createInitialized = false,
                mipChain = false,
            });
            
            _fillWithColor(resultTexture, Color.black);
            
            foreach (TerrainTileData sprite in atlas.tiles)
            {
                int x = sprite.index % resultTexture.width;
                int y = sprite.index / resultTexture.width;

                Color color = new (sprite.yCropHeight, sprite.yBlendCenter, 0, 0);

                resultTexture.SetPixel(x, y, color);
            }

            resultTexture.Apply();

            return resultTexture;
        }

        public Texture2D BakeIndexTexture(TerrainAtlas atlas)
        {
            Texture2D resultTexture = BGraphic.CreateTexture2D(this, new Texture2DParameters
            {
                filterMode = FilterMode.Point,
                size = INDEX_MAP_SIZE,
                wrapMode = TextureWrapMode.Clamp,
                isLinear = true,
                format = TextureFormat.RHalf,
                createInitialized = false,
                mipChain = false,
            });
            
            _fillWithColor(resultTexture, Color.black);

            TerrainTileData[] tiles = atlas.tiles.Where(t => t.isBiomeTile).ToArray();
            Bounds2D[] bounds = tiles.Select(t => t.SpawnRangeAsBounds()).ToArray();
            int[] indexes = tiles.Select(t => t.index).ToArray();

            const int EMPTY_SPACE_TEXEL = 32;
            HashSet<int> emptySpaceX = new();
            HashSet<int> emptySpaceY = new();
            List<float2> emptySpace = new();
            int totalWrittenPixels = 0;
            
            for (int y = 0; y < INDEX_MAP_SIZE; y++)
            for (int x = 0; x < INDEX_MAP_SIZE; x++)
            {
                float2 point;
                point.x = x / (float)INDEX_MAP_SIZE;
                point.y = y / (float)INDEX_MAP_SIZE;

                bool foundIntersection = false;

                for (int i = 0; i < tiles.Length; i++)
                {
                    if (!bounds[i].Contains(point))
                    {
                        continue;
                    }

                    float tileIndex = BMath.Clamp01(indexes[i] / 255f);
                    Color color = new (tileIndex, 0, 0, 0);
                    resultTexture.SetPixel(x, y, color);
                    foundIntersection = true;
                    totalWrittenPixels++;
                    
                    break;
                }

                if (!foundIntersection)
                {
                    int xHash = BMath.FloorToInt(x / (float)EMPTY_SPACE_TEXEL);
                    int yHash = BMath.FloorToInt(y / (float)EMPTY_SPACE_TEXEL);

                    if (emptySpaceX.Contains(xHash) || emptySpaceY.Contains(yHash))
                    {
                        continue;
                    }

                    emptySpaceX.Add(xHash);
                    emptySpaceY.Add(yHash);
                    
                    emptySpace.Add(point);
                }
            }

            if (emptySpace.Count > 0)
            {
                StringBuilder builder = new();
                builder.AppendLine("Found empty space during baking index map!");
                builder.AppendLine("Total written pixels: " + totalWrittenPixels);
                builder.AppendLine("Expected: " + INDEX_MAP_SIZE * INDEX_MAP_SIZE);
                builder.AppendLine("Delta: " + (INDEX_MAP_SIZE * INDEX_MAP_SIZE - totalWrittenPixels));
                builder.Append("Ranges: ");
                
                foreach (float2 space in emptySpace)
                {
                    builder.Append('[');
                    builder.Append(BMath.Round(space.x * 100f) / 100f);
                    builder.Append(',');
                    builder.Append(BMath.Round(space.y * 100f) / 100f);
                    builder.Append(']');
                }

                builder.AppendLine();
                
                BDebug.LogError(this, builder.ToString());
            }

            resultTexture.Apply();

            return resultTexture;
        }
        
        private static void _fillWithColor(Texture2D texture, Color color)
        {
            for (int x = 0; x < texture.width; x++)
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }
    }
}