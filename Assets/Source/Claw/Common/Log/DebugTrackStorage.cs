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

        public IEnumerable<KeyValuePair<string, float>> ReadAllFloat() => ReadAll(_float);

        public IEnumerable<KeyValuePair<string, int>> ReadAllInt() => ReadAll(_int);

        public IEnumerable<KeyValuePair<string, float2>> ReadAllFloat2() => ReadAll(_float2);

        public IEnumerable<KeyValuePair<string, float3>> ReadAllFloat3() => ReadAll(_float3);

        public IEnumerable<KeyValuePair<string, float4>> ReadAllFloat4() => ReadAll(_float4);

        public IEnumerable<KeyValuePair<string, string>> ReadAllString() => ReadAll(_string);

        public void WriteFloat(string id, float value) => Write(_float, id, value);

        public void WriteInt(string id, int value) => Write(_int, id, value);

        public void WriteFloat2(string id, float2 value) => Write(_float2, id, value);
        
        public void WriteFloat3(string id, float3 value) => Write(_float3, id, value);
        
        public void WriteFloat4(string id, float4 value) => Write(_float4, id, value);
        
        public void WriteString(string id, string value) => Write(_string, id, value);

        private IEnumerable<KeyValuePair<string, T>> ReadAll<T>(StringDictionary<T> storage)
        {
            return storage.ReadAll();
        }

        private void Write<T>(StringDictionary<T> storage, string id, T value)
        {
            storage.Write(id, value);
        }
    }
}