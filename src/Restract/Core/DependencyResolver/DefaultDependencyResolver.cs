namespace Restract.Core.DependencyResolver
{
    using System;
    using System.Reflection;
    using System.Collections.Concurrent;

    public class DefaultDependencyResolver : IDependencyResolver
    {
        private readonly ConcurrentDictionary<Type, IInstanceResolver> _services = new ConcurrentDictionary<Type, IInstanceResolver>();

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            if (!_services.ContainsKey(type))
            {
                throw new InvalidOperationException($"{type.FullName} is not registered.");
            }
            return _services[type].Resolve();
        }

        public void Add(Type serviceType, IInstanceResolver resolver, ServiceLifetime lifetime)
        {
            if (!serviceType.GetTypeInfo().IsAssignableFrom(resolver.GetObjectType()))
            {
                throw new InvalidOperationException($"{resolver.GetObjectType().FullName} is not assignable to ${serviceType.FullName}");
            }

            if (lifetime == ServiceLifetime.Singleton)
            {
                resolver = resolver.AsSingletone();
            }
            _services[serviceType] = resolver;
        }

    }
}