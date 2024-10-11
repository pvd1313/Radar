using Beast.Claw.GameCycle;
using Beast.Radar.GameCycle;
using Beast.Radar.InputControl;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class MapGenerationWindow : IWindow, IGameCycleListener
    {
        private static readonly string[] Seeds = { "sky", "cloud", "black forest", "snail" }; 
        private static readonly int[] Sizes = { 512, 1024, 2058, 4096 }; 
        
        private readonly IWindowManager _windowManager;
        private readonly IUIFactory _uiFactory;
        private readonly IUIInputController _input;

        private UISelectionLoop _uiSelectableLoop;
        private MapGenerationView _menuView;
        private GenerationMapData _data = new();

        public MapGenerationWindow(
            IUIFactory uiFactory, 
            IWindowManager windowManager, 
            IUIInputController input)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
            _input = input;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
        }
        
        private UniTask<OperationResult> Warmup()
        {
            _uiFactory.Create(this, PrefabID.MapGenerationWindow, out _menuView);
            _windowManager.Register(this, WindowID.MapGeneration, _menuView);
            
            _uiSelectableLoop = new UISelectionLoop(_input, new[]
            {
                _menuView.seedButton.Selectable,
                _menuView.sizeButtons[0].Selectable,
                _menuView.sizeButtons[1].Selectable,
                _menuView.sizeButtons[2].Selectable,
                _menuView.sizeButtons[3].Selectable,
                _menuView.forestSlider.Selectable,
                _menuView.waterSlider.Selectable,
                _menuView.createMapButton.Selectable
            });
            
            SubscribeView();
            
            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> Dispose()
        {
            UnsubscribeView();
            
            return OperationResult.SuccessSync();
        }
        
        public void Open()
        {
            _uiSelectableLoop.Enable();
            _menuView.gameObject.SetActive(true);
        }

        public void Close()
        {
            _menuView.gameObject.SetActive(false);
            _uiSelectableLoop.Disable();
        }

        private void SubscribeView()
        {
            for (int i = 0; i < _menuView.sizeButtons.Length; i++)
            {
                int index = i;
                _menuView.sizeButtons[index].Clicked += () => OnClickSizeButton(index);
            }
            
            _menuView.closeButton.Clicked += OnClickCloseButton;
            _menuView.createMapButton.Clicked += OnCreateMapButton;
            _menuView.seedButton.Clicked += OnClickSeedButton;
            _menuView.forestSlider.EndedChange += OnEndedChangeForest;
            _menuView.waterSlider.EndedChange += OnEndedChangeWater;
        }

        private void UnsubscribeView()
        {
            _menuView.closeButton.Clicked -= OnClickCloseButton;
            _menuView.createMapButton.Clicked -= OnCreateMapButton;
            _menuView.seedButton.Clicked -= OnClickSeedButton;
            _menuView.forestSlider.EndedChange -= OnEndedChangeForest;
            _menuView.waterSlider.EndedChange -= OnEndedChangeWater;
        }

        private void OnEndedChangeWater(float value)
        {
            _data.water = value;
        }

        private void OnEndedChangeForest(float value)
        {
            _data.forest = value;
        }

        private void OnClickSeedButton()
        {
            string seedName = Seeds[Random.Range(0, Seeds.Length)];
            _menuView.seedResult.SetText(seedName);
            _data.seed = seedName.GetHashCode();
        }

        private void OnClickSizeButton(int index)
        {
            _data.size = Sizes[index];
        }

        private void OnCreateMapButton()
        {
            //todo add create map action 
        }

        private void OnClickCloseButton()
        {
            //todo add close menu action 
        }
    }
}