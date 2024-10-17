using System.Collections.Generic;
using Beast.Radar.InputControl;

namespace Beast.Radar.UI
{
    public class InputIconsController
    {
        private readonly IPlayerInputProvider _playerInput;
        private readonly List<InputIconDisplay> _icons;

        public InputIconsController(IPlayerInputProvider playerInput, List<InputIconDisplay> icons)
        {
            _playerInput = playerInput;
            _icons = icons;
        }

        public void Warmup()
        {
            _playerInput.ChangedScheme += _onPlayerInputChangedScheme;    
            
            foreach (InputIconDisplay icon in _icons)
            {
                icon.Refresh(_playerInput.Scheme);
            }
        }

        public void Dispose()
        {
            _playerInput.ChangedScheme -= _onPlayerInputChangedScheme;
        }

        private void _onPlayerInputChangedScheme(EInputScheme scheme)
        {
            foreach (InputIconDisplay icon in _icons)
            {
                icon.Refresh(scheme);
            }
        }
    }
}