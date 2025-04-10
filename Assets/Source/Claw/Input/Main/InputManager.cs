using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Math;
using Beast.Claw.Validation;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.Input
{
    public class InputManager : Actor, IInputManager
    {
        private List<ConsumerInfo> _consumers;
        private Int32Mask _validFlags;

        protected override UniTask _warmup(ConcurrentContext arg)
        {
            _consumers = new List<ConsumerInfo>(8);
            
            _validFlags = new Int32Mask();

            AxisType.DRAG.AddTo(ref _validFlags);
            AxisType.ZOOM.AddTo(ref _validFlags);
            AxisType.RAW_TOUCH.AddTo(ref _validFlags);
            
            return UniTask.CompletedTask;
        }

        public bool TryFindConsumer(PackedAxis axis, out ConsumerInfo info)
        {
            info = default;

            int axisTypeInt = 1 << axis.GetAxisType().id;
            
            foreach (ConsumerInfo consumer in _consumers)
            {
                if (consumer.axisMask.Intersects(axisTypeInt) && 
                    consumer.consumer.TryCaptureAxis(axis))
                {
                    info = consumer;
                    return true;
                }
            }

            return false;
        }
        
        public void RegisterConsumer(IInputConsumer consumer, int priority)
        {
            consumer.ValidateNotNullThrowException(nameof(consumer));

            Int32Mask axisMask = consumer.SupportedAxis;

            if (Build.IS_DEVELOPMENT)
            {
                if (axisMask.value == 0)
                {
                    throw new ArgumentException($"Consumer ({consumer.Meta.fullName}) has unassigned  axis mask.");
                }

                if (axisMask.HitsButNotEqual(1 << AxisType.RAW_TOUCH.id))
                {
                    throw new ArgumentException($"Invalid axis mode. ({consumer.Meta.fullName}) Has RawTouch, " +
                                                $"but also other. This is not supported. " +
                                                $"Consumer axis mask: ({axisMask})");
                }
            }

            int i = 0;

            for (; i < _consumers.Count; i++)
            {
                if (priority > _consumers[i].priority)
                {
                    break;
                }
            }
            
            _consumers.Insert(i, new ConsumerInfo
            {
                axisMask = axisMask,
                consumer = consumer,
                priority = priority
            });
        }
    }
}