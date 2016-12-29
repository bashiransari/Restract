namespace Restract
{
    using System;
    using Restract.Contract;

    public class InterceptorRegistration
    {
        internal HttpMessageInterceptor InterceptorInstance { get; }
        internal Type InterceptorType { get; }
        internal Func<HttpMessageInterceptor> InterceptorFactory { get; }

        internal InterceptorRegistration(HttpMessageInterceptor interceptorInstance)
        {
            if (interceptorInstance == null)
                throw new ArgumentNullException(nameof(interceptorInstance), "Interceptor instance cannot be null");

            InterceptorInstance = interceptorInstance;
        }

        internal InterceptorRegistration(Type interceptorType)
        {
            InterceptorType = interceptorType;
        }

        internal InterceptorRegistration(Func<HttpMessageInterceptor> interceptorFactory)
        {
            InterceptorFactory = interceptorFactory;
        }
    }
}