using Beast.Claw.Common;
using Beast.Claw.Graphic;
using Beast.Claw.Input;
using Noesis;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class NoesisInputConsumer : Controller, IInputConsumer
    {
        private readonly NoesisView _view;
        private readonly Visual _viewRoot;
        private readonly IScreen _screen;

        public NoesisInputConsumer(NoesisView view, IScreen screen)
        {
            _view = view;
            _screen = screen;
            
            _viewRoot = (Visual)VisualTreeHelper.GetRoot(view.Content);
        }
        
        public bool TryCaptureDevice(IInputDevice device, Int32Mask axisMask)
        {
            if (axisMask.Intersects((int)EInputAxis.RawTouch))
            {
                NativeSlice<RawTouchAxis> touches = device.RawTouch;
                
                foreach (RawTouchAxis touch in touches)
                {
                    float2 proj = _projectPosition(touch.position);

                    // Maybe there should be more complex logic. For example check if control can capture.
                    if (_hitTest(proj))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void UpdateInput(IInputDevice device, Int32Mask axisMask)
        {
            NativeSlice<RawTouchAxis> touches = device.RawTouch;

            foreach (RawTouchAxis touch in touches)
            {
                float2 proj = _projectPosition(touch.position);

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
            Point p = _viewRoot.PointFromScreen(new Point(position.x, position.y));

            return VisualTreeHelper.HitTest(_viewRoot, p).VisualHit != null;
        }
        
        private float2 _projectPosition(float2 position)
        {
            float x = position.x * _screen.Width;
            float y = (1 - position.y) * _screen.Height;

            return new float2(x, y);
        }
    }
}