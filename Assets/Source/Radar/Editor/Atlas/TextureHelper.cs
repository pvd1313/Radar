using System;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beast.Radar.Editor.Atlas
{
    [Serializable]
    public class TextureHelper
    {
        public float GetSpriteHeight(Texture texture)
        {
            Texture2D clone = _cloneTexture(texture);

            try
            {
                Color[] pixels = clone.GetPixels();
                int lastNonBlackRow = clone.height - 1;

                for (int y = 0; y < clone.height; y++)
                {
                    for (int x = 0; x < texture.width; x++)
                    {
                        int j = y * clone.height + x;

                        if (!_isBlack(pixels[j]))
                        {
                            lastNonBlackRow = y;
                            break;
                        };
                    }
                }

                return (lastNonBlackRow + 1) / (float)texture.height;
            }
            finally
            {
                Object.DestroyImmediate(clone);
            }
        }

        public Color GetAverageColor(Texture texture)
        {
            Texture2D clone = _cloneTexture(texture);

            try
            {
                Color[] pixels = clone.GetPixels();
                double avgR = 0;
                double avgG = 0;
                double avgB = 0;
                int pixelCount = 0;

                for (int i = 0; i < pixels.Length; i++)
                {
                    Color pixel = pixels[i];

                    if (!_isBlack(pixels[i]))
                    {
                        avgR += pixel.r;
                        avgG += pixel.g;
                        avgB += pixel.b;
                        pixelCount++;
                    }
                }

                if (pixelCount == 0)
                {
                    return Color.black;
                }

                Color resultColor;
                resultColor.r = (float)(avgR / pixelCount);
                resultColor.g = (float)(avgG / pixelCount);
                resultColor.b = (float)(avgB / pixelCount);
                resultColor.a = 1;

                return resultColor;
            }
            finally
            {
                Object.DestroyImmediate(clone);
            }
        }

        private bool _isBlack(Color color)
        {
            return BMath.Approximately(color.r, 0) &&
                   BMath.Approximately(color.g, 0) &&
                   BMath.Approximately(color.b, 0);
        }

        private Texture2D _cloneTexture(Texture texture)
        {
            Material material = BGraphic.CreateMaterial(this, Shader.Find("Unlit/Texture"));

            RenderTexture renderTexture = BGraphic.CreateTempRenderTexture(this, new RenderTextureData
                {
                    texture = new PlanarTextureData
                    {
                        size = texture.Size(),
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp
                    },
                    temp = new TempRenderTextureData
                    {
                        antiAliasing = 1,
                        readWrite = RenderTextureReadWrite.Linear
                    },
                    format = RenderTextureFormat.ARGB32,
                });
            
            BGraphic.Blit(new BlitParameters
            {
                shaderPass = 0,
                material = material,
                sourceTexture = texture,
                targetTexture = renderTexture
            });

            Object.DestroyImmediate(material);

            Texture2D resultTexture = BGraphic.CreateTexture2D(this, new Texture2DParameters
            {
                texture = new PlanarTextureData
                {
                    size = texture.Size(),
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    name = "TempToGetHeight",
                },
                format = TextureFormat.RGBA32,
                isLinear = true,
                mipChain = false,
                createInitialized = true,
            });
            
            BGraphic.BlitRenderTextureTo2D(renderTexture, resultTexture);
            
            BGraphic.Dispose(renderTexture);

            return resultTexture;
        }
    }
}