using System;
using System.Collections.Generic;
using Beast.Claw.Collection;
using Beast.Claw.Debug;
using Beast.Claw.Editor;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Radar.Graphic.Terrain;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beast.Radar.Editor
{
    [Serializable]
    public class TerrainAtlasSpriteEditor
    {
        private EditorFlow flow => _window.flow;
        
        [SerializeField] TerrainAtlasEditor _window;
        [SerializeField] private float2 _scrollPosition;
        [SerializeField] private GUILayoutOption _smallButtonSize = GUILayout.Width(EditorUI.SingleCharButtonSize);
        [SerializeField] private bool _mainFold;
        [SerializeField] private UnitySerializableDictionary<int, bool> _spriteFolds = new();

        public TerrainAtlasSpriteEditor(TerrainAtlasEditor window)
        {
            _window = window;
        }

        public bool DrawUI(List<TerrainTileSprite> sprites)
        {
            bool isDirty = false;

            flow.Horizontal(ui =>
            {
                ui.Foldout($"Sprites ({sprites.Count})", ref _mainFold);

                flow.Option(_smallButtonSize).Button(EditorUI.CharPlus, () =>
                {
                    sprites.Add(new TerrainTileSprite());
                    isDirty = true;
                });
            });

            (int a, int b) swapIndex = default;
            int deleteIndex = -1;

            if (_mainFold)
            {
                flow.Indent().Vertical().Scroll(ref _scrollPosition, ui =>
                {
                    for (int i = 0; i < sprites.Count; i++)
                    {
                        isDirty |= _spriteUI(sprites, i, ref swapIndex, ref deleteIndex);
                    }
                });
            }

            if (sprites.TrySwap(swapIndex.a, swapIndex.b))
            {
                isDirty = true;
            }

            if (sprites.TryRemoveAt(deleteIndex))
            {
                isDirty = true;
            }

            return isDirty;
        }

        private bool _spriteUI(
            IList<TerrainTileSprite> spriteList,
            int spriteIndex, 
            ref (int a, int b) swapIndex,
            ref int deleteIndex)
        {
            TerrainTileSprite sprite = spriteList[spriteIndex];

            (int a, int b) swapIndexLocal = swapIndex;
            int deleteIndexLocal = deleteIndex;
            
            _spriteFolds.TryGetValue(spriteIndex, out bool spriteFold);

            flow.Horizontal(ui =>
            {
                spriteFold = ui.Foldout(sprite.name, spriteFold);

                flow.Option(_smallButtonSize, ui =>
                {
                    ui.Button(EditorUI.CharUpArrow, () => swapIndexLocal = (spriteIndex, spriteIndex - 1));
                    ui.Button(EditorUI.CharDownArrow, () => swapIndexLocal = (spriteIndex, spriteIndex + 1));
                    ui.Button(EditorUI.CharCross, () => deleteIndexLocal = spriteIndex);
                });

                _spriteFolds[spriteIndex] = spriteFold;
            });
            
            swapIndex = swapIndexLocal;
            deleteIndex = deleteIndexLocal;

            if (!spriteFold)
            {
                return false;
            }

            EditorUIResult spyResult = flow.Indent().Spy(ui =>
            {
                ui.AssetField(nameof(sprite.texture), ref sprite.texture);
                ui.StringField(nameof(sprite.name), ref sprite.name);
                ui.IntField(nameof(sprite.index), ref sprite.index);

                flow.Horizontal(ui =>
                {
                    ui.FloatField(nameof(sprite.yCropHeight), ref sprite.yCropHeight);
                    flow.Enabled(sprite.texture != null).Option(_smallButtonSize).Button(EditorUI.CharA, () =>
                    {
                        sprite.yCropHeight = 1 - GetSpriteHeight(sprite.texture);
                    });
                });

                flow.Horizontal(ui =>
                {
                    ui.FloatField(nameof(sprite.yBlendCenter), ref sprite.yBlendCenter);
                    flow.Enabled(sprite.texture != null).Option(_smallButtonSize).Button(EditorUI.CharA, () =>
                    {
                        sprite.yBlendCenter = GetSpriteHeight(sprite.texture) * 0.5f;
                    });
                });
                
                flow.Horizontal(ui =>
                {
                    ui.ColorField(nameof(sprite.averageColor), ref sprite.averageColor);
                    flow.Enabled(sprite.texture != null).Option(_smallButtonSize).Button(EditorUI.CharA, () =>
                    {
                        sprite.averageColor = GetAverageColor(sprite.texture);
                    });
                });
                
                ui.Space(8);
            });

            return spyResult.changesDetected;
        }

        private float GetSpriteHeight(Texture texture)
        {
            Texture2D clone = CloneTexture(texture);

            Color[] pixels = clone.GetPixels();
            int lastNonBlackRow = clone.height - 1;

            for (int y = 0; y < clone.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    int j = y * clone.height + x;

                    if (!IsBlack(pixels[j]))
                    {
                        lastNonBlackRow = y;
                        lastNonBlackRow = y;
                        break;
                    };
                }
            }
            
            Object.DestroyImmediate(clone);

            return (lastNonBlackRow + 1) / (float)texture.height;
        }

        private Color GetAverageColor(Texture texture)
        {
            Texture2D clone = CloneTexture(texture);
            Color[] pixels = clone.GetPixels();
            double avgR = 0;
            double avgG = 0;
            double avgB = 0;
            int pixelCount = 0;

            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];
                
                if (!IsBlack(pixels[i]))
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

        private bool IsBlack(Color color)
        {
            return BMath.Approximately(color.r, 0) &&
                   BMath.Approximately(color.g, 0) &&
                   BMath.Approximately(color.b, 0);
        }

        private Texture2D CloneTexture(Texture texture)
        {
            Material material = BGraphic.CreateMaterial(this, Shader.Find("Unlit/Texture"));

            RenderTexture renderTexture = BGraphic.CreateTemporaryRenderTexture(this, 
                new NewTempRenderTextureData
                {
                    antiAliasing = 1,
                    filterMode = FilterMode.Point,
                    size = texture.Size(),
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

            Texture2D resultTexture = BGraphic.CreateTexture2DFromRenderTexture(this, new Texture2DParametersA
            {
                sourceTexture = renderTexture,
                format = TextureFormat.RGBA32,
                isLinear = true,
                mipChain = false,
                name = "TempForGetHeight"
            });
            
            BGraphic.DisposeRenderTexture(renderTexture);

            return resultTexture;
        }
    }
}