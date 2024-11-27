using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Radar.NewGame;
using Beast.Radar.Terrain;
using Cysharp.Threading.Tasks;
using Noesis;

using Random = UnityEngine.Random;

namespace Beast.Radar.UI
{
    public class MapGenerationWindow : Actor, IWindow<Empty>
    {
        private readonly IWindowManager _windowManager;
        private readonly MapGenerationWindowConfig _generationConfig;
        private readonly ITerrainPreviewRender _previewRender;
        private readonly INewGameSetup _newGameSetup;
        private readonly CorePrefabLibrary _library;
        private readonly UIConfig _uiConfig;
        private readonly INoesisWindowManager _noesisWindowManager;

        private MapGenerationWindowView _view;
        private Dictionary<RadioButton, int> _buttonSizes;

        private TerrainData _terrainData;
        private TerrainPreviewTexture _terrainTexture;
        private RenderTextureBrush _renderTextureBrush;
        private int _seedIndex;
        private IWindowHandle<Empty> _mainHUD;

        public MapGenerationWindow(
            MapGenerationWindowConfig generationConfig,
            IWindowManager windowManager,
            ITerrainPreviewRender previewRender,
            INewGameSetup newGameSetup,
            CorePrefabLibrary library,
            UIConfig uiConfig,
            INoesisWindowManager noesisWindowManager)
        {
            _generationConfig = generationConfig;
            _windowManager = windowManager;
            _previewRender = previewRender;
            _newGameSetup = newGameSetup;
            _library = library;
            _uiConfig = uiConfig;
            _noesisWindowManager = noesisWindowManager;
        }

        public UniTask Open(Empty data)
        {
            _configureView();

            _drawTexture();

            _view.Visibility = Visibility.Visible;

            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            _view.Visibility = Visibility.Collapsed;

            _terrainTexture.Dispose();
            _renderTextureBrush.Dispose();

            _terrainTexture = null;

            return UniTask.CompletedTask;
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _uiConfig.MapGenerationWindowAsset.Instantiate(out _view);

            _view.Visibility = Visibility.Collapsed;

            _noesisWindowManager.Attach(_view);

            _windowManager.Register(this, WindowID.MAP_GENERATION);

            _mainHUD = _windowManager.GetHandle<Empty>(this, WindowID.HUD);

            _subscribeView();

            return UniTask.CompletedTask;
        }

        private void _configureView()
        {
            int imageSize = 300;
            int size = (int)(imageSize * _generationConfig.previewSizeMultiplier);
            _terrainTexture = _previewRender.CreateTexture(this, size);
            _renderTextureBrush = new RenderTextureBrush(_terrainTexture.color);
            _view.texture.Fill = _renderTextureBrush.Brush;

            _seedIndex = Random.Range(0, _generationConfig.seeds.Length);
            MapGenerationSeed seed = _generationConfig.seeds[_seedIndex];

            _terrainData = new TerrainData
            {
                biomeSize01 = 0.5f,
                seed = seed.value,
                terrainSize = 2048,
                water01 = 0.2f,
                forest01 = 0.2f
            };

            _view.sliderBiomeSize.Value = _terrainData.biomeSize01 * 100;
            _view.sliderWater.Value = _terrainData.water01 * 100;
            _view.sliderBiomeSize.Value = _terrainData.biomeSize01 * 100;
            _view.textSeed.Text = seed.name;
            _view.buttonSize2048.IsChecked = true;
        }

        private void _subscribeView()
        {
            _view.buttonCreate.Click += (_, _) =>
            {
                _newGameSetup.StartNewGame(_terrainData);
                _mainHUD.Open(default);
            };

            _view.buttonClose.Click += (_, _) =>
            {
                _mainHUD.Open(default);
            };

            _view.buttonNextSeed.Click += (_, _) =>
            {
                _seedIndex++;
                if (_seedIndex == _generationConfig.seeds.Length)
                {
                    _seedIndex = 0;
                }

                _refreshSeed();
            };

            _view.buttonPreviousSeed.Click += (_, _) =>
            {
                _seedIndex--;

                if (_seedIndex < 0)
                {
                    _seedIndex = _generationConfig.seeds.Length - 1;
                }

                _refreshSeed();
            };
            
            for (int i = 0; i < _view.buttonsSize.Length; i++)
            {
                int size = (i + 1) * 512;
                
                _view.buttonsSize[i].Click += (_, _) =>
                {
                    _terrainData.terrainSize = size;
                    _drawTexture();
                    BDebug.Track(this, "size", _terrainData.terrainSize);
                };
            }

            _view.sliderWater.ValueChanged += (_, args) =>
            {
                _terrainData.water01 = args.NewValue / 100;
                _drawTexture();
                BDebug.Track(this, "water", _terrainData.water01);
            };
            
            _view.sliderForest.ValueChanged += (_, args) =>
            {
                _terrainData.forest01 = args.NewValue / 100;
                _drawTexture();
                BDebug.Track(this, "forest", _terrainData.forest01);
            };
            
            _view.sliderBiomeSize.ValueChanged += (_, args) =>
            {
                _terrainData.biomeSize01 = args.NewValue / 100;
                _drawTexture();
                BDebug.Track(this, "biome size", _terrainData.biomeSize01);
            };
        }
        
        private void _refreshSeed()
        {
            MapGenerationSeed seedData = _generationConfig.seeds[_seedIndex];

            _view.textSeed.Text = seedData.name;

            _terrainData.seed = seedData.value;

            _drawTexture();

            BDebug.Track(this, "seed", _terrainData.seed);
        }
        
        private void _drawTexture()
        {
            _previewRender.Render(new TerrainPreviewRenderTask
            {
                terrain = _terrainData,
                texture = _terrainTexture
            });
            
            _renderTextureBrush.Blit();
        }
    }
}