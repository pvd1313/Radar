using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Math;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.Input
{
    public class InputManager : Controller, IInputManager, IGameCycleListener
    {
        private List<ConsumerInfo> _consumers;
        private Int32Mask _validFlags;
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }

        private UniTask _warmup(ConcurrentContext arg)
        {
            _consumers = new List<ConsumerInfo>(8);
            
            _validFlags = new Int32Mask();
            _validFlags.Union((int) EInputAxis.Zoom);
            _validFlags.Union((int) EInputAxis.Drag);
            _validFlags.Union((int) EInputAxis.RawTouch);
            
            return UniTask.CompletedTask;
        }

        public bool TryFindConsumer(PackedAxis axis, out ConsumerInfo info)
        {
            info = default;

            int axisTypeInt = (int)axis.GetAxisType();
            
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

            Int32Mask axisMask = new((int)consumer.SupportedAxis);

            if (Build.IS_DEVELOPMENT)
            {
                if (axisMask.value == 0)
                {
                    throw new ArgumentException($"Consumer ({consumer.Meta.name}) has invalid axis mask. (none).");
                }

                if (axisMask.HitsButNotEqual((int)EInputAxis.RawTouch))
                {
                    throw new ArgumentException($"Invalid axis mode. ({consumer.Meta.name}) Has RawTouch, " +
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