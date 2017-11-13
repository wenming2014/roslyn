// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Parameters;
using BenchmarkDotNet.Running;
using Mono.Options;

namespace CompilerPerfTests
{
    public static class Program
    {
        private sealed class EndToEndRoslynConfig : ManualConfig
        {
            public EndToEndRoslynConfig(string exePath)
            {
                Add(new Job("RoslynExternalRun")
                    .With(new ExternalProcessToolchain(exePath))
                    .With(RunStrategy.Monitoring)
                    .WithWarmupCount(0)
                    .WithLaunchCount(0)
                    .WithTargetCount(25));
                Add(ConsoleLogger.Default);
                Add(DefaultColumnProviders.Instance);
                Add(RankColumn.Arabic);
            }
        }

        public static int Main(string[] args)
        {
            bool runEmitTests = false;
            bool runParsingTests = false;

            var options = new OptionSet
            {
                { "emit", "Run emit tests", (_) => runEmitTests = true },
                { "parsing", "Run parsing tests", (_) => runParsingTests = true }
            };

            List<string> extra;
            try
            {
                extra = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine("CompilerPerfTests: " + e.Message);
                return 1;
            }

            if (!runEmitTests && !runParsingTests)
            {
                var slnDir = Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory,
                    "../../../../../../"));

                var config = new EndToEndRoslynConfig(
                    Path.Combine(slnDir, "Binaries/Release/Exes/csc/netcoreapp2.0/csc.dll"));
                var benchmarks = MakeBenchmarks(slnDir, config, new[] { "HEAD^", "HEAD" });
                var summary = BenchmarkRunner.Run(benchmarks, config);
            }
            else
            {
                if (runEmitTests)
                {
                    var summary = BenchmarkRunner.Run<EmitTest>();
                }
                else
                {
                    var summary = BenchmarkRunner.Run<ParsingTest>();
                }
            }

            return 0;
        }

        /// <summary>
        /// Holds the parameters that will be used in execution,
        /// including the arguments to pass to csc.exe and the
        /// commits to checkout.
        /// </summary>
        private static ParameterInstances MakeParameterInstances(string commit)
        {
            var items = new[]
            {
                new ParameterInstance(
                    new ParameterDefinition("Commit",
                        isStatic: true,
                        values: null),
                    value: commit),
            };
            return new ParameterInstances(items);
        }

        private static Benchmark[] MakeBenchmarks(string slnDir, EndToEndRoslynConfig config, string[] commits)
        {
            var benchmarks = new Benchmark[commits.Length];
            for (int i = 0; i < commits.Length; i++)
            {
                benchmarks[i] = new ExternalProcessBenchmark(
                    Path.Combine(slnDir, "Binaries/CodeAnalysisRepro"),
                    "-noconfig @repro.rsp",
                    commit => Utils.CheckoutAndBuildCsc(slnDir, commit),
                    config.GetJobs().Single(),
                    MakeParameterInstances(commits[i]));
            }
            return benchmarks;
        }
    }
}
