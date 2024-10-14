using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Common;
using Beast.Radar.InputControl;

namespace Beast.Radar.UI
{
    public class DebugItemPanel : IPanel
    {
        private static readonly float[] ItemsCount = { 1, 10, 100 };

        private readonly ItemData[] _itemsData;
        private readonly DebugItemPanelView _view;
        private readonly IUIFactory _uiFactory;
        private readonly ISelectionZoneBuilder _zoneBuilder;
        private readonly IUIInputController _input;

        private SelectableController _selectableController;
        private UIPool<DebugItemButton> _itemsPool;

        private UIPool<DebugItemTypeButton> _itemTypePool;
        private UIButtonRadioGroup<DebugItemTypeButton> _itemTypeGroup;


        private DebugItemButton _selectedButton;

        public DebugItemPanel(
            DebugItemPanelView view,
            IUIFactory uiFactory,
            ISelectionZoneBuilder zoneBuilder,
            IUIInputController input,
            ItemData[] itemsData)
        {
            _view = view;
            _uiFactory = uiFactory;
            _zoneBuilder = zoneBuilder;
            _input = input;
            _itemsData = itemsData;
        }

        public void Warmup()
        {
            _itemsPool = new UIPool<DebugItemButton>(_uiFactory, PrefabID.DebugItemButton, _view.itemsRoot);
            foreach (ItemData data in _itemsData)
            {
                DebugItemButton itemButton = _itemsPool.GetNext();
                itemButton.SetItemData(data);
            }

            _itemTypePool = new UIPool<DebugItemTypeButton>(_uiFactory, PrefabID.DebugItemTypeButton, _view.itemTypeRoot);
            foreach (var type in Enum.GetValues(typeof(EItemType)).Cast<EItemType>().ToArray())
            {
                _itemTypePool.GetNext(type);
            }
            _itemTypeGroup = new UIButtonRadioGroup<DebugItemTypeButton>(_itemTypePool.GetAllEnabled().ToArray());
            _itemTypeGroup.Warmup();
            _itemTypeGroup.Selected += ItemTypeGroupOnSelected;
            
            ConfigureSelectables();
            SubscribeView();
            
            _selectableController.Warmup();
        }

        public void Dispose()
        {
            UnsubscribeView();
            _itemTypeGroup.Dispose();

            _selectableController.Dispose();
        }

        private void ItemTypeGroupOnSelected(DebugItemTypeButton typeButton)
        {
            _itemsPool.DisableAll();
            
            ItemData[] typedData = typeButton.ItemType == EItemType.None 
                ? _itemsData
                : _itemsData.Where(i => i.type == typeButton.ItemType).ToArray();
            
            foreach (ItemData itemData in typedData)
            {
                DebugItemButton itemButton = _itemsPool.GetNext();
                itemButton.SetItemData(itemData);
            }
            
            UISelectable[] dpadSelectables = _itemsPool.GetAllEnabled().Select(b => b.Selectable).ToArray();
            Tuple<ESelectionZone, SelectableGridData> dpadGrid = _zoneBuilder.Build(
                zone: ESelectionZone.DPad,
                selectables: dpadSelectables,
                cols: 15, rows: 4,
                isFillHorizontally: false,
                isLooping: false);

            _selectableController.Refresh(dpadGrid.Item1, dpadGrid.Item2);
        }

        public void Open()
        {
            _selectableController.Enable();
            _view.body.SetActive(true);
        }

        public void Close()
        {
            _view.body.SetActive(false);
            _selectableController.Disable();
            Reset();
        }

        private void ConfigureSelectables()
        {
            Tuple<ESelectionZone, SelectableGridData> shoulderGrid = _zoneBuilder.Build(
                ESelectionZone.Shoulder,
                _itemTypePool.GetAll().Select(i => i.Selectable).ToArray(),
                isFillHorizontally: true,
                isLooping: true);

            Tuple<ESelectionZone, SelectableGridData> west = _zoneBuilder.Build(
                ESelectionZone.WestButton, _view.itemGrantButtons[0].Selectable);

            Tuple<ESelectionZone, SelectableGridData> north = _zoneBuilder.Build(
                ESelectionZone.NorthButton, _view.itemGrantButtons[1].Selectable);

            Tuple<ESelectionZone, SelectableGridData> east = _zoneBuilder.Build(
                ESelectionZone.EastButton, _view.itemGrantButtons[2].Selectable);

            UISelectable[] dpadSelectables = _itemsPool.GetAllEnabled().Select(b => b.Selectable).ToArray();
            Tuple<ESelectionZone, SelectableGridData> dpadGrid = _zoneBuilder.Build(
                zone: ESelectionZone.DPad,
                selectables: dpadSelectables,
                cols: 15, rows: 4,
                isFillHorizontally: false,
                isLooping: false);

            _selectableController = new SelectableController(_input, new Dictionary<ESelectionZone, SelectableGridData>
            {
                { shoulderGrid.Item1, shoulderGrid.Item2 },
                { dpadGrid.Item1, dpadGrid.Item2 },
                { north.Item1, north.Item2 },
                { west.Item1, west.Item2 },
                { east.Item1, east.Item2 },
            });
        }

        private void SubscribeView()
        {
            foreach (DebugItemButton button in _itemsPool.GetAll())
            {
                button.Clicked += OnItemButtonClicked;
            }

            for (int i = 0; i < _view.itemGrantButtons.Length; i++)
            {
                int index = i;
                _view.itemGrantButtons[i].Clicked += () => OnGrantButtonClicked(index);
            }
        }

        private void UnsubscribeView()
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
            //todo Add an action to issue an item 
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