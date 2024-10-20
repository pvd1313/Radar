using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Beast.Claw.JobUtilities
{
    [BurstCompile]
    internal struct DataCollectJob<T> : IJob where T : struct
    {
        public NativeArray<T> inOutData;

        [ReadOnly] public NativeArray<int> inCounts;
        [ReadOnly] public int inOffset;

        [WriteOnly] public NativeReference<int> outCount;
        
        public void Execute()
        {
            int resultCount = 0;
            int offsetCount = inCounts.Length;
            for (int i = 0; i < offsetCount; i++)
            {
                int localCount = inCounts[i];
                if (localCount == 0)
                {
                    continue;
                }

                int offset = i * inOffset;
                for (int j = 0; j < localCount; j++)
                {
                    inOutData[resultCount] = inOutData[offset + j];
                    resultCount++;
                }
            }

            outCount.Value = resultCount;
        }
    }
}