using System.Collections.Generic;
using System.Linq;
using Beast.Radar.InputControl;
using Unity.Mathematics;
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

                if (selectables == null)
                {
                    return;
                }
                
                SelectableGridData grid = selectables.FirstOrDefault().Value; 
                
                if (grid.selectables.Length == 0 || grid.selectables.Rank == 0)
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
                _input.NavigationPressed += OnNavigationPressed;
            }

            private void OnNavigationPressed(float2 obj)
            {
                throw new System.NotImplementedException();
            }

            private void Unsubscribe()
            {
                
            }
        }
    }