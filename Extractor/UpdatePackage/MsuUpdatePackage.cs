using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Extractor.Extractor;

namespace Extractor.UpdatePackage
{
    public sealed class MsuUpdatePackage
    {
        public string PackageFilePath { get; private set; }
        public List<DetailedFileInfo> Files { get; private set; }
        public byte[] FileHash { get; private set; }
        public int KbNumber { get; private set; }
        public int PackageVersion { get; private set; }
        public string PatchType { get; private set; }

        private string DestinationDirectoryPath { get; set; }

        public MsuUpdatePackage(string packageFilePath)
        {
            PackageFilePath = packageFilePath;
            Files = new List<DetailedFileInfo>();
            DestinationDirectoryPath = getDefaultDestinationDirectoryPath();

            FileHash = new byte[0];
            KbNumber = -1;
            PackageVersion = -1;
            PatchType = string.Empty;
        }

        public void Chew()
        {
            // Calculalte the file hash.
            FileHash = FileHasher.ComputeHash(PackageFilePath);

            // TODO: Extract the infomation from the package.

            // Extract *cab files from .msu file.
            extractMsuPackage(PackageFilePath, DestinationDirectoryPath);

            // Extract the inner *.cab files.
            string innerCabFilePath = getInnerCabFilePath();
            string innerCabDestinationDirectoryPath = getInnerCabDestinationDirectoryPath(DestinationDirectoryPath);
            extractInnerCabFiles(innerCabFilePath, innerCabDestinationDirectoryPath);

            // TODO: file information collection.

        }

        private static void extractMsuPackage(string packageFilePath, string destinationDirectoryPath)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(packageFilePath));
            Debug.Assert(!string.IsNullOrWhiteSpace(destinationDirectoryPath));

            MsuExtractor extractor = new MsuExtractor()
            {
                SourcePackagePath = packageFilePath,
                DestinationDirectoryPath = destinationDirectoryPath,
            };
            extractor.Extract();
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

        private static void extractInnerCabFiles(string packageFilePath, string destinationDirectoryPath)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(packageFilePath));
            Debug.Assert(!string.IsNullOrWhiteSpace(destinationDirectoryPath));

            // Extract innner cab file.
            CabExtractor extractor = new CabExtractor()
            {
                SourcePackagePath = packageFilePath,
                DestinationDirectoryPath = destinationDirectoryPath,
            };
            extractor.Extract();
        }

        private static string getDefaultDestinationDirectoryPath()
        {
            return Path.GetTempPath() + Path.DirectorySeparatorChar + Path.GetRandomFileName().Substring(0, 3);
        }

        private static string getInnerCabDestinationDirectoryPath(string baseDirectoryPath)
        {
            return baseDirectoryPath + Path.DirectorySeparatorChar + Path.GetRandomFileName().Substring(0, 3);
        }
    }
}
