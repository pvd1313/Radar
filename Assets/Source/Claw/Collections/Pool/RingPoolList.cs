using System;
using System.Collections.Generic;

namespace Beast.Claw.Collections
{
    public class RingPoolList<TValue> : RingPool<List<TValue>>
    {
        public RingPoolList(int capacity, Func<List<TValue>> factory) : base(capacity, factory)
        {
            
        }
    }
}