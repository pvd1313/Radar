using System.Collections.Generic;
using System.Linq;
using Beast.Radar.InputControl;
using Unity.Mathematics;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class SelectableController
    {
        private readonly IUIInputController _input;
        private readonly Dictionary<ESelectionZone, SelectableGridData> _selectableGrids;

        private UISelectable _currentSelectable;

        public SelectableController(
            IUIInputController input,
            Dictionary<ESelectionZone, SelectableGridData> selectables)
        {
            _input = input;
            _selectableGrids = selectables;
        }
        
        public void Warmup()
        {
            SubscribeSelectables();  
        }

        public void Dispose()
        {
            UnsubscribeSelectables();
        }

        public void Enable()
        {
            if (_currentSelectable == null)
            {
                SelectableGridData grid = _selectableGrids.FirstOrDefault().Value;
                _currentSelectable = grid.selectables[grid.currentIndex.x,grid.currentIndex.y];
            }

            if (!(_selectableGrids.Count == 1 && _selectableGrids.ContainsKey(ESelectionZone.Trigger)))
            {
                _currentSelectable.Select();
            }
            
            SubscribeInput();
        }

        public void Disable()
        {
            if (_currentSelectable != null)
            {
                _currentSelectable.OnDeselect(null);
            }

            UnsubscribeInput();
        }

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
        
        private void SubscribeInput()
        {
            _input.DpadPressed += OnDpadPressed;
            _input.TriggerPressed += OnTriggerPressed;
            _input.RightStickMoved += OnRightStickMoved;
            _input.ActionButtonPressed += OnActionButtonPressed;

        }

        private void UnsubscribeInput()
        {
            _input.DpadPressed -= OnDpadPressed;
            _input.TriggerPressed -= OnTriggerPressed;
            _input.RightStickMoved -= OnRightStickMoved;
            _input.ActionButtonPressed -= OnActionButtonPressed;
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
            UnityEngine.Debug.Log(direction);
            PerformVectorAction(ESelectionZone.DPad, direction);
        }

        private void OnRightStickMoved(int2 direction)
        {
            UnityEngine.Debug.Log(direction);
            PerformVectorAction(ESelectionZone.RightStick, direction);
        }

        private void OnTriggerPressed(int value)
        {
            if (!_selectableGrids.ContainsKey(ESelectionZone.Trigger))
            {
                return;
            }

            SelectableGridData grid = _selectableGrids[ESelectionZone.Trigger];

            int current = grid.currentIndex.x;
            int newCol = current + value;
            int cols = grid.selectables.GetLength(1);
            
            if (newCol >= cols)
            {
                newCol = grid.isLooping ? 0 : current;
            }
            else if (newCol < 0)
            {
                newCol = grid.isLooping ? cols - 1 : current;
            }

            grid.currentIndex.y = newCol;

            _selectableGrids[ESelectionZone.Trigger] = grid;

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

            int rows = grid.selectables.GetLength(1);
            int cols = grid.selectables.GetLength(0);
            int currentRow = grid.currentIndex.y;
            int currentCol = grid.currentIndex.x;

            int newRow = currentRow;
            int newCol = currentCol;

            if (direction.y != 0)
            {
                newCol += direction.y;

                if (newCol < 0)
                {
                    newCol = grid.isLooping ? cols - 1 : 0;
                }
                else if (newCol >= cols)
                {
                    newCol = grid.isLooping ? 0 : cols - 1;
                }
            }

            if (direction.x != 0)
            {
                newRow += direction.x;

                if (newRow < 0)
                {
                    newRow = grid.isLooping ? rows - 1 : 0;
                }
                else if (newRow >= rows)
                {
                    newRow = grid.isLooping ? 0 : rows - 1;
                }
            }

            int2 index = new int2(newCol, newRow);
            UnityEngine.Debug.Log($"index = {index} / size {new int2(cols, rows)}");
            UISelectable selectable = grid.selectables[index.x, index.y];

            if (selectable != null)
            {
                grid.currentIndex = index;
                _selectableGrids[zone] = grid;
                SwitchSelectable(selectable);
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
    }
}
