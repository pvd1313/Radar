using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
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

        public bool TryFindConsumer(IInputDevice device, EInputAxis axis, out ConsumerInfo info)
        {
            info = default;

            Int32Mask modeMask = new ((int)axis);
            
            foreach (ConsumerInfo consumer in _consumers)
            {
                if (consumer.axisMask.Intersects((int) axis) && 
                    consumer.consumer.TryCaptureDevice(device, modeMask))
                {
                    info = consumer;
                    return true;
                }
            }

            return false;
        }
        
        public void RegisterConsumer(IInputConsumer consumer, EInputAxis modes, int priority)
        {
            consumer.ValidateNotNullThrowException(nameof(consumer));

            Int32Mask axisMask = new ((int)modes);

            if (Build.IS_DEVELOPMENT)
            {
                if (axisMask.value == 0)
                {
                    throw new ArgumentException("Invalid axis mode (none)");
                }

                if (axisMask.HitsButNotEqual((int)EInputAxis.RawTouch))
                {
                    throw new ArgumentException($"Invalid axis mode. ({consumer.Meta.name}) Has raw touch, " +
                                                $"but also other. This is not supported. ({modes})");
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