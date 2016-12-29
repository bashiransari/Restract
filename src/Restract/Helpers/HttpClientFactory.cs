namespace Restract.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;

    //Copied form System.Net.Http.Formatting(5.2.3.0)
    //Only exception messages customized
    public static class HttpClientFactory
    {
        public static HttpClient Create(params DelegatingHandler[] handlers)
        {
            return Create(new HttpClientHandler(), handlers);
        }

        public static HttpClient Create(HttpMessageHandler innerHandler, params DelegatingHandler[] handlers)
        {
            HttpMessageHandler handler = CreatePipeline(innerHandler, handlers);
            return new HttpClient(handler);
        }

        public static HttpMessageHandler CreatePipeline(HttpMessageHandler innerHandler, IEnumerable<DelegatingHandler> handlers)
        {
            if (innerHandler == null)
            {
                throw new ArgumentNullException(nameof(innerHandler));
            }
            if (handlers == null)
            {
                return innerHandler;
            }
            var httpMessageHandler = innerHandler;
            var enumerable = handlers.Reverse();
            foreach (var current in enumerable)
            {
                if (current == null)
                {
                    throw new ArgumentException("HttpMessageInterceptors array containes null item", nameof(handlers));
                }
                if (current.InnerHandler != null)
                {
                    throw new ArgumentException($"HttpMessageInterceptor {current.GetType().Name} has already used in another factory. To use an intercetor in multiple factories you should add intercetor using it's type or a factory method using RestClientConfiguration.Add() method.", nameof(handlers));
                }
                current.InnerHandler = httpMessageHandler;
                httpMessageHandler = current;
            }
            return httpMessageHandler;
        }
    }
}
