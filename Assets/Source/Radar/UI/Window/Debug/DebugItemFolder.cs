using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Common;
using Beast.Radar.UI.Debug;
using Unity.Mathematics;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class DebugItemFolder : IFolder
    {
        public Dictionary<ESelectionZone, SelectableGridData> Selections { get; private set; }

        private static readonly float[] ItemsCount = { 1, 10, 100 };
        
        private readonly ItemData[] _itemsData;
        private readonly DebugItemFolderView _view;
        private readonly IUIFactory _uiFactory;
        private readonly ISelectionZoneBuilder _zoneBuilder;

        private DebugItemBuilder _builder;

        private DebugItemButton _selectedButton;
        
        public DebugItemFolder(
            DebugItemFolderView view, 
            IUIFactory uiFactory, 
            ISelectionZoneBuilder zoneBuilder,
            ItemData[] itemsData)
        {
            _itemsData = itemsData;
            _view = view;
            _uiFactory = uiFactory;
            _zoneBuilder = zoneBuilder;
        }

        public void Warmup()
        {
            _builder = new DebugItemBuilder(_uiFactory, _view.itemsRoot);
            _view.itemButtons = _builder.BuildItemButtons(_itemsData);

            InitializeSelectables();
            Subscribe();
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        public void Open()
        {
            _view.body.SetActive(true);
        }

        public void Close()
        {
            _view.body.SetActive(false);
            Reset();
        }

        private void InitializeSelectables()
        {
            List<Selectable> dpadSelectables = new List<Selectable>();
            dpadSelectables.Add(_view.previousItems.Selectable);
            dpadSelectables.Add(_view.nextItems.Selectable);
            dpadSelectables.AddRange(_view.itemGrantButtons.Select(b => b.Selectable));
            SelectableGridData dpadGrid = _zoneBuilder.Build(dpadSelectables.ToArray(), new int2(5, 1), false);

            Selectable[] rightStickSelectables = _view.itemButtons.Select(b => b.Selectable).ToArray();
            SelectableGridData rightStickGrid = _zoneBuilder.Build(rightStickSelectables, new int2(15, 5), false);

            Selections = new Dictionary<ESelectionZone, SelectableGridData>()
            {
                { ESelectionZone.DPad, dpadGrid },
                { ESelectionZone.RightStick, rightStickGrid }
            };
        }

        private void Subscribe()
        {
            foreach (DebugItemButton button in _view.itemButtons)
            {
                button.Clicked += OnItemButtonClicked;
            }

            for (var i = 0; i < _view.itemGrantButtons.Length; i++)
            {
                int index = i;
                _view.itemGrantButtons[i].Clicked += () => OnGrantButtonClicked(index);
            }
        }

        private void Unsubscribe()
        {
            foreach (DebugItemButton button in _view.itemButtons)
            {
                button.Clicked -= OnItemButtonClicked;
            }
        }

        private void OnItemButtonClicked(DebugItemButton button)
        {
            if (_selectedButton != null)
            {
                _selectedButton.SetDefaultMode();
            }

            _selectedButton = button;
            _selectedButton.SetSelectedMode();
            
            _view.itemName.SetText(button.Item.name);
            _view.itemWeight.SetText($"{button.Item.weight} kg");
        }

        private void OnGrantButtonClicked(int index)
        {
            BDebug.LogInfo(this, $"Granted {ItemsCount[index]} {_selectedButton.Item.name}");
        }

        private void Reset()
        {
            if (_selectedButton != null)
            {
                _selectedButton.SetDefaultMode();
            }
            _view.itemName.SetText("name");
            _view.itemWeight.SetText("weight");
        }
    }
}