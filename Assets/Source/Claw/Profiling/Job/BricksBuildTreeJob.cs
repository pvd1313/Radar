using System.Runtime.CompilerServices;
using Beast.Claw.Math;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Beast.Claw.Profiling
{
    [BurstCompile]
    internal struct BricksBuildTreeJob : IJob
    {
        [ReadOnly] public NativeArray<BricksRecord> inRecords;
        [ReadOnly] public int inRecordCount;
        [ReadOnly] public long inFrequency;

        public NativeArray<ushort> parentStack;
        public NativeArray<ushort> siblingStack;
        public NativeArray<int> nameHashStack;
        public NativeArray<int> timeStack;

        public NativeArray<ushort> outChilds;
        [WriteOnly] public NativeArray<ushort> outSiblings;
        [WriteOnly] public NativeArray<ushort> outNames;
        [WriteOnly] public NativeArray<float> outTimes;
        [WriteOnly] public NativeArray<BricksTreeBuildState> outState;

        private int _stackLast;
        private int _nodeCount;
        private int _stackMax;
        private long _ticksTotal;
        private float _ticksToMilliseconds;
        private EBricksTreeBuildCode _code;

        public void Execute()
        {
            _ticksToMilliseconds = 1000f / inFrequency;
            _stackLast = 0;
            _nodeCount = 1;
            parentStack[0] = 0;
            siblingStack[0] = 0;
            nameHashStack[0] = 0;
            outChilds[0] = 0;
            outNames[0] = 0;
            outSiblings[0] = 0;
            outTimes[0] = 0;
            outState[0] = new BricksTreeBuildState
            {
                code = EBricksTreeBuildCode.Unknown
            };
            _code = EBricksTreeBuildCode.Unknown;
            _stackMax = BMath.Min(parentStack.Length, siblingStack.Length);
            
            for (int i = 0; i < inRecordCount; i++)
            {
                BricksRecord record = inRecords[i];
                
                record.Read(out int timeSamples, out bool isBeginSampleCommand, out int nameHash);
                
                if (isBeginSampleCommand)
                {
                    FlushBeginSample(i + 1, timeSamples, nameHash);
                }
                else
                {
                    FlushEndSample(timeSamples, nameHash);
                }

                if (_code != EBricksTreeBuildCode.Unknown)
                {
                    outState[0] = new BricksTreeBuildState
                    {
                        code = _code,
                        stackLast = _stackLast,
                        recordLast = i
                    };
     
                    return;
                }
            }

            if (_stackLast != 0)
            {
                outState[0] = new BricksTreeBuildState
                {
                    code = EBricksTreeBuildCode.StackIsNotEmptyAfterJobComplete,
                    stackLast = _stackLast
                };

                return;
            }

            outTimes[0] = _ticksTotal * _ticksToMilliseconds;
            outState[0] = new BricksTreeBuildState
            {
                code = EBricksTreeBuildCode.Success
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FlushBeginSample(int nameIndex, int timeSamples, int nameHash)
        {
            if (_stackLast + 1 >= _stackMax)
            {
                _code = EBricksTreeBuildCode.StackOverflow;
                return;
            }
            
            ushort nodeIndex =  (ushort) _nodeCount++;
            ushort siblingIndex = siblingStack[_stackLast];

            if (siblingIndex == 0)
            {
                ushort parentIndex = parentStack[_stackLast];
                outChilds[parentIndex] = nodeIndex;
            }
            else
            {
                outSiblings[siblingIndex] = nodeIndex;
            }

            siblingStack[_stackLast++] = nodeIndex;
            siblingStack[_stackLast] = 0;
            nameHashStack[_stackLast] = nameHash;
            parentStack[_stackLast] = nodeIndex;
            timeStack[_stackLast] = timeSamples;

            outNames[nodeIndex] = (ushort) nameIndex;
            outChilds[nodeIndex] = 0;
            outSiblings[nodeIndex] = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FlushEndSample(int timeSamples, int nameHash)
        {
            if (_stackLast == 0)
            {
                _code = EBricksTreeBuildCode.StackUnderflow;
                return;
            }

            if (nameHash != nameHashStack[_stackLast])
            {
                _code = EBricksTreeBuildCode.NameMismatch;
                return;
            }

            ushort nodeIndex = parentStack[_stackLast];
            int ticks = timeSamples - timeStack[_stackLast];

            if (outChilds[nodeIndex] == 0)
            {
                _ticksTotal += ticks;
            }

            outTimes[nodeIndex] = ticks * _ticksToMilliseconds;
            
            _stackLast--;
        }
    }
}