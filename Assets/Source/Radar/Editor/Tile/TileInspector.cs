using Beast.Claw.Editor;
using Beast.Radar.Editor.Atlas;
using Beast.Radar.Terrain;
using UnityEngine;
using EditorWindow = Beast.Claw.Editor.Window.EditorWindow;

namespace Beast.Radar.Editor.Tile
{
    public class TileInspector : EditorWindow
    {
        [SerializeField] private Object _explorerObject;

        private ITileExplorer _explorer;
        private GUILayoutOption _smallButtonSize;
        private TextureHelper _helper;

        protected override void Warmup()
        {
            _smallButtonSize = GUILayout.Width(UIStyle.SIZE_ONE_CHAR);
            _helper = new TextureHelper();
        }

        protected override void Reset()
        {
            _explorerObject = null;
        }

        protected override void DrawUI()
        {
            _explorer = _explorerObject as ITileExplorer;
            
            flow.If(_explorerObject == null).Draw(ui =>
            {
                ui.WindowField("Explorer", ref _explorer);
                
                _explorerObject = _explorer as Object;
            });

            if (_explorerObject == null)
            {
                flow.InfoBox("Open and link explorer window");
                return;
            }

            TerrainTileData tile = _explorer.SelectedTile;
            if (tile == null)
            {
                flow.InfoBox("No tile selected");
                return;
            }

            flow.Record(_explorer.SelectedAsset, "Modify tile data").Draw(ui =>
            {
                flow.Horizontal(ui =>
                {
                    ui.Label("Texture:");
                    
                    ui.FlexibleSpace();
                    
                    flow.Enabled(BAsset.IsDirty(_explorer.SelectedAsset)).LabelWidthAndButton("SaveAsset", () =>
                    {
                        BAsset.SaveIfDirty(_explorer.SelectedAsset);
                    });
                });
            
                bool spriteHasTexture = tile.texture != null;
                
                flow.Tab().Horizontal().Draw(ui =>
                {
                    flow.Enabled(false).PassiveLabel(spriteHasTexture ? tile.texture.name : "N/A");
                    
                    ui.AssetField(Label.NoLabel, ref tile.texture);
                });
            
                flow.Horizontal(ui =>
                {
                    ui.StringField(nameof(tile.name), ref tile.name);
                    flow.Enabled(spriteHasTexture).Option(_smallButtonSize).Button(UIStyle.CHAR_A, () =>
                    {
                        tile.name = tile.texture.name;
                    });
                });
                
                ui.IntField(nameof(tile.index), ref tile.index);
            
                flow.Horizontal(ui =>
                {
                    ui.FloatField(nameof(tile.yCropHeight), ref tile.yCropHeight);
                    flow.Enabled(spriteHasTexture).Option(_smallButtonSize).Button(UIStyle.CHAR_A, () =>
                    {
                        tile.yCropHeight = 1 - _helper.GetSpriteHeight(tile.texture);
                    });
                });
            
                flow.Horizontal(ui =>
                {
                    ui.FloatField(nameof(tile.yBlendCenter), ref tile.yBlendCenter);
                    flow.Enabled(spriteHasTexture).Option(_smallButtonSize).Button(UIStyle.CHAR_A, () =>
                    {
                        tile.yBlendCenter = _helper.GetSpriteHeight(tile.texture) * 0.5f;
                    });
                });
                
                flow.Horizontal(ui =>
                {
                    ui.ColorField(nameof(tile.averageColor), ref tile.averageColor);
                    flow.Enabled(spriteHasTexture).Option(_smallButtonSize).Button(UIStyle.CHAR_A, () =>
                    {
                        tile.averageColor = _helper.GetAverageColor(tile.texture);
                    });
                });

                ui.ExperimentalGenericField(nameof(tile.spawnWaterRange), ref tile.spawnWaterRange);
                ui.ExperimentalGenericField(nameof(tile.spawnForestRange), ref tile.spawnForestRange);
            });
        }

        protected override void Dispose()
        {
            
        }
    }
}