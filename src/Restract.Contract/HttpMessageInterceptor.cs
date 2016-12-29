namespace Restract.Contract
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class HttpMessageInterceptor : DelegatingHandler
    {
        protected sealed override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var methodCallInfo = request.Properties["MethodCallInfo"] as MethodCallInfo;
            var resourceActionDescriptor = request.Properties["ResourceActionDescriptor"] as IResourceActionDescriptor;
            return SendAsync(request, resourceActionDescriptor, methodCallInfo, cancellationToken);
        }


        public virtual Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            IResourceActionDescriptor resourceActionDescriptor,
            MethodCallInfo methodCallInfo,
            CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken);
        }
    }

}