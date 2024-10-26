using System.Collections.Generic;

namespace Beast.Claw.GameCycle
{
    public class GameCycleOperationList
    {
        public List<OperationInfo> All { get; } = new()
        {
            new OperationInfo
            {
                id = OperationID.WARMUP,
                order = EOperationOrder.FromFirstToLastInjected,
                flow = EOperationFlowType.Concurrent,
                frequency = EOperationFrequency.OncePerObjectLife
            },
            new OperationInfo
            {
                id = OperationID.DISPOSE,
                order = EOperationOrder.FromLastToFirstInjected,
                flow = EOperationFlowType.Concurrent,
                frequency = EOperationFrequency.OncePerObjectLife
            },
            
            new OperationInfo
            {
                id = OperationID.GAME_LOAD,
                order = EOperationOrder.FromFirstToLastInjected,
                flow = EOperationFlowType.Concurrent,
                frequency = EOperationFrequency.EveryTimeEventFired
            },
            new OperationInfo
            {
                id = OperationID.GAME_SAVE,
                order = EOperationOrder.FromFirstToLastInjected,
                flow = EOperationFlowType.Concurrent,
                frequency = EOperationFrequency.EveryTimeEventFired
            },
            
            new OperationInfo
            {
                id = OperationID.PREFERENCES_LOAD,
                order = EOperationOrder.FromFirstToLastInjected,
                flow = EOperationFlowType.Concurrent,
                frequency = EOperationFrequency.EveryTimeEventFired
            },
            new OperationInfo
            {
                id = OperationID.PREFERENCES_SAVE,
                order = EOperationOrder.FromFirstToLastInjected,
                flow = EOperationFlowType.Concurrent,
                frequency = EOperationFrequency.EveryTimeEventFired
            },
            
            new OperationInfo
            {
                id = OperationID.TICK,
                order = EOperationOrder.FromFirstToLastInjected,
                flow = EOperationFlowType.Blocking,
                frequency = EOperationFrequency.EveryTimeEventFired
            },
            new OperationInfo
            {
                id = OperationID.RENDER,
                order = EOperationOrder.FromFirstToLastInjected,
                flow = EOperationFlowType.Blocking,
                frequency = EOperationFrequency.EveryTimeEventFired
            },
            new OperationInfo
            {
                id = OperationID.GIZMOS_DRAW,
                order = EOperationOrder.FromFirstToLastInjected,
                flow = EOperationFlowType.Blocking,
                frequency = EOperationFrequency.EveryTimeEventFired
            },
            new OperationInfo
            {
                id = OperationID.FIXED_UPDATE,
                order = EOperationOrder.FromFirstToLastInjected,
                flow = EOperationFlowType.Blocking,
                frequency = EOperationFrequency.EveryTimeEventFired
            }
        };
    }
}