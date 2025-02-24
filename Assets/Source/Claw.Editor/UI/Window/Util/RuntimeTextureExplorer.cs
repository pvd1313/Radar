using System.IO;
using Beast.Claw.Debug;
using Beast.Claw.Editor.UI.UI;
using Beast.Claw.Graphic;
using Beast.Claw.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Beast.Claw.Editor.UI
{
    public class RuntimeTextureExplorer : EditorWindow
    {
        [SerializeField] private int _selectedTextureInstanceID;
        [SerializeField] private RepaintTickRateBar _tickRateBar;
        [SerializeField] private float2 _scrollPosition;
        [SerializeField] private string _lastFilePath;

        private Texture _selectedTexture;

        private Style _buttonStyleNormal;
        private Style _buttonStyleSelected;
        private Texture _lastTexture;

        public RuntimeTextureExplorer()
        {
            _tickRateBar = new RepaintTickRateBar(this);
        }

        protected override void Warmup()
        {
            _tickRateBar.Warmup();
            _buttonStyleNormal = _rectUI.Style.Button;
            _buttonStyleSelected = _rectUI.Style.Button.Clone();
            _buttonStyleSelected.Scheme.normal.textColor = Color.yellow;
        }

        protected override void Reset()
        {

        }

        protected override void DrawUI()
        {
            _flow.Begin().Horizontal(ui =>
            {
                _tickRateBar.Draw(_flow);

                ui.FlexibleSpace();
            });

            _flow.Begin().Vertical().Scroll(ref _scrollPosition).Draw(ui =>
            {
                foreach (Texture texture in BGraphic.GetAllocatedTextures())
                {
                    _lastTexture = texture;

                    _flow.Begin().Horizontal().Draw(ui =>
                    {
                        Style buttonStyle = _buttonStyleNormal;

                        if (texture.GetInstanceID() == _selectedTextureInstanceID)
                        {
                            _selectedTexture = texture;
                            buttonStyle = _buttonStyleSelected;
                        }

                        _flow.Begin().Style(buttonStyle).Button(texture.name, _selectTexture);

                        _flow.Begin().LabelWidth(60).Draw(ui =>
                        {
                            ui.FlexibleSpace();

                            if (texture is Texture2D)
                            {
                                ui.Label("(Texture2D)");
                            }

                            if (texture is RenderTexture)
                            {
                                ui.Label("(RenderTexture)");
                            }
                        });
                    });
                }
            });

            if (_selectedTexture != null)
            {
                _flow.Begin().Draw(ui =>
                {
                    _flow.Begin().Horizontal().Draw(ui =>
                    {
                        ui.Label(_selectedTexture.name);

                        ui.Button("Export", _exportSelectedTexture);
                    });

                    ui.GenericField("Size", _selectedTexture.Size());

                    GraphicsFormat format = _selectedTexture.graphicsFormat;
                    
                    ui.EnumSingleField("Format", ref format);

                    ui.TexturePreview(_selectedTexture, 256);
                });
            }
        }

        private void _exportSelectedTexture(FlowUI ui)
        {
            ui.SaveFileDialogWithMemory(_selectedTexture.name + ".png", ref _lastFilePath, (path) =>
            {
                BDebug.StaticLogInfo($"Exported texture to '{path}'");

                if (_selectedTexture is not Texture2D texture2D)
                {
                    texture2D = new Texture2D(
                        _selectedTexture.width, 
                        _selectedTexture.height,
                        _selectedTexture.graphicsFormat, 
                        TextureCreationFlags.None);
                    
                    if (_selectedTexture is RenderTexture renderTexture)
                    {
                        BGraphic.BlitRenderTextureTo2D(renderTexture, texture2D);
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Unknown texture type: " + _selectedTexture.GetType().Q());
                    }
                }
                
                File.WriteAllBytes(path, texture2D.EncodeToPNG());

                if (_selectedTexture != texture2D)
                {
                    DestroyImmediate(texture2D);
                }
            });
        }

    private void _selectTexture()
        {
            _selectedTextureInstanceID = _lastTexture.GetInstanceID();
        }

        protected override void Dispose()
        {
            _tickRateBar.Dispose();
        }
    }
}