#if NETSTANDARD1_6
#else
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Reflection;

namespace Restract.Core.Proxy.RemotingProxy
{
    internal class RemotingProxy<T> : RealProxy where T : class
    {
        private IProxyInterceptor _interceptor;

        public RemotingProxy()
         : base(typeof(T))
        {
        }

        public T GetProxy(IProxyInterceptor interceptor)
        {
            _interceptor = interceptor;
            return GetTransparentProxy() as T;
        }

        public override IMessage Invoke(IMessage msg)
        {
            var methodCall = msg as IMethodCallMessage;
            
            if (methodCall == null)
                return null;

            //SetProperty
            //methodCall.MethodBase.DeclaringType.GetProperty("Name").GetSetMethod() == methodCall.MethodBase

            var result = _interceptor.OnMethodCall(methodCall.MethodBase as MethodInfo, methodCall.Args);

            return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
        }
    }

}
#endif

