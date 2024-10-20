using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Editor;
using Beast.Claw.Math;
using Beast.Radar.Terrain;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using EditorWindow = Beast.Claw.Editor.Window.EditorWindow;

namespace Beast.Radar.Editor.Terrain
{
    public class TileExplorer : EditorWindow
    {
        private const float MIN_CELL_SIZE = 48;
        private const float MAX_CELL_SIZE = 128;
        
        [SerializeField] private ScriptableContainer<TerrainAtlas> _atlasAsset;
        [SerializeField] private float2 _scrollPosition;
        [SerializeField] private float _cellSize;
        
        private GUILayoutOption _sliderWidth;
        private string _assetNameDefault;
        
        protected override void Warmup()
        {
            _sliderWidth = GUILayout.Width(100);
            _assetNameDefault = "ScriptableContainer<TerrainAtlas>";
        }

        protected override void InspectorUI()
        {
            flow.Horizontal(ui =>
            {
                if (_atlasAsset == null)
                {
                    flow.FieldWidth(100).LabelWidth(_assetNameDefault).AssetField(_assetNameDefault, ref _atlasAsset);
                }
                else
                {
                    flow.FieldWidth(_atlasAsset.name).AssetField(SmartLabel.NoLabel, ref _atlasAsset);
                    
                    ui.FlexibleSpace();
                
                    FloatMinMax minMax = new (MIN_CELL_SIZE, MAX_CELL_SIZE);
                    flow.Option(_sliderWidth).
                        FieldWidth(0).
                        Draw(ui => ui.FloatSlider(SmartLabel.NoLabel, minMax, ref _cellSize));
                }
            });

            if (_atlasAsset == null)
            {
                flow.InfoBox(AtlasEditor.WELCOME_MESSAGE);
                return;
            }

            flow.Vertical().Scroll(ref _scrollPosition, ui =>
            {
                List<TerrainTileSprite> sprites = _atlasAsset.data.sprites;

                int spriteCount = 24;

                RectCellGrid rectCellGrid = new (new RectCellGrid.Input
                {
                    spacing = 5,
                    windowSize = new float2(position.width, position.height),
                    cellSize = _cellSize,
                    elementCount = spriteCount,
                    padding = 10
                });

                ui.AllocateRect(rectCellGrid.TotalSize);

                for (int i = 0; i < spriteCount; i++)
                {
                    Rect rect = rectCellGrid.GetElement(i);

                    DrawItem(rect);
                }
            });

            BDebug.Track(this, nameof(_cellSize), _cellSize);
        }

        private void DrawItem(Rect rect)
        {
            EditorGUI.DrawRect(rect, Color.black);
        }

        protected override void Dispose()
        {
            
        }
    }
}