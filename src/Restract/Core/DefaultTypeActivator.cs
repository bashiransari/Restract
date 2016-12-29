using System;

namespace Restract.Core
{
    internal class DefaultTypeActivator : ITypeActivator
    {
        public object Activate(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}