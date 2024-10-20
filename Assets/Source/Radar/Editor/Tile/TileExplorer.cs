using System.Collections.Generic;
using Beast.Claw.Collection;
using Beast.Claw.Common;
using Beast.Claw.Editor;
using Beast.Claw.Math;
using Beast.Radar.Editor.Terrain;
using Beast.Radar.Terrain;
using Unity.Mathematics;
using UnityEngine;

using EditorWindow = Beast.Claw.Editor.Window.EditorWindow;
using Object = UnityEngine.Object;

namespace Beast.Radar.Editor.Tile
{
    public class TileExplorer : EditorWindow
    {
        private const float MIN_CELL_SIZE = 48;
        private const float MAX_CELL_SIZE = 128;
        
        [SerializeField] private ScriptableContainer<TerrainAtlas> _atlas;
        [SerializeField] private float2 _scrollPosition;
        [SerializeField] private float _cellSize;
        [SerializeField] private int _selectedIndex;
        [SerializeField] private bool _assetFieldShown = true;
        
        private GUILayoutOption _sliderWidth;
        private string _assetNameDefault;
        private Object[] _thisAndAtlas;

        private RectUI _rectUI;

        protected override void Warmup()
        {
            _sliderWidth = GUILayout.Width(60);
            _assetNameDefault = "ScriptableContainer<TerrainAtlas>";
            
            _rectUI = new RectUI();
            _rectUI.Bind<TileDrawer, TileDrawerTask>();
            _thisAndAtlas = new Object[2];
        }
        
        protected override void Reset()
        {
            
        }

        protected override void InspectorUI()
        {
            if (_atlas == null || 
                _atlas.data.tiles == null || 
                !_atlas.data.tiles.IsIndexInRange(_selectedIndex))
            {
                _selectedIndex = -1;
            }

            _thisAndAtlas[0] = this;
            _thisAndAtlas[1] = _atlas;
            
            flow.Horizontal(ui =>
            {
                if (_atlas == null)
                {
                    flow.FieldWidth(100).LabelWidth(_assetNameDefault).AssetField(_assetNameDefault, ref _atlas);
                }
                else
                {
                    flow.If(_assetFieldShown).FieldWidth(_atlas.name).AssetField(Label.NoLabel, ref _atlas);
                    flow.If(_assetFieldShown).Enabled(BAsset.IsDirty(_atlas)).LabelWidthAndButton("Save", () =>
                    {
                        BAsset.SaveIfDirty(_atlas);
                    });

                    flow.LabelWidthAndButton(":", () => _assetFieldShown = !_assetFieldShown);
                    
                    ui.FlexibleSpace();

                    bool hasTiles = _atlas.data.tiles.NotNullAndHasItems();
                    int tileCount = hasTiles ? _atlas.data.tiles.Count : 0;

                    flow.Enabled(_selectedIndex > 0).LabelWidthAndButton("<<", _moveTileLeftMax);
                    flow.Enabled(_selectedIndex > 0).LabelWidthAndButton("<", _moveTileLeft);
                    flow.Enabled(hasTiles && _selectedIndex < tileCount - 1).LabelWidthAndButton(">", _moveTileRight);
                    flow.Enabled(hasTiles && _selectedIndex < tileCount - 1).LabelWidthAndButton(">>", _moveTileRightMax);
                    flow.LabelWidthAndButton("NEW", _createNewItem);
                    flow.Enabled(_selectedIndex >= 0).LabelWidthAndButton("DUP", _duplicateSelectedItem);
                    flow.Enabled(_selectedIndex >= 0).LabelWidthAndButton("DEL", _deleteSelectedItem);
                
                    FloatMinMax minMax = new (MIN_CELL_SIZE, MAX_CELL_SIZE);
                    flow.Option(_sliderWidth).
                        FieldWidth(0).
                        Draw(ui => ui.FloatSlider(Label.NoLabel, minMax, ref _cellSize));
                }
            });

            if (_atlas == null)
            {
                flow.InfoBox(AtlasEditor.WELCOME_MESSAGE);
                return;
            }

            flow.If(_atlas.data.tiles.NotNullAndHasItems()).Vertical().Scroll(ref _scrollPosition, ui =>
            {
                List<TerrainTileData> tiles = _atlas.data.tiles;

                RectCellGrid cellGrid = new (new RectCellGrid.Input
                {
                    spacing = 5,
                    windowSize = new float2(position.width, position.height),
                    cellSize = _cellSize,
                    itemCount = tiles.Count,
                    padding = new Padding(10, 10, 5, 10)
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

                    if (_selectedIndex != task.selectedIndex)
                    {
                        ui.Record(this, "Atlas new selected index");
                        _selectedIndex = task.selectedIndex;
                    }
                }
            });
        }

        private void _createNewItem(FlowUI ui)
        {
            ui.RecordMultiple(_thisAndAtlas, "Atlas new item");
                        
            if (_atlas.data.tiles == null)
            {
                _atlas.data.tiles = new List<TerrainTileData>();
            }
                        
            _atlas.data.tiles.Add(new TerrainTileData());

            _selectedIndex = _atlas.data.tiles.Count - 1;
        }
        
        private void _deleteSelectedItem(FlowUI ui)
        {
            ui.RecordMultiple(_thisAndAtlas, "Atlas new item");
                        
            _atlas.data.tiles.RemoveAt(_selectedIndex);

            _selectedIndex--;
        }

        private void _duplicateSelectedItem(FlowUI ui)
        {
            ui.RecordMultiple(_thisAndAtlas, "Atlas duplicate item");
            
            const string arrayPath = nameof(_atlas.data) + "." + nameof(_atlas.data.tiles);

            _selectedIndex = BAsset.DuplicateArrayElement(_atlas, arrayPath, _selectedIndex);
        }

        private void _moveTileLeft(FlowUI ui)
        {
            ui.RecordMultiple(_thisAndAtlas, "Atlas move item left");

            if (_atlas.data.tiles.TrySwap(_selectedIndex, _selectedIndex - 1))
            {
                _selectedIndex--;
            }
        }
        
        private void _moveTileRight(FlowUI ui)
        {
            ui.RecordMultiple(_thisAndAtlas, "Atlas move item left");

            if (_atlas.data.tiles.TrySwap(_selectedIndex, _selectedIndex + 1))
            {
                _selectedIndex++;
            }
        }
        
        private void _moveTileLeftMax(FlowUI ui)
        {
            ui.RecordMultiple(_thisAndAtlas, "Atlas move item max left");

            if (_atlas.data.tiles.TryMoveEditorOnly(_selectedIndex, 0))
            {
                _selectedIndex = 0;
            }
        }
        
        private void _moveTileRightMax(FlowUI ui)
        {
            ui.RecordMultiple(_thisAndAtlas, "Atlas move item max right");

            if (_atlas.data.tiles.TryMoveEditorOnly(_selectedIndex, _atlas.data.tiles.Count - 1))
            {
                _selectedIndex = _atlas.data.tiles.Count - 1;
            }
        }

        protected override void Dispose()
        {
            
        }
    }
}