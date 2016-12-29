namespace Restract.Core.Proxy.RoslynProxy
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
#if NETSTANDARD1_6
    using System.Runtime.Loader;
#endif

    internal class RoslynAssemblyGenerator
    {
        private readonly List<MetadataReference> _references = new List<MetadataReference>();

        public RoslynAssemblyGenerator()
        {
            ReferenceAssemblyContainingType<object>();
#if NETSTANDARD1_6
            ReferenceAssembly("mscorlib");
            ReferenceAssembly("System.Reflection");
#endif            
        }

        public void ReferenceAssembly(Assembly assembly)
        {
            _references.Add(MetadataReference.CreateFromFile(assembly.Location));
        }

        public void ReferenceAssembly(AssemblyName assemblyName)
        {
            var assembly = Assembly.Load(assemblyName);
            ReferenceAssembly(assembly);
        }

        public void ReferenceAssembly(string assemblyName)
        {
            ReferenceAssembly(new AssemblyName(assemblyName));
        }

        public void ReferenceAssemblyContainingType<T>()
        {
            ReferenceAssembly(typeof(T).GetTypeInfo().Assembly);
        }

        public Assembly Generate(string code)
        {
            var assemblyName = Path.GetRandomFileName();

            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            var references = _references.ToArray();
            var compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree }, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var stream = new MemoryStream())
            {
                var result = compilation.Emit(stream);
                if (!result.Success)
                {
                    var failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    var messages = failures.Select(x => $"{x.Id}: {x.GetMessage()}");
                    var message = string.Join("\n", messages);
                    throw new InvalidOperationException("Compilation failures!\n\n" + message + "\n\nCode:\n\n" + code);
                }

                stream.Seek(0, SeekOrigin.Begin);
#if NETSTANDARD1_6
                var assemblyLoader = new AssemblyLoader();
                return assemblyLoader.Load(stream);
#else
                return Assembly.Load(stream.ToArray());
#endif
            }
        }
    }

#if NETSTANDARD1_6
    public class AssemblyLoader : AssemblyLoadContext
    {
        protected override Assembly Load(AssemblyName assemblyName)
        {
            return Assembly.Load(assemblyName);
        }

        public Assembly Load(Stream stream)
        {
            return LoadFromStream(stream);
        }
    }
#endif

}
