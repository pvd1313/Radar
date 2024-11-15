using System.Collections.Generic;

namespace Beast.Claw.GameLoop
{
    public class GameLoopActionList
    {
        private const int BUFFER_CAPACITY = 128;
        
        public List<ActionProperties> All { get; } = new()
        {
            new ActionProperties
            {
                id = ActionID.WARMUP,
                order = EActionOrder.FromFirstToLastBinded,
                flow = EActionFlowType.Concurrent,
                frequency = EActionFrequency.OncePerActorLife,
                bufferCapacity = BUFFER_CAPACITY,
                flushProfilerToConsole = true,
            },
            
            new ActionProperties
            {
                id = ActionID.DISPOSE,
                order = EActionOrder.FromLastToFirstBinded,
                flow = EActionFlowType.Blocking,
                frequency = EActionFrequency.OncePerActorLife,
                bufferCapacity = BUFFER_CAPACITY,
                flushProfilerToConsole = true
            },
            
            new ActionProperties
            {
                id = ActionID.GAME_LOAD,
                order = EActionOrder.FromFirstToLastBinded,
                flow = EActionFlowType.Concurrent,
                frequency = EActionFrequency.EachTimeEventFired,
                bufferCapacity = BUFFER_CAPACITY,
                flushProfilerToConsole = true
            },
            
            new ActionProperties
            {
                id = ActionID.GAME_SAVE,
                order = EActionOrder.FromFirstToLastBinded,
                flow = EActionFlowType.Blocking,
                frequency = EActionFrequency.EachTimeEventFired,
                bufferCapacity = BUFFER_CAPACITY,
                flushProfilerToConsole = true
            },
            
            new ActionProperties
            {
                id = ActionID.PREFERENCES_LOAD,
                order = EActionOrder.FromFirstToLastBinded,
                flow = EActionFlowType.Concurrent,
                frequency = EActionFrequency.EachTimeEventFired,
                bufferCapacity = BUFFER_CAPACITY,
                flushProfilerToConsole = true,
            },
            
            new ActionProperties
            {
                id = ActionID.PREFERENCES_SAVE,
                order = EActionOrder.FromFirstToLastBinded,
                flow = EActionFlowType.Blocking,
                frequency = EActionFrequency.EachTimeEventFired,
                bufferCapacity = BUFFER_CAPACITY,
                flushProfilerToConsole = true
            },
            
            new ActionProperties
            {
                id = ActionID.TICK,
                order = EActionOrder.FromFirstToLastBinded,
                flow = EActionFlowType.Blocking,
                frequency = EActionFrequency.EachTimeEventFired,
                bufferCapacity = BUFFER_CAPACITY,
                flushProfilerToConsole = false
            },
            
            new ActionProperties
            {
                id = ActionID.RENDER,
                order = EActionOrder.FromFirstToLastBinded,
                flow = EActionFlowType.Blocking,
                frequency = EActionFrequency.EachTimeEventFired,
                bufferCapacity = BUFFER_CAPACITY,
                flushProfilerToConsole = false
            },
            
            new ActionProperties
            {
                id = ActionID.FIXED_UPDATE,
                order = EActionOrder.FromFirstToLastBinded,
                flow = EActionFlowType.Blocking,
                frequency = EActionFrequency.EachTimeEventFired,
                bufferCapacity = BUFFER_CAPACITY,
                flushProfilerToConsole = false,
            }
        };
    }
}