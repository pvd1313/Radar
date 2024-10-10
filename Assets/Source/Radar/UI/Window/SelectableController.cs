using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Common;
using Beast.Radar.InputControl;
using Unity.Mathematics;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class SelectableController
    {
        private readonly IUIInputController _input;

        private readonly Dictionary<ESelectionZone, SelectableGridData> _selectableGrids;
        private Selectable _currentSelectable;

        public SelectableController(IUIInputController input, Dictionary<ESelectionZone, SelectableGridData> selectables)
        {
            _input = input;
            _selectableGrids = selectables;
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
            
            Subscribe();
        }

        public void Disable()
        {
            if (_currentSelectable != null)
            {
                _currentSelectable.OnDeselect(null);
            }

            Unsubscribe();
        }

        private void Subscribe()
        {
            _input.NavigationPressed += OnDpadPressed;
            _input.TriggerPressed += OnTriggerPressed;
        }

        private void Unsubscribe()
        {
            _input.NavigationPressed -= OnDpadPressed;
            _input.TriggerPressed -= OnTriggerPressed;
        }
        
        private void OnDpadPressed(float2 direction)
        {
            if (!_selectableGrids.ContainsKey(ESelectionZone.DPad))
            {
                return;
            }

            SelectableGridData grid = _selectableGrids[ESelectionZone.DPad];

            int rows = grid.selectables.GetLength(0); //y
            int cols = grid.selectables.GetLength(1); //x
            int currentRow = grid.currentIndex.y;
            int currentCol = grid.currentIndex.x;

            int newRow = currentRow;
            int newCol = currentCol;
            
            if (direction.x != 0)
            {
                newCol += (int)direction.x;

                if (newCol < 0)
                {
                    newCol = grid.isLooping ? cols - 1 : 0;
                }
                else if (newCol >= cols)
                {
                    newCol = grid.isLooping ? 0 : cols - 1;
                }
            }

            if (direction.y != 0)
            {
                newRow += (int)direction.y;

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
            Selectable selectable = grid.selectables[index.y, index.x];
            
            if (selectable != null)
            {
                grid.currentIndex = index;
                _selectableGrids[ESelectionZone.DPad] = grid;
                SwitchSelectable(selectable);
            }
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

            UnityEngine.Debug.Log($"{newCol}/{cols}");
            grid.currentIndex.y = newCol;

            _selectableGrids[ESelectionZone.Trigger] = grid;

            Selectable triggerSelectable = grid.selectables[grid.currentIndex.x, grid.currentIndex.y];
            if (triggerSelectable is Button button)
            {
                button.onClick.Invoke();
            }
        }

        private void SwitchSelectable(Selectable selectable)
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
