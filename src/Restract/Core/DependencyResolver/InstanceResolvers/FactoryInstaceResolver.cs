namespace Restract.Core.DependencyResolver.InstanceResolvers
{
    using System;
    using System.Reflection;

    public class FactoryInstaceResolver : BaseInstaceResolver
    {
        private readonly Func<IDependencyResolver, object> _factory;
        private readonly IDependencyResolver _dependencyResolver;
        public FactoryInstaceResolver(Func<IDependencyResolver, object> factory, IDependencyResolver dependencyResolver)
        {
            _factory = factory;
            _dependencyResolver = dependencyResolver;
        }

        public override object Resolve()
        {
            return _factory(_dependencyResolver);
        }

        public override Type GetObjectType()
        {
            return _factory.GetMethodInfo().ReturnType;
        }
    }
}