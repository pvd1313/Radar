using System.Collections.Generic;
using Beast.Radar.InputControl;

namespace Beast.Radar.UI
{
    public class InputIconsController
    {
        private readonly IInputSystem _inputActions;
        private readonly List<InputIconDisplay> _icons;

        public InputIconsController(IInputSystem inputActions, List<InputIconDisplay> icons)
        {
            _inputActions = inputActions;
            _icons = icons;
        }

        public void Warmup()
        {
            _inputActions.ChangedScheme += _onInputActionsChangedScheme;    
            
            foreach (InputIconDisplay icon in _icons)
            {
                icon.Refresh(_inputActions.Scheme);
            }
        }

        public void Dispose()
        {
            _inputActions.ChangedScheme -= _onInputActionsChangedScheme;
        }

        private void _onInputActionsChangedScheme(EInputScheme scheme)
        {
            foreach (InputIconDisplay icon in _icons)
            {
                icon.Refresh(scheme);
            }
        }
    }
}