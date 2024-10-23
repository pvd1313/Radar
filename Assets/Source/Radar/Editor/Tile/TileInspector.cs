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
            
            Begin.If(_explorerObject == null).Draw(ui =>
            {
                ui.WindowField("Explorer", ref _explorer);
                
                _explorerObject = _explorer as Object;
            });

            if (_explorerObject == null)
            {
                Begin.InfoBox("Open and link explorer window");
                return;
            }

            TerrainTileData tile = _explorer.SelectedTile;
            if (tile == null)
            {
                Begin.InfoBox("No tile selected");
                return;
            }

            Begin.Record(_explorer.SelectedAsset, "Modify tile data").Draw(ui =>
            {
                ui.Validate(tile, tile.name ?? "tile");
                
                Begin.Horizontal(ui =>
                {
                    ui.Label("Texture:");
                    
                    ui.FlexibleSpace();
                    
                    Begin.Enabled(BAsset.IsDirty(_explorer.SelectedAsset)).LabelWidthAndButton("SaveAsset", () =>
                    {
                        BAsset.SaveIfDirty(_explorer.SelectedAsset);
                    });
                });
            
                bool spriteHasTexture = tile.texture != null;
                
                Begin.Tab().Horizontal().Draw(ui =>
                {
                    Begin.Enabled(false).PassiveLabel(spriteHasTexture ? tile.texture.name : "N/A");
                    
                    ui.AssetField(Label.NoLabel, ref tile.texture);
                });
            
                Begin.Horizontal(ui =>
                {
                    ui.StringField(nameof(tile.name), ref tile.name);
                    Begin.Enabled(spriteHasTexture).Option(_smallButtonSize).Button(UIStyle.CHAR_A, () =>
                    {
                        tile.name = tile.texture.name;
                    });
                });
                
                ui.IntField(nameof(tile.index), ref tile.index);
            
                Begin.Horizontal(ui =>
                {
                    ui.FloatField(nameof(tile.yCropHeight), ref tile.yCropHeight);
                    Begin.Enabled(spriteHasTexture).Option(_smallButtonSize).Button(UIStyle.CHAR_A, () =>
                    {
                        tile.yCropHeight = 1 - _helper.GetSpriteHeight(tile.texture);
                    });
                });
            
                Begin.Horizontal(ui =>
                {
                    ui.FloatField(nameof(tile.yBlendCenter), ref tile.yBlendCenter);
                    Begin.Enabled(spriteHasTexture).Option(_smallButtonSize).Button(UIStyle.CHAR_A, () =>
                    {
                        tile.yBlendCenter = _helper.GetSpriteHeight(tile.texture) * 0.5f;
                    });
                });
                
                Begin.Horizontal(ui =>
                {
                    ui.ColorField(nameof(tile.averageColor), ref tile.averageColor);
                    Begin.Enabled(spriteHasTexture).Option(_smallButtonSize).Button(UIStyle.CHAR_A, () =>
                    {
                        tile.averageColor = _helper.GetAverageColor(tile.texture);
                    });
                });

                //flow.Tab(ui =>
                {
                    ui.GenericField(nameof(tile.waterRange), ref tile.waterRange);
                    ui.GenericField(nameof(tile.forestRange), ref tile.forestRange);
                }//);
            });
        }

        protected override void Dispose()
        {
            
        }
    }
}