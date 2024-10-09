using System.Collections.Generic;
using System.Linq;
using Beast.Radar.InputControl;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class SelectableManager : ISelectableManager
    {
        private readonly IUIInputController _input;

        private Dictionary<ESelectionZone, SelectableGridData> _selectableGrids;
        private Selectable _currentSelectable;

        public SelectableManager(IUIInputController input)
        {
            _input = input;
        }

        public void Set(Dictionary<ESelectionZone, SelectableGridData> selectables)
        {
            _selectableGrids = selectables;

            if (selectables == null)
            {
                return;
            }

            SelectableGridData grid = selectables.FirstOrDefault().Value;

            if (grid.selectables == null || grid.selectables.GetLength(0) == 0 || grid.selectables.GetLength(1) == 0)
            {
                return;
            }

            _currentSelectable = grid.selectables[grid.currentIndex.x, grid.currentIndex.y];
            _currentSelectable.Select();

            Subscribe();
        }

        public void Reset()
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
            
            int rows = grid.selectables.GetLength(1);
            int cols = grid.selectables.GetLength(0);

            int currentRow = grid.currentIndex.y;
            int currentCol = grid.currentIndex.x;

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
                newCol = grid.isLooping ? cols - 1 : 0;
            }
            else if (newCol >= cols)
            {
                newCol = grid.isLooping ? 0 : currentCol;
            }

            grid.currentIndex = new int2(newCol, newRow);

            _selectableGrids[ESelectionZone.DPad] = grid;
            //todo bad logic
            if (_currentSelectable != null)
            {
                _currentSelectable.OnDeselect(null);
            }

            _currentSelectable = grid.selectables[newCol, newRow];
            _currentSelectable.Select();
            UnityEngine.Debug.Log($"select = {_currentSelectable.name} navigation size = {cols} {rows} index = {grid.currentIndex} dir = {direction}");
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

            int cols = grid.selectables.GetLength(0);
            
            if (newCol >= cols)
            {
                newCol = grid.isLooping ? 0 : current;
            }
            else if(newCol < 0)
            {
                newCol = grid.isLooping ? cols - 1 : current;
            }

            grid.currentIndex.x = newCol;

            _selectableGrids[ESelectionZone.Trigger] = grid;
            _currentSelectable = grid.selectables[grid.currentIndex.x, grid.currentIndex.y];

            //todo bad logic
            if (_currentSelectable != null)
            {
                _currentSelectable.OnDeselect(null);
            }

            _currentSelectable.Select();

            if (_currentSelectable is Button button)
            {
                button.onClick.Invoke();
            }
        }
    }
}