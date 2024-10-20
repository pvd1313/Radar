using System;
using System.Collections.Generic;
using Beast.Claw.Collection;
using Beast.Claw.Editor;
using Beast.Radar.Terrain;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Editor.Terrain
{
    [Serializable]
    public class TileEditor
    {
        private IEditorFlow flow => _window.flow;
        
        [SerializeField] AtlasEditor _window;
        [SerializeField] private float2 _scrollPosition;
        [SerializeField] private GUILayoutOption _smallButtonSize = GUILayout.Width(UIStyle.SIZE_ONE_CHAR);
        [SerializeField] private bool _mainFold;
        [SerializeField] private UnitySerializableDictionary<int, bool> _spriteFolds = new();
        [SerializeField] private TextureHelper _helper = new ();

        public TileEditor(AtlasEditor window)
        {
            _window = window;
        }

        public void DrawUI(List<TerrainTileSprite> sprites, out bool changesDetected)
        {
            bool changesDetectedLocal = false;

            flow.Horizontal(ui =>
            {
                ui.Foldout($"Sprites ({sprites.Count})", ref _mainFold);

                flow.Option(_smallButtonSize).Button(UIStyle.CHAR_PLUS, () =>
                {
                    sprites.Add(new TerrainTileSprite());
                    changesDetectedLocal = true;
                });
            });

            (int a, int b) swapIndex = default;
            int deleteIndex = -1;

            flow.If(_mainFold).Tab().Vertical().Scroll(ref _scrollPosition, ui =>
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    changesDetectedLocal |= _spriteUI(sprites, i, ref swapIndex, ref deleteIndex);
                }
            });

            if (sprites.TrySwap(swapIndex.a, swapIndex.b))
            {
                changesDetectedLocal = true;
            }

            if (sprites.TryRemoveAt(deleteIndex))
            {
                changesDetectedLocal = true;
            }

            changesDetected = changesDetectedLocal;
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
                ui.Foldout(sprite.name, ref spriteFold);

                flow.Option(_smallButtonSize, ui =>
                {
                    ui.Button(UIStyle.CHAR_ARROW_UP, () => swapIndexLocal = (spriteIndex, spriteIndex - 1));
                    ui.Button(UIStyle.CHAR_ARROW_DOWN, () => swapIndexLocal = (spriteIndex, spriteIndex + 1));
                    ui.Button(UIStyle.CHAR_CROSS, () => deleteIndexLocal = spriteIndex);
                });

                _spriteFolds[spriteIndex] = spriteFold;
            });
            
            EditorUIResult spyResult = flow.If(spriteFold).Tab().Spy(ui =>
            {
                ui.PassiveLabel("Texture");

                bool spriteHasTexture = sprite.texture != null;
                
                flow.Tab().Horizontal().Draw(ui =>
                {
                    flow.Enabled(false).PassiveLabel(spriteHasTexture ? sprite.texture.name : "N/A");
                    
                    ui.AssetField(SmartLabel.NoLabel, ref sprite.texture);
                });

                flow.Horizontal(ui =>
                {
                    ui.StringField(nameof(sprite.name), ref sprite.name);
                    flow.Enabled(spriteHasTexture).Option(_smallButtonSize).Button(UIStyle.CHAR_A, () =>
                    {
                        sprite.name = sprite.texture.name;
                    });
                });
                
                ui.IntField(nameof(sprite.index), ref sprite.index);

                flow.Horizontal(ui =>
                {
                    ui.FloatField(nameof(sprite.yCropHeight), ref sprite.yCropHeight);
                    flow.Enabled(spriteHasTexture).Option(_smallButtonSize).Button(UIStyle.CHAR_A, () =>
                    {
                        sprite.yCropHeight = 1 - _helper.GetSpriteHeight(sprite.texture);
                    });
                });

                flow.Horizontal(ui =>
                {
                    ui.FloatField(nameof(sprite.yBlendCenter), ref sprite.yBlendCenter);
                    flow.Enabled(spriteHasTexture).Option(_smallButtonSize).Button(UIStyle.CHAR_A, () =>
                    {
                        sprite.yBlendCenter = _helper.GetSpriteHeight(sprite.texture) * 0.5f;
                    });
                });
                
                flow.Horizontal(ui =>
                {
                    ui.ColorField(nameof(sprite.averageColor), ref sprite.averageColor);
                    flow.Enabled(spriteHasTexture).Option(_smallButtonSize).Button(UIStyle.CHAR_A, () =>
                    {
                        sprite.averageColor = _helper.GetAverageColor(sprite.texture);
                    });
                });
                
                ui.Space(UIStyle.SIZE_MINOR_SPACE);
            });
            
            swapIndex = swapIndexLocal;
            deleteIndex = deleteIndexLocal;

            return spyResult.changesDetected;
        }
    }
}