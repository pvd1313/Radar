using Beast.Radar.InputControl;
using Unity.Mathematics;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class BeastUISelectionLoop 
    {
        private readonly Selectable[] _selectables; 
        private readonly IBeastUIInputController _input;
        private int currentIndex; 
        
        public BeastUISelectionLoop(IBeastUIInputController inputController, Selectable[] selectables)
        {
            _selectables = selectables;
            _input = inputController;
        }
        
        public void Enable()
        {
            _input.NavigationPressed += OnMove;
            currentIndex = 0; 
            _selectables[currentIndex].Select();                 
        }

        public void Disable()
        {
            _input.NavigationPressed -= OnMove;
            _selectables[currentIndex].OnDeselect(null);
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
            _selectables[currentIndex]?.OnDeselect(null);
            currentIndex = (currentIndex + direction + _selectables.Length) % _selectables.Length;
            _selectables[currentIndex].Select();
        }
    }
}