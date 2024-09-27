using Beast.Claw.Common;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputControl
{
    public class PlayerInputView : BeastMono
    {
        public PlayerInput PlayerInput => _playerInput;
        
        [SerializeField] private PlayerInput _playerInput;
    }
}