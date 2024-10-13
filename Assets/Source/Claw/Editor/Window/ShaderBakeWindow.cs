using System.IO;
using Beast.Claw.Graphic;
using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor.Window
{
    public class ShaderBakeWindow : EditorWindow
    {
        private static readonly int2 PreviewTextureSize = new int2(256, 256);
        
        [SerializeField] private Material _renderMaterial;
        [SerializeField] private int _shaderPass;
        [SerializeField] private RenderTextureFormat _renderTextureFormat;
        [SerializeField] private TextureFormat _outputTextureFormat;
        [SerializeField] private int2 _outputTextureSize;

        [SerializeField] private Texture2D _bakedTexture;
        [SerializeField] private Texture2D _previewTexture;
        
        protected override void Dispose()
        {
            if (_bakedTexture != null)
            {
                DestroyImmediate(_bakedTexture);
                _bakedTexture = null;
            }
        }
        
        protected override void Initialize()
        {
            
        }
        
        protected override void OnInspectorGUI()
        {
            UI.Vertical(
                    () =>
                    {
                        _renderMaterial = UI.AssetField(nameof(_renderMaterial), _renderMaterial);
                        _renderMaterial.ValidateNotNull(nameof(_renderMaterial));
                    },
                    () =>
                    {
                        _renderTextureFormat = UI.EnumSingleField(nameof(_renderTextureFormat), _renderTextureFormat);
                        _shaderPass = UI.IntField(nameof(_shaderPass), _shaderPass);
                    },
                    () =>
                    {
                        _outputTextureFormat = UI.EnumSingleField(nameof(_outputTextureFormat), _outputTextureFormat);
                        _outputTextureSize = UI.Int2Field(nameof(_outputTextureSize), _outputTextureSize);
                        BAssert.IsTrue(_outputTextureSize is { x: > 0, y: > 0 }, "Invalid texture size");
                    })
                .Then(result =>
                {
                    UI.HorizontalEnabledGroup(result.success, () =>
                    {
                        UI.Button("Bake", BakeMap);

                        UI.EnabledGroup(_bakedTexture != null, () =>
                        {
                            UI.Button("Save", SaveTexture);
                        });
                    });

                    if (_previewTexture != null)
                    {
                        UI.TexturePreview("Preview", _previewTexture, PreviewTextureSize.y);
                    }
                });
        }

        private void SaveTexture()
        {
            UI.SaveAssetDialog("BackedTexture.png", (path) =>
            {
                byte[] bin = _bakedTexture.EncodeToPNG();
                
                File.WriteAllBytes(path, bin);
            });
        }
        
        private void BakeMap()
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
    }
}