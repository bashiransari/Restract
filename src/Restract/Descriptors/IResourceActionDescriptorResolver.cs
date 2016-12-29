namespace Restract.Descriptors
{
    using System.Reflection;
    using Restract.Contract;

    public interface IResourceActionDescriptorResolver
    {
        IResourceActionDescriptor Resolve(MethodInfo methodInfo, IResourceDescriptor resourceDescriptor);
    }
}