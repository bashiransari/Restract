namespace Restract.Tests
{
    using System;
    using System.Net.Http;
    using System.Reflection;
    using Moq;
    using NUnit.Framework;
    using Restract.Contract;
    using Restract.Descriptors;
    using Restract.Execution;
    using Restract.Tests.Fixtures;

    [TestFixture]
    public class RestractProxyInterceptorTests
    {
        private RestractProxyInterceptor _proxyInterceptor;
        private HttpClient _httpClient;
        private Mock<IActionResultBuilder> _actionResultBuilder;
        private Mock<IResourceDescriptorResolver> _resourceDescriptorResolver;
        private Mock<IHttpRequestMessageFactory> _httpRequestMessageFactory;
        private Mock<IResourceDescriptor> _resrouceDescriptor;
        private Mock<MethodInfo> _methodInfo;

        [SetUp]
        public void SetUp()
        {
            _httpClient = new HttpClient(new MockHttpMessageHandler());
            _actionResultBuilder = new Mock<IActionResultBuilder>();
            _resourceDescriptorResolver = new Mock<IResourceDescriptorResolver>();
            _httpRequestMessageFactory = new Mock<IHttpRequestMessageFactory>();
            _resrouceDescriptor = new Mock<IResourceDescriptor>();
            _methodInfo = new Mock<MethodInfo>();


            _methodInfo.Setup(p => p.DeclaringType).Returns(typeof(string));

            _resourceDescriptorResolver.Setup(p => p.Resolve(It.IsAny<Type>()))
                .Returns(_resrouceDescriptor.Object);

            _resrouceDescriptor.Setup(p => p.GetActionDescriptor(It.IsAny<MethodInfo>()))
                .Returns(new ResourceActionDescriptor(null));

            _httpRequestMessageFactory.Setup(
                p =>
                    p.GetHttpRequest(It.IsAny<ResourceActionDescriptor>(), It.IsAny<MethodCallInfo>(),
                        It.IsAny<RestClientConfiguration>())).Returns(() => new HttpRequestMessage(HttpMethod.Get, "http://localhost"));

            _proxyInterceptor = new RestractProxyInterceptor(_httpClient, _actionResultBuilder.Object, _resourceDescriptorResolver.Object, _httpRequestMessageFactory.Object);
        }

        [Test]
        public void GivenResourceDescriptorHasBeenResolvedOnce_WhenResolvedInvoked_ThenResourceDescriptorResolverShouldNotBeCalled()
        {
            _proxyInterceptor.OnMethodCall(_methodInfo.Object, new object[0]);

            _resourceDescriptorResolver.Verify(p => p.Resolve(It.IsAny<Type>()), Times.Once());

            _resourceDescriptorResolver.Reset();

            _proxyInterceptor.OnMethodCall(_methodInfo.Object, new object[0]);

            _resourceDescriptorResolver.Verify(p => p.Resolve(It.IsAny<Type>()), Times.Never());
        }


    }
}
