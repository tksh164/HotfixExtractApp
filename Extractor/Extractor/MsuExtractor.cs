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
    public class MsuExtractor : ExtractorBase
    {
        private const string CommandFilePath = @"C:\Windows\System32\expand.exe";

        public override void Extract()
        {
            Trace.Assert(!string.IsNullOrWhiteSpace(SourcePackagePath));
            Trace.Assert(!string.IsNullOrWhiteSpace(DestinationDirectoryPath));

            // Create destination directory if not exist.
            if (!Directory.Exists(DestinationDirectoryPath)) Directory.CreateDirectory(DestinationDirectoryPath);

            // Extract *cab files from .msu file.
            string commandParameter = string.Format(@"-f:*.cab ""{0}"" ""{1}""", SourcePackagePath, DestinationDirectoryPath);
            ExtractHelper.ExecuteExternalCommand(CommandFilePath, commandParameter);

            // Retrieve inner cab file path.
            //string innerCabFilePath = getInnerCabFilePath();

            // Extract inner *.cab files.
            //extractInnerCabFiles(innerCabFilePath);
        }

        private string getInnerCabFilePath()
        {
            List<string> resultPaths = new List<string>();
            IEnumerable<string> extractedCabFilePaths = Directory.EnumerateFiles(DestinationDirectoryPath, @"*.cab", SearchOption.TopDirectoryOnly);
            foreach (string extractedCabFilePath in extractedCabFilePaths)
            {
                // Exclude wsusscan.cab
                if (string.Compare(Path.GetFileName(extractedCabFilePath), @"wsusscan.cab", true) == 0) continue;

                resultPaths.Add(extractedCabFilePath);
            }

            // Expect one cab file path.
            if (resultPaths.Count != 1) throw new PackageStructureException(@"Unexpected package structure.");

            // Retrieve inner *.cab file path.
            string innerCabFilePath = null;
            foreach (string path in resultPaths) innerCabFilePath = path;
            return innerCabFilePath;
        }

        private void extractInnerCabFiles(string innerCabFilePath)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(innerCabFilePath));

            // Extract innner cab file.
            string destinationDirectoryPath = DestinationDirectoryPath + Path.DirectorySeparatorChar + Path.GetRandomFileName().Substring(0, 3);
            CabExtractor extractor = new CabExtractor()
            {
                DestinationDirectoryPath = destinationDirectoryPath,
                SourcePackagePath = innerCabFilePath,
            };
            extractor.Extract();
        }
    }
}
