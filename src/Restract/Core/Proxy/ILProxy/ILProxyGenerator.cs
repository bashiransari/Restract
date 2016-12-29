namespace Restract.Core.Proxy.ILProxy
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    public class IlProxyGenerator : IProxyGenerator
    {
        public T GetProxy<T>(IProxyInterceptor interceptor) where T : class
        {
            return (T)GetProxy(typeof(T), interceptor);
        }

        private static AssemblyBuilder dynamicAssembly;
        public static AssemblyBuilder DynamicAssembly
        {
            get
            {
                if (dynamicAssembly == null)
                {
                    dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(
                        new AssemblyName("PureProxyGenerator"),
                        AssemblyBuilderAccess.Run);
                }
                return dynamicAssembly;
            }
        }

        private static ModuleBuilder moduleBuilder;
        public static ModuleBuilder DynamicModule
        {
            get
            {
                if (moduleBuilder == null)
                {
                    moduleBuilder = DynamicAssembly.DefineDynamicModule("PureProxyGenerator");
                }
                return moduleBuilder;
            }
        }

        public static object GetProxy(Type type, IProxyInterceptor interceptor)
        {
            var typeKey = type.AssemblyQualifiedName.Replace(".", "_");
            ProxyTypeInfo proxyTypeInfo;
            if (!ProxyTypeInfoCache.HasProxyTypeInfo(typeKey))
            {
                proxyTypeInfo = GetProxyType(typeKey, type);
                ProxyTypeInfoCache.AddProxyTypeInfo(typeKey, proxyTypeInfo);
            }
            else
            {
                proxyTypeInfo = ProxyTypeInfoCache.GetProxyTypeInfo(typeKey);
            }

            var proxy = Activator.CreateInstance(proxyTypeInfo.ProxyType);
            var baseProxy = proxy as BaseProxy;
            if (baseProxy != null)
            {
                baseProxy.Interceptor = interceptor;
            }

            return proxy;
        }

        internal static ProxyTypeInfo GetProxyType(string typeKey, Type type)
        {
            var typeInfo = new ProxyTypeInfo { Type = type };

            var proxyBuilder = DynamicModule.DefineType(typeKey, TypeAttributes.Class | TypeAttributes.Public, typeof(BaseProxy));

            proxyBuilder.AddInterfaceImplementation(type);

            var methods = type.GetRuntimeMethods();
            int methodIndex = 0;
            foreach (var method in methods)
            {
                var methodKey = method.Name + methodIndex++;
                var parameters = method.GetParameters().ToList();

                var methodBuilder = proxyBuilder.DefineMethod(
                    method.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    method.ReturnType,
                    parameters.Select(p => p.ParameterType).ToArray());

                typeInfo.AddMethodInfo(methodKey, method);

                var ilGen = methodBuilder.GetILGenerator();
                ilGen.DeclareLocal(typeof(MethodInfo));
                ilGen.DeclareLocal(typeof(object[]));
                ilGen.DeclareLocal(method.ReturnType);

                ilGen.Emit(OpCodes.Nop);
                ilGen.Emit(OpCodes.Ldstr, typeKey);
                ilGen.Emit(OpCodes.Ldstr, methodKey);
                ilGen.Emit(OpCodes.Call, typeof(ProxyTypeInfoCache)
                    .GetRuntimeMethod("GetMethodInfo", new[] { typeof(string), typeof(string) }));
                ilGen.Emit(OpCodes.Stloc_0);//set mi

                //create object[]
                ilGen.Emit(OpCodes.Ldc_I4, parameters.Count);
                ilGen.Emit(OpCodes.Newarr, typeof(object));

                //fill object[]
                for (var i = 0; i < parameters.Count; i++)
                {
                    ilGen.Emit(OpCodes.Dup);//crerate new arrayCell

                    ilGen.Emit(OpCodes.Ldc_I4, i);//arrayIndex
                    ilGen.Emit(OpCodes.Ldarg, i + 1);//ArgIndex
                    ilGen.Emit(OpCodes.Box, parameters[i].ParameterType);
                    ilGen.Emit(OpCodes.Stelem_Ref);//set array cell value
                }
                ilGen.Emit(OpCodes.Stloc_1);//set args

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldloc_0);
                ilGen.Emit(OpCodes.Ldloc_1);
                ilGen.Emit(OpCodes.Call, typeof(BaseProxy).GetTypeInfo().GetMethod("CallIntercetor", BindingFlags.Public | BindingFlags.Instance));
                ilGen.Emit(OpCodes.Unbox_Any, method.ReturnType);

                ilGen.Emit(OpCodes.Stloc_2);//set res
                ilGen.Emit(OpCodes.Ldloc_2);
                ilGen.Emit(OpCodes.Ret);
            }

            var proxyType = proxyBuilder.CreateTypeInfo().AsType();

            typeInfo.ProxyType = proxyType;
            return typeInfo;
        }
    }
}
