using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.GameCycle;
using Beast.Radar.GameCycle;
using Beast.Radar.InputControl;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class DebugMenuWindow : IWindow, IGameCycleListener
    {
        private readonly IWindowManager _windowManager;
        private readonly IUIFactory _uiFactory;
        private readonly ISelectionZoneBuilder _zoneBuilder;
        private readonly IUIInputController _input;
        private readonly IPlayerInputProvider _inputProvider;

        private SelectableController _selectableController;
        private DebugMenuView _view;
        private PanelsController _panelsController;

        public DebugMenuWindow(
            IWindowManager windowManager,
            IUIFactory uiFactory,
            ISelectionZoneBuilder zoneBuilder,
            IUIInputController input,
            IPlayerInputProvider inputProvider)
        {
            _windowManager = windowManager;
            _uiFactory = uiFactory;
            _zoneBuilder = zoneBuilder;
            _input = input;
            _inputProvider = inputProvider;
        }

        public void Register(IGameCycleHandle handle)
        {
#if BEAST_DEVELOPMENT
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
#endif
        }

        public void Open()
        {
            _panelsController.Enable();
            _selectableController.Enable();
            _view.screen.SetActive(true);
        }

        public void Close()
        {
            _view.screen.SetActive(false);
            _panelsController.Disable();
            _selectableController.Disable();
        }

        private UniTask<OperationResult> Warmup()
        {
            _uiFactory.Create(this, PrefabID.DebugWindow, out _view);
            _windowManager.Register(this, WindowID.Debug, _view);

            foreach (InputIconDisplay icon in _view.inputIcons)
            {
                icon.Refresh(_inputProvider.Scheme);
            }
            
            InitializePanels();
            ConfigureSelectables();

            SubscribeView();
            SubscribeInput();
            
            _panelsController.Warmup();
            _selectableController.Warmup();
                        
            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> Dispose()
        {
            UnsubscribeView();
            UnsubscribeInput();
            
            _panelsController.Dispose();
            _selectableController.Dispose();

            return OperationResult.SuccessSync();
        }

        private void InitializePanels()
        {
            _panelsController = new PanelsController(_view.panelButtons, new IPanel[]
            {
                //todo replace itemsData with data from the configuration file.
                new DebugItemPanel(_view.itemView, _uiFactory, _zoneBuilder, _input, new ItemData[]
                {
                    new() { type = EItemType.Equipment, name = "shirt", weight = 1 },
                    new() { type = EItemType.Equipment, name = "pants", weight = 1.2f },
                    new() { type = EItemType.Equipment, name = "gloves", weight = 0.6f },
                    new() { type = EItemType.Weapons, name = "knife", weight = 1.5f },
                    new() { type = EItemType.Tools, name = "hammer", weight = 3 },
                    new() { type = EItemType.Eat, name = "bread", weight = 0.2f },
                    new() { type = EItemType.Eat, name = "water", weight = 0.2f },
                    new() { type = EItemType.Eat, name = "dirty water", weight = 0.2f },
                    new() { type = EItemType.Eat, name = "meat", weight = 0.7f },
                }),
                new DebugSettingsPanel(_view.settingsView, _zoneBuilder, _input),
            });
        }

        private void ConfigureSelectables()
        {
            Tuple<ESelectionZone, SelectableGridData> triggerGrid = _zoneBuilder.Build(
                zone: ESelectionZone.Trigger,
                selectables: _view.panelButtons.Select(b => b.Selectable).ToArray(),
                isFillHorizontally: true,
                isLooping: false);
            
            _selectableController = new SelectableController(_input, new Dictionary<ESelectionZone, SelectableGridData>
            {
                { triggerGrid.Item1, triggerGrid.Item2 }
            });
        }

        private void SubscribeView()
        {
            _view.openButton.Clicked += OnOpenButtonClicked;
            _view.closeButton.Clicked += OnCloseButtonClicked;
        }

        private void UnsubscribeView()
        {
            _view.openButton.Clicked -= OnOpenButtonClicked;
            _view.closeButton.Clicked -= OnCloseButtonClicked;
        }

        private void SubscribeInput()
        {
            _inputProvider.ChangedScheme += OnInputSchemeChanged;
        }

        private void UnsubscribeInput()
        {
            _inputProvider.ChangedScheme -= OnInputSchemeChanged;
        }
        
        private void OnInputSchemeChanged(EInputScheme scheme)
        {
            foreach (InputIconDisplay icon in _view.inputIcons)
            {
                icon.Refresh(scheme);
            }
        }

        private void OnCloseButtonClicked()
        {
            UnityEngine.Debug.Log("click close");
            _windowManager.Show(WindowID.MainHUD);
        }

        private void OnOpenButtonClicked()
        {
            _windowManager.Show(WindowID.Debug);
        }
        
    }
}