using Beast.Claw.Common;
using Beast.Claw.Graphic;
using Beast.Claw.Input;
using Beast.Claw.Math;
using Noesis;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class NoesisInputConsumer : Controller, IInputConsumer
    {
        public Int32Mask SupportedAxis => AxisType.RAW_TOUCH.AsMask();
        
        private readonly NoesisView _view;
        private readonly Visual _viewRoot;
        private readonly IScreen _screen;

        public NoesisInputConsumer(NoesisView view, IScreen screen)
        {
            _view = view;
            _screen = screen;
            
            _viewRoot = (Visual)VisualTreeHelper.GetRoot(view.Content);
        }

        public bool TryCaptureAxis(PackedAxis axis)
        {
            if (axis.GetAxisType() == AxisType.RAW_TOUCH)
            {
                axis.Unpack(out NativeSlice<RawTouchAxis> touches);
                
                foreach (RawTouchAxis touch in touches)
                {
                    float2 proj = _projectPosition(touch.position);

                    // Maybe in future there be more complex logic.
                    // For example check if control can capture (transparent panel, debug stats panel, etc.)
                    
                    if (_hitTest(proj))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void UpdateAxis(PackedAxis axis)
        {
            axis.Unpack(out NativeSlice<RawTouchAxis> touches);
            
            foreach (RawTouchAxis touch in touches)
            {
                float2 proj = _projectPosition(touch.position);

                // Probably in distance future we will use our axis to control all elements.
                switch (touch.status)
                {
                    case EAxisStatus.Starting:
                        _view.TouchDown((int)proj.x, (int)proj.y, (uint)touch.fingerId);
                        break;
                    
                    case EAxisStatus.Working:
                        _view.TouchMove((int)proj.x, (int)proj.y, (uint)touch.fingerId);
                        break;
                    
                    default:
                        _view.TouchUp((int)proj.x, (int)proj.y, (uint)touch.fingerId);
                        break;
                }
            }
        }
        
        private bool _hitTest(float2 position)
        {
            UnityEngine.Profiling.Profiler.BeginSample("NoesisInputConsumer._hitTest()");
            
            Point p = _viewRoot.PointFromScreen(new Point(position.x, position.y));
            
            bool result = VisualTreeHelper.HitTest(_viewRoot, p).VisualHit != null;
            
            UnityEngine.Profiling.Profiler.EndSample();

            return result;
        }
        
        private float2 _projectPosition(float2 position)
        {
            float x = position.x * _screen.Width;
            float y = (1 - position.y) * _screen.Height;

            return new float2(x, y);
        }
    }
}