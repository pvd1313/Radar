using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Editor;
using Beast.Claw.Math;
using Beast.Radar.Terrain;
using Unity.Mathematics;
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
        private int _selectedIndex;

        private RectUI _rectUI;

        protected override void Warmup()
        {
            _sliderWidth = GUILayout.Width(100);
            _assetNameDefault = "ScriptableContainer<TerrainAtlas>";
            
            _rectUI = new RectUI();
            _rectUI.Bind<TileDrawer, TileDrawerTask>();
        }
        
        protected override void Reset()
        {
            _selectedIndex = -1;
            
            _rectUI = new RectUI();
            _rectUI.Bind<TileDrawer, TileDrawerTask>();
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
                    flow.FieldWidth(_atlasAsset.name).AssetField(Label.NoLabel, ref _atlasAsset);
                    
                    ui.FlexibleSpace();
                    
                    DrawResetButton();
                
                    FloatMinMax minMax = new (MIN_CELL_SIZE, MAX_CELL_SIZE);
                    flow.Option(_sliderWidth).
                        FieldWidth(0).
                        Draw(ui => ui.FloatSlider(Label.NoLabel, minMax, ref _cellSize));
                }
            });

            if (_atlasAsset == null)
            {
                flow.InfoBox(AtlasEditor.WELCOME_MESSAGE);
                return;
            }

            flow.Vertical().Scroll(ref _scrollPosition, ui =>
            {
                List<TerrainTileData> tiles = _atlasAsset.data.tiles;

                RectCellGrid cellGrid = new (new RectCellGrid.Input
                {
                    spacing = 5,
                    windowSize = new float2(position.width, position.height),
                    cellSize = _cellSize,
                    elementCount = tiles.Count,
                    padding = 10
                });

                ui.AllocateRect(cellGrid.TotalSize);

                for (int i = 0; i < tiles.Count; i++)
                {
                    _rectUI.Temp(out TileDrawerTask task);
                    {
                        task.tile = tiles[i];
                        task.selectedIndex = _selectedIndex;
                        task.tileIndex = i;
                    }
                    _rectUI.Draw(cellGrid.GetElement(i), task);
                }
            });

            BDebug.Track(this, nameof(_cellSize), _cellSize);
        }

        protected override void Dispose()
        {
            
        }
    }
}