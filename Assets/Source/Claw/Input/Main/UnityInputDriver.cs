#if UNITY_2022_3_OR_NEWER

using System.Runtime.CompilerServices;
using Beast.Claw.Attributes;
using Beast.Claw.Collections;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

using UnityInput = UnityEngine.Input;

namespace Beast.Claw.Input
{
    public class UnityInputDriver : Actor, IInputDriver
    {
        [Required] private readonly IScreen _screen;

        public UnityInputDriver(IScreen screen)
        {
            _screen = screen;
        }
        
        protected override UniTask _warmup(ConcurrentContext context)
        {
            UnityInput.gyro.enabled = false;
            UnityInput.compass.enabled = false;
            UnityInput.compensateSensors = false;
            UnityInput.simulateMouseWithTouches = false;
            
            return UniTask.CompletedTask;
        }

        public RawTouchAxis ReadMousePointer()
        {
            float2 position = UnityInput.mousePosition.XY() /  _screen.Size;

            EAxisStatus status = _getMouseButtonStatus(0);

            return new RawTouchAxis(position, status, 1);
        }

        public float ReadMouseWheel()
        {
            return UnityInput.mouseScrollDelta.y;
        }

        public void ReadTouchScreen(ref NativeFixedList<RawTouchAxis> rawTouches)
        {
            rawTouches.Clear();
            
            int touchCount = UnityInput.touchCount;
            int touchLimit = BMath.Min(touchCount, rawTouches.Capacity);

            for (int i = 0; i < touchLimit; i++)
            {
                UnityEngine.Touch touch = UnityInput.GetTouch(i);
                
                EAxisStatus status = EAxisStatus.Disabled;
                
                if (touch.phase == UnityEngine.TouchPhase.Began)
                {
                    status = EAxisStatus.Starting;
                }
                else if (touch.phase == UnityEngine.TouchPhase.Moved || 
                         touch.phase == UnityEngine.TouchPhase.Stationary)
                {
                    status = EAxisStatus.Working;
                }
                
                rawTouches.Add(new RawTouchAxis
                {
                    fingerId = touch.fingerId,
                    status = status,
                    position = (float2)touch.position / _screen.Size
                });
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static EAxisStatus _getMouseButtonStatus(int buttonID)
        {
            if (UnityInput.GetMouseButtonDown(buttonID))
            {
                return EAxisStatus.Starting;
            }

            if (UnityInput.GetMouseButton(buttonID))
            {
                return EAxisStatus.Working;
            }

            return EAxisStatus.Disabled;
        }
    }
}

#endif