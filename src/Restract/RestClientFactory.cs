namespace Restract
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using Core.DependencyResolver;
    using Restract.Descriptors;
    using Restract.Core;
    using Restract.Core.Proxy;
    using Restract.Execution;
    using Restract.Helpers;
    using Restract.Serialization;

    public class RestClientFactory
    {
        public IDependencyResolver DependencyResolver { get; }

        public RestClientConfiguration Configuration { get; }

        private Lazy<HttpClient> _httpClient;
        private HttpClientHandler _httpClientHandler;

        public RestClientFactory(RestClientConfiguration configuration, IDependencyResolver dependencyResolver)
        {
            Configuration = configuration;
            DependencyResolver = dependencyResolver;
            Initialize();
        }

        public RestClientFactory(RestClientConfiguration configuration)
            : this(configuration, new DefaultDependencyResolver())
        {

        }

        public RestClientFactory(string baseUrl)
            : this(new RestClientConfiguration(baseUrl))
        {
        }

        private void Initialize()
        {
            InitDependencyResolver();
            //something like this
            //ServicePointManager.FindServicePoint(_restClientConfiguration.BaseUrl ).RelaseTimeOut = _restClientConfiguration.ConnectionRelasetimeout.
            _httpClient = new Lazy<HttpClient>(CreateClient);
        }

        private void InitDependencyResolver()
        {
            DependencyResolver.AddSingletone<IHttpRequestMessageFactory, HttpRequestMessageFactory>();
            DependencyResolver.AddSingletone<IResourceDescriptorResolver, ResourceDescriptorResolver>();
            DependencyResolver.AddSingletone<IResourceActionDescriptorResolver, ResourceActionDescriptorResolver>();
            DependencyResolver.AddSingletone<ISerializer, NewtonsoftJsonSerializer>();
            DependencyResolver.AddSingletone<IProxyGenerator, CrossPlatformProxyGenerator>();
            DependencyResolver.AddSingletone<IAttributeFinder, DefaultAttributeFinder>();
            DependencyResolver.AddSingletone<ArgumentValueResolver, ArgumentValueResolver>();
            DependencyResolver.AddSingletone<IActionResultDataTypeResolver, ActionResultDataTypeResolver>();
            DependencyResolver.AddSingletone<IActionResultBuilder, ActionResultBuilder>();
            DependencyResolver.AddSingletone<ITypeActivator, DefaultTypeActivator>();
            DependencyResolver.AddSingletone<IInterceptorActivator, InterceptorActivator>();
        }

        private HttpClient CreateClient()
        {
            _httpClientHandler = new HttpClientHandler
            {
                UseCookies = false
            };

            Configuration.HttpClientHandlerCustomiser?.Invoke(_httpClientHandler);

            var interceptorActivator = DependencyResolver.Resolve<IInterceptorActivator>();

            var interceptors = Configuration
                .InterceptorRegistrations
                .Select(interceptorActivator.Activate);

            var httpClient = HttpClientFactory.Create(
                      _httpClientHandler,
                     interceptors.ToArray());

            httpClient.Timeout = TimeSpan.FromMilliseconds(Configuration.Timeout);

            Configuration.HttpClientCustomiser?.Invoke(httpClient);

            return httpClient;
        }

        public TServiceType CreateClient<TServiceType>() where TServiceType : class
        {
            var actionResultBuilder = DependencyResolver.Resolve<IActionResultBuilder>();
            var resourceDescriptorResolver = DependencyResolver.Resolve<IResourceDescriptorResolver>();
            var httpRequestMessageFactory = DependencyResolver.Resolve<IHttpRequestMessageFactory>();

            var proxyInterceptor = new RestractProxyInterceptor(_httpClient.Value, actionResultBuilder, resourceDescriptorResolver, httpRequestMessageFactory)
            {
                RestClientConfiguration = Configuration
            };

            var proxyGenetaor = DependencyResolver.Resolve<IProxyGenerator>();

            var proxy = proxyGenetaor.GetProxy<TServiceType>(proxyInterceptor);

            return proxy;
        }
    }
}
