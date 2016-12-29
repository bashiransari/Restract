namespace Restract
{
    using System;
    using System.Collections.Concurrent;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Restract.Contract;
    using Restract.Core.Proxy;
    using Restract.Descriptors;
    using Restract.Execution;

    internal class RestractProxyInterceptor : IProxyInterceptor
    {
        private readonly IActionResultBuilder _actionResultBuilder;
        private readonly IHttpRequestMessageFactory _httpRequestMessageFactory;
        private readonly IResourceDescriptorResolver _resourceDescriptorResolver;
        private static readonly ConcurrentDictionary<Type, Lazy<IResourceDescriptor>> ResourceDescriptors = new ConcurrentDictionary<Type, Lazy<IResourceDescriptor>>();

        public RestClientConfiguration RestClientConfiguration { get; set; }

        private readonly HttpClient _httpClient;

        public RestractProxyInterceptor(HttpClient httpClient, IActionResultBuilder actionResultBuilder, IResourceDescriptorResolver resourceDescriptorResolver, IHttpRequestMessageFactory httpRequestMessageFactory)
        {
            _httpClient = httpClient;
            _actionResultBuilder = actionResultBuilder;
            _resourceDescriptorResolver = resourceDescriptorResolver;
            _httpRequestMessageFactory = httpRequestMessageFactory;
        }

        public object OnMethodCall(MethodInfo methodInfo, object[] args)
        {
            var resourceInfo = GetResourceDescriptor(methodInfo.DeclaringType);
            var actionInfo = resourceInfo.GetActionDescriptor(methodInfo);

            var methodCallInfo = new MethodCallInfo()
            {
                MethodInfo = methodInfo,
                Arguments = args
            };

            var httpReqeust = _httpRequestMessageFactory.GetHttpRequest(actionInfo, methodCallInfo, RestClientConfiguration);

            var response = Execute(methodCallInfo, httpReqeust, actionInfo);
            var res = _actionResultBuilder.BuildActionResult(actionInfo.ResultDataType, response);
            return res;
        }

        private IResourceDescriptor GetResourceDescriptor(Type resourceType)
        {
            var resourceDescriptor = ResourceDescriptors.GetOrAdd(resourceType,
                (type) => new Lazy<IResourceDescriptor>(() => _resourceDescriptorResolver.Resolve(type)));

            return resourceDescriptor.Value;
        }

        private Task<HttpResponseMessage> Execute(MethodCallInfo methodCallInfo, HttpRequestMessage request, IResourceActionDescriptor resourceActionDescriptor)
        {
            request.Properties.Add("MethodCallInfo", methodCallInfo);
            request.Properties.Add("ResourceActionDescriptor", resourceActionDescriptor);

            var cancellationToken = resourceActionDescriptor.Parameters.CancellationToken?.GetValue<CancellationToken>(methodCallInfo);

            var response = _httpClient.SendAsync(request, cancellationToken ?? CancellationToken.None);
            return response;
        }
    }
}