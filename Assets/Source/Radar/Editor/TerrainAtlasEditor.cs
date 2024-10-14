using System.IO;
using Beast.Claw.Debug;
using Beast.Claw.Editor;
using Beast.Claw.Graphic;
using Beast.Radar.Graphic.Terrain;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Beast.Radar.Editor
{
    public class TerrainAtlasEditor : Beast.Claw.Editor.Window.EditorWindow
    {
        private const string MainTexturePostfix = "_main.png";
        
        [SerializeField] private TerrainTileAtlasAsset _atlasAsset;
        [SerializeField] private TerrainAtlasSpriteEditor _spriteEditor;
        [SerializeField] private bool _isDirty;

        public TerrainAtlasEditor()
        {
            _spriteEditor = new TerrainAtlasSpriteEditor(this);
        }
        
        protected override void InspectorUI()
        {
            flow.Draw(ui =>
            {
                ui.AssetField(nameof(_atlasAsset), ref _atlasAsset);

                flow.Horizontal(ui =>
                {
                    flow.Button("Fix imports", FixAllTextureImports);
                    flow.Enabled(_isDirty && _atlasAsset != null).Button("Save", SaveAtlas);
                });
            });

            EditorUIResult result = flow.Draw(ui =>
            {
                ui.ValidateArgumentNotNull(_atlasAsset, nameof(_atlasAsset));
            });

            if (result.errorsEncountered)
            {
                return;
            }

            flow.Vertical(ui =>
            {
                TerrainTileAtlas atlas = _atlasAsset.atlas;

                bool atlasIsValid = ui.Validate(atlas, nameof(atlas));

                EditorUIResult spyResult = flow.Spy(ui =>
                {
                    flow.Horizontal(ui =>
                    {
                        ui.AssetField(nameof(atlas.mainTexture), ref atlas.mainTexture);
                        flow.Enabled(atlasIsValid).Button("Bake", BakeAtlasMainTexture);
                    });
                    
                    ui.Int2Field(nameof(atlas.tileCount), ref atlas.tileCount);
                    ui.IntField(nameof(atlas.atlasPPU), ref atlas.atlasPPU);
                });

                _isDirty |= spyResult.changesDetected;
                _isDirty |= _spriteEditor.DrawUI(atlas.sprites);
            });
        }

        private void FixAllTextureImports()
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

        private void SaveAtlas()
        {
            _isDirty = false;
            
            EditorUtility.SetDirty(_atlasAsset);
            AssetDatabase.SaveAssetIfDirty(_atlasAsset);
        }

        private void BakeAtlasMainTexture(EditorUI ui)
        {
            bool isNewAsset = _atlasAsset.atlas.mainTexture == null;
            string assetPath;
            
            if (isNewAsset)
            {
                assetPath = AssetDatabase.GetAssetPath(_atlasAsset);
                string directory = Path.GetDirectoryName(assetPath);
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                
                ui.SaveAssetDialogWithDefaultPath(fileName + MainTexturePostfix, directory, path =>
                {
                    assetPath = path;
                });
            }
            else
            {
                assetPath = AssetDatabase.GetAssetPath(_atlasAsset);
            }

            RenderTexture renderTexture = BGraphic.CreateTemporaryRenderTexture(this, new NewTempRenderTextureData
            {
                size = _atlasAsset.atlas.tileCount * _atlasAsset.atlas.atlasPPU,
                antiAliasing = 1,
                format = RenderTextureFormat.ARGB32,
                filterMode = FilterMode.Point,
                readWrite = RenderTextureReadWrite.Linear
            });

            foreach (TerrainTileSprite tile in _atlasAsset.atlas.sprites)
            {
                int xOffset = tile.index % _atlasAsset.atlas.tileCount.y * _atlasAsset.atlas.atlasPPU;
                int yOffset = tile.index / _atlasAsset.atlas.tileCount.y * _atlasAsset.atlas.atlasPPU;
                
                BGraphic.DrawTexture2DToRenderTexture(new Texture2DToRenderTexture
                {
                    targetTexturePixelOffset = new float2(xOffset, yOffset),
                    sourceTexture = tile.texture,
                    targetTexture = renderTexture
                });
            }

            Texture2D resultTexture = BGraphic.CreateTexture2DFromRenderTexture(this, new Texture2DParametersA
            {
                isLinear = true,
                mipChain = true,
                sourceTexture = renderTexture,
                format = TextureFormat.RGB24,
                filterMode = FilterMode.Point
            });
            
            BGraphic.DisposeRenderTexture(renderTexture);

            File.WriteAllBytes(assetPath, resultTexture.EncodeToPNG());
        }
        
        protected override void Dispose()
        {
            
        }
    }
}