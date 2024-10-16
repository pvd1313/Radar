using System.IO;
using Beast.Claw;
using Beast.Claw.Debug;
using Beast.Claw.Editor;
using Beast.Radar.Graphic.Terrain;
using UnityEditor;
using UnityEngine;

namespace Beast.Radar.Editor.TerrainAtlas
{
    public class TerrainAtlasEditor : Beast.Claw.Editor.Window.EditorWindow
    {
        private const string MainPostfix = "_main.png";
        private const string ColorPostfix = "_color.png";
        private const string ConfigPostfix = "_config.png";
        private const string NameSave = "Save";
        private const string NameOverride = "Override";
        
        [SerializeField] private TerrainTileAtlasAsset _atlasAsset;
        [SerializeField] private TerrainTileEditor _tileEditor;
        [SerializeField] private bool _isDirty;
        [SerializeField] private GUILayoutOption _bakeButtonSize = GUILayout.Width(100);
        [SerializeField] private TerrainAtlasBaker _baker = new();
        [SerializeField] private TerrainAtlasImporter _importer = new();
        [SerializeField] private bool _texturesFold;

        public TerrainAtlasEditor()
        {
            _tileEditor = new TerrainTileEditor(this);
        }
        
        protected override void InspectorUI()
        {
            bool atlasIsValid = false;
            string atlasErrorMessage = null;

            if (_atlasAsset.atlas != null)
            {
                _atlasAsset.atlas.TryValidate(out atlasIsValid, out atlasErrorMessage);
            }
            
            flow.Draw(ui =>
            {
                ui.AssetField(nameof(_atlasAsset), ref _atlasAsset);

                flow.If(_atlasAsset == null).InfoBox("Open or create atlas.");

                flow.If(_atlasAsset != null).Horizontal(ui =>
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
                    
                    flow.Enabled(_isDirty && _atlasAsset != null).Button("Save Asset", _saveAtlasAsset);
                });
            });

            flow.If(_atlasAsset != null).Vertical(ui =>
            {
                TerrainTileAtlas atlas = _atlasAsset.atlas;

                flow.If(!atlasIsValid).WarningBox(atlasErrorMessage);
                
                EditorUIResult spyResult = flow.Spy(ui =>
                {
                    ui.Foldout("Textures", ref _texturesFold);

                    flow.If(_texturesFold).Indent().Horizontal(ui =>
                    {
                        ui.AssetField(nameof(atlas.mainTexture), ref atlas.mainTexture);
                        string buttonName = _atlasAsset.atlas.mainTexture == null ? NameSave : NameOverride;
                        flow.Option(_bakeButtonSize).Enabled(atlasIsValid).Button(buttonName, _bakeMainTexture);
                    });

                    flow.If(_texturesFold).Indent().Horizontal(ui =>
                    {
                        ui.AssetField(nameof(atlas.colorTexture), ref atlas.colorTexture);
                        string buttonName = _atlasAsset.atlas.colorTexture == null ? NameSave : NameOverride;
                        flow.Option(_bakeButtonSize).Enabled(atlasIsValid).Button(buttonName, _bakeColorTexture);
                    });

                    flow.If(_texturesFold).Indent().Horizontal(ui =>
                    {
                        ui.AssetField(nameof(atlas.configTexture), ref atlas.configTexture);
                        string buttonName = _atlasAsset.atlas.configTexture == null ? NameSave : NameOverride;
                        flow.Option(_bakeButtonSize).Enabled(atlasIsValid).Button(buttonName, _bakeConfigTexture);
                    });
                    
                    ui.Int2Field(nameof(atlas.tileCount), ref atlas.tileCount);
                    ui.IntField(nameof(atlas.pixelsPerTile), ref atlas.pixelsPerTile);
                });

                ui.Space(EditorUI.MinorSpace);

                _isDirty |= spyResult.changesDetected;
                _isDirty |= _tileEditor.DrawUI(atlas.sprites);
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
                    TextureImporter importer = (TextureImporter) AssetImporter.GetAtPath(path);

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
            _isDirty = false;
            
            EditorUtility.SetDirty(_atlasAsset);
            AssetDatabase.SaveAssetIfDirty(_atlasAsset);
        }

        private void _bakeMainTexture(EditorUI ui)
        {
            Texture2D bakedTexture = null;
            
            try
            {
                string texturePath = _tryGetTexturePathOrNew(ui, _atlasAsset.atlas.mainTexture, MainPostfix);
                if (texturePath == null)
                {
                    return;
                }

                bakedTexture = _baker.BakeMainTexture(_atlasAsset.atlas);
                
                _atlasAsset.atlas.mainTexture = BAsset.SaveTexture(bakedTexture, texturePath, _importer.MainTexture);
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
                string texturePath = _tryGetTexturePathOrNew(ui, _atlasAsset.atlas.colorTexture, ColorPostfix);
                if (texturePath == null)
                {
                    return;
                }
                
                bakedTexture = _baker.BakeColorTexture(_atlasAsset.atlas);

                _atlasAsset.atlas.colorTexture = BAsset.SaveTexture(bakedTexture, texturePath, _importer.ColorTexture);
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
                string texturePath = _tryGetTexturePathOrNew(ui, _atlasAsset.atlas.configTexture, ConfigPostfix);
                if (texturePath == null)
                {
                    return;
                }
                
                bakedTexture = _baker.BakeConfigTexture(_atlasAsset.atlas);

                _atlasAsset.atlas.configTexture = BAsset.SaveTexture(bakedTexture, texturePath, _importer.ConfigTexture);
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