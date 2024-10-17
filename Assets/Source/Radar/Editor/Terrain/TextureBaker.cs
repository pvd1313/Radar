using System;
using Beast.Claw.Graphic;
using Beast.Radar.Graphic.Terrain;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Editor.Terrain
{
    [Serializable]
    public class TextureBaker
    {
        public Texture2D BakeMainTexture(TerrainAtlas atlas)
        {
            int2 textureSize = atlas.tileCount * atlas.pixelsPerTile;
            
            RenderTexture renderTexture = BGraphic.CreateTemporaryRenderTexture(this, new NewTempRenderTextureData
            {
                size = textureSize,
                antiAliasing = 1,
                format = RenderTextureFormat.ARGB32,
                filterMode = FilterMode.Point,
                readWrite = RenderTextureReadWrite.Linear
            });

            foreach (TerrainTileSprite tile in atlas.sprites)
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
                isLinear = true,
                mipChain = true,
                createInitialized = false,
                filterMode = FilterMode.Point,
                format = TextureFormat.RGB24,
                size = textureSize,
                wrapMode = TextureWrapMode.Clamp
            });

            BGraphic.BlitRenderTextureTo2D(renderTexture, resultTexture);
            
            BGraphic.Dispose(renderTexture);

            return resultTexture;
        }

        public Texture2D BakeColorTexture(TerrainAtlas atlas)
        {
            Texture2D resultTexture = BGraphic.CreateTexture2D(this, new Texture2DParameters
            {
                format = TextureFormat.RGB24,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                size = atlas.tileCount,
                isLinear = true,
                mipChain = false,
            });

            _fillWithColor(resultTexture, Color.black);

            foreach (TerrainTileSprite sprite in atlas.sprites)
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
                isLinear = true,
                format = TextureFormat.RG16,
                createInitialized = false,
                filterMode = FilterMode.Point,
                mipChain = false,
                size = atlas.tileCount,
            });
            
            _fillWithColor(resultTexture, Color.black);
            
            foreach (TerrainTileSprite sprite in atlas.sprites)
            {
                int x = sprite.index % resultTexture.width;
                int y = sprite.index / resultTexture.width;

                Color color = new (sprite.yCropHeight, sprite.yBlendCenter, 0, 0);

                resultTexture.SetPixel(x, y, color);
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