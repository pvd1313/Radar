using System.IO;
using Beast.Claw;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Editor;
using Beast.Radar.Terrain;
using UnityEditor;
using UnityEngine;

namespace Beast.Radar.Editor.Atlas
{
    public class AtlasInspector : Beast.Claw.Editor.Window.EditorWindow
    {
        private const string MAIN_POSTFIX = "_main.png";
        private const string COLOR_POSTFIX = "_color.png";
        private const string CONFIG_POSTFIX = "_config.png";
        private const string NAME_SAVE = "Save";
        private const string NAME_OVERRIDE = "Override";
        
        [SerializeField] private ScriptableContainer<TerrainAtlas> _asset;
        
        private GUILayoutOption _bakeButtonSize;
        private TextureImporter _textureImporter;
        private bool _texturesFold;
        private TextureBaker _textureBaker;

        protected override void Warmup()
        {
            _bakeButtonSize = GUILayout.Width(100);
            _textureImporter = new TextureImporter();
            _textureBaker = new TextureBaker();
        }
        
        protected override void Reset()
        {
            
        }

        protected override void DrawUI()
        {
            Begin.AssetField(Label.NoLabel, ref _asset);

            if (_asset == null)
            {
                Begin.InfoBox("Open or create atlas");
                return;
            }
            
            _asset.data.TryValidate(out bool atlasIsValid, out string atlasErrorMessage);

            Begin.Horizontal(ui =>
            {
                Begin.Enabled(_asset != null).Button("FixImports", _fixImports);

                bool hasAllTextures = _asset != null &&
                                      _asset.data.mainTexture != null &&
                                      _asset.data.colorTexture != null &&
                                      _asset.data.configTexture != null;

                Begin.Enabled(hasAllTextures && atlasIsValid).Button("BakeTextures", ui =>
                {
                    _bakeMainTexture(ui);
                    _bakeColorTexture(ui);
                    _bakeConfigTexture(ui);
                });
                
                Begin.Enabled(BAsset.IsNullOrDirty(_asset)).Button("SaveAsset", _saveAtlasAsset);
            });

            Begin.Record(_asset, "Change atlas data").Vertical(ui =>
            {
                TerrainAtlas atlas = _asset.data;

                Begin.If(!atlasIsValid).WarningBox(atlasErrorMessage);
                
                ui.Foldout("Textures", ref _texturesFold);

                Begin.If(_texturesFold).Tab().Horizontal(ui =>
                {
                    ui.AssetField(nameof(atlas.mainTexture), ref atlas.mainTexture);
                    string buttonName = atlas.mainTexture == null ? NAME_SAVE : NAME_OVERRIDE;
                    Begin.Option(_bakeButtonSize).Enabled(atlasIsValid).Button(buttonName, _bakeMainTexture);
                });

                Begin.If(_texturesFold).Tab().Horizontal(ui =>
                {
                    ui.AssetField(nameof(atlas.colorTexture), ref atlas.colorTexture);
                    string buttonName = atlas.colorTexture == null ? NAME_SAVE : NAME_OVERRIDE;
                    Begin.Option(_bakeButtonSize).Enabled(atlasIsValid).Button(buttonName, _bakeColorTexture);
                });

                Begin.If(_texturesFold).Tab().Horizontal(ui =>
                {
                    ui.AssetField(nameof(atlas.configTexture), ref atlas.configTexture);
                    string buttonName = atlas.configTexture == null ? NAME_SAVE : NAME_OVERRIDE;
                    Begin.Option(_bakeButtonSize).Enabled(atlasIsValid).Button(buttonName, _bakeConfigTexture);
                });

                ui.GenericField(nameof(atlas.tileCount), ref atlas.tileCount);
                ui.IntField(nameof(atlas.pixelsPerTile), ref atlas.pixelsPerTile);
            });
        }

        private void _fixImports()
        {
            if (_asset.data.tiles != null)
            {
                foreach (TerrainTileData sprite in _asset.data.tiles)
                {
                    if (sprite.texture == null)
                    {
                        continue;
                    }

                    string path = AssetDatabase.GetAssetPath(sprite.texture);
                    UnityEditor.TextureImporter importer = (UnityEditor.TextureImporter) AssetImporter.GetAtPath(path);

                    if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.SaveAndReimport();
                        BDebug.LogInfo(this, $"Changed compression of '{path}' to Uncompressed");
                    }
                }
            }
        }

        private void _saveAtlasAsset()
        {
            BAsset.SaveIfDirty(_asset);
        }

        private void _bakeMainTexture(FlowUI ui)
        {
            Texture2D bakedTexture = null;
            
            try
            {
                string texturePath = _tryGetTexturePathOrNew(ui, _asset.data.mainTexture, MAIN_POSTFIX);
                if (texturePath == null)
                {
                    return;
                }

                bakedTexture = _textureBaker.BakeMainTexture(_asset.data);
                
                _asset.data.mainTexture = BAsset.SaveTexture(bakedTexture, texturePath, _textureImporter.MainTexture);
            }
            finally
            {
                if (bakedTexture != null)
                {
                    DestroyImmediate(bakedTexture);
                }
            }
        }
        
        private void _bakeColorTexture(FlowUI ui)
        {
            Texture2D bakedTexture = null;
            
            try
            {
                string texturePath = _tryGetTexturePathOrNew(ui, _asset.data.colorTexture, COLOR_POSTFIX);
                if (texturePath == null)
                {
                    return;
                }
                
                bakedTexture = _textureBaker.BakeColorTexture(_asset.data);

                _asset.data.colorTexture = BAsset.SaveTexture(bakedTexture, texturePath, _textureImporter.ColorTexture);
            }
            finally
            {
                if (bakedTexture != null)
                {
                    DestroyImmediate(bakedTexture);
                }
            }
        }
        
        private void _bakeConfigTexture(FlowUI ui)
        {
            Texture2D bakedTexture = null;

            try
            {
                string texturePath = _tryGetTexturePathOrNew(ui, _asset.data.configTexture, CONFIG_POSTFIX);
                if (texturePath == null)
                {
                    return;
                }
                
                bakedTexture = _textureBaker.BakeConfigTexture(_asset.data);

                _asset.data.configTexture = BAsset.SaveTexture(bakedTexture, texturePath, _textureImporter.ConfigTexture);
            }
            finally
            {
                if (bakedTexture != null)
                {
                    DestroyImmediate(bakedTexture);
                }
            }
        }

        private string _tryGetTexturePathOrNew(FlowUI ui, Texture2D texture, string texturePostfix)
        {
            if (texture == null)
            {
                string texturePath = AssetDatabase.GetAssetPath(_asset);
                string directory = Path.GetDirectoryName(texturePath);
                string fileName = Path.GetFileNameWithoutExtension(texturePath);

                texturePath = ui.SaveFileDialog(fileName + texturePostfix, directory);

                if (string.IsNullOrEmpty(texturePath))
                {
                    return null;
                }
                
                return BAsset.AbsoluteToProjectPath(texturePath);
            }

            return AssetDatabase.GetAssetPath(texture);
        }
        
        protected override void Dispose()
        {
            
        }
    }
}