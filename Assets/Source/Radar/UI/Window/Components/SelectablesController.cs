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
        private readonly IUIInputController _input;
        private readonly Dictionary<ESelectionZone, SelectableGridData> _selectableGrids;

        private UISelectable _currentSelectable;
        private bool _isSubscribedToInput;
        private bool _isUsed;

        public SelectablesController(
            IUIInputController input,
            Dictionary<ESelectionZone, SelectableGridData> selectables)
        {
            _input = input;
            _selectableGrids = selectables;
        }

        public void Warmup()
        {
            SubscribeInputState();
            SubscribeSelectables();
        }

        public void Dispose()
        {
            UnsubscribeInputState();
            UnsubscribeSelectables();
        }

        public void Enable()
        {
            _isUsed = true;
            SetupCurrentSelectable();
        }

        public void Disable()
        {
            _isUsed = false;
            CleanupInputSubscription();
        }


        #region Input state event

        private void SubscribeInputState()
        {
            _input.ChangedEnable += OnInputChangedEnable;
        }

        private void UnsubscribeInputState()
        {
            _input.ChangedEnable -= OnInputChangedEnable;
        }

        private void OnInputChangedEnable()
        {
            if (_input.IsHardwareInput)
            {
                SetupCurrentSelectable();
            }
            else
            {
                CleanupInputSubscription();
            }
        }

        #endregion

        #region Selectables events

        private void SubscribeSelectables()
        {
            foreach (KeyValuePair<ESelectionZone, SelectableGridData> grids in _selectableGrids)
            {
                foreach (UISelectable selectable in grids.Value.selectables)
                {
                    if (selectable == null)
                    {
                        continue;
                    }

                    selectable.Selected += OnSelected;
                }
            }
        }

        private void UnsubscribeSelectables()
        {
            foreach (KeyValuePair<ESelectionZone, SelectableGridData> grids in _selectableGrids)
            {
                foreach (UISelectable selectable in grids.Value.selectables)
                {
                    if (selectable == null)
                    {
                        continue;
                    }

                    selectable.Selected -= OnSelected;
                }
            }
        }

        private void OnSelected(ESelectionZone zone, int2 index)
        {
            SelectableGridData grid = _selectableGrids[zone];
            grid.currentIndex = index;
            _selectableGrids[zone] = grid;
            _currentSelectable = grid.selectables[grid.currentIndex.x,grid.currentIndex.y];
            //_currentSelectable.Select();
        }

        #endregion

        #region Input action events

        private void SubscribeInputActions()
        {
            _input.DpadPressed += OnDpadPressed;
            _input.ShoulderPressed += OnShoulderPressed;
            _input.TriggerPressed += OnTriggerPressed;
            _input.RightStickMoved += OnRightStickMoved;
            _input.ActionButtonPressed += OnActionButtonPressed;
            _isSubscribedToInput = true;
        }

        private void UnsubscribeInputActions()
        {
            _input.DpadPressed -= OnDpadPressed;
            _input.ShoulderPressed -= OnShoulderPressed;
            _input.TriggerPressed -= OnTriggerPressed;
            _input.RightStickMoved -= OnRightStickMoved;
            _input.ActionButtonPressed -= OnActionButtonPressed;
            _isSubscribedToInput = false;
        }

        private void OnActionButtonPressed(ESelectionZone zone)
        {
            if (!_selectableGrids.ContainsKey(zone))
            {
                return;
            }

            if (_selectableGrids[zone].selectables[0, 0].TryGetComponent(out Button button))
            {
                button.onClick.Invoke();
            }
        }

        private void OnDpadPressed(int2 direction)
        {
            PerformVectorAction(ESelectionZone.DPad, direction);
        }

        private void OnRightStickMoved(float2 direction)
        {
            if (_currentSelectable != null && _currentSelectable.TryGetComponent(out UISlider slider))
            {
                slider.AddValue(direction.x);
            }
        }

        private void OnShoulderPressed(int value)
        {
            PerformHorizontalAction(ESelectionZone.Shoulder, value);
        }

        private void OnTriggerPressed(int value)
        {
            PerformHorizontalAction(ESelectionZone.Trigger, value);
        }

        private void PerformHorizontalAction(ESelectionZone zone, int value)
        {
            if (!_selectableGrids.ContainsKey(zone))
            {
                return;
            }

            SelectableGridData grid = _selectableGrids[zone];
            int newCol = grid.currentIndex.y;

            UpdatePosition(ref newCol, value, grid.selectables.GetLength(1), grid.isLooping);


            grid.currentIndex.y = newCol;
            _selectableGrids[zone] = grid;

            UISelectable triggerSelectable = grid.selectables[grid.currentIndex.x, grid.currentIndex.y];
            if (triggerSelectable.TryGetComponent(out Button button))
            {
                button.onClick.Invoke();
            }
        }

        private void PerformVectorAction(ESelectionZone zone, int2 direction)
        {
            if (!_selectableGrids.ContainsKey(zone))
            {
                return;
            }

            SelectableGridData grid = _selectableGrids[zone];

            int newCol = grid.currentIndex.y;
            int newRow = grid.currentIndex.x;

            int2 bounds = new(grid.selectables.GetLength(0), grid.selectables.GetLength(1));

            UpdatePosition(ref newRow, direction.y, bounds.x, grid.isLooping);
            UpdatePosition(ref newCol, direction.x, bounds.y, grid.isLooping);

            int2 index = new(newRow, newCol);
            UISelectable selectable = grid.selectables[index.x, index.y];

            if (selectable != null)
            {
                grid.currentIndex = index;
                _selectableGrids[zone] = grid;
                SwitchSelectable(selectable);
            }

            CheckGridLimits(grid.currentIndex, direction, bounds, zone);
        }

        private void CheckGridLimits(int2 currentPosition, int2 direction, int2 bounds, ESelectionZone zone)
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

        private void UpdatePosition(ref int coordinate, int delta, int limit, bool isLooping)
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

        private void SwitchSelectable(UISelectable selectable)
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

        private void SetupCurrentSelectable()
        {
            if (_isUsed && _input.IsHardwareInput && !_isSubscribedToInput)
            {
                if (_currentSelectable == null)
                {
                    SelectableGridData grid = _selectableGrids.FirstOrDefault().Value;
                    _currentSelectable = grid.selectables[grid.currentIndex.x, grid.currentIndex.y];
                }

                if (!(_selectableGrids.Count == 1 && _selectableGrids.ContainsKey(ESelectionZone.Trigger)))
                {
                    _currentSelectable.Select();
                }

                SubscribeInputActions();
            }
        }

        private void CleanupInputSubscription()
        {
            if (_isSubscribedToInput)
            {
                if (_currentSelectable != null)
                {
                    _currentSelectable.Deselect();
                }

                UnsubscribeInputActions();
            }
        }

        public void RefreshGrid(ESelectionZone zone, SelectableGridData grid, bool reverseColumnIndex)
        {
            if (!_input.IsHardwareInput)
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

            foreach (UISelectable selectable in _selectableGrids[zone].selectables)
            {
                if (selectable == null)
                {
                    continue;
                }

                selectable.Selected -= OnSelected;
            }

            foreach (UISelectable selectable in grid.selectables)
            {
                if (selectable == null)
                {
                    continue;
                }

                selectable.Selected += OnSelected;
            }

            _selectableGrids[zone] = grid;
            SwitchSelectable(_selectableGrids[zone].selectables[grid.currentIndex.x, grid.currentIndex.y]);
        }
    }
}