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

        public void Refresh(ESelectionZone zone, SelectableGridData grid)
        {
            if (_selectableGrids.ContainsKey(zone))
            {
                int2 currentIndex = _selectableGrids[zone].currentIndex;
                
                if (currentIndex.x >= grid.selectables[currentIndex.y].Length)
                {
                    currentIndex.x = grid.selectables[currentIndex.y].Length - 1;
                }

                if (currentIndex.y >= grid.selectables.Length)
                {
                    currentIndex.y = grid.selectables.Length - 1;
                }

                grid.currentIndex = currentIndex;

                if (_currentSelectable != grid.selectables[currentIndex.y][currentIndex.x])
                {
                    SwitchSelectable(grid.selectables[currentIndex.y][currentIndex.x]);
                }

                _selectableGrids[zone] = grid;
            }
            else
            {
                _selectableGrids.Add(zone, grid);
            }
        }

        public void Enable()
        {
            if (_currentSelectable == null)
            {
                SelectableGridData grid = _selectableGrids.FirstOrDefault().Value;
                _currentSelectable = grid.selectables[grid.currentIndex.y][grid.currentIndex.x];
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

            int rows = grid.selectables.Length; 
            int currentRow = grid.currentIndex.y;

            if (currentRow >= rows)
            {
                BDebug.LogError(this, "Current row index is out of bounds");
                return;
            }

            int cols = grid.selectables[currentRow].Length; 
            int currentCol = grid.currentIndex.x; 

            if (currentCol >= cols)
            {
                BDebug.LogError(this, "Current column index is out of bounds");
                return;
            }

            int newRow = currentRow + (int)direction.y;
            int newCol = currentCol + (int)direction.x;

            if (newRow < 0)
            {
                newRow = grid.isLooping ? rows - 1 : currentRow;
            }
            else if (newRow >= rows)
            {
                newRow = grid.isLooping ? 0 : currentRow;
            }

            if (newCol < 0)
            {
                newCol = grid.isLooping ? cols - 1 : currentCol;
            }
            else if (newCol >= cols)
            {
                newCol = grid.isLooping ? 0 : currentCol;
            }

            grid.currentIndex = new int2(newCol, newRow);

            _selectableGrids[ESelectionZone.DPad] = grid;

            var selectable = grid.selectables[newRow][newCol];
            if (selectable == null)
            {
                BDebug.LogError(this, "Selectable is null");
            }
            
            SwitchSelectable(selectable);
        }

        private void OnTriggerPressed(float value)
        {
            if (!_selectableGrids.ContainsKey(ESelectionZone.Trigger))
            {
                return;
            }

            SelectableGridData grid = _selectableGrids[ESelectionZone.Trigger];

            int current = grid.currentIndex.x;
            int newCol = current + (int)value;

            int cols = grid.selectables[grid.currentIndex.y].Length;

            if (newCol >= cols)
            {
                newCol = grid.isLooping ? 0 : current;
            }
            else if (newCol < 0)
            {
                newCol = grid.isLooping ? cols - 1 : current;
            }

            grid.currentIndex.x = newCol;

            _selectableGrids[ESelectionZone.Trigger] = grid;

            Selectable triggerSelectable = grid.selectables[grid.currentIndex.y][grid.currentIndex.x];
            if (triggerSelectable is Button button)
            {
                button.onClick.Invoke();
            }

            UnityEngine.Debug.Log($"{value} {triggerSelectable?.name} {newCol}/{cols}");
        }

        private void SwitchSelectable(Selectable selectable)
        {
            //delete this
            if (selectable == null || selectable == _currentSelectable)
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
