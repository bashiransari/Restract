namespace Restract.Core.Proxy
{
    using System.Collections.Concurrent;
    using System.Reflection;

    internal static class ProxyTypeInfoCache
    {
        private static readonly ConcurrentDictionary<string, ProxyTypeInfo> TypeInfos = new ConcurrentDictionary<string, ProxyTypeInfo>();
        public static MethodInfo GetMethodInfo(string typeKey, string methodKey)
        {
            return TypeInfos[typeKey].GetMethodInfo(methodKey);
        }

        public static void AddProxyTypeInfo(string typeKey, ProxyTypeInfo proxyTypeInfo)
        {
            TypeInfos.TryAdd(typeKey, proxyTypeInfo);
        }

        public static ProxyTypeInfo GetProxyTypeInfo(string typeKey)
        {
            return TypeInfos[typeKey];
        }

        public static bool HasProxyTypeInfo(string typeKey)
        {
            return TypeInfos.ContainsKey(typeKey);
        }
    }
}