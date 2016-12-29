namespace Restract.Execution
{
    using System.Net.Http;
    using Restract.Contract;

    public interface IHttpRequestMessageFactory
    {
        HttpRequestMessage GetHttpRequest(IResourceActionDescriptor resourceActionDescriptor, MethodCallInfo methodCallInfo, RestClientConfiguration restClientConfiguration);
    }
}