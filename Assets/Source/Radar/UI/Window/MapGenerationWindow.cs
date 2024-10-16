using System;
using System.Collections.Generic;
using Beast.Claw.GameCycle;
using Beast.Radar.GameCycle;
using Beast.Radar.InputControl;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

namespace Beast.Radar.UI
{
    public class MapGenerationWindow : IWindow, IGameCycleListener
    {
        private static readonly string[] Seeds = { "sky", "cloud", "black forest", "snail" };
        private static readonly int[] Sizes = { 512, 1024, 2058, 4096 };

        private readonly IWindowManager _windowManager;
        private readonly IUIFactory _uiFactory;
        private readonly IUIInputController _uiInput;
        private readonly IPlayerInputProvider _playerInput;
        private readonly ISelectableZoneBuilder _selectableBuilder;

        private MapGenerationView _menuView;
        private SelectablesController _selectablesController;
        private ButtonRadioGroup<IntRadioButton> _sizeButtonsGroup;
        private GenerationMapData _data;
        private int _seedIndex;

        public MapGenerationWindow(
            IUIFactory uiFactory,
            IWindowManager windowManager,
            ISelectableZoneBuilder selectableBuilder,
            IUIInputController uiInput,
            IPlayerInputProvider playerInput)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
            _selectableBuilder = selectableBuilder;
            _uiInput = uiInput;
            _playerInput = playerInput;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, _warmup);
            handle.ListenOperation(this, OperationID.Dispose, _dispose);
        }

        public void Open()
        {
            _selectablesController.Enable();
            _menuView.gameObject.SetActive(true);
        }

        public void Close()
        {
            _menuView.gameObject.SetActive(false);
            _selectablesController.Disable();
        }

        private UniTask<OperationResult> _warmup()
        {
            _uiFactory.Create(this, PrefabID.MapGenerationWindow, out _menuView);
            _windowManager.Register(this, WindowID.MapGeneration, _menuView);

            _sizeButtonsGroup = new ButtonRadioGroup<IntRadioButton>(_menuView.sizeButtons);
            _sizeButtonsGroup.Warmup(startIndex: _menuView.sizeButtons.Length - 1);

            _configureView();
            _configureSelectables();
            _subscribeView();
            
            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> _dispose()
        {
            _unsubscribeView();

            _selectablesController.Dispose();
            _sizeButtonsGroup.Dispose();

            return OperationResult.SuccessSync();
        }

        private void _configureView()
        {
            _menuView.waterSlider.SetValue(value: 0.5f, withNotify: false);
            _menuView.forestSlider.SetValue(value: 0.5f, withNotify: false);
            _menuView.biomeSlider.SetValue(value: 0.5f, withNotify: false);
            _seedIndex = Random.Range(0, Seeds.Length);
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
                zone: ESelectionZone.DPad,
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
            _data.biomeSize = value;
        }

        private void _onSizeSelected(IntRadioButton button)
        {
            _data.size = button.Value;
        }

        private void _onWaterEndedChange(float value)
        {
            _data.water = value;
        }

        private void _onForestEndedChange(float value)
        {
            _data.forest = value;
        }

        private void _onSeedButtonsClicked(int step)
        {
            _seedIndex += step;
            if (_seedIndex < 0)
            {
                _seedIndex = Seeds.Length - 1;
            }
            else if(_seedIndex == Seeds.Length)
            {
                _seedIndex = 0;
            }
            
            _refreshSeed();
        }

        private void _refreshSeed()
        {
            string seedName = Seeds[_seedIndex];
            _menuView.seedResult.SetText(seedName);
            _data.seed = seedName.GetHashCode();
        }


        private void _onCreateMapButtonClicked()
        {
            //todo add create map action 
        }

        private void _onCloseButtonClicked()
        {
            _windowManager.Show(WindowID.MainHUD);
        }
    }
}