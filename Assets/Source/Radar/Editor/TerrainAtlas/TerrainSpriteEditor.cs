using System;
using System.Collections.Generic;
using Beast.Claw.Collection;
using Beast.Claw.Editor;
using Beast.Radar.Graphic.Terrain;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Editor.TerrainAtlas
{
    [Serializable]
    public class TerrainSpriteEditor
    {
        private IEditorFlow flow => _window.flow;
        
        [SerializeField] TerrainAtlasEditor _window;
        [SerializeField] private float2 _scrollPosition;
        [SerializeField] private GUILayoutOption _smallButtonSize = GUILayout.Width(EditorUI.OneCharButtonSize);
        [SerializeField] private bool _mainFold;
        [SerializeField] private UnitySerializableDictionary<int, bool> _spriteFolds = new();
        [SerializeField] private TerrainSpriteHelper _helper = new ();

        public TerrainSpriteEditor(TerrainAtlasEditor window)
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

            flow.If(_mainFold).Indent().Vertical().Scroll(ref _scrollPosition, ui =>
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    isDirty |= _spriteUI(sprites, i, ref swapIndex, ref deleteIndex);
                }
            });

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
            
            EditorUIResult spyResult = flow.If(spriteFold).Indent().Spy(ui =>
            {
                ui.AssetField(nameof(sprite.texture), ref sprite.texture);
                ui.StringField(nameof(sprite.name), ref sprite.name);
                ui.IntField(nameof(sprite.index), ref sprite.index);

                flow.Horizontal(ui =>
                {
                    ui.FloatField(nameof(sprite.yCropHeight), ref sprite.yCropHeight);
                    flow.Enabled(sprite.texture != null).Option(_smallButtonSize).Button(EditorUI.CharA, () =>
                    {
                        sprite.yCropHeight = 1 - _helper.GetSpriteHeight(sprite.texture);
                    });
                });

                flow.Horizontal(ui =>
                {
                    ui.FloatField(nameof(sprite.yBlendCenter), ref sprite.yBlendCenter);
                    flow.Enabled(sprite.texture != null).Option(_smallButtonSize).Button(EditorUI.CharA, () =>
                    {
                        sprite.yBlendCenter = _helper.GetSpriteHeight(sprite.texture) * 0.5f;
                    });
                });
                
                flow.Horizontal(ui =>
                {
                    ui.ColorField(nameof(sprite.averageColor), ref sprite.averageColor);
                    flow.Enabled(sprite.texture != null).Option(_smallButtonSize).Button(EditorUI.CharA, () =>
                    {
                        sprite.averageColor = _helper.GetAverageColor(sprite.texture);
                    });
                });
                
                ui.Space(EditorUI.MinorSpace);
            });
            
            swapIndex = swapIndexLocal;
            deleteIndex = deleteIndexLocal;

            return spyResult.changesDetected;
        }
    }
}