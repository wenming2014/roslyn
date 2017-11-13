using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using static CompilerPerfTests.RoslynReflectionShim;

namespace CompilerPerfTests
{
    public class ParsingTest
    {
        [Params("HEAD", "HEAD^")]
        public string Commit;

        private readonly string _slnDir =
            Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "../../../../../../../../../../"));

        private CSharpCodeAnalysis _csCodeAnalysis;
        private string[] _csFilePaths;
        private string[] _csFileText;
        private CSharpCodeAnalysis.CSharpParseOptions _parseOptions;

        [GlobalSetup]
        public void BuildRoslyn()
        {
            if (Utils.CheckoutAndBuildCsc(_slnDir, Commit) != 0)
            {
                throw new InvalidOperationException("Setup did not complete succesfully");
            }

            var reproPath = Path.Combine(_slnDir, "Binaries/CodeAnalysisRepro");
            _csCodeAnalysis = CSharpCodeAnalysis.GetInstance(_slnDir);
            _csFilePaths = Directory.GetFiles(reproPath, "*.cs", SearchOption.AllDirectories);
            _parseOptions = _csCodeAnalysis.DefaultParseOptions.WithPreprocessorSymbols("DEBUG", "COMPILERCORE");

            _csFileText = new string[_csFilePaths.Length];
            for (int i = 0; i < _csFilePaths.Length; i++)
            {
                _csFileText[i] = File.ReadAllText(_csFilePaths[i]);
            }
        }

        [Benchmark]
        public object[] TestEmit()
        {
            var trees = new object[_csFilePaths.Length];
            for (int i = 0; i < _csFilePaths.Length; i++)
            {
                trees[i] = _csCodeAnalysis.ParseSyntaxTree(_csFileText[i], _parseOptions, _csFilePaths[i]);
            }

            return trees;
        }
    }
}
