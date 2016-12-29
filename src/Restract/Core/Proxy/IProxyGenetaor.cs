namespace Restract.Core.Proxy
{
    public interface IProxyGenerator
    {
        T GetProxy<T>(IProxyInterceptor interceptor) where T : class;
    }
}