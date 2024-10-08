using System.Collections.Generic;
using Beast.Claw.GameCycle;
using Beast.Radar.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Beast.Radar.UI
{
    public class MapGenerationWindow : IWindow, IGameCycleListener
    {
        public Dictionary<ESelectionZone, SelectableGridData> Selections { get; private set; }
        
        private static readonly string[] Seeds = { "sky", "cloud", "black forest", "snail" };
        private static readonly int[] Sizes = { 512, 1024, 2058, 4096 };

        private readonly IWindowManager _windowManager;
        private readonly ISelectionZoneBuilder _selectionBuilder;
        private readonly IUIFactory _uiFactory;

        private MapGenerationView _menuView;
        private GenerationMapData _data;

        public MapGenerationWindow(
            IUIFactory uiFactory, 
            IWindowManager windowManager, 
            ISelectionZoneBuilder selectionBuilder)
        {
            _uiFactory = uiFactory;
            _windowManager = windowManager;
            _selectionBuilder = selectionBuilder;
        }

        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
        }

        private UniTask<OperationResult> Warmup()
        {
            _uiFactory.Create(this, PrefabID.MapGenerationWindow, out _menuView);
            _windowManager.Register(this, WindowID.MapGeneration, _menuView);
            
            Selectable[] selections = 
            {
                _menuView.seedButton.Selectable,
                _menuView.sizeButtons[0].Selectable,
                _menuView.sizeButtons[1].Selectable,
                _menuView.sizeButtons[2].Selectable,
                _menuView.sizeButtons[3].Selectable,
                _menuView.forestSlider.Selectable,
                _menuView.waterSlider.Selectable,
                _menuView.createMapButton.Selectable
            };
            
            SelectableGridData dpadGrid = _selectionBuilder.Build(selections, new int2(1, selections.Length), false);
            Selections = new Dictionary<ESelectionZone, SelectableGridData>() { { ESelectionZone.DPad, dpadGrid } };
            
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
            _menuView.gameObject.SetActive(true);
        }

        public void Close()
        {
            _menuView.gameObject.SetActive(false);
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