using System;
using System.Text;
using CreativeMode.Generic;
using CreativeMode.Serializer;
using Newtonsoft.Json;

namespace CreativeMode
{
    public class JsonNetEntitySerializer<T> : IEntitySerializer<T>
    {
        public static readonly JsonNetEntitySerializer<T> Instance 
            = new JsonNetEntitySerializer<T>();
        
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new WhitelistContractResolver()
                .AddUnityTypes()
        };

        private JsonNetEntitySerializer() { }

        public byte[] Serialize(T value)
        {
            return Encoding.Default.GetBytes(JsonConvert.SerializeObject(value, typeof(T), jsonSettings));
        }

        public T Deserialize(byte[] data)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.Default.GetString(data), jsonSettings);
        }
    }
}