// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace CompilerBenchmarks
{
    public class Program
    {
        private class ValueOnlyExporter : CsvMeasurementsExporter
        {
            public ValueOnlyExporter() : base(CsvSeparator.Comma)
            { }

            public override void ExportToLog(Summary summary, ILogger logger)
            {
                string realSeparator = Separator;
                var columns = new[] {
                    new MeasurementColumn("Target", (summary, report, m) => report.BenchmarkCase.Descriptor.Type.Name + "." + report.BenchmarkCase.Descriptor.WorkloadMethodDisplayInfo)
                };
                logger.WriteLine(string.Join(realSeparator, columns.Select(c => CsvHelper.Escape(c.Title, realSeparator))));

                foreach (var report in summary.Reports)
                {
                    foreach (var measurement in report.AllMeasurements)
                    {
                        for (int i = 0; i < columns.Length;)
                        {
                            logger.Write(CsvHelper.Escape(columns[i].GetValue(summary, report, measurement), realSeparator));

                            if (++i < columns.Length)
                            {
                                logger.Write(realSeparator);
                            }
                        }
                        logger.WriteLine();
                    }
                }
            }

            private struct MeasurementColumn
            {
                public string Title { get; }
                public Func<Summary, BenchmarkReport, Measurement, string> GetValue { get; }

                public MeasurementColumn(string title, Func<Summary, BenchmarkReport, Measurement, string> getValue)
                {
                    Title = title;
                    GetValue = getValue;
                }
            }

            public static new ValueOnlyExporter Default = new ValueOnlyExporter();
        }

        private class IgnoreReleaseOnly : ManualConfig
        {
            public IgnoreReleaseOnly()
            {
                Add(JitOptimizationsValidator.DontFailOnError);
                Add(DefaultConfig.Instance.GetLoggers().ToArray());
                Add(DefaultConfig.Instance
                    .GetExporters()
                    .Concat(new[] { ValueOnlyExporter.Default })
                    .ToArray());
                Add(DefaultConfig.Instance.GetColumnProviders().ToArray());
                Add(MemoryDiagnoser.Default);
                Add(Job.Core.WithGcServer(true));
            }
        }

        public static void Main(string[] args)
        {
            var projectPath = args[0];
            var artifactsPath = Path.Combine(projectPath, "../BenchmarkDotNet.Artifacts");

            var config = new IgnoreReleaseOnly();
            var artifactsDir = Directory.CreateDirectory(artifactsPath);
            config.ArtifactsPath = artifactsDir.FullName;

            // Benchmark.NET creates a new process to run the benchmark, so the easiest way
            // to communicate information is pass by environment variable
            Environment.SetEnvironmentVariable(Helpers.TestProjectEnvVarName, projectPath);

            _ = BenchmarkRunner.Run<StageBenchmarks>(config);
        }
    }
}
