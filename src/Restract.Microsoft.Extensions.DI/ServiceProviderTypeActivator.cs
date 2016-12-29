using System;
using Restract.Core;

namespace Restract.Microsoft.Extensions.DI
{
    public class ServiceProviderTypeActivator : ITypeActivator
    {
        private readonly IServiceProvider _serviceProvider;
        public ServiceProviderTypeActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object Activate(Type type)
        {
            return _serviceProvider.GetService(type);
        }
    }
}