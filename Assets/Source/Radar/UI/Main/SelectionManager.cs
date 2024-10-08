using System.Collections.Generic;
using Beast.Radar.InputControl;
using UnityEngine.UI;

namespace Beast.Radar.UI
    {
        public class SelectionManager : ISelectionManager
        {
            private readonly IUIInputController _input;

            private Dictionary<ESelectionZone, SelectableGridData> _selectableGrids;
            private Selectable _currentSelectable;
            
            public SelectionManager(IUIInputController input)
            {
                _input = input;
            }
            
            public void Set(Dictionary<ESelectionZone, SelectableGridData> selectables)
            {
                _selectableGrids = selectables;
                SelectableGridData grid = selectables[ESelectionZone.DPad];
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
                
            }

            private void Unsubscribe()
            {
                
            }
        }
    }