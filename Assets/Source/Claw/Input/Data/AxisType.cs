
using System;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Math;

namespace Beast.Claw.Input
{
    [ConstantGroup]
    public readonly struct AxisType : IEquatable<AxisType>
    {
        public static readonly AxisType NONE = new (0, "none");
        public static readonly AxisType RAW_TOUCH = new (1, "raw_touch");
        public static readonly AxisType DRAG = new(2, "drag");
        public static readonly AxisType ZOOM = new(3, "zoom");
        
        public static AxisType Parse(int id)
        {
            return id switch
            {
                0 => NONE,
                1 => RAW_TOUCH,
                2 => DRAG,
                3 => ZOOM,
                _ => throw new ArgumentException($"Unknown AxisType id: {id}")
            };
        }
        
        public readonly int id;
        public readonly string name;

        public AxisType(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
        
        public bool Intersects(Int32Mask mask)
        {
            return mask.Intersects(1 << id);
        }

        public Int32Mask AsMask()
        {
            return new Int32Mask(1 << id);
        }

        public void AddTo(ref Int32Mask mask)
        {
            mask.Union(1 << id);
        }

        public static bool operator ==(AxisType a, AxisType b)
        {
            return a.id == b.id;
        }
        
        public static bool operator !=(AxisType a, AxisType b)
        {
            return a.id != b.id;
        }

        public bool Equals(AxisType other)
        {
            return id == other.id;
        }

        public override bool Equals(object obj)
        {
            return obj is AxisType other && Equals(other);
        }

        public override int GetHashCode()
        {
            return id;
        }

        public override string ToString()
        {
            return "(" + id + ", " + name + ")";
        }
    }
}