
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace CompilerPerfTests
{
    /// <summary>
    /// When creating microbenchmarks with Roslyn APIs, we
    /// need to be able to call into the API, but since
    /// BenchmarkDotNet doesn't support specifying exact
    /// references for each run, we have to reference
    /// the assemblies via reflection.
    /// </summary>
    internal static class RoslynReflectionShim
    {
        public class CSharpCodeAnalysis
        {
            private Assembly _csCodeAnalysisAssembly;
            private Assembly _codeAnalysisAssembly;
            private Type _parseOptionsType; 
            private MethodInfo _parseSyntaxTreeMethod;
            private MethodInfo _createMetadataRefFromFile;
            private MethodInfo _emitMethod;

            private CSharpCodeAnalysis(Assembly csCodeAnalysisAssembly)
            {
                _csCodeAnalysisAssembly = csCodeAnalysisAssembly;
                _codeAnalysisAssembly = Assembly.LoadFrom(
                    Path.Combine(Path.GetDirectoryName(_csCodeAnalysisAssembly.Location),
                                 "Microsoft.CodeAnalysis.dll"));
                _parseOptionsType = _codeAnalysisAssembly.GetType("Microsoft.CodeAnalysis.ParseOptions");
                _parseSyntaxTreeMethod = _csCodeAnalysisAssembly
                    .GetType("Microsoft.CodeAnalysis.CSharp.SyntaxFactory")
                    .GetMethod("ParseSyntaxTree", new[] { 
                        typeof(string),
                        _parseOptionsType,
                        typeof(string),
                        typeof(Encoding),
                        typeof(CancellationToken)
                    });
                _createMetadataRefFromFile = _codeAnalysisAssembly
                    .GetType("Microsoft.CodeAnalysis.MetadataReference")
                    .GetMethod("CreateFromFile");
                _emitMethod = _csCodeAnalysisAssembly
                    .GetType("Microsoft.CodeAnalysis.CSharp.CSharpCompilation")
                    .GetMethods()
                    .Where(m => m.Name == "Emit")
                    .ElementAt(2);
            }

            public static CSharpCodeAnalysis GetInstance(string slnDir)
                => new CSharpCodeAnalysis(Assembly.LoadFrom(Path.Combine(slnDir,
                "Binaries/Release/Exes/csc/netcoreapp2.0/Microsoft.CodeAnalysis.CSharp.dll")));

            internal object CreateCompilation(
                string assemblyName,
                object[] syntaxTrees,
                object[] references,
                object options)
            {
                var convertedTrees = Array.CreateInstance(syntaxTrees[0].GetType(), syntaxTrees.Length);
                Array.Copy(syntaxTrees, convertedTrees, syntaxTrees.Length);

                var convertedRefs = Array.CreateInstance(references[0].GetType(), references.Length);
                Array.Copy(references, convertedRefs, references.Length);

                return _csCodeAnalysisAssembly.GetType("Microsoft.CodeAnalysis.CSharp.CSharpCompilation")
                    .GetMethod("Create")
                    .Invoke(null, new object[] {
                        assemblyName,
                        convertedTrees,
                        convertedRefs,
                        options
                    });
            }

            public static IEnumerable<object> GetDiagnostics(object compilation)
                => (IEnumerable<object>)compilation
                    .GetType()
                    .GetMethod("GetDiagnostics")
                    .Invoke(compilation, new object[] { Type.Missing});

            public bool Emit(object compilation, Stream peStream)
            {
                object result = _emitMethod.Invoke(compilation, new object[] {
                    peStream,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    default(CancellationToken)
                });

                return (bool)result.GetType().GetProperty("Success").GetMethod.Invoke(result, null);
            }

            internal object CreateMetadataReference(string refPath)
                => _createMetadataRefFromFile.Invoke(null, new object[] {
                    refPath,
                    Type.Missing,
                    Type.Missing
                });

            internal object CreateCompilationOptions(bool dll, bool allowUnsafe)
            {
                var ctor = _csCodeAnalysisAssembly.GetType("Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions")
                    .GetConstructors()[0];
                //new CSharpCompilationOptions(
                //  OutputKind.DynamicallyLinkedLibrary,
                //  reportSuppressedDiagnostics: false,
                //  moduleName: null,
                //  mainTypeName: null,
                //  scriptClassName: null,
                //  usings: null,
                //  optimizationLevel: OptimizationLevel.Debug,
                //  checkOverflow: false,
                //  allowUnsafe: true,
                //  cryptoKeyContainer: null,
                //  cryptoKeyFile: null,
                //  cryptoPublicKey: default(ImmutableArray<byte>),
                //  delaySign: null,
                //  platform: Platform.AnyCpu,
                //  generalDiagnosticOption: ReportDiagnostic.Default,
                //  warningLevel: 4,
                //  specificDiagnosticOptions: null,
                //  concurrentBuild: true,
                //  deterministic: false,
                //  xmlReferenceResolver: null,
                //  sourceReferenceResolver: null,
                //  metadataReferenceResolver: null,
                //  assemblyIdentityComparer: null,
                //  strongNameProvider: null,
                //  publicSign: false);
                return ctor.Invoke(new object[] {
                    2,
                    false,
                    null,
                    null,
                    null,
                    null,
                    0,
                    false,
                    true,
                    null,
                    null,
                    Type.Missing,
                    null,
                    0,
                    0,
                    4,
                    null,
                    true,
                    false,
                    null,
                    null,
                    null,
                    null,
                    null,
                    false
                });
            }

            public CSharpParseOptions DefaultParseOptions
            {
                get
                {
                    var csharpParseOptionsType = _csCodeAnalysisAssembly.GetType(
                        "Microsoft.CodeAnalysis.CSharp.CSharpParseOptions");
                    var defaultOptions = csharpParseOptionsType.GetProperty("Default").GetMethod.Invoke(null, null);
                    return new CSharpParseOptions(defaultOptions);
                }
            }

            public class CSharpParseOptions
            {
                internal object _underlying;

                internal CSharpParseOptions(object underlying)
                {
                    _underlying = underlying;
                }

                public CSharpParseOptions WithPreprocessorSymbols(params string[] symbols)
                {
                    var newOptions = _underlying.GetType()
                        .GetMethod("WithPreprocessorSymbols", new[] { typeof(string[]) })
                        .Invoke(_underlying, new[] { symbols });
                    return new CSharpParseOptions(newOptions);
                }
            }


            public object ParseSyntaxTree(string text, CSharpParseOptions options, string path)
                => _parseSyntaxTreeMethod.Invoke(null, new object[] {
                    text,
                    options._underlying,
                    path,
                    null,
                    default(CancellationToken)
                });
        }
    }
}