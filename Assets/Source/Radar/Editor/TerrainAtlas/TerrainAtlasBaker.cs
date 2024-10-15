using System;
using Beast.Claw.Graphic;
using Beast.Radar.Graphic.Terrain;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Editor.TerrainAtlas
{
    [Serializable]
    public class TerrainAtlasBaker
    {
        public Texture2D BakeMainTexture(TerrainTileAtlas atlas)
        {
            int2 textureSize = atlas.tileCount * atlas.atlasPPU;
            
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
                int xOffset = tile.index % atlas.tileCount.y * atlas.atlasPPU;
                int yOffset = tile.index / atlas.tileCount.y * atlas.atlasPPU;
                
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
            
            BGraphic.DisposeRenderTexture(renderTexture);

            return resultTexture;
        }

        public Texture2D BakeColorTexture(TerrainTileAtlas tileAtlas)
        {
            Texture2D resultTexture = BGraphic.CreateTexture2D(this, new Texture2DParameters
            {
                format = TextureFormat.RGB24,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                size = tileAtlas.tileCount,
                isLinear = true,
                mipChain = false,
            });

            _fillWithColor(resultTexture, Color.black);

            foreach (TerrainTileSprite sprite in tileAtlas.sprites)
            {
                int x = sprite.index % resultTexture.height;
                int y = sprite.index / resultTexture.height;

                resultTexture.SetPixel(x, y, sprite.averageColor);
            }

            resultTexture.Apply();

            return resultTexture;
        }

        public Texture2D BakeConfigTexture(TerrainTileAtlas tileAtlas)
        {
            Texture2D resultTexture = BGraphic.CreateTexture2D(this, new Texture2DParameters
            {
                isLinear = true,
                format = TextureFormat.RG16,
                createInitialized = false,
                filterMode = FilterMode.Point,
                mipChain = false,
                size = tileAtlas.tileCount,
            });
            
            _fillWithColor(resultTexture, Color.black);
            
            foreach (TerrainTileSprite sprite in tileAtlas.sprites)
            {
                int x = sprite.index % resultTexture.height;
                int y = sprite.index / resultTexture.height;

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