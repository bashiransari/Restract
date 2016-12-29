using System;

namespace Restract.Core
{
    public interface ITypeActivator
    {
        object Activate(Type type);
    }
}