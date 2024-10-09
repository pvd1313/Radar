using System.Collections.Generic;
using System.Linq;
using Beast.Claw.GameCycle;
using Beast.Radar.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class DebugMenuWindow : IWindow, IGameCycleListener
    {
        public Dictionary<ESelectionZone, SelectableGridData> Selections { get; private set; }

        private readonly IWindowManager _windowManager;
        private readonly IUIFactory _uiFactory;
        private readonly ISelectionZoneBuilder _selectionZoneBuilder;

        private DebugMenuView _view;
        private WindowFoldersController _foldersController;

        public DebugMenuWindow(
            IWindowManager windowManager,
            IUIFactory uiFactory,
            ISelectionZoneBuilder selectionZoneBuilder)
        {
            _windowManager = windowManager;
            _uiFactory = uiFactory;
            _selectionZoneBuilder = selectionZoneBuilder;
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
            _view.screen.SetActive(true);
        }

        public void Close()
        {
            _view.screen.SetActive(false);
        }

        private UniTask<OperationResult> Warmup()
        {
            _uiFactory.Create(this, PrefabID.DebugWindow, out _view);
            _windowManager.Register(this, WindowID.Debug, _view);

            InitializeFolders();
            InitializeSelectables();

            Subscribe();

            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> Dispose()
        {
            Unsubscribe();

            _foldersController.Dispose();

            return OperationResult.SuccessSync();
        }

        private void InitializeFolders()
        {
            _foldersController = new WindowFoldersController(_view.folderButtons, new IFolder[]
            {
                new DebugItemFolder(_view.itemView, _uiFactory, _selectionZoneBuilder, itemsData: new ItemData[]
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
                new DebugSettingsFolder(_view.settingsView, _selectionZoneBuilder),
            });
            _foldersController.Warmup();
        }

        private void InitializeSelectables()
        {
            SelectableGridData triggerGrid = _selectionZoneBuilder.Build
            (
                selectables: _view.folderButtons.Select(b => b.Selectable).ToArray(),
                size: new int2(2, 1),
                isLooping: false
            );

            Selections = new Dictionary<ESelectionZone, SelectableGridData>
            {
                { ESelectionZone.Trigger, triggerGrid }
            };

            foreach (var folderSelection in _foldersController.CurrentFolder.Selections)
            {
                Selections.TryAdd(folderSelection.Key, folderSelection.Value);
            }
        }

        private void Subscribe()
        {
            _foldersController.FolderSwitched += OnFolderSwitched;
            _view.openButton.Clicked += OnOpenButtonClicked;
            _view.closeButton.Clicked += OnCloseButtonClicked;
        }

        private void Unsubscribe()
        {
            _foldersController.FolderSwitched -= OnFolderSwitched;
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

        private void OnFolderSwitched(IFolder oldFolder, IFolder newFolder)
        {
            foreach (var oldSelection in oldFolder.Selections)
            {
                Selections.Remove(oldSelection.Key);
            }

            foreach (var newSelection in newFolder.Selections)
            {
                Selections.Add(newSelection.Key, newSelection.Value);
            }
        }
    }
}