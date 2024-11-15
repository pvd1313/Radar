using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Radar.NewGame;
using Beast.Radar.Terrain;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
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

        private MapGenerationView _menuView;
        private ButtonRadioGroup<RadioButton> _sizeButtonsGroup;
        private Dictionary<RadioButton, int> _buttonSizes;
        
        private TerrainData _terrainData;
        private TerrainPreviewTexture _terrainTexture;
        private int _seedIndex;
        private IWindowHandle<Empty> _mainHUD;

        public MapGenerationWindow(
            MapGenerationWindowConfig generationConfig,
            IWindowManager windowManager,
            ITerrainPreviewRender previewRender,
            INewGameSetup newGameSetup,
            CorePrefabLibrary library)
        {
            _generationConfig = generationConfig;
            _windowManager = windowManager;
            _previewRender = previewRender;
            _newGameSetup = newGameSetup;
            _library = library;
        }

        public UniTask Open(Empty data)
        {
            _configureView();
            _drawTexture();
            _menuView.gameObject.SetActive(true);

            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            _hideView();
            _terrainTexture.Dispose();
            _terrainTexture = null;

            return UniTask.CompletedTask;
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _menuView = BScene.InstantiateMono(this, _library.mapGeneration, float3.zero, quaternion.identity);
            _hideView();
            _windowManager.Register(this, WindowID.MAP_GENERATION);

            _menuView.transform.SetParent(_windowManager.CanvasTransform, false);

            _mainHUD = _windowManager.GetHandle<Empty>(this, WindowID.HUD);

            _buttonSizes = new()
            {
                {_menuView.sizeButtons[0], 512},
                {_menuView.sizeButtons[1], 1024},
                {_menuView.sizeButtons[2], 2048},
                {_menuView.sizeButtons[3], 4096},
            };
            _sizeButtonsGroup = new ButtonRadioGroup<RadioButton>(_menuView.sizeButtons);
            _sizeButtonsGroup.Warmup();
            
            _subscribeView();
            
            return UniTask.CompletedTask;
        }

        private void _configureView()
        {
            int size = (int)(_menuView.mapImage.rectTransform.sizeDelta.x * _generationConfig.previewSizeMultiplier); 
            _terrainTexture = _previewRender.CreateTexture(this, size);
            
            RadioButton startSizeButton = _menuView.sizeButtons[3]; 
            
            _seedIndex = Random.Range(0, _generationConfig.seeds.Length);
            MapGenerationSeed seed = _generationConfig.seeds[_seedIndex];
        
            _terrainData = new TerrainData
            {
                biomeSize01 = 0.5f,
                seed = seed.value,
                terrainSize = _buttonSizes[startSizeButton],
                water01 = 0.2f,
                forest01 = 0.2f
            };
            
            _menuView.biomeSlider.SetValueWithoutNotify(_terrainData.biomeSize01);
            _menuView.waterSlider.SetValueWithoutNotify(_terrainData.water01);
            _menuView.forestSlider.SetValueWithoutNotify(_terrainData.forest01);
            _menuView.seedResult.SetText(seed.name);
            _sizeButtonsGroup.Select(startSizeButton);
        }

        private void _subscribeView()
        {
            _sizeButtonsGroup.Selected += _onSizeSelected;
            _menuView.seedButtons.Clicked += _onSeedButtonsClicked;
            _menuView.closeButton.Clicked += _onCloseButtonClicked;
            _menuView.createMapButton.Clicked += _onCreateMapButtonClicked;
            _menuView.forestSlider.EndedChange += _onForestEndedChange;
            _menuView.waterSlider.EndedChange += _onWaterEndedChange;
            _menuView.biomeSlider.EndedChange += _onBiomeSizeEndedChange;
        }

        private void _unsubscribeView()
        {
            _sizeButtonsGroup.Selected -= _onSizeSelected;
            _menuView.seedButtons.Clicked -= _onSeedButtonsClicked;
            _menuView.closeButton.Clicked -= _onCloseButtonClicked;
            _menuView.createMapButton.Clicked -= _onCreateMapButtonClicked;
            _menuView.forestSlider.EndedChange -= _onForestEndedChange;
            _menuView.waterSlider.EndedChange -= _onWaterEndedChange;
            _menuView.biomeSlider.EndedChange -= _onBiomeSizeEndedChange;
        }

        private void _onBiomeSizeEndedChange(float value)
        {
            _terrainData.biomeSize01 = value;
            _drawTexture();
        }

        private void _onSizeSelected(RadioButton button)
        {
            _terrainData.terrainSize = _buttonSizes[button];
            _drawTexture();
            BDebug.Track(this, "size", _terrainData.terrainSize);
        }

        private void _onWaterEndedChange(float value)
        {
            _terrainData.water01 = value;
            _drawTexture();
            BDebug.Track(this, "water", _terrainData.water01);
        }

        private void _onForestEndedChange(float value)
        {
            _terrainData.forest01 = value;
            _drawTexture();
            BDebug.Track(this, "forest", _terrainData.forest01);
        }

        private void _onSeedButtonsClicked(int step)
        {
            _seedIndex += step;
            if (_seedIndex < 0)
            {
                _seedIndex = _generationConfig.seeds.Length - 1;
            }
            else if (_seedIndex == _generationConfig.seeds.Length)
            {
                _seedIndex = 0;
            }

            MapGenerationSeed seedData = _generationConfig.seeds[_seedIndex];
            string seedName = seedData.name;
            _menuView.seedResult.SetText(seedName);
            _terrainData.seed = seedData.value; 
            _drawTexture();
            
            BDebug.Track(this, "seed", _terrainData.seed);
        }
        
        private void _onCreateMapButtonClicked()
        {
            _newGameSetup.StartNewGame(_terrainData);
            _mainHUD.Open(default);
        }

        private void _onCloseButtonClicked()
        {
            _mainHUD.Open(default);
        }

        private void _hideView()
        {
            _menuView.gameObject.SetActive(false);
        }

        private void _drawTexture()
        {
            _previewRender.Render(new TerrainPreviewRenderTask
            {
                terrain = _terrainData,
                texture = _terrainTexture
            });
            
            _menuView.mapImage.texture = _terrainTexture.color;
        }

        protected override void _dispose(BlockingContext context)
        {
            _unsubscribeView();

            _sizeButtonsGroup.Dispose();
        }
    }
}