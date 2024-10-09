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

        private SelectableController _selectableController;
        private DebugMenuView _view;
        private WindowPanelsController _panelsController;

        public DebugMenuWindow(
            IWindowManager windowManager,
            IUIFactory uiFactory,
            ISelectionZoneBuilder zoneBuilder,
            IUIInputController input)
        {
            _windowManager = windowManager;
            _uiFactory = uiFactory;
            _zoneBuilder = zoneBuilder;
            _input = input;
        }

        public void ListenGameCycle(IGameCycleHandle handle)
        {
#if !BEAST_DEVELOPMENT
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

            InitializeFolders();
            ConfigureSelectables();

            SubscribeView();

            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> Dispose()
        {
            UnsubscribeView();

            _panelsController.Dispose();

            return OperationResult.SuccessSync();
        }

        private void InitializeFolders()
        {
            _panelsController = new WindowPanelsController(_view.folderButtons, new IPanel[]
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
            
            _panelsController.Warmup();
        }

        private void ConfigureSelectables()
        {
            SelectableGridData triggerGrid = _zoneBuilder.Build
            (
                selectables: _view.folderButtons.Select(b => b.Selectable).ToArray(),
                isFillHorizontally: false,
                isLooping: false
            );

            _selectableController = new SelectableController(_input, new Dictionary<ESelectionZone, SelectableGridData>
            {
                { ESelectionZone.Trigger, triggerGrid }
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

        private void OnCloseButtonClicked()
        {
            _windowManager.Show(WindowID.MainHUD);
        }

        private void OnOpenButtonClicked()
        {
            _windowManager.Show(WindowID.Debug);
        }
    }
}