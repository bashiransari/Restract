namespace Restract.Serialization
{
    using System;

    public interface ISerializer
    {
        string Serialize(object obj);
        T Deserialize<T>(string objString) where T : new();
        object Deserialize(string objString, Type objectType);
    }
}