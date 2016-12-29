namespace Restract.Core.Proxy
{
    using System.Reflection;

    public interface IProxyInterceptor
    {
        object OnMethodCall(MethodInfo mi, object[] args);

        RestClientConfiguration RestClientConfiguration { get; set; }
    }
}