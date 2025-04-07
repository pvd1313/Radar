using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Debug;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Beast.Claw.Jobs
{
    [ExtensionClass]
    public static class JobExtension
    {
        private const int MULTI_THREAD_THRESHOLD = 2;
        private const int ECS_CHUNK_BATCH_SIZE = 4;

        public static void Invoke<T>(this T job) where T : unmanaged, ISingleJob
        {
            BProfiler.BeginSample(typeof(T).FullName);

            IJobExtensions.Run(job);
            
            BProfiler.EndSample();
        }
        
        public static void Invoke<T>(this T job, NativeSlice<ArchetypeChunk> chunks)
            where T : unmanaged, IMultiJob
        {
            int threadCount = chunks.Length / ECS_CHUNK_BATCH_SIZE;
            
            BProfiler.BeginSample(typeof(T).FullName);

            if (threadCount < MULTI_THREAD_THRESHOLD)
            {
                IJobParallelForExtensions.Run(job, chunks.Length);
            }
            else
            {
                IJobParallelForExtensions.Schedule(job, chunks.Length, ECS_CHUNK_BATCH_SIZE).Complete();
            }

            BProfiler.EndSample();
        }

        public static void Invoke<T>(this T job, int arraySize, int batchSize)
            where T : unmanaged, IMultiJob
        {
            int threadCount = arraySize / batchSize;
            
            BProfiler.BeginSample(typeof(T).FullName);

            if (threadCount < MULTI_THREAD_THRESHOLD)
            {
                IJobParallelForExtensions.Run(job, arraySize);
            }
            else
            {
                IJobParallelForExtensions.Schedule(job, arraySize, batchSize).Complete();
            }

            BProfiler.EndSample();
        }
        
        public static void InvokeDebug<T>(this T job) where T : unmanaged, ISingleJob
        {
            BProfiler.BeginSample(typeof(T).FullName);

            job.Execute();
            
            BProfiler.EndSample();
        }

        public static void InvokeDebug<T>(this T job, int arrayLength) 
            where T : unmanaged, IMultiJob 
        {
            BProfiler.BeginSample(typeof(T).FullName);

            try
            {
                for (int i = 0; i < arrayLength; i++)
                {
                    job.Execute(i);
                }
            }
            finally
            {
                BProfiler.EndSample();
            }
            
        }
    }
}