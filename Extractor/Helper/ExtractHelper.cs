using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Extractor.Helper
{
    internal static class ExtractHelper
    {
        public static void ExecuteExternalCommand(string commandFilePath, string commandParameter)
        {
            if (string.IsNullOrWhiteSpace(commandFilePath)) throw new ArgumentException(@"The command file path is null or whitespace.", @"commandFilePath");

            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                FileName = commandFilePath,
                Arguments = commandParameter ?? string.Empty,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            using (Process proc = Process.Start(processStartInfo))
            {
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();

                int exitCode = proc.ExitCode;
                if (exitCode != 0)
                {
                    string commandline = string.Format(@"""{1}"" {2}", commandFilePath, commandParameter);
                    string message = string.Format(@"Failed: {0}: {1}", exitCode, commandline);
                    throw new ExternalCommandException(commandline, exitCode, message);
                }
            }
        }
    }
}
