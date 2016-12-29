namespace Restract.Core
{
    using System;

    internal class FactoryFunctionTypeActivator : ITypeActivator
    {
        private readonly Func<Type, object> _factory;

        public FactoryFunctionTypeActivator(Func<Type, object> factory)
        {
            _factory = factory;
        }

        public object Activate(Type type)
        {
            return _factory(type);
        }
    }
}