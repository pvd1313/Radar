using System.IO;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor.Window
{
    public class ShaderBakeWindow : EditorWindow
    {
        private static readonly int2 PREVIEW_TEXTURE_SIZE = new (256, 256);
        
        [SerializeField] private Material _renderMaterial;
        [SerializeField] private int _shaderPass;
        [SerializeField] private RenderTextureFormat _renderTextureFormat;
        [SerializeField] private TextureFormat _outputTextureFormat;
        [SerializeField] private int2 _outputTextureSize;

        [SerializeField] private Texture2D _bakedTexture;
        [SerializeField] private Texture2D _previewTexture;

        protected override void Warmup()
        {
            
        }

        protected override void InspectorUI()
        {
            EditorUIResult result = flow.Vertical(ui =>
            {
                ui.AssetField(nameof(_renderMaterial), ref _renderMaterial);
                ui.ValidateArgumentNotNull(_renderMaterial, nameof(_renderMaterial));
                
                ui.EnumSingleField(nameof(_renderTextureFormat), ref _renderTextureFormat);
                
                ui.IntField(nameof(_shaderPass), ref _shaderPass);
                
                ui.EnumSingleField(nameof(_outputTextureFormat), ref _outputTextureFormat);
                
                ui.Int2FieldTwoLine(nameof(_outputTextureSize), ref _outputTextureSize);
                ui.ValidateIsTrue(_outputTextureSize is { x: > 0, y: > 0 }, "Invalid texture size");
            });
            
            flow.Enabled(!result.validationFailed).Horizontal(ui =>
            {
                ui.Button("Bake", BakeMap);

                flow.Enabled(_bakedTexture != null).Button("Save", SaveTexture);
            });

            flow.If(_previewTexture != null).Draw(ui =>
            {
                ui.PassiveLabel("Preview: ");
                ui.TexturePreview(_previewTexture, PREVIEW_TEXTURE_SIZE.y);
            });
        }

        private void SaveTexture(EditorUI ui)
        {
            ui.SaveFileDialog("BackedTexture.png", (path) =>
            {
                byte[] bin = _bakedTexture.EncodeToPNG();
                
                File.WriteAllBytes(path, bin);
            });
        }
        
        private void BakeMap()
        {
            // RenderTexture renderTextureBig = BGraphic.CreateTemporaryRenderTexture(this,
            //     new TempRenderTextureData
            //     {
            //         format = _renderTextureFormat,
            //         size = _outputTextureSize,
            //         antiAliasing = 1
            //     });
            //
            // RenderTexture renderTextureSmall = BGraphic.CreateTemporaryRenderTexture(this,
            //     new TempRenderTextureData
            //     {
            //         format = _renderTextureFormat,
            //         size = PREVIEW_TEXTURE_SIZE,
            //         antiAliasing = 1
            //     });
            //
            // try
            // {
            //     BGraphic.Blit(new BlitParameters
            //     {
            //         material = _renderMaterial,
            //         shaderPass = _shaderPass,
            //         targetTexture = renderTextureBig
            //     });
            //     
            //     BGraphic.Blit(new BlitParameters
            //     {
            //         material = _renderMaterial,
            //         shaderPass = _shaderPass,
            //         targetTexture = renderTextureSmall
            //     });
            //
            //     if (_bakedTexture != null)
            //     {
            //         DestroyImmediate(_bakedTexture);
            //     }
            //
            //     _bakedTexture = BGraphic.CreateTexture2D(this, new Texture2DParameters
            //     {
            //         format = _outputTextureFormat,
            //         isLinear = true,
            //         mipChain = false,
            //         createInitialized = true,
            //         size = renderTextureBig.Size(),
            //         filterMode = renderTextureBig.filterMode,
            //         wrapMode = renderTextureBig.wrapMode,
            //     });
            //
            //     BGraphic.BlitRenderTextureTo2D(renderTextureBig, _bakedTexture);
            //
            //     if (_previewTexture != null)
            //     {
            //         DestroyImmediate(_previewTexture);
            //     }
            //
            //     _previewTexture = BGraphic.CreateTexture2D(this, new Texture2DParameters
            //     {
            //         format = TextureFormat.RGBA32,
            //         isLinear = true,
            //         mipChain = false,
            //         createInitialized = true,
            //         filterMode = FilterMode.Point,
            //         size = renderTextureSmall.Size(),
            //         wrapMode = renderTextureSmall.wrapMode
            //     });
            //
            //     BGraphic.BlitRenderTextureTo2D(renderTextureSmall, _previewTexture);
            // }
            // finally
            // {
            //     BGraphic.Dispose(renderTextureBig);
            //     BGraphic.Dispose(renderTextureSmall);
            // }
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