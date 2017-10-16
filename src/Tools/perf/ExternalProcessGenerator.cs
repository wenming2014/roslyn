﻿using System;
using System.IO;
using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.Results;

namespace perf
{
    class ExternalProcessGenerator : IGenerator
    {
        private readonly ArtifactsPaths _artifactsPaths;

        public ExternalProcessGenerator(string exePath)
        {
            _artifactsPaths = new ArtifactsPaths(
                rootArtifactsFolderPath: "",
                buildArtifactsDirectoryPath: "",
                binariesDirectoryPath: "",
                programCodePath: "",
                appConfigPath: "",
                projectFilePath: "",
                buildScriptFilePath: "",
                executablePath: exePath,
                programName: Path.GetFileName(exePath));
        }

        public GenerateResult GenerateProject(Benchmark benchmark, ILogger logger, string rootArtifactsFolderPath, IConfig config, IResolver resolver)
        {
            if (!(benchmark is ExternalProcessBenchmark))
            {
                return GenerateResult.Failure(null, Array.Empty<string>());
            }

            return GenerateResult.Success(_artifactsPaths, Array.Empty<string>());
        }
    }
}
