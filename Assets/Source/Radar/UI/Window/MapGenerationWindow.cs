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

        private SelectablesController _selectablesController;
        private MapGenerationView _menuView;
        private GenerationMapData _data;

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
            
            _configureSelectables();
            _subscribeView();
            
            _selectablesController.Warmup();
            
            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> _dispose()
        {
            _unsubscribeView();
            
            _selectablesController.Dispose();
            
            return OperationResult.SuccessSync();
        }

        private void _configureSelectables()
        {
            UISelectable[,] selectablesGrid =
            {
                {_menuView.seedButton.Selectable, null, null, null},
                {_menuView.sizeButtons[0].Selectable, _menuView.sizeButtons[1].Selectable, 
                _menuView.sizeButtons[2].Selectable, _menuView.sizeButtons[3].Selectable },
                {_menuView.forestSlider.Selectable, null, null, null},
                {_menuView.waterSlider.Selectable, null, null, null},
                {_menuView.createMapButton.Selectable, null, null, null}
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
        }

        private void _subscribeView()
        {
            for (int i = 0; i < _menuView.sizeButtons.Length; i++)
            {
                int index = i;
                _menuView.sizeButtons[index].Clicked += () => _onClickSizeButton(index);
            }
            
            _menuView.closeButton.Clicked += _onClickCloseButton;
            _menuView.createMapButton.Clicked += _onCreateMapButton;
            _menuView.seedButton.Clicked += _onClickSeedButton;
            _menuView.forestSlider.EndedChange += _onEndedChangeForest;
            _menuView.waterSlider.EndedChange += _onEndedChangeWater;
        }

        private void _unsubscribeView()
        {
            _menuView.closeButton.Clicked -= _onClickCloseButton;
            _menuView.createMapButton.Clicked -= _onCreateMapButton;
            _menuView.seedButton.Clicked -= _onClickSeedButton;
            _menuView.forestSlider.EndedChange -= _onEndedChangeForest;
            _menuView.waterSlider.EndedChange -= _onEndedChangeWater;
        }

        private void _onEndedChangeWater(float value)
        {
            _data.water = value;
        }

        private void _onEndedChangeForest(float value)
        {
            _data.forest = value;
        }

        private void _onClickSeedButton()
        {
            string seedName = Seeds[Random.Range(0, Seeds.Length)];
            _menuView.seedResult.SetText(seedName);
            _data.seed = seedName.GetHashCode();
        }

        private void _onClickSizeButton(int index)
        {
            _data.size = Sizes[index];
        }

        private void _onCreateMapButton()
        {
            //todo add create map action 
        }

        private void _onClickCloseButton()
        {
            _windowManager.Show(WindowID.MainHUD);
        }
    }
}