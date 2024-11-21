using System;
using Beast.Claw.Attributes.Annotation;
using Unity.Collections;

namespace Beast.Claw.Common
{
    [ExtensionClass]
    public static class NativeArrayExtension
    {
        public static void CopyFromDifferenceSize<T>(this NativeArray<T> target, NativeArray<T> source) where T : unmanaged
        {
            if (source.Length > target.Length)
            {
                throw new InvalidOperationException("Source array is bigger than target array");
            }
            
            target.Slice(0, source.Length).CopyFrom(source.Slice(0, source.Length));
        }
    }
}