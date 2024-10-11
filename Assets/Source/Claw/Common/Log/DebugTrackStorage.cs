using System.Collections.Generic;
using Unity.Mathematics;

namespace Beast.Claw.Common
{
    public partial class DebugTrackStorage
    {
        private const int StateStorageCapacity = 0;
        
        private readonly StringDictionary<int> _int = new(StateStorageCapacity);
        private readonly StringDictionary<float> _float = new(StateStorageCapacity);
        private readonly StringDictionary<float2> _float2 = new(StateStorageCapacity);
        private readonly StringDictionary<float3> _float3 = new(StateStorageCapacity);
        private readonly StringDictionary<float4> _float4 = new(StateStorageCapacity);
        private readonly StringDictionary<string> _string = new(StateStorageCapacity);

        public IEnumerable<KeyValuePair<string, float>> ReadAllFloat() => _ReadAll(_float);

        public IEnumerable<KeyValuePair<string, int>> ReadAllInt() => _ReadAll(_int);

        public IEnumerable<KeyValuePair<string, float2>> ReadAllFloat2() => _ReadAll(_float2);

        public IEnumerable<KeyValuePair<string, float3>> ReadAllFloat3() => _ReadAll(_float3);

        public IEnumerable<KeyValuePair<string, float4>> ReadAllFloat4() => _ReadAll(_float4);

        public IEnumerable<KeyValuePair<string, string>> ReadAllString() => _ReadAll(_string);

        public void Write(string id, float value) => _Write(_float, id, value);

        public void Write(string id, int value) => _Write(_int, id, value);

        public void Write(string id, float2 value) => _Write(_float2, id, value);
        
        public void Write(string id, float3 value) => _Write(_float3, id, value);
        
        public void Write(string id, float4 value) => _Write(_float4, id, value);
        
        public void Write(string id, string value) => _Write(_string, id, value);

        private IEnumerable<KeyValuePair<string, T>> _ReadAll<T>(StringDictionary<T> storage)
        {
            return storage.ReadAll();
        }

        private void _Write<T>(StringDictionary<T> storage, string id, T value)
        {
            storage.Write(id, value);
        }
    }
}