using System.Runtime.CompilerServices;
using Beast.Claw.Collection;
using Beast.Claw.Jobs;
using Beast.Claw.Mathematics;
using Unity.Collections;

namespace Beast.Claw.Gizmo
{
    public static class GizmoUtil
    {
        public const int FLOAT_PER_SHAPE = 8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCapacity(ref NativeArray<float> array, int targetCapacity)
        {
            if (array.IsCreated && array.Length > targetCapacity * FLOAT_PER_SHAPE)
            {
                return;
            }

            int newBufferLength = _shapeCountToBufferSize(targetCapacity);
            NativeArray<float> newBuffer = Native.PermArray<float>(newBufferLength);

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