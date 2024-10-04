using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;

namespace Beast.Claw.Serialization.Json
{
    public class BeastJsonFloat3Converter : JsonConverter
    {
        public override bool CanRead => true;
        
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(float3);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            float3 vector = (float3)value;
            
            writer.WriteStartObject();
            
            writer.WritePropertyName("x");
            writer.WriteValue(vector.x);
            
            writer.WritePropertyName("y");
            writer.WriteValue(vector.y);
            
            writer.WritePropertyName("z");
            writer.WriteValue(vector.z);
            
            writer.WriteEnd();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            float x = jObject.Property("x")?.Value.ToObject<float>() ?? 0;
            float y = jObject.Property("y")?.Value.ToObject<float>() ?? 0;
            float z = jObject.Property("z")?.Value.ToObject<float>() ?? 0;

            return new float3(x, y, z);
        }
    }
}