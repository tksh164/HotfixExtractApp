using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extractor.UpdatePackage
{
    public sealed class NdpUpdatePackage
    {
        public string PackageFilePath { get; private set; }
        public List<DetailedFileInfo> Files { get; private set; }
        public byte[] FileHash { get; private set; }
        public int KbNumber { get; private set; }
        public int PackageVersion { get; private set; }
        public string PatchType { get; private set; }

        public NdpUpdatePackage(string packageFilePath)
        {
            PackageFilePath = packageFilePath;
            Files = new List<DetailedFileInfo>();

            FileHash = new byte[0];
            KbNumber = -1;  // TODO: from ParameterInfo.xml
            PackageVersion = -1;
            PatchType = string.Empty;  // TODO: from ParameterInfo.xml
        }

        public void Chew()
        {
            // Calculalte the file hash.
            FileHash = FileHasher.ComputeHash(PackageFilePath);

            // TODO: Extract the infomation from the package.
        }
    }
}
