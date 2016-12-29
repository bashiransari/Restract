namespace Restract.Core.Proxy.RoslynProxy
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public class RoslynProxyGenerator : IProxyGenerator
    {

        public T GetProxy<T>(IProxyInterceptor interceptor) where T : class
        {
            return (T)GetProxy(typeof(T), interceptor);
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
            var proxyClass = new StringBuilder();
            //var proxyBuilder =  DynamicModule.DefineType(typeKey, TypeAttributes.Class | TypeAttributes.Public, typeof(BaseProxy));
            var proxyTypeName = type.Name + "Proxy";
            proxyClass.AppendLine("using System.Reflection;");
            proxyClass.AppendLine("using Restract.Proxy;");
            proxyClass.AppendLine("using System;");

            proxyClass.AppendLine("namespace RoslynCompileSample");
            proxyClass.AppendLine("{");
            proxyClass.Append($"public class {proxyTypeName}: Restract.Proxy.BaseProxy, ");
            proxyClass.Append(GetTypeName(type));
            proxyClass.AppendLine("{");

            var methods = type.GetRuntimeMethods();
            int methodIndex = 0;
            foreach (var method in methods)
            {
                var methodKey = method.Name + methodIndex++;

                typeInfo.AddMethodInfo(methodKey, method);

                var parameters = method.GetParameters().ToList();
                proxyClass.Append("public ");
                proxyClass.Append(GetTypeName(method.ReturnType));
                proxyClass.Append(" ");
                proxyClass.Append(method.Name);
                proxyClass.Append("(");

                var parameterTypes = new StringBuilder();
                for (var i = 0; i < parameters.Count; i++)
                {
                    if (i > 0)
                    {
                        proxyClass.Append(", ");
                        parameterTypes.Append(", ");
                    }

                    proxyClass.Append(GetTypeName(parameters[i].ParameterType));
                    parameterTypes.Append($"typeof({GetTypeName(parameters[i].ParameterType)})");
                    proxyClass.Append(" ");
                    proxyClass.Append(parameters[i].Name);
                }

                proxyClass.Append(")");
                proxyClass.AppendLine("{");
                //proxyClass.AppendLine($"var methodInfo = typeof({GetTypeName(type)}).GetTypeInfo().GetMethod(\"{method.Name}\", new Type[] {{{parameterTypes}}});");
                proxyClass.AppendLine($"var methodInfo = ProxyTypeInfoCache.GetMethodInfo(\"{typeKey}\",\"{methodKey}\");");
                proxyClass.AppendLine("var result = base.CallIntercetor(methodInfo, new object[]{");

                for (var i = 0; i < parameters.Count; i++)
                {
                    if (i > 0)
                        proxyClass.Append(", ");
                    proxyClass.Append(parameters[i].Name);
                }

                proxyClass.AppendLine("});");
                if (method.ReturnType != typeof(void))
                {
                    proxyClass.AppendLine($"return ({GetTypeName(method.ReturnType)})result;");
                }
                proxyClass.AppendLine("}");

            }
            proxyClass.AppendLine("}");
            proxyClass.AppendLine("}");

            var asmGen = new RoslynAssemblyGenerator();
            var references = type.GetTypeInfo().Assembly.GetReferencedAssemblies();
            foreach (var reference in references)
            {
                asmGen.ReferenceAssembly(reference);
            }

            asmGen.ReferenceAssemblyContainingType<RoslynProxyGenerator>();

            asmGen.ReferenceAssembly(type.GetTypeInfo().Assembly);

            var asm = asmGen.Generate(proxyClass.ToString());
            var proxyType = asm.GetExportedTypes().Single(p => p.Name == proxyTypeName);

            typeInfo.ProxyType = proxyType;
            return typeInfo;
        }

        public static string GetTypeName(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericType)
            {
                if (type == typeof(void))
                {
                    return "void";
                }
                return type.FullName;
            }

            var typeName = type.GetGenericTypeDefinition().FullName.Split('`')[0] + "<";
            int i = 0;
            foreach (var arg in typeInfo.GenericTypeArguments)
            {
                if (i > 0)
                    typeName += ", ";
                typeName += GetTypeName(arg);
                i++;
            }
            typeName += ">";
            return typeName;
        }

    }

}
