#if NETSTANDARD1_6
#else
namespace Restract.Core.Proxy.RemotingProxy
{

    public class RemotingProxyGenerator : IProxyGenerator
    {
        public T GetProxy<T>(IProxyInterceptor interceptor) where T : class
        {
            var dynamicProxy = new RemotingProxy<T>();
            return dynamicProxy.GetProxy(interceptor);
        }
    }
}
#endif