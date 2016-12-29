namespace Restract.Core.DependencyResolver
{
    using System;

    public interface IDependencyResolver
    {
        T Resolve<T>();

        object Resolve(Type type);
        void Add(Type serviceType, IInstanceResolver resolver, ServiceLifetime lifetime);
    }
}