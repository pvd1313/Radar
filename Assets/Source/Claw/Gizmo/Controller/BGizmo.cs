using System.Runtime.CompilerServices;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Common;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Unity.Collections;

namespace Beast.Claw.Gizmo
{
    [StaticUtility]
    public static class BGizmo
    {
        public const int FLOAT_PER_SHAPE = 8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCapacity(ref NativeArray<ushort> array, int targetCapacity)
        {
            if (array.IsCreated && array.Length > targetCapacity * FLOAT_PER_SHAPE)
            {
                return;
            }

            int newBufferLength = _shapeCountToBufferSize(targetCapacity);
            NativeArray<ushort> newBuffer = BNative.PermArray<ushort>(newBufferLength);

            if (array.IsCreated)
            {
                newBuffer.CopyFromDifferenceSize(array);
                
                array.Dispose();
            }

            array = newBuffer;
        }
        
        private static int _shapeCountToBufferSize(int shapeCount)
        {
            int size = BMath.CeilToInt(BMath.Sqrt(shapeCount * FLOAT_PER_SHAPE));
            
            size = 1 << BMath.CeilToInt(BMath.Log2(size));

            return size * size;
        }
    }
}