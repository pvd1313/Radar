using System;
using System.Collections.Generic;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Radar.Core;
using Beast.Radar.InputHandling;
using Beast.Radar.Terrain;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

namespace Beast.Radar.UI
{
    public class MapGenerationWindow : IWindow, IGameCycleListener
    {
        private readonly IWindowManager _windowManager;
        private readonly MapGenerationWindowConfig _generationConfig;
        private readonly IUIFactory _uiFactory;
        private readonly ITerrainPreviewRender _previewRender;
        private readonly INewGameSetup _newGameSetup;
        private readonly IInputSystem _inputSystem;
        private readonly ISelectableZoneBuilder _selectableBuilder;

        private MapGenerationView _menuView;
        private SelectablesController _selectablesController;
        private ButtonRadioGroup<RadioButton> _sizeButtonsGroup;
        private Dictionary<RadioButton, int> _buttonSizes;
        
        private TerrainData _terrainData;
        private TerrainPreviewTexture _terrainTexture;
        private int _seedIndex;

        public MapGenerationWindow(
            MapGenerationWindowConfig generationConfig,
            IUIFactory uiFactory,
            IWindowManager windowManager,
            ISelectableZoneBuilder selectableBuilder,
            ITerrainPreviewRender previewRender,
            INewGameSetup newGameSetup,
            IInputSystem inputSystem)
        {
            _generationConfig = generationConfig;
            _uiFactory = uiFactory;
            _windowManager = windowManager;
            _selectableBuilder = selectableBuilder;
            _previewRender = previewRender;
            _newGameSetup = newGameSetup;
            _inputSystem = inputSystem;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, _warmup);
            handle.ListenOperation(OperationID.DISPOSE, _dispose);
        }

        public void Open()
        {

            _selectablesController.Enable();
            _configureView();
            _drawTexture();
            _menuView.gameObject.SetActive(true);
        }

        public void Close()
        {
            _hideView();
            _selectablesController.Disable();
            _terrainTexture.Dispose();
            _terrainTexture = null;
        }

        private UniTask<OperationResult> _warmup(OperationContext context)
        {
            _uiFactory.Create(this, PrefabID.MapGenerationWindow, out _menuView);
            _hideView();
            _windowManager.Register(this, WindowID.MapGeneration, _menuView);

            _buttonSizes = new()
            {
                {_menuView.sizeButtons[0], 512},
                {_menuView.sizeButtons[1], 1024},
                {_menuView.sizeButtons[2], 2048},
                {_menuView.sizeButtons[3], 4096},
            };
            _sizeButtonsGroup = new ButtonRadioGroup<RadioButton>(_menuView.sizeButtons);
            _sizeButtonsGroup.Warmup();
            
            _configureSelectables();
            _subscribeView();
            
            return OperationResult.SuccessSync();
        }

        private void _configureView()
        {
            int size = (int)(_menuView.mapImage.rectTransform.sizeDelta.x * _generationConfig.previewSizeMultiplier); 
            _terrainTexture = _previewRender.CreateTexture(this, size);
            
            RadioButton startSizeButton = _menuView.sizeButtons[3]; 
            
            _seedIndex = Random.Range(0, _generationConfig.seeds.Length);
            SeedData seedData = _generationConfig.seeds[_seedIndex];
        
            _terrainData = new TerrainData
            {
                biomeSize01 = 0.5f,
                seed = seedData.value,
                terrainSize = _buttonSizes[startSizeButton],
                water01 = 0.2f,
                forest01 = 0.2f
            };
            
            _menuView.biomeSlider.SetValueWithoutNotify(_terrainData.biomeSize01);
            _menuView.waterSlider.SetValueWithoutNotify(_terrainData.water01);
            _menuView.forestSlider.SetValueWithoutNotify(_terrainData.forest01);
            _menuView.seedResult.SetText(seedData.name);
            _sizeButtonsGroup.Select(startSizeButton);
        }

        private void _configureSelectables()
        {
            UISelectable[,] selectablesGrid =
            {
                { _menuView.seedButtons.Decrement.Selectable, _menuView.seedButtons.Increment.Selectable, null, null },
                {
                    _menuView.sizeButtons[0].Selectable, _menuView.sizeButtons[1].Selectable,
                    _menuView.sizeButtons[2].Selectable, _menuView.sizeButtons[3].Selectable
                },
                { _menuView.forestSlider.Selectable, null, null, null },
                { _menuView.waterSlider.Selectable, null, null, null },
                { _menuView.biomeSlider.Selectable, null, null, null },
                { _menuView.createMapButton.Selectable, null, null, null }
            };

            Tuple<ESelectionZone, SelectableGridData> dpadGrid = _selectableBuilder.Build(
                zone: ESelectionZone.Navigate,
                selectableGrid: selectablesGrid,
                isLooping: true);

            Dictionary<ESelectionZone, SelectableGridData> grids = new()
            {
                { dpadGrid.Item1, dpadGrid.Item2 }
            };

            _selectablesController = new SelectablesController(_inputSystem, grids);
            _selectablesController.Warmup();
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

            SeedData seedData = _generationConfig.seeds[_seedIndex];
            string seedName = seedData.name;
            _menuView.seedResult.SetText(seedName);
            _terrainData.seed = seedData.value; 
            _drawTexture();
            
            BDebug.Track(this, "seed", _terrainData.seed);
        }
        
        private void _onCreateMapButtonClicked()
        {
            _newGameSetup.StartNewGame(_terrainData);
            _windowManager.Show(WindowID.MainHUD);
        }

        private void _onCloseButtonClicked()
        {
            _windowManager.Show(WindowID.MainHUD);
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

        private UniTask<OperationResult> _dispose(OperationContext context)
        {
            _unsubscribeView();

            _selectablesController.Dispose();
            _sizeButtonsGroup.Dispose();

            return OperationResult.SuccessSync();
        }
    }
}