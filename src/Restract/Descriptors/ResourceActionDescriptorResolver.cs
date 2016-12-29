namespace Restract.Descriptors
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Restract.Contract;
    using Restract.Contract.Attributes;
    using Restract.Contract.Parameter;
    using Restract.Core;
    using Restract.Execution;
    using Restract.Helpers;

    public class ResourceActionDescriptorResolver : IResourceActionDescriptorResolver
    {
        private readonly IAttributeFinder _attributeFinder;
        private readonly ITypeActivator _typeActivator;
        private readonly IActionResultDataTypeResolver _actionResultDataTypeResolver;

        public ResourceActionDescriptorResolver(IAttributeFinder attributeFinder, IActionResultDataTypeResolver actionResultDataTypeResolver, ITypeActivator typeActivator)
        {
            _attributeFinder = attributeFinder;
            _actionResultDataTypeResolver = actionResultDataTypeResolver;
            _typeActivator = typeActivator;
        }

        public virtual IResourceActionDescriptor Resolve(MethodInfo methodInfo, IResourceDescriptor resourceDescriptor)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            if (resourceDescriptor == null)
                throw new ArgumentNullException(nameof(resourceDescriptor));

            var resourceActionDescriptor = new ResourceActionDescriptor(resourceDescriptor);

            var resourceActionAttribute = _attributeFinder.GetAttributes<ResourceActionAttribute>(methodInfo).FirstOrDefault();

            resourceActionDescriptor.Parameters = resourceDescriptor.Parameters.Clone();

            resourceActionDescriptor.Method = GetMethod(methodInfo, resourceActionAttribute);

            var actionPath = GetPath(methodInfo, resourceActionAttribute);

            foreach (var uriParameter in actionPath.ParameterNames)
            { //check for parameter existance
                resourceActionDescriptor.Parameters.Add(new HttpParameter()
                {
                    Name = uriParameter,
                    Type = HttpParameterType.Uri
                });
            }

            resourceActionDescriptor.ActionPath = TemplateUri.Append(resourceDescriptor.ResourceUrl, actionPath);

            AddMethodParameters(resourceActionDescriptor.Parameters, methodInfo);
            AddArgumentParameters(resourceActionDescriptor.Parameters, methodInfo);

            resourceActionDescriptor.ResultDataType = _actionResultDataTypeResolver.Resolve(methodInfo);

            return resourceActionDescriptor;
        }

        protected virtual TemplateUri GetPath(MethodInfo methodInfo, ResourceActionAttribute resourceActionAttribute)
        {
            return new TemplateUri(resourceActionAttribute?.Path ?? "");
        }

        protected virtual HttpMethod GetMethod(MethodInfo methodInfo, ResourceActionAttribute resourceActionAttribute)
        {
            HttpMethod method;
            if (resourceActionAttribute != null)
            {
                method = resourceActionAttribute.Method;
            }
            else
            {
                var methodName = methodInfo.Name.ToUpper();
                if (methodName.EndsWith("ASYNC"))
                {
                    if (typeof(Task).GetTypeInfo().IsAssignableFrom(methodInfo.ReturnType))
                    {
                        methodName = methodName.Substring(0, methodName.Length - 5);
                    }
                }
                method = new HttpMethod(methodName);
            }

            return method;
        }

        protected virtual void AddMethodParameters(HttpParameterCollection parameters, MethodInfo methodInfo)
        {
            var paramterAttributes = _attributeFinder.GetAttributes<ParameterBindingAttribute>(methodInfo);

            foreach (var parameterBinding in paramterAttributes)
            {
                var parameter = new HttpParameter()
                {
                    Name = parameterBinding.Name,
                    Type = parameterBinding.ParameterType
                };

                if (parameterBinding.Value != null)
                {
                    parameter.ValueResolver = new FixValueResolver(parameterBinding.Value);
                }
                else if (parameterBinding.ValueResolver != null)
                {
                    parameter.ValueResolver = (ParameterValueResolver)_typeActivator.Activate(parameterBinding.ValueResolver);
                    if (parameter.ValueResolver == null)
                    {
                        throw new InvalidOperationException($"Cannot activate ValueResolver type {parameterBinding.ValueResolver} for action parameter. Resource: {methodInfo.DeclaringType}, Action: {methodInfo}, Parameter: {parameterBinding.Name}");
                    }
                }
                AddOrMergeParameter(parameters, parameter);
            }
        }

        protected virtual void AddArgumentParameters(HttpParameterCollection parameters, MethodInfo methodInfo)
        {
            int parameterIndex = -1;
            foreach (var methodParameter in methodInfo.GetParameters())
            {
                parameterIndex++;
                var parameterBinding = GetParameterBindingType(methodParameter);
                if (parameterBinding.ParameterType == HttpParameterType.None)
                {
                    continue;
                }

                if (parameterBinding.ValueResolver != null)
                {
                    throw new InvalidOperationException("Cannot have custom value resolver for action method arguments");
                }

                var parameter = new HttpParameter()
                {
                    Name = parameterBinding.Name,
                    Type = parameterBinding.ParameterType,
                    ValueResolver = new ArgumentValueResolver(parameterIndex)
                };

                AddOrMergeParameter(parameters, parameter);
            }
        }

        private static void AddOrMergeParameter(HttpParameterCollection parameters, HttpParameter parameter)
        {
            var param = parameters.SingleOrDefault(p => p.Name == parameter.Name && p.Type == parameter.Type);
            if (param != null)
            {
                param.ValueResolver = parameter.ValueResolver;
            }
            else
            {
                parameters.Add(parameter);
            }
        }

        protected virtual ParameterBindingAttribute GetParameterBindingType(ParameterInfo parameterInfo)
        {
            var parameterBinding = _attributeFinder.GetAttributes<ParameterBindingAttribute>(parameterInfo).FirstOrDefault();
            if (parameterBinding != null)
            {
                if (string.IsNullOrWhiteSpace(parameterBinding.Name))
                {
                    parameterBinding.Name = parameterInfo.Name;
                }
                return parameterBinding;
            }
            if (TypeHelper.CanConvertFromString(parameterInfo.ParameterType))
            {
                return new UriAttribute(parameterInfo.Name);
            }

            if (parameterInfo.ParameterType == typeof(CancellationToken))
            {
                return new ParameterBindingAttribute(HttpParameterType.CancellationToken, parameterInfo.Name);
            }
            return new BodyAttribute(parameterInfo.Name);
        }
    }
}