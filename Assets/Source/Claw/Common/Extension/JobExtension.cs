using Beast.Claw.Attributes.Annotation;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Beast.Claw.Common
{
    [ExtensionClass]
    public static class JobExtension
    {
        public static void ExecuteOn<T>(this T job, NativeSlice<ArchetypeChunk> chunks)
            where T : unmanaged, IJobParallelFor
        {
            job.Schedule(chunks.Length, Build.ECS_PARALLEL_JOB_INNER_LOOP_BATCH_COUNT).Complete();
        }
        
        public static void SmartExecute<T>(this T job, int arraySize, int batchCount)
            where T : unmanaged, IJobParallelFor
        {
            int threadCount = arraySize / batchCount;

            if (threadCount <= 2)
            {
                job.Run(arraySize);
            }
            else
            {
                job.Schedule(arraySize, batchCount).Complete();
            }
        }

        public static void DebugExecute<T>(this T job, int arrayLength) 
            where T : unmanaged, IJobParallelFor 
        {
            for (int i = 0; i < arrayLength; i++)
            {
                job.Execute(i);
            }
        }
    }
}