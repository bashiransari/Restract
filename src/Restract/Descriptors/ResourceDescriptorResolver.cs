namespace Restract.Descriptors
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Restract.Core;
    using System.Collections.Generic;
    using Restract.Contract;
    using Restract.Contract.Attributes;
    using Restract.Contract.Parameter;
    using Restract.Execution;

    public class ResourceDescriptorResolver : IResourceDescriptorResolver
    {
        private readonly IAttributeFinder _attributeFinder;
        private readonly ITypeActivator  _typeActivator;
        private readonly IResourceActionDescriptorResolver _resourceActionDescriptorResolver;

        public ResourceDescriptorResolver(IAttributeFinder attributeFinder, ITypeActivator typeActivator, IResourceActionDescriptorResolver resourceActionDescriptorResolver)
        {
            _attributeFinder = attributeFinder;
            _typeActivator = typeActivator;
            _resourceActionDescriptorResolver = resourceActionDescriptorResolver;
        }

        public virtual IResourceDescriptor Resolve(Type resourceType)
        {
            if (resourceType == null)
                throw new ArgumentNullException(nameof(resourceType));

            var resourceAttribute = _attributeFinder.GetAttributes<ResourceAttribute>(resourceType.GetTypeInfo()).FirstOrDefault();
            var resourceDescriptor = new ResourceDescriptor(_resourceActionDescriptorResolver);

            var template = (resourceAttribute == null ? resourceType.Name : resourceAttribute.ResourcePath);
            resourceDescriptor.ResourceUrl = new TemplateUri(template);

            resourceDescriptor.Parameters = FindResourceParameters(resourceType, resourceDescriptor.ResourceUrl.ParameterNames);
            return resourceDescriptor;
        }

        protected virtual HttpParameterCollection FindResourceParameters(Type resourceType, HashSet<string> uriParameters)
        {
            var paramterBindings = _attributeFinder.GetAttributes<ParameterBindingAttribute>(resourceType.GetTypeInfo());
            var parameters = new HttpParameterCollection();

            foreach (var uriParameter in uriParameters)
            {
                parameters.Add(new HttpParameter()
                {
                    Name = uriParameter,
                    Type = HttpParameterType.Uri
                });
            }

            foreach (var parameterBinding in paramterBindings)
            {
                HttpParameter parameter = null;
                if (parameterBinding.ParameterType == HttpParameterType.Uri)
                {
                    parameter = parameters.FirstOrDefault(p => p.Name == parameterBinding.Name && p.Type == HttpParameterType.Uri);
                }

                if (parameter == null)
                {
                    parameter = new HttpParameter() { Name = parameterBinding.Name, Type = parameterBinding.ParameterType };
                    parameters.Add(parameter);
                }

                if (parameterBinding.Value != null)
                {
                    parameter.ValueResolver = new FixValueResolver(parameterBinding.Value);
                }

                if (parameterBinding.ValueResolver != null)
                {
                    parameter.ValueResolver = (ParameterValueResolver)_typeActivator.Activate(parameterBinding.ValueResolver);
                    if (parameter.ValueResolver == null)
                    {
                        throw new InvalidOperationException($"Cannot activate ValueResolver type {parameterBinding.ValueResolver} for resource parameter. Resource: {resourceType}, Parameter: {parameterBinding.Name}");
                    }
                }
            }
            return parameters;
        }
    }

}