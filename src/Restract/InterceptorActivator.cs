namespace Restract
{
    using System;
    using Core;
    using Restract.Contract;

    internal class InterceptorActivator : IInterceptorActivator
    {
        private readonly ITypeActivator _typeActivator;

        public InterceptorActivator(ITypeActivator typeActivator)
        {
            _typeActivator = typeActivator;
        }

        public HttpMessageInterceptor Activate(InterceptorRegistration registration)
        {
            if (registration.InterceptorInstance != null)
            {
                return registration.InterceptorInstance;
            }

            try
            {
                HttpMessageInterceptor interceptor;
                if (registration.InterceptorType != null)
                {
                    interceptor = _typeActivator.Activate(registration.InterceptorType) as HttpMessageInterceptor;
                }
                else
                {
                    interceptor = registration.InterceptorFactory();
                }

                if (interceptor == null)
                {
                    var interceptorType = registration.InterceptorType != null ? "type: " + registration.InterceptorType : " factory method";
                    throw new InvalidOperationException($"Interceptor registered with {interceptorType} resolved to null.");
                }

                return interceptor;
            }
            catch (Exception ex)
            {
                var interceptorType = registration.InterceptorType != null ? "type: " + registration.InterceptorType : " factory method";
                throw new InvalidOperationException($"Cannot activate interceptor registered with {interceptorType}.", ex);
            }
        }
    }
}