using System;
using Beast.Claw.Jobs;
using Unity.Collections;

namespace Beast.Claw.Gizmos
{
    public struct GizmosBuffer<T> : IDisposable 
        where T : unmanaged
    {
        public NativeArray<T> shapes;
        public int shapeCount;
        
        private readonly int _allocationSize;

        public GizmosBuffer(int allocationSize)
        {
            _allocationSize = allocationSize;
            shapes = Native.CreatePermArray<T>(allocationSize);
            shapeCount = 0;
        }

        public void Add(T shape)
        {
            Native.MaintainPersistentArrayLength(ref shapes, new ArrayMaintenanceData
            {
                copyOnResize = true,
                itemPerAllocation = _allocationSize,
                requiredCapacity = shapeCount + 1
            });

            shapes[shapeCount++] = shape;
        }

        public void Clear()
        {
            shapeCount = 0;
        }

        public void Dispose()
        {
            shapes.Dispose();
        }
    }
}