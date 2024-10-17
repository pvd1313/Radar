using System.IO;
using Beast.Claw;
using Beast.Claw.Debug;
using Beast.Claw.Editor;
using Beast.Radar.Graphic.Terrain;
using UnityEditor;
using UnityEngine;

namespace Beast.Radar.Editor.Terrain
{
    public class AtlasEditor : Beast.Claw.Editor.Window.EditorWindow
    {
        private const string MAIN_POSTFIX = "_main.png";
        private const string COLOR_POSTFIX = "_color.png";
        private const string CONFIG_POSTFIX = "_config.png";
        private const string NAME_SAVE = "Save";
        private const string NAME_OVERRIDE = "Override";
        
        [SerializeField] private TerrainTileAtlasAsset _atlasAsset;
        [SerializeField] private TileEditor _tileEditor;
        
        private GUILayoutOption _bakeButtonSize;
        TextureImporter _textureImporter;
        private bool _texturesFold;
        private TextureBaker _textureBaker;
        
        protected override void Warmup()
        {
            _bakeButtonSize = GUILayout.Width(100);
            _textureImporter = new TextureImporter();
            _textureBaker = new TextureBaker();
            _tileEditor = new TileEditor(this);
        }

        protected override void InspectorUI()
        {
            flow.AssetField(nameof(_atlasAsset), ref _atlasAsset);

            if (_atlasAsset == null)
            {
                flow.InfoBox("Open or create atlas");
                return;
            }
            
            _atlasAsset.atlas.TryValidate(out bool atlasIsValid, out string atlasErrorMessage);

            flow.Horizontal(ui =>
            {
                flow.Enabled(_atlasAsset != null).Button("Fix Imports", _fixImports);

                bool hasAllTextures = _atlasAsset != null &&
                                      _atlasAsset.atlas.mainTexture != null &&
                                      _atlasAsset.atlas.colorTexture != null &&
                                      _atlasAsset.atlas.configTexture != null;

                flow.Spy().Enabled(hasAllTextures && atlasIsValid).Button("Bake Textures", ui =>
                {
                    _bakeMainTexture(ui);
                    _bakeColorTexture(ui);
                    _bakeConfigTexture(ui);
                });
                
                flow.Enabled(IsNullOrDirty(_atlasAsset)).Button("Save Asset", _saveAtlasAsset);
            });

            flow.Vertical(ui =>
            {
                TerrainTileAtlas atlas = _atlasAsset.atlas;

                flow.If(!atlasIsValid).WarningBox(atlasErrorMessage);
                
                EditorUIResult spyResult = flow.Spy(ui =>
                {
                    ui.Foldout("Textures", ref _texturesFold);

                    flow.If(_texturesFold).Indent().Horizontal(ui =>
                    {
                        ui.AssetField(nameof(atlas.mainTexture), ref atlas.mainTexture);
                        string buttonName = _atlasAsset.atlas.mainTexture == null ? NAME_SAVE : NAME_OVERRIDE;
                        flow.Option(_bakeButtonSize).Enabled(atlasIsValid).Button(buttonName, _bakeMainTexture);
                    });

                    flow.If(_texturesFold).Indent().Horizontal(ui =>
                    {
                        ui.AssetField(nameof(atlas.colorTexture), ref atlas.colorTexture);
                        string buttonName = _atlasAsset.atlas.colorTexture == null ? NAME_SAVE : NAME_OVERRIDE;
                        flow.Option(_bakeButtonSize).Enabled(atlasIsValid).Button(buttonName, _bakeColorTexture);
                    });

                    flow.If(_texturesFold).Indent().Horizontal(ui =>
                    {
                        ui.AssetField(nameof(atlas.configTexture), ref atlas.configTexture);
                        string buttonName = _atlasAsset.atlas.configTexture == null ? NAME_SAVE : NAME_OVERRIDE;
                        flow.Option(_bakeButtonSize).Enabled(atlasIsValid).Button(buttonName, _bakeConfigTexture);
                    });
                    
                    ui.Int2Field(nameof(atlas.tileCount), ref atlas.tileCount);
                    ui.IntField(nameof(atlas.pixelsPerTile), ref atlas.pixelsPerTile);
                });

                ui.Space(EditorUI.MinorSpace);

                if (spyResult.changesDetected)
                {
                    SetDirty(_atlasAsset);
                }

                _tileEditor.DrawUI(atlas.sprites, out bool changesDetected);
                
                if (changesDetected)
                {
                    SetDirty(_atlasAsset);
                }
            });
        }

        private void _fixImports()
        {
            if (_atlasAsset.atlas.sprites != null)
            {
                foreach (TerrainTileSprite sprite in _atlasAsset.atlas.sprites)
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
            SaveIfDirty(_atlasAsset);
        }

        private void _bakeMainTexture(EditorUI ui)
        {
            Texture2D bakedTexture = null;
            
            try
            {
                string texturePath = _tryGetTexturePathOrNew(ui, _atlasAsset.atlas.mainTexture, MAIN_POSTFIX);
                if (texturePath == null)
                {
                    return;
                }

                bakedTexture = _textureBaker.BakeMainTexture(_atlasAsset.atlas);
                
                _atlasAsset.atlas.mainTexture = BAsset.SaveTexture(bakedTexture, texturePath, _textureImporter.MainTexture);
            }
            finally
            {
                if (bakedTexture != null)
                {
                    DestroyImmediate(bakedTexture);
                }
            }
        }
        
        private void _bakeColorTexture(EditorUI ui)
        {
            Texture2D bakedTexture = null;
            
            try
            {
                string texturePath = _tryGetTexturePathOrNew(ui, _atlasAsset.atlas.colorTexture, COLOR_POSTFIX);
                if (texturePath == null)
                {
                    return;
                }
                
                bakedTexture = _textureBaker.BakeColorTexture(_atlasAsset.atlas);

                _atlasAsset.atlas.colorTexture = BAsset.SaveTexture(bakedTexture, texturePath, _textureImporter.ColorTexture);
            }
            finally
            {
                if (bakedTexture != null)
                {
                    DestroyImmediate(bakedTexture);
                }
            }
        }
        
        private void _bakeConfigTexture(EditorUI ui)
        {
            Texture2D bakedTexture = null;

            try
            {
                string texturePath = _tryGetTexturePathOrNew(ui, _atlasAsset.atlas.configTexture, CONFIG_POSTFIX);
                if (texturePath == null)
                {
                    return;
                }
                
                bakedTexture = _textureBaker.BakeConfigTexture(_atlasAsset.atlas);

                _atlasAsset.atlas.configTexture = BAsset.SaveTexture(bakedTexture, texturePath, _textureImporter.ConfigTexture);
            }
            finally
            {
                if (bakedTexture != null)
                {
                    DestroyImmediate(bakedTexture);
                }
            }
        }

        private string _tryGetTexturePathOrNew(EditorUI ui, Texture2D texture, string texturePostfix)
        {
            if (texture == null)
            {
                string texturePath = AssetDatabase.GetAssetPath(_atlasAsset);
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