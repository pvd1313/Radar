using System;
using System.Runtime.CompilerServices;
using Beast.Claw.Attributes.Annotation;
using Unity.Collections;

namespace Beast.Claw.Jobs
{
    [StaticUtility]
    public static class BNative
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> TempJobArray<T>(int length) where T : unmanaged
        {
            return new NativeArray<T>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> TempArray<T>(int length) where T : unmanaged
        {
            return new NativeArray<T>(length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> PermArray<T>(int length) where T : unmanaged
        {
            return new NativeArray<T>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PermArray<T>(int length, out NativeArray<T> array) where T : unmanaged
        {
            array = new NativeArray<T>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PermHashSet<T>(int capacity, out NativeHashSet<T> array) where T : unmanaged, IEquatable<T>
        {
            array = new NativeHashSet<T>(capacity, Allocator.Persistent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeList<T> PermList<T>(int capacity) where T : unmanaged
        {
            return new NativeList<T>(capacity, Allocator.Persistent);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeList<T> TempJobList<T>(int capacity) where T : unmanaged
        {
            return new NativeList<T>(capacity, Allocator.TempJob);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeHashSet<T> TempJobHashSet<T>(int capacity) where T : unmanaged, IEquatable<T>
        {
            return new NativeHashSet<T>(capacity, Allocator.TempJob);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeHashSet<T> PermHashSet<T>(int capacity) where T : unmanaged, IEquatable<T>
        {
            return new NativeHashSet<T>(capacity, Allocator.Persistent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeReference<T> PermReference<T>(T withValue) where T : unmanaged
        {
            NativeReference<T> reference = new(Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            reference.Value = withValue;
            return reference;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeReference<T> TempJobReference<T>() where T : unmanaged
        {
            return new NativeReference<T>(Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }

        public static void Copy<T>(
            NativeArray<T> read,
            NativeArray<T> write,
            int readOffset,
            int writeOffset,
            int dataCount)
            where T : unmanaged
        {
            NativeSlice<T> source = read.Slice(readOffset, dataCount);
            NativeSlice<T> target = write.Slice(writeOffset, dataCount);

            target.CopyFrom(source);
        }
    }
}