using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Resources;
using Beast.Radar.Debug;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class DebugMenuWindow : Controller, IWindow<Empty>, IGameLoopListener
    {
        private readonly DebugPrefabLibrary _library;
        private readonly IWindowManager _windowManager;
        private readonly IDebugController _debugController;
        private readonly IResourceManager _resources;

        private DebugMenuView _view;
        private PanelsController _panelsController;
        private IWindowHandle<Empty> _mainHUD;
        private IWindowHandle<Empty> _debugWindow;

        private UniTask _viewBuildingTask;
        private bool _viewReady;
        
        public DebugMenuWindow(
            DebugPrefabLibrary library,
            IWindowManager windowManager,
            IDebugController debugController,
            IResourceManager resources)
        {
            _library = library;
            _windowManager = windowManager;
            _debugController = debugController;
            _resources = resources;
        }

        public void Register(IGameLoopHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }
        
        public async UniTask Open(Empty data)
        {
            if (!_viewReady)
            {
                await _viewBuildingTask;
            }
            
            _panelsController.Enable();
            _view.screen.SetActive(true);
        }

        public async UniTask Close()
        {
            if (!_viewReady)
            {
                await _viewBuildingTask;
            }
            
            _hideView();
            _panelsController.Disable();
        }

        private UniTask _warmup(ConcurrentContext context)
        {
            _windowManager.Register(this, WindowID.DEBUG);
            _mainHUD = _windowManager.GetHandle<Empty>(this, WindowID.HUD);
            _debugWindow = _windowManager.GetHandle<Empty>(this, WindowID.DEBUG);
            
            _viewBuildingTask = UniTask.Create(async () =>
            {
                _view = await BFactory.InstantiateMono(this, _library.debugMenu);
                _view.transform.SetSiblingIndex(2);
                _view.transform.SetParent(_windowManager.CanvasTransform, false);
                
                _configurePanels(_library);
                
                _subscribeView();
                
                _subscribeInput();
                
                _view.buildVersionText.SetText(Application.version);

                _viewReady = true;
            });
            
            return UniTask.CompletedTask;
        }

        private void _configurePanels(DebugPrefabLibrary library)
        {
            _panelsController = new PanelsController(_view.panelButtons, new IPanel[]
            {
                //TODO: https://risenbeast.youtrack.cloud/issue/R-50/Sync-JSON-items-with-debug-inventory
                new DebugItemPanel(library, _view.itemView, _getTestItems()),
                new DebugSettingsPanel(_view.settingsView, _debugController),
            });
            _panelsController.Warmup();
        }

        private void _subscribeView()
        {
            _view.openButton.Clicked += _showWindow;
            _view.closeButton.Clicked += _closeWindow;
        }

        private void _unsubscribeView()
        {
            _view.openButton.Clicked -= _showWindow;
            _view.closeButton.Clicked -= _closeWindow;
        }

        private void _subscribeInput()
        {
        }

        private void _unsubscribeInput()
        {
        }

        private void _dispose(BlockingContext context)
        {
            _unsubscribeView();
            _unsubscribeInput();
        }

        private void _closeWindow()
        {
            _mainHUD.Open(default);
        }

        private void _showWindow()
        {
            _debugWindow.Open(default);
        }

        private void _hideView()
        {
            _view.screen.SetActive(false);
        }

        private static ItemData[] _getTestItems()
        {
            return new ItemData[]
            {
                new() { type = EItemType.Equipment, name = "shirt", weight = 1 },
                new() { type = EItemType.Equipment, name = "pants", weight = 1.2f },
                new() { type = EItemType.Equipment, name = "gloves", weight = 0.6f },
                new() { type = EItemType.Equipment, name = "helmet", weight = 1.3f },
                new() { type = EItemType.Equipment, name = "boots", weight = 1.1f },
                new() { type = EItemType.Equipment, name = "shield", weight = 5.5f },
                new() { type = EItemType.Equipment, name = "backpack", weight = 2.2f },
                new() { type = EItemType.Equipment, name = "armor", weight = 6.5f },
                new() { type = EItemType.Equipment, name = "belt", weight = 0.3f },
                new() { type = EItemType.Equipment, name = "scarf", weight = 0.2f },
                new() { type = EItemType.Equipment, name = "ring", weight = 0.1f },
                new() { type = EItemType.Equipment, name = "bracers", weight = 0.8f },
                new() { type = EItemType.Equipment, name = "cape", weight = 0.9f },
                new() { type = EItemType.Equipment, name = "goggles", weight = 0.4f },
                new() { type = EItemType.Equipment, name = "shoulder pads", weight = 1.4f },
                new() { type = EItemType.Equipment, name = "shin guards", weight = 1.2f },
                new() { type = EItemType.Equipment, name = "cloak", weight = 1.1f },
                new() { type = EItemType.Equipment, name = "leggings", weight = 1.3f },

                new() { type = EItemType.Weapons, name = "knife", weight = 1.5f },
                new() { type = EItemType.Weapons, name = "sword", weight = 3.2f },
                new() { type = EItemType.Weapons, name = "bow", weight = 2.5f },
                new() { type = EItemType.Weapons, name = "crossbow", weight = 4.0f },
                new() { type = EItemType.Weapons, name = "dagger", weight = 0.9f },
                new() { type = EItemType.Weapons, name = "mace", weight = 4.3f },
                new() { type = EItemType.Weapons, name = "axe", weight = 3.8f },
                new() { type = EItemType.Weapons, name = "spear", weight = 3.6f },
                new() { type = EItemType.Weapons, name = "staff", weight = 2.0f },
                new() { type = EItemType.Weapons, name = "club", weight = 2.8f },
                new() { type = EItemType.Weapons, name = "flail", weight = 3.5f },
                new() { type = EItemType.Weapons, name = "morning star", weight = 4.5f },
                new() { type = EItemType.Weapons, name = "warhammer", weight = 5.0f },
                new() { type = EItemType.Weapons, name = "halberd", weight = 6.0f },
                new() { type = EItemType.Weapons, name = "throwing knife", weight = 0.5f },
                new() { type = EItemType.Weapons, name = "longsword", weight = 4.2f },

                new() { type = EItemType.Tools, name = "hammer", weight = 3 },
                new() { type = EItemType.Tools, name = "pickaxe", weight = 3.8f },
                new() { type = EItemType.Tools, name = "shovel", weight = 2.5f },
                new() { type = EItemType.Tools, name = "wrench", weight = 1.2f },
                new() { type = EItemType.Tools, name = "pliers", weight = 0.6f },
                new() { type = EItemType.Tools, name = "crowbar", weight = 2.3f },
                new() { type = EItemType.Tools, name = "saw", weight = 1.7f },
                new() { type = EItemType.Tools, name = "drill", weight = 3.0f },
                new() { type = EItemType.Tools, name = "screwdriver", weight = 0.4f },
                new() { type = EItemType.Tools, name = "axe", weight = 2.5f },
                new() { type = EItemType.Tools, name = "hoe", weight = 3.2f },
                new() { type = EItemType.Tools, name = "file", weight = 0.7f },
                new() { type = EItemType.Tools, name = "rake", weight = 2.0f },
                new() { type = EItemType.Tools, name = "trowel", weight = 0.3f },
                new() { type = EItemType.Tools, name = "bucket", weight = 1.1f },
                new() { type = EItemType.Tools, name = "lantern", weight = 1.6f },

                new() { type = EItemType.Eat, name = "bread", weight = 0.2f },
                new() { type = EItemType.Eat, name = "water", weight = 0.2f },
                new() { type = EItemType.Eat, name = "dirty water", weight = 0.2f },
                new() { type = EItemType.Eat, name = "meat", weight = 0.7f },
                new() { type = EItemType.Eat, name = "apple", weight = 0.3f },
                new() { type = EItemType.Eat, name = "cheese", weight = 0.5f },
                new() { type = EItemType.Eat, name = "fish", weight = 0.8f },
                new() { type = EItemType.Eat, name = "berry", weight = 0.1f },
                new() { type = EItemType.Eat, name = "milk", weight = 1.0f },
                new() { type = EItemType.Eat, name = "honey", weight = 0.4f },
                new() { type = EItemType.Eat, name = "wine", weight = 0.75f },
                new() { type = EItemType.Eat, name = "beer", weight = 0.6f },
                new() { type = EItemType.Eat, name = "mushroom", weight = 0.2f },
                new() { type = EItemType.Eat, name = "carrot", weight = 0.2f },
                new() { type = EItemType.Eat, name = "potato", weight = 0.5f },
                new() { type = EItemType.Eat, name = "fruit salad", weight = 0.7f },
                new() { type = EItemType.Eat, name = "vegetable stew", weight = 1.2f },
                new() { type = EItemType.Eat, name = "corn", weight = 0.4f },
                new() { type = EItemType.Eat, name = "grapes", weight = 0.3f }
            };
        }
    }
}