using System;
using System.Collections.Generic;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Radar.Core;
using Beast.Radar.GameCycle;
using Beast.Radar.InputControl;
using Beast.Radar.Terrain;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Beast.Radar.UI
{
    public class MapGenerationWindow : IWindow, IGameCycleListener
    {
        private readonly IWindowManager _windowManager;
        private readonly MapGenerationWindowConfig _generationConfig;
        private readonly IUIFactory _uiFactory;
        private readonly IUIInputController _uiInput;
        private readonly ITerrainPreviewRender _previewRender;
        private readonly INewGameSetup _newGameSetup;
        private readonly ISelectableZoneBuilder _selectableBuilder;

        private MapGenerationView _menuView;
        private SelectablesController _selectablesController;
        private ButtonRadioGroup<IntRadioButton> _sizeButtonsGroup;
        
        private Terrain.TerrainData _terrainData;
        private RenderTexture _screenTexture;
        private int _seedIndex;

        public MapGenerationWindow(
            MapGenerationWindowConfig generationConfig,
            IUIFactory uiFactory,
            IWindowManager windowManager,
            ISelectableZoneBuilder selectableBuilder,
            IUIInputController uiInput,
            ITerrainPreviewRender previewRender,
            INewGameSetup newGameSetup)
        {
            _generationConfig = generationConfig;
            _uiFactory = uiFactory;
            _windowManager = windowManager;
            _selectableBuilder = selectableBuilder;
            _uiInput = uiInput;
            _previewRender = previewRender;
            _newGameSetup = newGameSetup;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, _warmup);
            handle.ListenOperation(this, OperationID.Dispose, _dispose);
        }

        public void Open()
        {
            _screenTexture = _previewRender.CreateTexture(this, 256);
            _selectablesController.Enable();
            _configureView();
            _menuView.gameObject.SetActive(true);
        }

        public void Close()
        {
            _menuView.gameObject.SetActive(false);
            _selectablesController.Disable();
            BGraphic.TryDispose(ref _screenTexture);
        }

        private UniTask<OperationResult> _warmup()
        {
            _terrainData = new Terrain.TerrainData()
            {
                biomeSize01 = 0.5f,
                seed = 1,
                terrainSize = 256,
                water01 = 0.2f,
            };

            _uiFactory.Create(this, PrefabID.MapGenerationWindow, out _menuView);
            _windowManager.Register(this, WindowID.MapGeneration, _menuView);

            _sizeButtonsGroup = new ButtonRadioGroup<IntRadioButton>(_menuView.sizeButtons);
            _sizeButtonsGroup.Warmup(startIndex: _menuView.sizeButtons.Length - 1);

            _configureSelectables();
            _subscribeView();

            return OperationResult.SuccessSync();
        }

        private void _configureView()
        {
            _menuView.biomeSlider.SetValue(value: _terrainData.biomeSize01, withNotify: true);
            _menuView.waterSlider.SetValue(value: _terrainData.water01, withNotify: true);
            _menuView.forestSlider.SetValue(value: 0.5f, withNotify: true);
            _seedIndex = Random.Range(0, _generationConfig.seeds.Length);
            _refreshSeed();
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

            _selectablesController = new SelectablesController(_uiInput, grids);
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
            DrawTexture();
        }

        private void _onSizeSelected(IntRadioButton button)
        {
            _terrainData.terrainSize = button.Value;
            DrawTexture();
        }

        private void _onWaterEndedChange(float value)
        {
            _terrainData.water01 = value;
            DrawTexture();
        }

        private void _onForestEndedChange(float value)
        {
          
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

            _refreshSeed();
            DrawTexture();
        }

        private void _refreshSeed()
        {
            SeedData seedData = _generationConfig.seeds[_seedIndex];
            string seedName = seedData.name;
            _menuView.seedResult.SetText(seedName);
            _terrainData.seed = seedData.value; 
            DrawTexture();
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

        private void DrawTexture()
        {
            _previewRender.Render(new TerrainPreviewRenderTask
            {
                terrain = _terrainData,
                outputTexture = _screenTexture
            });
            _menuView.mapImage.texture = _screenTexture;
        }

        private UniTask<OperationResult> _dispose()
        {
            _unsubscribeView();

            _selectablesController.Dispose();
            _sizeButtonsGroup.Dispose();

            return OperationResult.SuccessSync();
        }
    }
}