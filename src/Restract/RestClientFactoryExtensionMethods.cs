namespace Restract
{
    using System;
    using Core;
    using Core.DependencyResolver;
    using Restract.Core.Proxy;
    using Restract.Core.Proxy.ILProxy;
    using Restract.Core.Proxy.RoslynProxy;

    public static class RestClientFactoryExtensionMethods
    {
        public static RestClientFactory UseTypeActivator(this RestClientFactory restClientFactory, Func<Type, object> factory)
        {
            restClientFactory.DependencyResolver.AddSingletone<ITypeActivator>(new FactoryFunctionTypeActivator(factory));
            return restClientFactory;
        }

        public static RestClientFactory UseTypeActivator(this RestClientFactory restClientFactory, ITypeActivator typeActivator)
        {
            restClientFactory.DependencyResolver.AddSingletone(typeActivator);
            return restClientFactory;
        }

        public static RestClientFactory UseRoslynProxyGenerator(this RestClientFactory restClientFactory)
        {
            restClientFactory.DependencyResolver.AddSingletone<IProxyGenerator, RoslynProxyGenerator>();
            return restClientFactory;
        }

        public static RestClientFactory UseIlProxyGenerator(this RestClientFactory restClientFactory)
        {
            restClientFactory.DependencyResolver.AddSingletone<IProxyGenerator, IlProxyGenerator>();
            return restClientFactory;
        }

#if NET461
        public static RestClientFactory UseRemotingRealProxyGenerator(this RestClientFactory restClientFactory)
        {
            restClientFactory.DependencyResolver.AddSingletone<IProxyGenerator, Core.Proxy.RemotingProxy.RemotingProxyGenerator>();
            return restClientFactory;
        }
#endif
    }
}
