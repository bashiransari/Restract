namespace Restract
{
    using Restract.Contract;

    public interface IInterceptorActivator
    {
        HttpMessageInterceptor Activate(InterceptorRegistration registration);
    }
}