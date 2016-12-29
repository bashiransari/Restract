namespace Restract.Core.DependencyResolver
{
    using System;
    using Restract.Core.DependencyResolver.InstanceResolvers;

    public static class DependencyResolverExtensions
    {
        public static IDependencyResolver AddSingletone<TService, TImplementation>(this IDependencyResolver dependencyResolver) where TImplementation : TService
        {
            dependencyResolver.Add(typeof(TService), new TypeInstaceResolver(typeof(TImplementation), dependencyResolver), ServiceLifetime.Singleton);
            return dependencyResolver;
        }

        public static IDependencyResolver AddSingletone<TService>(this IDependencyResolver dependencyResolver, TService serviceInstance)
        {
            dependencyResolver.Add(typeof(TService), new StaticInstaceResolver(serviceInstance), ServiceLifetime.Singleton);
            return dependencyResolver;
        }

        public static IDependencyResolver AddSingletone<TService>(this IDependencyResolver dependencyResolver, Func<IDependencyResolver, TService> serviceFactory) where TService : class
        {
            dependencyResolver.Add(typeof(TService), new FactoryInstaceResolver(serviceFactory, dependencyResolver), ServiceLifetime.Singleton);
            return dependencyResolver;
        }

        public static IDependencyResolver AddTransient<TService, TImplementation>(this IDependencyResolver dependencyResolver) where TImplementation : TService
        {
            dependencyResolver.Add(typeof(TService), new TypeInstaceResolver(typeof(TImplementation), dependencyResolver), ServiceLifetime.Transient);
            return dependencyResolver;
        }

        public static IDependencyResolver AddTransient<TService>(this IDependencyResolver dependencyResolver, Func<IDependencyResolver, TService> serviceFactory) where TService : class
        {
            dependencyResolver.Add(typeof(TService), new FactoryInstaceResolver(serviceFactory, dependencyResolver), ServiceLifetime.Transient);
            return dependencyResolver;
        }
    }
}