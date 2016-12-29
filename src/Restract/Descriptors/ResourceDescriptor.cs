namespace Restract.Descriptors
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;
    using Restract.Contract;
    using Restract.Contract.Parameter;

    public class ResourceDescriptor : IResourceDescriptor
    {
        private readonly IResourceActionDescriptorResolver _resourceActionDescriptorResolver;
        private readonly ConcurrentDictionary<MethodInfo, Lazy<IResourceActionDescriptor>> _resourceActionDescriptors;

        public TemplateUri ResourceUrl { get; set; }

        public HttpParameterCollection Parameters { get; set; }

        public ResourceDescriptor(IResourceActionDescriptorResolver resourceActionDescriptorResolver)
        {
            _resourceActionDescriptorResolver = resourceActionDescriptorResolver;
            _resourceActionDescriptors = new ConcurrentDictionary<MethodInfo, Lazy<IResourceActionDescriptor>>();
            Parameters = new HttpParameterCollection();
        }

        public IResourceActionDescriptor GetActionDescriptor(MethodInfo methodInfo)
        {
            var actionDescriptor = _resourceActionDescriptors.GetOrAdd(methodInfo,
                (mi) => new Lazy<IResourceActionDescriptor>(() => _resourceActionDescriptorResolver.Resolve(mi, this)));

            return actionDescriptor.Value;
        }
    }
}