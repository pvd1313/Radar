using System;
using System.Runtime.CompilerServices;
using Beast.Claw.Math;
using Unity.Collections;

namespace Beast.Claw.Jobs
{
    public static class Native
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> CreateTempArray<T>(int length) where T : unmanaged
        {
            return new NativeArray<T>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> PermArray<T>(int length) where T : unmanaged
        {
            return new NativeArray<T>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeList<T> CreatePermList<T>(int capacity) where T : unmanaged
        {
            return new NativeList<T>(capacity, Allocator.Persistent);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeList<T> CreateTempList<T>(int capacity) where T : unmanaged
        {
            return new NativeList<T>(capacity, Allocator.TempJob);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeHashSet<T> CreateTempHashSet<T>(int capacity) where T : unmanaged, IEquatable<T>
        {
            return new NativeHashSet<T>(capacity, Allocator.TempJob);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeHashSet<T> CreatePermHashSet<T>(int capacity) where T : unmanaged, IEquatable<T>
        {
            return new NativeHashSet<T>(capacity, Allocator.Persistent);
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

            int newSize = 1 << BMath.CeilToInt(BMath.Log2(requiredCapacity));
            NativeArray<T> newArray = PermArray<T>(newSize);
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
            int chunkBasedLength = BMath.CeilToInt(requiredCapacity / (float) itemsPerAllocation) * itemsPerAllocation;
            NativeArray<T> newArray = PermArray<T>(chunkBasedLength);

            if (rule.copyOnResize)
            {
                NativeArray<T>.Copy(array, newArray, array.Length);
            }
            
            array.Dispose();
            array = newArray;
        }
    }
}