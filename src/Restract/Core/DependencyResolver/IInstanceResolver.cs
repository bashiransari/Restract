namespace Restract.Core.DependencyResolver
{
    using System;

    public interface IInstanceResolver
    {
        T Resolve<T>();
        object Resolve();

        Type GetObjectType();
    }
}