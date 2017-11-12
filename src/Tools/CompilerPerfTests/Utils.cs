// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;

namespace CompilerPerfTests
{
    internal static class Utils
    {
        internal static int CheckoutAndBuildCsc(string slnDir, string commit)
        {
            var compilersDir = Path.Combine(slnDir, "src/Compilers");
            var cscProj = Path.Combine(compilersDir, "CSharp/csc/csc.csproj");

            const int Failed = 1;

            // git show -s --pretty=short COMMIT
            var result = RunProcess(
                "git", $"show -s --pretty=short {commit}",
                captureStdout: true);

            if (result.ExitCode != 0)
            {
                return Failed;
            }

            // git checkout COMMIT src/Compilers
            result = RunProcess(
                "git", $"checkout {commit} \"{compilersDir}\"",
                captureStdout: true);
            if (result.ExitCode != 0)
            {
                return Failed;
            };

            // restore.cmd
            result = RunProcess(
                $"\"{Path.Combine(slnDir, "Restore.cmd")}\"",
                captureStdout: true);
            if (result.ExitCode != 0)
            {
                return Failed;
            }

            // dotnet build -c Release csc.csproj
            result = RunProcess(
                "dotnet", $"build -c Release \"{cscProj}\"",
                captureStdout: true);
            if (result.ExitCode != 0)
            {
                return Failed;
            }

            return 0;
        }

        private static (int ExitCode, string StdOut, string StdErr) RunProcess(
            string fileName,
            string arguments = null,
            bool captureStdout = false,
            bool captureStderr = false)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName
            };

            if (arguments != null)
            {
                psi.Arguments = arguments;
            }

            if (captureStdout)
            {
                psi.RedirectStandardOutput = true;
            }

            if (captureStderr)
            {
                psi.RedirectStandardError = true;
            }

            var proc = new Process() { StartInfo = psi };
            proc.Start();

            var stdOut = captureStdout ? proc.StandardOutput.ReadToEnd() : null;
            var stdErr = captureStderr ? proc.StandardError.ReadToEnd() : null;
            proc.WaitForExit();

            return (proc.ExitCode, stdOut, stdErr);
        }
    }
}
