using Beast.Claw.Attributes.Annotation;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Beast.Claw.Common
{
    [ExtensionClass]
    public static class JobExtension
    {
        public static void ExecuteOn<T>(this T jobData, NativeSlice<ArchetypeChunk> chunks)
            where T : unmanaged, IJobParallelFor
        {
            jobData.Schedule(chunks.Length, Build.ECS_PARALLEL_JOB_INNER_LOOP_BATCH_COUNT).Complete();
        }

        public static void DebugExecute<T>(this T jobData, int arrayLength) 
            where T : unmanaged, IJobParallelFor 
        {
            for (int i = 0; i < arrayLength; i++)
            {
                jobData.Execute(i);
            }
        }
    }
}