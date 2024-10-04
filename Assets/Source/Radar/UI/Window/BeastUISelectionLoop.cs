using Beast.Radar.InputControl;
using Unity.Mathematics;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class BeastUISelectionLoop 
    {
        private readonly Selectable[] _selectables; 
        private readonly IBeastUIInputController _input;
        private int _currentIndex; 

        
        public BeastUISelectionLoop(IBeastUIInputController inputController, Selectable[] selectables)
        {
            _selectables = selectables;
            _input = inputController;
        }
        
        public void Enable()
        {
            _input.NavigationPressed += OnMove;
            _currentIndex = 0; 
            _selectables[_currentIndex].Select();                 
        }

        public void Disable()
        {
            _input.NavigationPressed -= OnMove;
            _selectables[_currentIndex].OnDeselect(null);
        }

        private void OnMove(float2 moveInput)
        {
            switch (moveInput.y)
            {
                case -1:
                    MoveSelection(-1);
                    break;
                case 1:
                    MoveSelection(1);
                    break;
            }
        }

        private void MoveSelection(int direction)
        {
            _selectables[_currentIndex]?.OnDeselect(null);
            _currentIndex = (_currentIndex + direction + _selectables.Length) % _selectables.Length;
            _selectables[_currentIndex].Select();
        }
    }
}