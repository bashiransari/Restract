namespace Restract.Serialization
{
    using System;
    using Newtonsoft.Json;

    public class NewtonsoftJsonSerializer : ISerializer
    {
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public T Deserialize<T>(string objString) where T : new()
        {
            return JsonConvert.DeserializeObject<T>(objString);
        }

        public object Deserialize(string objString, Type objectType)
        {
            return JsonConvert.DeserializeObject(objString, objectType);
        }
    }
}