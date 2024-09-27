using System;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.Cinematic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class PlayerInputProvider: IPlayerInputProvider 
    {
        public EInputScheme Scheme => _inputScheme;
        
        private EInputScheme _inputScheme;
        private PlayerInputView _view;
        
        public PlayerInputProvider(CameraConfig config)
        {
            _view = config.PlayerInputView;
        }
        
        public UniTask<InitializationResult> Initialize()
        {
            PlayerInputView newInstance = BeastScene.Instantiate(_view, float3.zero, quaternion.identity);
            _view = newInstance;
            
            _view.PlayerInput.onControlsChanged += RefreshCurrentScheme;
            RefreshCurrentScheme(_view.PlayerInput);
            
            return InitializationResult.SuccessNotAsync();
        }

        public UniTask<DispositionResult> Dispose()
        {
            _view.PlayerInput.onControlsChanged -= RefreshCurrentScheme;
            return DispositionResult.SuccessNotAsync();
        }
        
        private void RefreshCurrentScheme(PlayerInput input)
        {
            Enum.TryParse(_view.PlayerInput.currentControlScheme, out  _inputScheme);
            
            if (_inputScheme == EInputScheme.Mouse && Mouse.current == null)
            {
                BeastLog.Error(this,"Try setting the mouse scheme with an empty mouse device.");
                _inputScheme = EInputScheme.Touch;
            }
        }
    }
}