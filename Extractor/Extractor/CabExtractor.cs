using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Extractor.Helper;

namespace Extractor.Extractor
{
    public class CabExtractor : ExtractorBase
    {
        private const string CommandFilePath = @"C:\Windows\System32\expand.exe";

        public override void Extract()
        {
            Trace.Assert(!string.IsNullOrWhiteSpace(SourcePackagePath));
            Trace.Assert(!string.IsNullOrWhiteSpace(DestinationDirectoryPath));

            // Create destination directory if not exist.
            if (!Directory.Exists(DestinationDirectoryPath)) Directory.CreateDirectory(DestinationDirectoryPath);

            // Extract files from .cab file.
            string commandParameter = string.Format(@"-f:* ""{0}"" ""{1}""", SourcePackagePath, DestinationDirectoryPath);
            ExtractHelper.ExecuteExternalCommand(CommandFilePath, commandParameter);
        }
    }
}
