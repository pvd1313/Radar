using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Radar.InputControl;
using Unity.Mathematics;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class SelectablesController
    {
        public event Action<ESelectionZone, int2> TryWentBeyond;

        private readonly IUIInputController _inputController;
        private readonly Dictionary<ESelectionZone, SelectableGridData> _selectableGrids;

        private UISelectable _currentSelectable;
        private bool _isSubscribedToInput;
        private bool _isUsed;

        public SelectablesController(
            IUIInputController inputController,
            Dictionary<ESelectionZone, SelectableGridData> selectables)
        {
            _inputController = inputController;
            _selectableGrids = selectables;
        }

        public void Warmup()
        {
            _subscribeInputState();
            foreach (KeyValuePair<ESelectionZone,SelectableGridData> grid in _selectableGrids)
            {
                _subscribeSelectables(grid.Key);
            }
        }

        public void Dispose()
        {
            _unsubscribeInputState();
            foreach (KeyValuePair<ESelectionZone, SelectableGridData> grid in _selectableGrids)
            {
                _unsubscribeSelectables(grid.Key);
            }
        }

        public void Enable()
        {
            _isUsed = true;
            _setupCurrentSelectable();
        }

        public void Disable()
        {
            _isUsed = false;
            _cleanupInputSubscription();
        }

        public void RefreshGrid(ESelectionZone zone, SelectableGridData grid, bool reverseColumnIndex)
        {
            if (!_inputController.IsHardwareInput)
            {
                return;
            }
            
            if (reverseColumnIndex)
            {
                SelectableGridData currentGrid = _selectableGrids[zone];
                int lastIndexY = currentGrid.selectables.GetLength(1) - 1;

                if (grid.currentIndex.y == lastIndexY || grid.currentIndex.y == 0)
                {
                    grid.currentIndex.x = currentGrid.currentIndex.x;
                }

                if (currentGrid.currentIndex.y == 0 && grid.selectables[grid.currentIndex.x, lastIndexY] != null)
                {
                    grid.currentIndex.y = currentGrid.selectables.GetLength(1) - 1;
                }
            }

            _unsubscribeSelectables(zone);
            _selectableGrids[zone] = grid;
            _subscribeSelectables(zone);
            _switchSelectable(_selectableGrids[zone].selectables[grid.currentIndex.x, grid.currentIndex.y]);
        }
        
        private void _setupCurrentSelectable()
        {
            if (_isUsed && _inputController.IsHardwareInput && !_isSubscribedToInput)
            {
                if (_currentSelectable == null)
                {
                    SelectableGridData grid = _selectableGrids.FirstOrDefault().Value;
                    _currentSelectable = grid.selectables[grid.currentIndex.x, grid.currentIndex.y];
                   
                    if (!(_selectableGrids.Count == 1 && _selectableGrids.ContainsKey(ESelectionZone.Trigger)))
                    {
                        _currentSelectable.Select();
                    }
                }
                else
                {
                    _currentSelectable.Select();   
                }
                
                _subscribeInputActions();
            }
        }

        private void _cleanupInputSubscription()
        {
            if (_isSubscribedToInput)
            {
                if (_currentSelectable != null)
                {
                    _currentSelectable.Deselect();
                }

                _unsubscribeInputActions();
            }
        }

        #region Input state event

        private void _subscribeInputState()
        {
            _inputController.ChangedEnable += _onInputControllerChangedEnable;
        }

        private void _unsubscribeInputState()
        {
            _inputController.ChangedEnable -= _onInputControllerChangedEnable;
        }

        private void _onInputControllerChangedEnable()
        {
            if (_inputController.IsHardwareInput)
            {
                _setupCurrentSelectable();
            }
            else
            {
                _cleanupInputSubscription();
            }
        }

        #endregion

        #region Selectables events

        private void _subscribeSelectables(ESelectionZone selectionZone)
        {
            foreach (UISelectable selectable in _selectableGrids[selectionZone].selectables)
            {
                if (selectable == null)
                {
                    continue;
                }

                selectable.Selected += _onSelected;
            }
        }

        private void _unsubscribeSelectables(ESelectionZone selectionZone)
        {
            foreach (UISelectable selectable in _selectableGrids[selectionZone].selectables)
            {
                if (selectable == null)
                {
                    continue;
                }

                selectable.Selected -= _onSelected;
            }
        }

        private void _onSelected(ESelectionZone zone, int2 index)
        {
            if (zone is ESelectionZone.Shoulder or ESelectionZone.Trigger)
            {
                return;
            }
            SelectableGridData grid = _selectableGrids[zone];
            grid.currentIndex = index;
            _selectableGrids[zone] = grid;
            _currentSelectable = grid.selectables[grid.currentIndex.x,grid.currentIndex.y];
        }

        #endregion

        #region Input action events

        private void _subscribeInputActions()
        {
            _inputController.NavigatePressed += _onDpadPressed;
            _inputController.ShoulderPressed += _onShoulderPressed;
            _inputController.TriggerPressed += _onTriggerPressed;
            _inputController.RightStickMoved += _onRightStickMoved;
            _inputController.ActionButtonPressed += _onActionButtonPressed;
            _isSubscribedToInput = true;
        }

        private void _unsubscribeInputActions()
        {
            _inputController.NavigatePressed -= _onDpadPressed;
            _inputController.ShoulderPressed -= _onShoulderPressed;
            _inputController.TriggerPressed -= _onTriggerPressed;
            _inputController.RightStickMoved -= _onRightStickMoved;
            _inputController.ActionButtonPressed -= _onActionButtonPressed;
            _isSubscribedToInput = false;
        }

        private void _onActionButtonPressed(ESelectionZone zone)
        {
            if (!_selectableGrids.ContainsKey(zone))
            {
                return;
            }

            bool isTrySelect = _inputController.GetScheme() == EInputScheme.Gamepad
                               && zone == ESelectionZone.SouthButton
                               && _selectableGrids.ContainsKey(ESelectionZone.Navigate); 
            
            if (isTrySelect)
            {
                SelectableGridData dpadGrid = _selectableGrids[ESelectionZone.Navigate];
                UISelectable dpadSelectable = dpadGrid.selectables[dpadGrid.currentIndex.x, dpadGrid.currentIndex.y];
                if (dpadSelectable.TryGetComponent(out IRadioSelected selected) && !selected.IsSelected)
                {
                    selected.Select();
                    return;
                }
            }

            if (_selectableGrids[zone].selectables[0, 0].TryGetComponent(out Button button))
            {
                button.onClick.Invoke();
            }
        }

        private void _onDpadPressed(int2 direction)
        {
            _performVectorAction(ESelectionZone.Navigate, direction);
        }

        private void _onRightStickMoved(float2 direction)
        {
            if (_currentSelectable != null && _currentSelectable.TryGetComponent(out UISlider slider))
            {
                slider.AddValue(direction.x);
            }
        }

        private void _onShoulderPressed(int value)
        {
            _performHorizontalAction(ESelectionZone.Shoulder, value);
        }

        private void _onTriggerPressed(int value)
        {
            _performHorizontalAction(ESelectionZone.Trigger, value);
        }

        private void _performHorizontalAction(ESelectionZone zone, int value)
        {
            if (!_selectableGrids.ContainsKey(zone))
            {
                return;
            }

            SelectableGridData grid = _selectableGrids[zone];
            _updatePosition(ref grid.currentIndex.y, value, grid.selectables.GetLength(1), grid.isLooping);
            _selectableGrids[zone] = grid;
            
            if (zone is ESelectionZone.Shoulder or ESelectionZone.Trigger)
            {
                UISelectable triggerSelectable = grid.selectables[grid.currentIndex.x, grid.currentIndex.y];
                if (triggerSelectable.TryGetComponent(out Button button))
                {
                    button.onClick.Invoke();
                }
            }
        }

        private void _performVectorAction(ESelectionZone zone, int2 direction)
        {
            if (!_selectableGrids.ContainsKey(zone))
            {
                return;
            }

            SelectableGridData grid = _selectableGrids[zone];

            int newCol = grid.currentIndex.y;
            int newRow = grid.currentIndex.x;

            int2 bounds = new(grid.selectables.GetLength(0), grid.selectables.GetLength(1));

            _updatePosition(ref newRow, direction.y, bounds.x, grid.isLooping);
            _updatePosition(ref newCol, direction.x, bounds.y, grid.isLooping);

            int2 index = new(newRow, newCol);
            UISelectable selectable = grid.selectables[index.x, index.y];

            if (selectable != null)
            {
                grid.currentIndex = index;
                _selectableGrids[zone] = grid;
                _switchSelectable(selectable);
            }

            _checkGridLimits(grid.currentIndex, direction, bounds, zone);
        }

        private void _checkGridLimits(int2 currentPosition, int2 direction, int2 bounds, ESelectionZone zone)
        {
            int2 newPosition = new(currentPosition.x + direction.y, currentPosition.y + direction.x);
            bool isOutOfBounds =
                newPosition.x < 0 || newPosition.x >= bounds.x ||
                newPosition.y < 0 || newPosition.y >= bounds.y;

            if (isOutOfBounds)
            {
                TryWentBeyond?.Invoke(zone, direction);
            }
        }

        private void _updatePosition(ref int coordinate, int delta, int limit, bool isLooping)
        {
            if (delta != 0)
            {
                coordinate += delta;

                if (coordinate < 0)
                {
                    coordinate = isLooping ? limit - 1 : 0;
                }
                else if (coordinate >= limit)
                {
                    coordinate = isLooping ? 0 : limit - 1;
                }
            }
        }
        
        private void _switchSelectable(UISelectable selectable)
        {
            if (selectable == _currentSelectable)
            {
                return;
            }
            
            if (_currentSelectable != null)
            {
                _currentSelectable.Deselect();
            }

            _currentSelectable = selectable;
            _currentSelectable.Select();
        }

        #endregion
    }
}