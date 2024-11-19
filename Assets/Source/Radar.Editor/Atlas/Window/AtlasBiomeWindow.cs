using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Editor;
using Beast.Claw.Editor.Window;
using Beast.Claw.Math;
using Beast.Radar.Editor.Tile;
using Beast.Radar.Terrain;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Editor.Atlas
{
    public class AtlasBiomeWindow : EditorWindow
    {
        [SerializeField] private WindowContainer<ITileExplorer> _explorer = new();
        
        protected override void Warmup()
        {
            _rectUI.BindDrawer<TileBiomeDrawer, TileBiomeTask>();
        }

        protected override void Reset()
        {
            
        }

        protected override void DrawUI()
        {
            _flow.Begin().If(_explorer.Window == null).Draw(ui =>
            {
                ui.WindowField("Select explorer", _explorer);
            });

            if (_explorer.Window == null)
            {
                return;
            }

            if (_explorer.Window.SelectedAsset == null)
            {
                _flow.Begin().InfoBox("No asset selected");
                return;
            }

            List<TerrainTileData> tiles = _explorer.Window.SelectedAsset.data.tiles;
            if (tiles.IsNullOrEmpty())
            {
                return;
            }

            _flow.Begin().Draw(ui =>
            {
                float screenSize = BMath.Min(position.width, position.height);

                Rect screen = ui.AllocateRect(screenSize);

                float paddingX = BMath.Max(0, screen.size.x * 0.5f - ((float2)screenSize).x * 0.5f);
                float paddingY = BMath.Max(0, screen.size.y * 0.5f - ((float2)screenSize).y * 0.5f);
                screen = screen.AddPadding(Padding.LeftRight(paddingX).WithTopDown(paddingY));

                _drawTiles(screen, tiles, TileBiomeTask.Mode.Background);
                _drawTiles(screen, tiles, TileBiomeTask.Mode.Label);
            });
        }

        private void _drawTiles(Rect screen, List<TerrainTileData> tiles, TileBiomeTask.Mode mode)
        {
            foreach (TerrainTileData tile in tiles)
            {
                if (!tile.isBiomeTile)
                {
                    continue;
                }
                    
                _rectUI.TempTaskWithDefaultStyle(out TileBiomeTask task);
                    
                task.tile = tile;
                task.isSelected = tile == _explorer.Window.SelectedTile;
                task.mode = mode;

                Rect tileRect = tile.SpawnRangeAsBounds().ToRect(screen.size);
                tileRect.position += screen.position;

                _rectUI.Draw(tileRect, task);
            }
        }

        protected override void Dispose()
        {
            
        }
    }
}