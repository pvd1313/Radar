using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Editor;
using Beast.Claw.Math;
using Beast.Radar.Terrain;
using Unity.Mathematics;
using UnityEngine;

using EditorWindow = Beast.Claw.Editor.Window.EditorWindow;
using Object = UnityEngine.Object;

namespace Beast.Radar.Editor.Tile
{
    public class TileExplorer : EditorWindow, ITileExplorer
    {
        private static readonly FloatMinMax CELL_SIZE = new (48, 196);
        
        public ScriptableContainer<TerrainAtlas> SelectedAsset => _atlas;

        public TerrainTileData SelectedTile
        {
            get
            {
                if (_atlas == null || 
                    _atlas.data.tiles == null || 
                    !_atlas.data.tiles.IsIndexInRange(_selectedIndex))
                {
                    return null;
                }

                return _atlas.data.tiles[_selectedIndex];
            }
        }
        
        [SerializeField] private ScriptableContainer<TerrainAtlas> _atlas;
        [SerializeField] private float2 _scrollPosition;
        [SerializeField] private float _cellSize = BMath.Lerp(CELL_SIZE, 0.5f);
        [SerializeField] private int _selectedIndex;
        [SerializeField] private bool _assetFieldShown = true;
        [SerializeField] private bool _spawnRangeDisplay;
        
        private GUILayoutOption _sliderWidth;
        private Object[] _thisAndAtlas;

        protected override void Warmup()
        {
            _sliderWidth = GUILayout.Width(60);
            
            _rectUI.BindDrawer<TileDrawer, TileTask>();
            
            _thisAndAtlas = new Object[2];
        }
        
        protected override void Reset()
        {
            
        }

        protected override void DrawUI()
        {
            if (_atlas == null || 
                _atlas.data.tiles == null || 
                !_atlas.data.tiles.IsIndexInRange(_selectedIndex))
            {
                _selectedIndex = -1;
            }

            _thisAndAtlas[0] = this;
            _thisAndAtlas[1] = _atlas;

            Begin.If(_atlas != null).Draw(ui =>
            {
                ui.Validate(_atlas.data, "Atlas");
            });

            Begin.Horizontal(ui =>
            {
                Begin.If(_assetFieldShown || _atlas == null)
                    .Record(this, "Change tile atlas")
                    .FieldWidth(100)
                    .AssetField(Label.NoLabel, ref _atlas);

                if (_atlas == null)
                {
                    return;
                }

                Begin.If(_assetFieldShown).Enabled(BAsset.IsDirty(_atlas)).LabelWidthAndButton("Save", () =>
                {
                    BAsset.SaveIfDirty(_atlas);
                });

                Begin.LabelWidthAndButton(":", () => _assetFieldShown = !_assetFieldShown);

                ui.FlexibleSpace();

                bool hasTiles = _atlas.data.tiles.NotNullAndHasItems();
                int tileCount = hasTiles ? _atlas.data.tiles.Count : 0;

                Begin.LabelWidthAndButton("S", () => _spawnRangeDisplay = !_spawnRangeDisplay);
                
                Begin.Enabled(_selectedIndex > 0).LabelWidthAndButton("<<", _moveTileLeftMax);
                Begin.Enabled(_selectedIndex > 0).LabelWidthAndButton("<", _moveTileLeft);
                Begin.Enabled(hasTiles && _selectedIndex < tileCount - 1).LabelWidthAndButton(">", _moveTileRight);
                Begin.Enabled(hasTiles && _selectedIndex < tileCount - 1).LabelWidthAndButton(">>", _moveTileRightMax);
                Begin.LabelWidthAndButton("NEW", _createNewItem);
                Begin.Enabled(_selectedIndex >= 0).LabelWidthAndButton("DUP", _duplicateSelectedItem);
                Begin.Enabled(_selectedIndex >= 0).LabelWidthAndButton("DEL", _deleteSelectedItem);

                Begin.Option(_sliderWidth).FieldWidth(0)
                    .Draw(ui => ui.FloatSlider(Label.NoLabel, CELL_SIZE, ref _cellSize));
            });

            if (_atlas == null)
            {
                Begin.InfoBox("Open or create atlas");
                return;
            }

            Begin.If(_atlas.data.tiles.NotNullAndHasItems()).Vertical().Scroll(ref _scrollPosition, ui =>
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
                    _rectUI.TempTaskWithDefaultStyle(out TileTask task);
                    {
                        task.tile = tiles[i];
                        task.selectedIndex = _selectedIndex;
                        task.tileIndex = i;
                        task.displaySpawnRange = _spawnRangeDisplay;
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

            if (_atlas.data.tiles.TryMove(_selectedIndex, 0))
            {
                _selectedIndex = 0;
            }
        }
        
        private void _moveTileRightMax(FlowUI ui)
        {
            ui.RecordMultiple(_thisAndAtlas, "Atlas move item max right");

            if (_atlas.data.tiles.TryMove(_selectedIndex, _atlas.data.tiles.Count - 1))
            {
                _selectedIndex = _atlas.data.tiles.Count - 1;
            }
        }

        protected override void Dispose()
        {
            
        }
    }
}