using System.Collections.Generic;

namespace Beast.Claw.GameCycle
{
    public class GameCycleActionList
    {
        public List<ActionInfo> All { get; } = new()
        {
            new ActionInfo
            {
                id = ActionID.WARMUP,
                order = EActionOrder.FromFirstToLastBinded,
                flow = EActionFlowType.Concurrent,
                frequency = EActionFrequency.OncePerObjectLife,
                bufferCapacity = 128,
                flushProfilerToConsole = true,
            },
            new ActionInfo
            {
                id = ActionID.DISPOSE,
                order = EActionOrder.FromLastToFirstBinded,
                flow = EActionFlowType.Blocking,
                frequency = EActionFrequency.OncePerObjectLife,
                bufferCapacity = 128,
                flushProfilerToConsole = true
            },
            
            new ActionInfo
            {
                id = ActionID.GAME_LOAD,
                order = EActionOrder.FromFirstToLastBinded,
                flow = EActionFlowType.Concurrent,
                frequency = EActionFrequency.EveryTimeEventFired,
                bufferCapacity = 32,
                flushProfilerToConsole = true
            },
            new ActionInfo
            {
                id = ActionID.GAME_SAVE,
                order = EActionOrder.FromFirstToLastBinded,
                flow = EActionFlowType.Blocking,
                frequency = EActionFrequency.EveryTimeEventFired,
                bufferCapacity = 32,
                flushProfilerToConsole = true
            },
            
            new ActionInfo
            {
                id = ActionID.PREFERENCES_LOAD,
                order = EActionOrder.FromFirstToLastBinded,
                flow = EActionFlowType.Concurrent,
                frequency = EActionFrequency.EveryTimeEventFired,
                bufferCapacity = 16,
                flushProfilerToConsole = true,
            },
            new ActionInfo
            {
                id = ActionID.PREFERENCES_SAVE,
                order = EActionOrder.FromFirstToLastBinded,
                flow = EActionFlowType.Blocking,
                frequency = EActionFrequency.EveryTimeEventFired,
                bufferCapacity = 16,
                flushProfilerToConsole = true
            },
            
            new ActionInfo
            {
                id = ActionID.TICK,
                order = EActionOrder.FromFirstToLastBinded,
                flow = EActionFlowType.Blocking,
                frequency = EActionFrequency.EveryTimeEventFired,
                bufferCapacity = 128,
                flushProfilerToConsole = false
            },
            new ActionInfo
            {
                id = ActionID.RENDER,
                order = EActionOrder.FromFirstToLastBinded,
                flow = EActionFlowType.Blocking,
                frequency = EActionFrequency.EveryTimeEventFired,
                bufferCapacity = 128,
                flushProfilerToConsole = false
            },
            new ActionInfo
            {
                id = ActionID.FIXED_UPDATE,
                order = EActionOrder.FromFirstToLastBinded,
                flow = EActionFlowType.Blocking,
                frequency = EActionFrequency.EveryTimeEventFired,
                bufferCapacity = 128,
                flushProfilerToConsole = false,
            }
        };
    }
}