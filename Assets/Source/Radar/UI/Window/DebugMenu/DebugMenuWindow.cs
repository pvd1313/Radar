using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.GameCycle;
using Beast.Radar.Debug;
using Beast.Radar.InputHandling;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class DebugMenuWindow : IWindow, IGameCycleListener
    {
        private readonly IWindowManager _windowManager;
        private readonly IUIFactory _uiFactory;
        private readonly ISelectableZoneBuilder _zoneBuilder;
        private readonly IInputSystem _inputSystem;
        private readonly IDebugController _debugController;

        private DebugMenuView _view;
        private PanelsController _panelsController;
        private SelectablesController _selectablesController;
        private InputIconsController _inputIconsController;
        
        public DebugMenuWindow(
            IWindowManager windowManager,
            IUIFactory uiFactory,
            ISelectableZoneBuilder zoneBuilder,
            IInputSystem inputSystem,
            IDebugController debugController)
        {
            _windowManager = windowManager;
            _uiFactory = uiFactory;
            _zoneBuilder = zoneBuilder;
            _inputSystem = inputSystem;
            _debugController = debugController;
        }

        public void Register(IGameCycleHandle handle)
        {
#if DEVELOPMENT
            handle.ListenOperation(ActionID.WARMUP, _warmup);
            handle.ListenOperation(ActionID.DISPOSE, _dispose);
#endif
        }
        
        public void Open()
        {
            _panelsController.Enable();
            _selectablesController.Enable();
            _view.screen.SetActive(true);
        }

        public void Close()
        {
            _hideView();
            _panelsController.Disable();
            _selectablesController.Disable();
        }

        private UniTask<ActionResult> _warmup(ActionContext context)
        {
            _uiFactory.Create(this, PrefabID.DebugWindow, out _view);
            _hideView();
            _view.transform.SetSiblingIndex(2);
            _windowManager.Register(this, WindowID.Debug, _view);
            
            _configurePanels();
            _configureSelectables();
            _configureInputIcons();

            _subscribeView();
            _subscribeInput();
            
            _view.buildVersionText.SetText(Application.version);
            
            return ActionResult.SuccessSyncronous();
        }

        private void _configurePanels()
        {
            _panelsController = new PanelsController(_view.panelButtons, new IPanel[]
            {
                //TODO: https://risenbeast.youtrack.cloud/issue/R-50/Sync-JSON-items-with-debug-inventory
                new DebugItemPanel(_view.itemView, _uiFactory, _zoneBuilder, _inputSystem, _getTestItems()),
                new DebugSettingsPanel(_view.settingsView, _zoneBuilder, _inputSystem, _debugController),
            });
            _panelsController.Warmup();
        }

        private void _configureSelectables()
        {
            Tuple<ESelectionZone, SelectableGridData> triggerGrid = _zoneBuilder.Build(
                zone: ESelectionZone.Trigger,
                selectables: _view.panelButtons.Select(b => b.Selectable).ToArray(),
                isFillHorizontally: true,
                isLooping: false);
            
            Dictionary<ESelectionZone, SelectableGridData> grids = new() 
            {
                { triggerGrid.Item1, triggerGrid.Item2 }
            };
            
            _selectablesController = new SelectablesController(_inputSystem, grids);
            _selectablesController.Warmup();
        }

        private void _configureInputIcons()
        {
            _inputIconsController = new InputIconsController(_inputSystem.Scheme, _view.inputIcons);
            _inputIconsController.Warmup();
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
            _inputSystem.Scheme.Changed += _onInputActionsSchemeChanged;
            _inputSystem.UIInput.OptionButtons.PressStarted += _onOpenButtonClicked;
        }

        private void _unsubscribeInput()
        {
            _inputSystem.Scheme.Changed -= _onInputActionsSchemeChanged;
            _inputSystem.UIInput.OptionButtons.PressStarted -= _onOpenButtonClicked;
        }

        private void _onInputActionsSchemeChanged(EInputScheme scheme)
        {
            foreach (InputIconDisplay icon in _view.inputIcons)
            {
                icon.Refresh(scheme);
            }
        }

        private void _onOpenButtonClicked(EInputOptionButton button)
        {
            if (button == EInputOptionButton.Back)
            {
                _windowManager.Show(_isOpened() ? WindowID.MainHUD : WindowID.Debug);
            }
            else if(button == EInputOptionButton.Fourth && _isOpened())
            {
               _windowManager.Show(WindowID.MainHUD);
            }
        }

        private bool _isOpened()
        {
            return _view.screen.activeSelf;
        }

        private UniTask<ActionResult> _dispose(ActionContext context)
        {
            _unsubscribeView();
            _unsubscribeInput();
            
            _panelsController.Dispose();
            _selectablesController.Dispose();
            _inputIconsController.Dispose();

            return ActionResult.SuccessSyncronous();
        }

        private void _closeWindow()
        {
            _windowManager.Show(WindowID.MainHUD);
        }

        private void _showWindow()
        {
            _windowManager.Show(WindowID.Debug);
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