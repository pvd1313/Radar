using System.Collections.Generic;
using Beast.Radar.InputHandling;

namespace Beast.Radar.UI
{
    public class InputIconsController
    {
        private readonly IInputSystem _inputSystem;
        private readonly List<InputIconDisplay> _icons;

        public InputIconsController(IInputSystem inputSystem, List<InputIconDisplay> icons)
        {
            _inputSystem = inputSystem;
            _icons = icons;
        }

        public void Warmup()
        {
            _inputSystem.Scheme.Changed += _onInputActionsChangedScheme;    
            
            foreach (InputIconDisplay icon in _icons)
            {
                icon.Refresh(_inputSystem.Scheme.Current);
            }
        }

        public void Dispose()
        {
            _inputSystem.Scheme.Changed -= _onInputActionsChangedScheme;
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