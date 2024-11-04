using System;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public struct CitySpawnPointSearch
    {
        private QuadPerimeterIterator _perimeterIterator;
        private QuadPointIterator _pointIterator;
        private int _radius;
        
        public CitySpawnPointSearch(int radius)
        {
            if (radius < 0)
            {
                throw new ArgumentException(nameof(radius));
            }

            _radius = radius;
            _perimeterIterator = new QuadPerimeterIterator(_radius);
            _pointIterator = new QuadPointIterator(_radius);
        }

        public bool GotNext(out int2 delta)
        {
            if (_radius < 5)
            {
                return _perimeterIterator.GotNext(out delta);
            }

            return _perimeterIterator.GotNext(out delta);
        }
    }
}