using System.Collections.Generic;

namespace Beast.Claw.Common
{
    partial class BeastDebugTrackStorage
    {
        private readonly struct StringDictionary<T>
        {
            private readonly Dictionary<string, T> _values;

            public StringDictionary(int capacity)
            {
                _values = new Dictionary<string, T>();
            }

            public IEnumerable<KeyValuePair<string, T>> ReadAll()
            {
                return _values;
            }

            public void Write(string id, T value)
            {
                _values[id] = value;
            }
        }
    }
}