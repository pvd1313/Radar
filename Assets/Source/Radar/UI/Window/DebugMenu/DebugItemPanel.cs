using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Radar.InputHandling;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class DebugItemPanel : IPanel
    {
        private static readonly float[] ItemsCount = { 1, 10, 100 };
        private static readonly int2 ItemGridSize = new(15, 4);

        private readonly int _maxItemsCount;
        private readonly UIPrefabLibrary _library;
        private readonly DebugItemPanelView _view;
        private readonly ItemData[] _itemsData;
        private readonly ISelectableZoneBuilder _zoneBuilder;
        private readonly IInputSystem _inputSystem;

        private UIPool<DebugItemButton> _itemsPool;
        private ButtonRadioGroup<DebugItemButton> _itemGroup;
        private UIPool<DebugItemTypeButton> _itemTypePool;
        private ButtonRadioGroup<DebugItemTypeButton> _itemTypeGroup;
        private SelectablesController _selectablesController;
        private InputIconsController _inputIconsController;

        private ItemData[] _currentItemsData;
        private int _currentPage = 1;

        public DebugItemPanel(
            UIPrefabLibrary library,
            DebugItemPanelView view,
            ISelectableZoneBuilder zoneBuilder,
            IInputSystem inputSystem,
            ItemData[] itemsData)
        {
            _library = library;
            _view = view;
            _zoneBuilder = zoneBuilder;
            _inputSystem = inputSystem;
            _itemsData = itemsData;
            _maxItemsCount = ItemGridSize.x * ItemGridSize.y;
        }

        public void Warmup()
        {
            _configureItemTypeButtons();
            _configureItemButtons();
            _configureSelectables();
            _configureInputIcons();
            _subscribe();
        }

        public void Dispose()
        {
            _unsubscribe();
            _selectablesController.Dispose();
            _inputIconsController.Dispose();
            _itemTypeGroup.Dispose();
            _itemGroup.Dispose();
        }

        public UniTask Open(Empty data)
        {
            _selectablesController.Enable();
            _view.body.SetActive(true);
            
            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            _view.body.SetActive(false);
            _selectablesController.Disable();
            
            return UniTask.CompletedTask;
        }
        

        private void _configureItemButtons()
        {
            _itemsPool = new UIPool<DebugItemButton>(_library.debugItemButton, _view.itemsRoot);

            _currentItemsData = _itemsData;
            _currentPage = 1;
            _refreshItemButtons(false);

            _itemGroup = new ButtonRadioGroup<DebugItemButton>(_itemsPool.GetAllEnabled().ToArray());
            _itemGroup.Warmup();
            _refreshItemInformation(_itemGroup.SelectedButton);
        }

        private void _configureItemTypeButtons()
        {
            InputIconDisplay leftShoulderIcon = BScene.InstantiateMono(this, _library.inputIconDisplay, float3.zero, quaternion.identity);
            leftShoulderIcon.SetKey(EInputButton.LeftShoulder);
            leftShoulderIcon.transform.SetParent(_view.itemTypeRoot);
            _view.inputIcons.Add(leftShoulderIcon);

            _itemTypePool = new UIPool<DebugItemTypeButton>(_library.debugItemTypeButton, _view.itemTypeRoot);

            foreach (EItemType type in Enum.GetValues(typeof(EItemType)).Cast<EItemType>().ToArray())
            {
                DebugItemTypeButton typeButton = _itemTypePool.GetNext();
                typeButton.SetType(type);
            }

            InputIconDisplay rightShoulderIcon = BScene.InstantiateMono(this, _library.inputIconDisplay, float3.zero, quaternion.identity);
            rightShoulderIcon.SetKey(EInputButton.RightShoulder);
            rightShoulderIcon.transform.SetParent(_view.itemTypeRoot);
            _view.inputIcons.Add(rightShoulderIcon);

            _itemTypeGroup = new ButtonRadioGroup<DebugItemTypeButton>(_itemTypePool.GetAllEnabled().ToArray());
            _itemTypeGroup.Warmup();
        }

        private void _configureSelectables()
        {
            Tuple<ESelectionZone, SelectableGridData> shoulderGrid = _zoneBuilder.Build(
                ESelectionZone.Shoulder,
                _itemTypePool.GetAllEnabled().Select(i => i.Selectable).ToArray(),
                isFillHorizontally: true,
                isLooping: true);

            Tuple<ESelectionZone, SelectableGridData> south = _zoneBuilder.Build(
                ESelectionZone.FirstOptionButton, _view.itemGrantButtons[0].Selectable);

            Tuple<ESelectionZone, SelectableGridData> west = _zoneBuilder.Build(
                ESelectionZone.SecondOptionButton, _view.itemGrantButtons[1].Selectable);

            Tuple<ESelectionZone, SelectableGridData> north = _zoneBuilder.Build(
                ESelectionZone.ThirdOptionButton, _view.itemGrantButtons[2].Selectable);

            UISelectable[] dpadSelectables = _itemsPool.GetAllEnabled().Select(b => b.Selectable).ToArray();
            Tuple<ESelectionZone, SelectableGridData> dpadGrid = _zoneBuilder.Build(
                zone: ESelectionZone.Navigate,
                selectables: dpadSelectables,
                cols: ItemGridSize.x, rows: ItemGridSize.y,
                isFillHorizontally: false,
                isLooping: false);

            _selectablesController = new SelectablesController(_inputSystem,
                new Dictionary<ESelectionZone, SelectableGridData>
                {
                    { dpadGrid.Item1, dpadGrid.Item2 },
                    { shoulderGrid.Item1, shoulderGrid.Item2 },
                    { north.Item1, north.Item2 },
                    { west.Item1, west.Item2 },
                    { south.Item1, south.Item2 },
                });

            _selectablesController.Warmup();
        }

        private void _configureInputIcons()
        {
            _inputIconsController = new InputIconsController(_inputSystem.Scheme, _view.inputIcons);
            _inputIconsController.Warmup();
        }
        
        private void _subscribe()
        {
            for (int i = 0; i < _view.itemGrantButtons.Length; i++)
            {
                int index = i;
                _view.itemGrantButtons[i].Clicked += () => _onGrantButtonClicked(index);
            }

            _itemTypeGroup.Selected += _onItemTypeSelected;
            _itemGroup.Selected += _onItemSelected;
            _view.nextItems.Clicked += _onNextItemsPressed;
            _view.previousItems.Clicked += _onPreviousItemsPressed;
            _selectablesController.TryWentBeyond += _onSelectablesTryWentBeyond;
        }

        private void _unsubscribe()
        {
            _itemTypeGroup.Selected -= _onItemTypeSelected;
            _itemGroup.Selected -= _onItemSelected;
            _view.nextItems.Clicked -= _onNextItemsPressed;
            _view.previousItems.Clicked -= _onPreviousItemsPressed;
            _selectablesController.TryWentBeyond -= _onSelectablesTryWentBeyond;
        }

        private void _onNextItemsPressed()
        {
            _currentPage++;
            _refreshItemButtons(reverseColumnIndex: true);
        }

        private void _onPreviousItemsPressed()
        {
            _currentPage--;
            _refreshItemButtons(reverseColumnIndex: true);
        }

        private void _onSelectablesTryWentBeyond(ESelectionZone zone, int2 direction)
        {
            if (zone is not ESelectionZone.Navigate || Math.Abs(direction.x) != 1)
            {
                return;
            }

            if (direction.x < 0 && _currentPage > 1)
            {
                _currentPage--;
                _refreshItemButtons(reverseColumnIndex: true);
            }
            else if (direction.x > 0)
            {
                int startIndex = _currentPage > 1 ? _maxItemsCount * (_currentPage - 1) : 0;
                int limit = Math.Min(_maxItemsCount + startIndex, _currentItemsData.Length);
                if (limit < _currentItemsData.Length)
                {
                    _currentPage++;
                    _refreshItemButtons(reverseColumnIndex: true);
                }
            }
        }

        private void _onGrantButtonClicked(int index)
        {
            if (_itemGroup.SelectedButton == null)
            {
                return;
            }

            BDebug.LogInfo(this, $"Granted {ItemsCount[index]} {_itemGroup.SelectedButton.Item.name}");
            //todo Add an action to issue an item 
        }

        private void _onItemTypeSelected(DebugItemTypeButton typeButton)
        {
            _itemsPool.DisableAll();

            _currentItemsData = typeButton.ItemType == EItemType.None
                ? _itemsData
                : _itemsData.Where(i => i.type == typeButton.ItemType).ToArray();

            _currentPage = 1;
            _refreshItemButtons(false);
        }

        private void _refreshItemButtons(bool reverseColumnIndex)
        {
            _itemsPool.DisableAll();
            int startIndex = _currentPage > 1 ? _maxItemsCount * (_currentPage - 1) : 0;
            int limit = Math.Min(_maxItemsCount + startIndex, _currentItemsData.Length);

            for (int index = startIndex; index < limit; index++)
            {
                ItemData itemData = _currentItemsData[index];
                DebugItemButton itemButton = _itemsPool.GetNext();
                itemButton.SetItemData(itemData);
            }

            UISelectable[] dpadSelectables = _itemsPool.GetAllEnabled().Select(b => b.Selectable).ToArray();
            Tuple<ESelectionZone, SelectableGridData> dpadGrid = _zoneBuilder.Build(
                zone: ESelectionZone.Navigate,
                selectables: dpadSelectables,
                cols: ItemGridSize.x, rows: ItemGridSize.y,
                isFillHorizontally: false,
                isLooping: false);
          
            _itemGroup?.SetOtherGroup(_itemsPool.GetAllEnabled().ToArray());
            _selectablesController?.RefreshGrid(dpadGrid.Item1, dpadGrid.Item2, reverseColumnIndex);
            
            _view.nextItems.SetInteractable(limit < _currentItemsData.Length);
            _view.previousItems.SetInteractable(_currentPage > 1);
        }

        private void _onItemSelected(DebugItemButton button)
        {
            _refreshItemInformation(button);
        }

        private void _refreshItemInformation(DebugItemButton itemButton)
        {
            if (itemButton != null)
            {
                _view.itemName.SetText(itemButton.Item.name);
                _view.itemWeight.SetText($"{itemButton.Item.weight} kg");
            }
            else
            {
                _view.itemName.SetText("name");
                _view.itemWeight.SetText("weight");
            }
        }
    }
}