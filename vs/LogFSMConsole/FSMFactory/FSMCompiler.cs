namespace LogFSM
{

    #region usings

    using LogFSMShared;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Loader;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Stateless;
    using Stateless.Graph; 
    using LogFSM;
    using System.Diagnostics;
    using System.Data;
    using Ionic.Zip;
    using Newtonsoft.Json;

    #endregion

    public class FSMCompiler : IFSMProvider
    {
        private string sourceFileName { get; }
        public string CodeNameSpace { get; }
        public string ClassName { get; }

        public FSMCompiler(string SourceFileName, string CodeNameSpace, string ClassName)
        {
            sourceFileName = SourceFileName;
            this.CodeNameSpace = CodeNameSpace;
            this.ClassName = ClassName;
        }

        public Assembly GetAssembly()
        {
            Assembly createdAssembly = null;

            var options = new CSharpCompilationOptions(
               OutputKind.ConsoleApplication,
               optimizationLevel: OptimizationLevel.Release,
               allowUnsafe: false);
 
            var compilation = CSharpCompilation.Create(Path.GetRandomFileName(), options: options);

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceFileName));
            compilation = compilation.AddSyntaxTrees(syntaxTree);

            // TODO: Optimize!

            #region Add Rferences

            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            List<MetadataReference> references = new List<MetadataReference>();
  
            references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")));
            references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Console.dll")));
            references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));

            var usings = compilation.SyntaxTrees.Select(tree => tree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>()).SelectMany(s => s).ToArray();

            foreach (var u in usings)
            {
                if (File.Exists(Path.Combine(assemblyPath, u.Name.ToString() + ".dll")))
                    references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, u.Name.ToString() + ".dll")));
            }

            // TODO: add path from parsedcommandline-object
            references.Add(MetadataReference.CreateFromFile(Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "Stateless.dll")));  
            references.Add(MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location));
            references.Add(MetadataReference.CreateFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogFSMShared.dll")));

            #endregion
 
            compilation = compilation.AddReferences(references);

            //compile

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}, {2}", diagnostic.Id, diagnostic.GetMessage(), diagnostic.Location);
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    AssemblyLoadContext context = AssemblyLoadContext.Default;
                    createdAssembly = context.LoadFromStream(ms);
                }
            }
            return createdAssembly;
        }
    }

}
