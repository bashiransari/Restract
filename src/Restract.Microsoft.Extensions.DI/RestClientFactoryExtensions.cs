using System;
using Restract.Core;
using Restract.Core.DependencyResolver;

namespace Restract.Microsoft.Extensions.DI
{
    public static class RestClientFactoryExtensions
    {
        public static void UseServiceProvider(this RestClientFactory restClientFactory, IServiceProvider serviceProvider)
        {
            restClientFactory.DependencyResolver.AddSingletone<ITypeActivator>(new ServiceProviderTypeActivator(serviceProvider));
        }
    }
}