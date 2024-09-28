using System;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.Prefabs;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class PlayerInputProvider: IPlayerInputProvider 
    {
        public event Action<EInputScheme> ChangedScheme;
        public EInputScheme Scheme => _inputScheme;

        private EInputScheme _inputScheme;
        private PlayerInputView _view;
        
        public PlayerInputProvider(PrefabLibrary config)
        {
            _view = config.playerInputPrefab;
        }
        
        public UniTask<InitializationResult> Initialize()
        {
            PlayerInputView newInstance = BeastScene.Instantiate(_view, float3.zero, quaternion.identity);
            _view = newInstance;
            
            RefreshCurrentScheme(_view.PlayerInput);
            
            _view.PlayerInput.onControlsChanged += RefreshCurrentScheme;
            
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
            
            if (_inputScheme == EInputScheme.MouseAndKeyboard && Mouse.current == null)
            {
                BeastLog.Error(this,"Try setting the mouse scheme with an empty mouse device.");
                _inputScheme = EInputScheme.Touch;
            }
            
            ChangedScheme?.Invoke(_inputScheme);
        }
    }
}