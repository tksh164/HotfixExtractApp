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
    public class MsiExtractor : ExtractorBase
    {
        private const string CommandFilePath = @"C:\Windows\System32\msiexec.exe";

        public override void Extract()
        {
            Trace.Assert(string.IsNullOrWhiteSpace(SourcePackagePath));
            Trace.Assert(string.IsNullOrWhiteSpace(DestinationDirectoryPath));

            if (!Directory.Exists(DestinationDirectoryPath))
            {
                Directory.CreateDirectory(DestinationDirectoryPath);
            }

            //msiexec.exe /a windows8.1-kb3081403-x64.msi /qb TargetDir=\\localhost\D$\HotfixDB\qfe-2015-09\handextract\windows8.1-kb3081403-x64
            ExtractHelper.ExecuteExternalCommand(CommandFilePath, string.Format(@"/a ""{0}"" /qb TargetDir=""{1}""", SourcePackagePath, DestinationDirectoryPath));
        }
    }
}
