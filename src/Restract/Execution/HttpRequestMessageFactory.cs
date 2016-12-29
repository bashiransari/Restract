namespace Restract.Execution
{
    using Restract.Serialization;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using Restract.Contract;
    using Restract.Contract.Parameter;

    public class HttpRequestMessageFactory : IHttpRequestMessageFactory
    {
        private readonly ISerializer _serializer;

        public HttpRequestMessageFactory(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public virtual HttpRequestMessage GetHttpRequest(IResourceActionDescriptor resourceActionDescriptor,
                                                 MethodCallInfo methodCallInfo, RestClientConfiguration restClientConfiguration)
        {
            var request = new HttpRequestMessage
            {
                Method = resourceActionDescriptor.Method
            };

            //Set request body
            var bodyParameter = resourceActionDescriptor.Parameters.SingleOrDefault(p => p.Type == HttpParameterType.Body);

            if (bodyParameter != null)
            {
                var bodyContent = bodyParameter.ValueResolver.GetValue(bodyParameter.Name, methodCallInfo);

                var requestContent = _serializer.Serialize(bodyContent);
                request.Content = new StringContent(requestContent, Encoding.UTF8, "application/json");
            }

            //Set request uri
            var actionUrlParameters = resourceActionDescriptor.Parameters.Uris;

            var urlParameterValues =
                actionUrlParameters
                    .ToDictionary(
                        p => p.Name,
                        p => p.ValueResolver.GetValue(p.Name, methodCallInfo)?.ToString());

            var url = resourceActionDescriptor.ActionPath.InjectParameterValues(urlParameterValues);

            var baseUri = restClientConfiguration.BaseUrl.ToString();

            if (!string.IsNullOrWhiteSpace(url))
            {
                if (url.StartsWith("/"))
                {
                    url = url.Substring(1);
                }

                if (!baseUri.EndsWith("/"))
                {
                    baseUri += "/";
                }
            }

            request.RequestUri = new Uri(baseUri + url);

            //Set request headers
            foreach (var header in resourceActionDescriptor.Parameters.Headers)
            {
                request.Headers.Add(header.Name, header.ValueResolver.GetValue(header.Name, methodCallInfo).ToString());
            }

            //Set request cookies
            var cookieStringBuilder = new StringBuilder();
            foreach (var cookie in resourceActionDescriptor.Parameters.Cookies)
            {
                cookieStringBuilder.Append($"{cookie.Name}={cookie.ValueResolver.GetValue(cookie.Name, methodCallInfo)}; ");
            }

            request.Headers.Add("Cookie", cookieStringBuilder.ToString());

            return request;
        }
    }
}