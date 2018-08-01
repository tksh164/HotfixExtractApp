using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extractor
{
    public sealed class ExternalCommandException : Exception
    {
        public string Commandline { get; private set; }
        public int ExitCode { get; private set; }

        public ExternalCommandException(string commandline, int exitCode) : base()
        {
            Commandline = commandline;
            ExitCode = exitCode;
        }

        public ExternalCommandException(string commandline, int exitCode, string message) : base(message)
        {
            Commandline = commandline;
            ExitCode = exitCode;
        }

        public ExternalCommandException(string commandline, int exitCode, string message, Exception innerException) : base(message, innerException)
        {
            Commandline = commandline;
            ExitCode = exitCode;
        }
    }
}
