using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Radar.InputControl;
using Unity.Mathematics;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class SelectablesController
    {
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

        private void OnRightStickMoved(int2 direction)
        {
            PerformVectorAction(ESelectionZone.RightStick, direction);
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
            BDebug.LogInfo(this, $"{zone} {value} {_selectableGrids[zone].selectables.GetLength(1)}  {newCol}");

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

            int newRow = grid.currentIndex.y;
            int newCol = grid.currentIndex.x;

            UpdatePosition(ref newCol, direction.y, grid.selectables.GetLength(0), grid.isLooping);
            UpdatePosition(ref newRow, direction.x, grid.selectables.GetLength(1), grid.isLooping);

            int2 index = new(newCol, newRow);
            UISelectable selectable = grid.selectables[index.x, index.y];

            if (selectable != null)
            {
                grid.currentIndex = index;
                _selectableGrids[zone] = grid;
                SwitchSelectable(selectable);
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
                _currentSelectable.OnDeselect(null);
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
                    _currentSelectable.OnDeselect(null);
                }

                UnsubscribeInputActions();
            }
        }

        public void Refresh(ESelectionZone zone, SelectableGridData grid)
        {
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
        }
    }
}