using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Beast.Claw.JobUtilities
{
    [BurstCompile]
    internal struct FillNativeArrayJob<T> : IJobParallelFor where T : struct
    {
        [ReadOnly] public T inValue;
        [ReadOnly] public int inItemPerJob;
        [ReadOnly] public int inTotalItem;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<T> outNativeArray;
        
        public void Execute(int jobIndex)
        {
            int startIndex = jobIndex * inItemPerJob;
            int endIndex = System.Math.Min(startIndex + inItemPerJob, inTotalItem);
            for (int i = startIndex; i < endIndex; i++)
            {
                outNativeArray[i] = inValue;
            }
        }
    }
}