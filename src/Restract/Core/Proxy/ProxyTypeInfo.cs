namespace Restract.Core.Proxy
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;

    internal class ProxyTypeInfo
    {
        private readonly ConcurrentDictionary<string, MethodInfo> _methodInfos = new ConcurrentDictionary<string, MethodInfo>();

        public Type Type { get; set; }
        public Type ProxyType { get; set; }

        public void AddMethodInfo(string methodKey, MethodInfo proxyTypeInfo)
        {
            _methodInfos.TryAdd(methodKey, proxyTypeInfo);
        }

        public MethodInfo GetMethodInfo(string methodKey)
        {
            return _methodInfos[methodKey];
        }

        public bool HasMethodInfo(string methodKey)
        {
            return _methodInfos.ContainsKey(methodKey);
        }
    }
}