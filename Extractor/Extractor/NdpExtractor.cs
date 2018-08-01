using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Extractor.Helper;

namespace Extractor.Extractor
{
    public class NdpExtractor : ExtractorBase
    {
        public override void Extract()
        {
            Trace.Assert(!string.IsNullOrWhiteSpace(SourcePackagePath));
            Trace.Assert(!string.IsNullOrWhiteSpace(DestinationDirectoryPath));

            // Create destination directory if not exist.
            if (!Directory.Exists(DestinationDirectoryPath)) Directory.CreateDirectory(DestinationDirectoryPath);

            // Extract files from NDP*.exe file.
            string commandFilePath = SourcePackagePath;
            string commandParameter = string.Format(@"/x:{0} /q", DestinationDirectoryPath);
            ExtractHelper.ExecuteExternalCommand(commandFilePath, commandParameter);

            //msix.exe NDP46-KB3057781.msp /out NDP46 - KB3057781 / ext
            //mkdir NDP46-KB3057781\NetFxSecurityUpdate
            //expand.exe "NDP46-KB3057781\NetFxSecurity Update.cab.cab" - F:*NDP46 - KB3057781\NetFxSecurityUpdate

        }
    }
}
