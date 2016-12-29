namespace Restract.Tests
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using Moq;
    using NUnit.Framework;
    using Restract.Contract;
    using Restract.Contract.Parameter;
    using Restract.Descriptors;
    using Restract.Execution;
    using Restract.Serialization;

    [TestFixture]
    public class HttpRequestMessageFactoryTests
    {
        private HttpRequestMessageFactory _httpRequestMessageFactory;
        private Mock<ISerializer> _serializer;
        private ResourceActionDescriptor _actionDescriptor;
        private MethodCallInfo _methodCall;
        private RestClientConfiguration _configuration;
        [SetUp]
        public void SetUp()
        {
            _serializer = new Mock<ISerializer>();
            _httpRequestMessageFactory = new HttpRequestMessageFactory(_serializer.Object);

            _actionDescriptor = new ResourceActionDescriptor(null)
            {
                Method = new HttpMethod("GET"),
                Parameters = new HttpParameterCollection(),
                ActionPath = new TemplateUri("http://localhost"),
                ResultDataType = new ActionResultDataType()
            };

            _methodCall = new MethodCallInfo();
            _configuration = new RestClientConfiguration("http://localhost");
            //var methodInfo = new Mock<MethodInfo>();
        }

        [Test]
        public void HttpRequestMethodShouldBeTheSameAsActionDescriptorMethod()
        {
            var result = _httpRequestMessageFactory.GetHttpRequest(_actionDescriptor, _methodCall, _configuration);
            Assert.AreEqual(_actionDescriptor.Method, result.Method);
        }


        [Test]
        public void GivenThereIsNoBodyParameter_WhenGethttpRequestInvoked_ThenRequestContentShouldBeNull()
        {
            var result = _httpRequestMessageFactory.GetHttpRequest(_actionDescriptor, _methodCall, _configuration);
            Assert.IsNull(result.Content);
        }

        [Test]
        public void RequestUrlShouldBeBaseUrlAndActionPath()
        {
            _actionDescriptor.Parameters.Add(new HttpParameter()
            {
                Type = HttpParameterType.Body,
                ValueResolver = new FixValueResolver("Test")
            });

            _serializer.Setup(p => p.Serialize("Test")).Returns("SerializedTest");

            var result = _httpRequestMessageFactory.GetHttpRequest(_actionDescriptor, _methodCall, _configuration);

            var stringContent = result.Content as StringContent;

            Assert.IsNotNull(stringContent);
            Assert.AreEqual("SerializedTest", stringContent.ReadAsStringAsync().Result);
        }

        [TestCase("http://myServer.com/", "/resource/action", "http://myServer.com/resource/action")]
        [TestCase("http://myServer.com", "/resource/action", "http://myServer.com/resource/action")]
        [TestCase("http://myServer.com", "resource/action", "http://myServer.com/resource/action")]
        [TestCase("http://myServer.com/", "resource/action", "http://myServer.com/resource/action")]
        public void GivenThereIsABodyParameter_WhenGethttpRequestInvoked_ThenRequestContentShouldBeBodyParameterValue(string baseUrl, string actionPath, string requestUri)
        {
            _configuration.BaseUrl = new Uri(baseUrl);
            _actionDescriptor.ActionPath = new TemplateUri(actionPath);

            var result = _httpRequestMessageFactory.GetHttpRequest(_actionDescriptor, _methodCall, _configuration);

            Assert.AreEqual(requestUri.ToLower(), result.RequestUri.ToString());
        }

        [Test]
        public void GivenThereAreHeaderParameters_WhenGethttpRequestInvoked_ThenResultShouldContainHeaders()
        {
            _actionDescriptor.Parameters.Add(new HttpParameter("Header1", HttpParameterType.Header, new FixValueResolver("Test1")));
            _actionDescriptor.Parameters.Add(new HttpParameter("Header2", HttpParameterType.Header, new FixValueResolver("Test2")));

            var result = _httpRequestMessageFactory.GetHttpRequest(_actionDescriptor, _methodCall, _configuration);

            Assert.IsTrue(result.Headers.Contains("Header1"));
            Assert.IsTrue(result.Headers.Contains("Header2"));

            Assert.AreEqual("Test1", result.Headers.GetValues("Header1").Single());
            Assert.AreEqual("Test2", result.Headers.GetValues("Header2").Single());
        }

        [Test]
        public void GivenThereAreUriParameters_WhenGetHttpRequestInvoked_ThenParameterValuesShouldBeInjectedIntoRequestUri()
        {
            _actionDescriptor.ActionPath= new TemplateUri("{param1}/{param2}/items");

            _actionDescriptor.Parameters.Add(new HttpParameter("param1", HttpParameterType.Uri, new FixValueResolver("Test1")));
            _actionDescriptor.Parameters.Add(new HttpParameter("param2", HttpParameterType.Uri, new FixValueResolver("Test2")));

            var result = _httpRequestMessageFactory.GetHttpRequest(_actionDescriptor, _methodCall, _configuration);

            Assert.AreEqual("http://localhost/Test1/Test2/items", result.RequestUri.ToString());
        }

        [Test]
        public void GivenThereAreCookieParameters_WhenGetHttpRequestInvoked_ThenParameterValuesShouldBeInjectedIntoCookieHeader()
        {
            _actionDescriptor.Parameters.Add(new HttpParameter("cookie1", HttpParameterType.Cookie, new FixValueResolver("Test1")));
            _actionDescriptor.Parameters.Add(new HttpParameter("cookie2", HttpParameterType.Cookie, new FixValueResolver("Test2")));

            var result = _httpRequestMessageFactory.GetHttpRequest(_actionDescriptor, _methodCall, _configuration);

            Assert.AreEqual("cookie1=Test1; cookie2=Test2; ", result.Headers.GetValues("Cookie").Single());
        }

    }
}
