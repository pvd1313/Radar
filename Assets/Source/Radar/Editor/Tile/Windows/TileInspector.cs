using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Common;
using Beast.Claw.Editor;
using Beast.Radar.Editor.Atlas;
using Beast.Radar.Terrain;
using UnityEngine;
using EditorWindow = Beast.Claw.Editor.Window.EditorWindow;

namespace Beast.Radar.Editor.Tile
{
    public class TileInspector : EditorWindow
    {
        [SerializeField] private WindowContainer<ITileExplorer> _explorer = new();
        
        private GUILayoutOption _smallButtonSize;
        private TextureHelper _helper;

        protected override void Warmup()
        {
            _smallButtonSize = GUILayout.Width(UIStyle.SIZE_ONE_CHAR);
            _helper = new TextureHelper();
        }

        protected override void Reset()
        {
            _explorer.SetReference(null);
        }

        protected override void DrawUI()
        {
            Begin.If(_explorer.Window == null).Draw(ui =>
            {
                ui.WindowField("Select Explorer", _explorer);
            });

            if (_explorer.Window == null)
            {
                return;
            }

            TerrainTileData tile = _explorer.Window.SelectedTile;
            if (tile == null)
            {
                Begin.InfoBox("No tile selected");
                return;
            }

            ScriptableContainer<TerrainAtlas> atlasAsset = _explorer.Window.SelectedAsset;

            Begin.Record(atlasAsset, "Modify tile data").Draw(ui =>
            {
                ui.Validate(tile, tile.name ?? "tile");
                
                Begin.Horizontal(ui =>
                {
                    ui.Label("Texture:");
                    
                    ui.FlexibleSpace();
                    
                    Begin.Enabled(BAsset.IsDirty(atlasAsset)).LabelWidthAndButton("SaveAsset", () =>
                    {
                        BAsset.SaveIfDirty(atlasAsset);
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
                
                Begin.Horizontal(ui =>
                {
                    ui.IntField(nameof(tile.index), ref tile.index);
                    Begin.Option(_smallButtonSize).Button(UIStyle.CHAR_A, () =>
                    {
                        tile.index = _findNextFreeIndex(atlasAsset.data);
                    });
                });
            
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

                ui.CheckBoxRight(nameof(tile.isBiomeTile), ref tile.isBiomeTile);

                Begin.Enabled(tile.isBiomeTile, ui =>
                {
                    ui.GenericField(nameof(tile.biomeWaterRange), ref tile.biomeWaterRange);
                    ui.GenericField(nameof(tile.biomeForestRange), ref tile.biomeForestRange);
                });
            });
        }

        private int _findNextFreeIndex(TerrainAtlas atlas)
        {
            HashSet<int> occupiedIndexes = atlas.tiles.Select(i => i.index).ToHashSet();

            int i = 0;
            for (; i < occupiedIndexes.Count && occupiedIndexes.Contains(i); i++)
            {
                
            }

            if (occupiedIndexes.Contains(i))
            {
                i++;
            }

            return i;
        }

        protected override void Dispose()
        {
            
        }
    }
}