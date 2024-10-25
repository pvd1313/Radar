using System.Runtime.CompilerServices;
using Unity.Collections;

namespace Beast.Claw.Jobs
{
    public static class NativeMemory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> CreateTempJobArray<T>(int length) where T : unmanaged
        {
            return new NativeArray<T>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> CreateTempArray<T>(int length) where T : unmanaged
        {
            return new NativeArray<T>(length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> CreatePermArray<T>(int length) where T : unmanaged
        {
            return new NativeArray<T>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeReference<T> CreatePersistentReference<T>(T withValue) where T : unmanaged
        {
            NativeReference<T> reference = new NativeReference<T>(Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            reference.Value = withValue;
            return reference;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeReference<T> CreateTempJobReference<T>() where T : unmanaged
        {
            return new NativeReference<T>(Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCapacity<T>(ref NativeArray<T> permArray, int requiredCapacity)
            where T : unmanaged
        {
            if (permArray.Length >= requiredCapacity)
            {
                return;
            }

            int newSize = 1 << (int) System.Math.Ceiling(System.Math.Log(requiredCapacity, 2));
            NativeArray<T> newArray = CreatePermArray<T>(newSize);
            NativeArray<T>.Copy(permArray, newArray, permArray.Length);
            
            permArray.Dispose();
            permArray = newArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MaintainPersistentArrayLength<T>(ref NativeArray<T> array, ArrayMaintenanceData rule) 
            where T : unmanaged
        {
            int requiredCapacity = rule.requiredCapacity;
            if (array.Length >= requiredCapacity)
            {
                return;
            }
            
            int itemsPerAllocation = rule.itemPerAllocation;
            int chunkBasedLength = (int) System.Math.Ceiling(requiredCapacity / (float) itemsPerAllocation) * itemsPerAllocation;
            NativeArray<T> newArray = CreatePermArray<T>(chunkBasedLength);

            if (rule.copyOnResize)
            {
                NativeArray<T>.Copy(array, newArray, array.Length);
            }
            
            array.Dispose();
            array = newArray;
        }
    }
}