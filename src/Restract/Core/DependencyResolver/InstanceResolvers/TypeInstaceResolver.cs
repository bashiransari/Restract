namespace Restract.Core.DependencyResolver.InstanceResolvers
{
    using System;
    using System.Linq;
    using System.Reflection;

    public class TypeInstaceResolver : BaseInstaceResolver
    {
        private readonly Type _destinationtype;
        private readonly IDependencyResolver _dependencyResolver;
        public TypeInstaceResolver(Type destinationtype, IDependencyResolver dependencyResolver)
        {
            _destinationtype = destinationtype;
            _dependencyResolver = dependencyResolver;
        }

        public override object Resolve()
        {
            var ctor = _destinationtype.GetTypeInfo().GetConstructors().OrderBy(p => p.GetParameters().Length).Last();

            var paramValues = new object[ctor.GetParameters().Length];

            var i = 0;

            foreach (var param in ctor.GetParameters())
            {
                try
                {
                    paramValues[i++] = _dependencyResolver.Resolve(param.ParameterType);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Unable to resolve service for type '{param.ParameterType}' while attempting to activate '{_destinationtype}'.", ex);
                }
            }

            return Activator.CreateInstance(_destinationtype, paramValues);
        }

        public override Type GetObjectType()
        {
            return _destinationtype;
        }
    }
}