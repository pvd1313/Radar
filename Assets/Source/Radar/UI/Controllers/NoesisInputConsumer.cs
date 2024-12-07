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
        private bool _gotHit;

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

            Point point = _viewRoot.PointFromScreen(new Point(position.x, position.y));
            
            _gotHit = false;

            VisualTreeHelper.HitTest(_viewRoot, _hitTestFilter, _hitResult, new PointHitTestParameters(point));
            
            UnityEngine.Profiling.Profiler.EndSample();
            
            return _gotHit;
        }

        private float2 _projectPosition(float2 position)
        {
            int2 screenSize = _screen.Size;

            float x = position.x * screenSize.x;
            float y = (1 - position.y) * screenSize.y;

            return new float2(x, y);
        }

        private HitTestResultBehavior _hitResult(HitTestResult hit)
        {
            if (hit.VisualHit != null)
            {
                _gotHit = true;
                return HitTestResultBehavior.Stop;
            }

            return HitTestResultBehavior.Continue;
        }

        private HitTestFilterBehavior _hitTestFilter(Visual hit)
        {
            if (hit is UIElement { IsHitTestVisible: false })
            {
                return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
            }

            return HitTestFilterBehavior.Continue;
        }
    }
}