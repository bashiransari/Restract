namespace Restract.Core.Proxy
{
    public class CrossPlatformProxyGenerator : IProxyGenerator
    {
        public T GetProxy<T>(IProxyInterceptor interceptor) where T : class
        {
#if NETSTANDARD1_6
            IProxyGenerator proxyGenetaor = new RoslynProxy.RoslynProxyGenerator();
#else
            IProxyGenerator proxyGenetaor = new RemotingProxy.RemotingProxyGenerator();
#endif
            return proxyGenetaor.GetProxy<T>(interceptor);
        }
    }
}