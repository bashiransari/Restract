namespace Restract
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    using Restract.Contract;

    public enum DataFormat
    {
        Json,
        Xml
    }
    public class RestClientConfiguration
    {
        public Uri BaseUrl { get; set; }
        public int Timeout { get; set; }
        internal IList<InterceptorRegistration> InterceptorRegistrations { get; }
        public DataFormat RequestFormat { get; set; }
        public Action<HttpClient> HttpClientCustomiser { get; set; }
        public Action<HttpClientHandler> HttpClientHandlerCustomiser { get; set; }

        public RestClientConfiguration()
        {
            Timeout = 600000;
            InterceptorRegistrations = new List<InterceptorRegistration>();
        }

        public void AddInterceptor(HttpMessageInterceptor interceptor, int index = -1)
        {
            AddInterceptor(new InterceptorRegistration(interceptor), index);
        }

        public void AddInterceptor<T>(int index = -1) where T : HttpMessageInterceptor
        {
            AddInterceptor(new InterceptorRegistration(typeof(T)), index);
        }

        public void AddInterceptor<T>(Func<T> interceptorFactory, int index = -1) where T : HttpMessageInterceptor
        {
            AddInterceptor(new InterceptorRegistration(interceptorFactory), index);
        }

        internal void AddInterceptor(InterceptorRegistration interceptorRegistration, int index = -1)
        {
            if (index == -1)
                index = InterceptorRegistrations.Count;

            InterceptorRegistrations.Insert(index, interceptorRegistration);
        }

        public void ClearInterceptors()
        {
            InterceptorRegistrations.Clear();
        }

        public RestClientConfiguration(string baseUrl)
            : this()
        {
            BaseUrl = new Uri(baseUrl);
        }

        public RestClientConfiguration(string baseUrl, params HttpMessageInterceptor[] httpMessageInterceptors)
            : this(baseUrl)
        {
            foreach (var httpMessageInterceptor in httpMessageInterceptors)
            {
                AddInterceptor(httpMessageInterceptor);
            }
        }

        public RestClientConfiguration CustomiseHttpClient(Action<HttpClient> customiserFn)
        {
            HttpClientCustomiser = customiserFn;
            return this;
        }

        public RestClientConfiguration CustomiseHttpClientHandler(Action<HttpClientHandler> customiserFn)
        {
            HttpClientHandlerCustomiser = customiserFn;
            return this;
        }
    }
}