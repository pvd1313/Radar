using System.Collections.Generic;
using Beast.Radar.InputHandling;

namespace Beast.Radar.UI
{
    public class InputIconsController
    {
        private readonly IInputSchemeObserver _inputScheme;
        private readonly List<InputIconDisplay> _icons;

        public InputIconsController(IInputSchemeObserver inputScheme, List<InputIconDisplay> icons)
        {
            _inputScheme = inputScheme;
            _icons = icons;
        }

        public void Warmup()
        {
            _inputScheme.Changed += _onInputActionsChangedScheme;    
            
            foreach (InputIconDisplay icon in _icons)
            {
                icon.Refresh(_inputScheme.Current);
            }
        }

        public void Dispose()
        {
            _inputScheme.Changed -= _onInputActionsChangedScheme;
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