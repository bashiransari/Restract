namespace Restract.Tests.Fixtures
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private HttpResponseMessage HttpResponseMessage { get; }

        public MockHttpMessageHandler()
        {
        }

        public MockHttpMessageHandler(HttpResponseMessage httpResponseMessage)
        {
            HttpResponseMessage = httpResponseMessage;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(HttpResponseMessage ?? new HttpResponseMessage());
        }
    }
}