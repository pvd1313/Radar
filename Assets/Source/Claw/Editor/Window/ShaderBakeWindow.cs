using System.IO;
using Beast.Claw.Graphic;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor.Window
{
    public class ShaderBakeWindow : EditorWindow
    {
        private static readonly int2 PreviewTextureSize = new (256, 256);
        
        [SerializeField] private Material _renderMaterial;
        [SerializeField] private int _shaderPass;
        [SerializeField] private RenderTextureFormat _renderTextureFormat;
        [SerializeField] private TextureFormat _outputTextureFormat;
        [SerializeField] private int2 _outputTextureSize;

        [SerializeField] private Texture2D _bakedTexture;
        [SerializeField] private Texture2D _previewTexture;
        
        protected override void Initialize()
        {
            
        }
        
        protected override void OnInspectorGUI(EditorUI ui)
        {
            ui.Vertical(
                    ui =>
                    {
                        ui.AssetField(nameof(_renderMaterial), ref _renderMaterial);
                        BAssert.ArgumentNotNull(_renderMaterial, nameof(_renderMaterial));
                    },
                    ui =>
                    {
                        ui.EnumSingleField(nameof(_renderTextureFormat), ref _renderTextureFormat);
                        ui.IntField(nameof(_shaderPass), ref _shaderPass);
                    },
                    ui =>
                    {
                        ui.EnumSingleField(nameof(_outputTextureFormat), ref _outputTextureFormat);
                        ui.Int2Field(nameof(_outputTextureSize), ref _outputTextureSize);
                        BAssert.IsTrue(_outputTextureSize is { x: > 0, y: > 0 }, "Invalid texture size");
                    })
                .Then(ui =>
                {
                    ui.HorizontalEnabledGroup(ui.success, ui =>
                    {
                        ui.Button("Bake", BakeMap);

                        ui.EnabledGroup(_bakedTexture != null, ui =>
                        {
                            ui.Button("Save", SaveTexture);
                        });
                    });

                    if (_previewTexture != null)
                    {
                        ui.TexturePreview("Preview", _previewTexture, PreviewTextureSize.y);
                    }
                });
        }

        private void SaveTexture(EditorUI ui)
        {
            ui.SaveAssetDialog("BackedTexture.png", (path) =>
            {
                byte[] bin = _bakedTexture.EncodeToPNG();
                
                File.WriteAllBytes(path, bin);
            });
        }
        
        private void BakeMap(EditorUI ui)
        {
            RenderTexture renderTextureBig = BGraphic.CreateTemporaryRenderTexture(this,
                new NewTempRenderTextureData
                {
                    format = _renderTextureFormat,
                    size = _outputTextureSize,
                    antiAliasing = 1
                });

            RenderTexture renderTextureSmall = BGraphic.CreateTemporaryRenderTexture(this,
                new NewTempRenderTextureData
                {
                    format = _renderTextureFormat,
                    size = PreviewTextureSize,
                    antiAliasing = 1
                });

            try
            {
                BGraphic.Blit(new BlitParameters
                {
                    material = _renderMaterial,
                    shaderPass = _shaderPass,
                    targetTexture = renderTextureBig
                });
                
                BGraphic.Blit(new BlitParameters
                {
                    material = _renderMaterial,
                    shaderPass = _shaderPass,
                    targetTexture = renderTextureSmall
                });

                if (_bakedTexture != null)
                {
                    DestroyImmediate(_bakedTexture);
                }
                
                _bakedTexture = BGraphic.CreateTexture2DFromRenderTexture(this, new Texture2DParametersA
                {
                    format = _outputTextureFormat,
                    isLinear = true,
                    mipChain = false,
                    sourceTexture = renderTextureBig,
                });

                if (_previewTexture != null)
                {
                    DestroyImmediate(_previewTexture);
                }

                _previewTexture = BGraphic.CreateTexture2DFromRenderTexture(this, new Texture2DParametersA
                {
                    format = TextureFormat.RGBA32,
                    isLinear = true,
                    mipChain = false,
                    sourceTexture = renderTextureSmall,
                });
            }
            finally
            {
                BGraphic.DisposeRenderTexture(renderTextureBig);
                BGraphic.DisposeRenderTexture(renderTextureSmall);
            }
            
        }
        
        protected override void Dispose()
        {
            if (_bakedTexture != null)
            {
                DestroyImmediate(_bakedTexture);
                _bakedTexture = null;
            }
        }
    }
}