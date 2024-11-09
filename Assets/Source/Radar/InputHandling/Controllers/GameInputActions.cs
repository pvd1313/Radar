using System;
using Beast.Claw.Mathematics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beast.Radar.InputHandling
{
    public class GameInputActions : IGameInputActions
    {
        public event Action ChangedEarlyValues;

        public InputValueControl<float2> Click { get; } = new();
        public InputValueControl<float2> ScreenPoint { get; } = new();
        public InputValueControl<float2> LastScreenPoint { get; } = new();
        public InputValueControl<float2> MoveDelta { get; } = new();
        public InputValueControl<int> CurrentTouchCount { get; } = new();
        public InputValueControl<int> LastTouchCount { get; } = new();
        public InputValueControl<float> Scroll { get; } = new();

        public bool IsPressMove { get; private set; }

        private readonly IInputSchemeObserver _inputSchemeObserver;
        private readonly InputActions _inputActions;
        
        private float2 _startClickTime;
        private float _lastZoomDistance;
        private bool _isBlock;

        public GameInputActions(IInputSchemeObserver inputSchemeObserver, InputActions inputAction)
        {
            _inputSchemeObserver = inputSchemeObserver;
            _inputActions = inputAction;
        }

        public void Update()
        {
            if (_isBlock)
            {
                return;
            }
            
            _earlyInput();   
            _lateInput();
        }

        public void ChangeBlock(bool isBlock)
        {
            _isBlock = isBlock;
            if (_isBlock)
            {
                Click.UpdateValue(new float2(0,0));
                LastTouchCount.UpdateValue(0);
                IsPressMove = false;
            }
        }

        private void _earlyInput()
        {
            switch (_inputSchemeObserver.Current)
            {
                case EInputScheme.MouseAndKeyboard:
                    CurrentTouchCount.UpdateValue(_getTouchCount());
                    if (_isCursorInsideGameWindow())
                    {
                        Scroll.UpdateValue(_inputActions.Game.Scroll.ReadValue<float>());
                    }
                    float2 mousePosition = _inputActions.Game.FirstPosition.ReadValue<Vector2>();
                    ScreenPoint.UpdateValue(mousePosition);
                    Click.UpdateValue(CurrentTouchCount.Value > 0 ? mousePosition : float2.zero);
                    MoveDelta.UpdateValue((float2)_inputActions.Game.Move.ReadValue<Vector2>());
                    break;
                
                case EInputScheme.Gamepad:
                    CurrentTouchCount.UpdateValue(0);
                    Scroll.UpdateValue(_inputActions.Game.Scroll.ReadValue<float>());
                    ScreenPoint.UpdateValue(_getScreenCenter());
                    MoveDelta.UpdateValue((float2)_inputActions.Game.Move.ReadValue<Vector2>());
                    break;
                
                case EInputScheme.Touch:
                    float2 firstTouch = (float2)_inputActions.Game.FirstPosition.ReadValue<Vector2>();
                    float2 secondTouch = (float2)_inputActions.Game.SecondPosition.ReadValue<Vector2>();
                    
                    int touchCount = firstTouch.Equals(float2.zero) ? 0 : 1;
                    touchCount += secondTouch.Equals(float2.zero) ? 0 : 1;
                    
                    CurrentTouchCount.UpdateValue(touchCount);
                    switch (CurrentTouchCount.Value)
                    {
                        case 0:
                            Scroll.UpdateValue(0);
                            Click.UpdateValue(float2.zero);
                            break;
                    
                        case 1:
                            Scroll.UpdateValue(0);
                            ScreenPoint.UpdateValue((float2)_inputActions.Game.FirstPosition.ReadValue<Vector2>());
                            Click.UpdateValue(ScreenPoint.Value);
                            break;
                    
                        case 2:
                            Scroll.UpdateValue(_lastZoomDistance == 0f 
                                ? 0 
                                : BMath.Distance(firstTouch, secondTouch) - _lastZoomDistance);
                            ScreenPoint.UpdateValue((firstTouch + secondTouch) * 0.5f);
                            break;
                    }
                    break;
            }
            
            IsPressMove = MoveDelta.State == EButtonState.Pressed || CurrentTouchCount.Value == 1;
            
            ChangedEarlyValues?.Invoke();
        }

        private void _lateInput()
        {
            switch (_inputSchemeObserver.Current)
            {
                case EInputScheme.MouseAndKeyboard:
                    _lastZoomDistance = 0f;
                    LastTouchCount.UpdateValue(_getTouchCount());
                    float2 mousePosition = _inputActions.Game.FirstPosition.ReadValue<Vector2>();
                    LastScreenPoint.UpdateValue(mousePosition);
                    break;
                
                case EInputScheme.Gamepad:
                    _lastZoomDistance = 0f;
                    LastTouchCount.UpdateValue(0);
                    LastScreenPoint.UpdateValue(_getScreenCenter());
                    break;
                
                case EInputScheme.Touch:
                    float2 firstTouch = (float2)_inputActions.Game.FirstPosition.ReadValue<Vector2>();
                    float2 secondTouch = (float2)_inputActions.Game.SecondPosition.ReadValue<Vector2>();
                    
                    int touchCount = firstTouch.Equals(float2.zero) ? 0 : 1;
                    touchCount += secondTouch.Equals(float2.zero) ? 0 : 1;
                    
                    LastTouchCount.UpdateValue(touchCount);
                        //LastTouchCount.UpdateValue(_getTouchCount());
                    
                    switch (LastTouchCount.Value)
                    {
                        case 0:
                            _lastZoomDistance = 0f;
                            break;
                        
                        case 1:
                            _lastZoomDistance = 0f;
                            LastScreenPoint.UpdateValue(firstTouch); 
                            break;
                        
                        case 2:
                            _lastZoomDistance = BMath.Distance(firstTouch, secondTouch);
                            LastScreenPoint.UpdateValue((firstTouch + secondTouch) * 0.5f);
                            break;
                    }

                    break;
            }
        }

        private float2 _getScreenCenter()
        {
            return new float2((float)Screen.width / 2, (float)Screen.height / 2);
        }

        private int _getTouchCount()
        {
            int touchCount = 0;
            switch (_inputSchemeObserver.Current)
            {
                case EInputScheme.MouseAndKeyboard:
                    touchCount = Mouse.current.press.isPressed  ? 1 : 0;
                    break;
                
                case EInputScheme.Gamepad:
                    break;

                case EInputScheme.Touch:
                    touchCount = Touchscreen.current.primaryTouch.press.isPressed ? 1 : 0;
                    touchCount += Touchscreen.current.touches[1].press.isPressed ? 1 : 0;
                    break;
            }
            return touchCount;
        }
        
        private bool _isCursorInsideGameWindow()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            return mousePosition.x >= 0 && mousePosition.y >= 0 &&
                   mousePosition.x <= Screen.width && mousePosition.y <= Screen.height;
        }
    }
}
