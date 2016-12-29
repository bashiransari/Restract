namespace Restract.Core.DependencyResolver.InstanceResolvers
{
    using System;

    public abstract class BaseInstaceResolver : IInstanceResolver
    {
        public T Resolve<T>()
        {
            return (T)Resolve();
        }

        public abstract object Resolve();

        public abstract Type GetObjectType();

    }
}