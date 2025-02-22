using System;
using Beast.Claw.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public struct CitySpawnPointSearch
    {
        private readonly int _radius;
        
        private QuadPerimeterIterator _perimeterIterator;
        private QuadPointIterator _pointIterator;
        
        public CitySpawnPointSearch(int radius)
        {
            if (radius < 0)
            {
                throw new ArgumentException(nameof(radius));
            }

            _radius = radius;

            if (_radius < 5)
            {
                _perimeterIterator = new QuadPerimeterIterator(_radius);
                _pointIterator = default;
            }
            else
            {
                _perimeterIterator = default;
                _pointIterator = new QuadPointIterator(_radius);
            }
        }

        public bool GotNext(out int2 delta)
        {
            if (_radius < 5)
            {
                return _perimeterIterator.GotNext(out delta);
            }

            return _pointIterator.GotNext(out delta);
        }
    }
}