namespace Restract.Descriptors
{
    using System;
    using Restract.Contract;

    public interface IResourceDescriptorResolver
    {
        IResourceDescriptor Resolve(Type resrouceType);
    }
}