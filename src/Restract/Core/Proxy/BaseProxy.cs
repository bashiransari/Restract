namespace Restract.Core.Proxy
{
    using System.Reflection;

    public abstract class BaseProxy
    {
        public IProxyInterceptor Interceptor;
        public object CallIntercetor(MethodInfo mi, object[] args)
        {
            return Interceptor.OnMethodCall(mi, args);
        }
    }
}