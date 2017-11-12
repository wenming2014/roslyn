// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace CompilerPerfTests
{
    public class EmitTest
    {
        [Params("HEAD", "HEAD^")]
        public string Commit;

        private readonly string _slnDir = 
            Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "../../../../../../../../../../"));

        private RoslynReflectionShim.CSharpCodeAnalysis _csCodeAnalysis;
        private object _compilation;

        [GlobalSetup]
        public void BuildRoslyn()
        {
            if (Utils.CheckoutAndBuildCsc(_slnDir, Commit) != 0)
            {
                throw new InvalidOperationException("Setup did not complete succesfully");
            }

            var reproPath = Path.Combine(_slnDir, "Binaries/CodeAnalysisRepro");
            _csCodeAnalysis = RoslynReflectionShim.CSharpCodeAnalysis.GetInstance(_slnDir);
            var csFilePaths = Directory.GetFiles(reproPath, "*.cs", SearchOption.AllDirectories);
            var trees = new object[csFilePaths.Length];
            var parseOptions = _csCodeAnalysis.DefaultParseOptions.WithPreprocessorSymbols("DEBUG", "COMPILERCORE");
            
            for (int i = 0; i < csFilePaths.Length; i++)
            {
                var path = csFilePaths[i];
                var text = File.ReadAllText(path);
                trees[i] = _csCodeAnalysis.ParseSyntaxTree(text, parseOptions, path);
            }

            var refs = Directory.GetFiles(Path.Combine(reproPath, "references"));

            var compilationOptions = _csCodeAnalysis.CreateCompilationOptions(dll: true, allowUnsafe: true);

            _compilation = _csCodeAnalysis.CreateCompilation(
                assemblyName: Guid.NewGuid().ToString(),
                syntaxTrees: trees,
                references: refs.Select(r => _csCodeAnalysis.CreateMetadataReference(r)).ToArray(),
                options: compilationOptions);

            // Request diagnostics for force binding to be completed
            _ = RoslynReflectionShim.CSharpCodeAnalysis.GetDiagnostics(_compilation);
        }

        [Benchmark]
        public void TestEmit()
        {
            var peStream = new MemoryStream();
            if (!_csCodeAnalysis.Emit(_compilation, peStream))
            {
                throw new Exception("Emit failed");
            }
        }
    }
}
