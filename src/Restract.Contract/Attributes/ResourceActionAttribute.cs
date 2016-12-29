namespace Restract.Contract.Attributes
{
    using System;
    using System.Net.Http;

    public class ResourceActionAttribute : Attribute
    {
        public HttpMethod Method { get; set; }
        public string MethodName { get; set; }
        public string Path { get; set; }

        public ResourceActionAttribute(HttpMethod method)
        {
            Method = method;
        }

        public ResourceActionAttribute(Method method)
            : this(new HttpMethod(method.ToString()))
        {
        }

        public ResourceActionAttribute(HttpMethod method, string path)
        {
            Method = method;
            Path = path;
        }

        public ResourceActionAttribute(Method method, string path)
            : this(new HttpMethod(method.ToString()), path)
        {
        }

        public ResourceActionAttribute(string path)
        {
            Path = path;
        }
    }
}