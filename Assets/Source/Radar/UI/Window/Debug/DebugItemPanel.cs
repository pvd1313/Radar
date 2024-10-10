using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Common;
using Beast.Radar.InputControl;
using Beast.Radar.UI.Debug;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class DebugItemPanel : IPanel
    {
        private static readonly float[] ItemsCount = { 1, 10, 100 };

        private readonly ItemData[] _itemsData;
        private readonly DebugItemFolderView _view;
        private readonly IUIFactory _uiFactory;
        private readonly ISelectionZoneBuilder _zoneBuilder;
        private readonly IUIInputController _input;

        private SelectableController _selectableController;
        private DebugItemPanelBuilder _builder;

        private DebugItemButton _selectedButton;

        public DebugItemPanel(
            DebugItemFolderView view,
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
            _builder = new DebugItemPanelBuilder(_uiFactory, _view.itemsRoot);
            _view.itemButtons = _builder.BuildItemButtons(_itemsData);

            ConfigureSelectables();
            SubscribeView();
        }

        public void Dispose()
        {
            UnsubscribeView();
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
            SelectableGridData shoulderGrid = _zoneBuilder.Build(new[]
                {
                    _view.previousItems.Selectable,
                    _view.nextItems.Selectable,
                },
                isFillHorizontally: true,
                isLooping: false);


            SelectableGridData west = _zoneBuilder.Build(_view.itemGrantButtons[0].Selectable);
            SelectableGridData north = _zoneBuilder.Build(_view.itemGrantButtons[1].Selectable);
            SelectableGridData east = _zoneBuilder.Build(_view.itemGrantButtons[2].Selectable);

            Selectable[] dpadSelectables = _view.itemButtons.Select(b => b.Selectable).ToArray();
            SelectableGridData dpadGrid = _zoneBuilder.Build(dpadSelectables,
                cols: 15, rows: 5,
                isFillHorizontally: false,
                isLooping: false);

            _selectableController = new SelectableController(_input, new Dictionary<ESelectionZone, SelectableGridData>
            {
                { ESelectionZone.DPad, dpadGrid },
                { ESelectionZone.Shoulder, shoulderGrid },
                { ESelectionZone.WestButton, west },
                { ESelectionZone.NorthButton, north },
                { ESelectionZone.EastButton, east },
            });
        }

        private void SubscribeView()
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