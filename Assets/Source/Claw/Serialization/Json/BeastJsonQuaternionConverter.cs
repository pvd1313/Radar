using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;

namespace Beast.Claw.Serialization.Json
{
    public class BeastJsonQuaternionConverter : JsonConverter
    {
        public override bool CanRead => true;
        
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(quaternion);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            quaternion q = (quaternion)value;
            
            writer.WriteStartObject();
            
            writer.WritePropertyName("x");
            writer.WriteValue(q.value.x);
            
            writer.WritePropertyName("y");
            writer.WriteValue(q.value.y);
            
            writer.WritePropertyName("z");
            writer.WriteValue(q.value.z);
            
            writer.WritePropertyName("w");
            writer.WriteValue(q.value.w);
            
            writer.WriteEnd();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            float x = jObject.Property("x")?.Value.ToObject<float>() ?? 0;
            float y = jObject.Property("y")?.Value.ToObject<float>() ?? 0;
            float z = jObject.Property("z")?.Value.ToObject<float>() ?? 0;
            float w = jObject.Property("w")?.Value.ToObject<float>() ?? 0;

            return new quaternion(x, y, z, w);
        }
    }
}