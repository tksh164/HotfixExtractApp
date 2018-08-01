using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Extractor
{
    public sealed class UpdatePackageTypeDetector
    {
        private static byte[] MsuSignature = new byte[] { 0x4D, 0x53, 0x43, 0x46 };
        private static byte[] MsiMspSignature = new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };
        private static byte[] ExeSignature = new byte[] { 0x4D, 0x5A };

        private UpdatePackageTypeDetector()
        { }

        public static UpdatePackageType Detect(string filePath)
        {
            byte[] signature = readSignature(filePath);
            if (compareByteArray(signature, MsuSignature, 0, MsuSignature.Length))
            {
                return UpdatePackageType.Msu;
            }
            else if (compareByteArray(signature, MsiMspSignature, 0, MsiMspSignature.Length))
            {
                return UpdatePackageType.MsiMsp;
            }
            else if (compareByteArray(signature, ExeSignature, 0, ExeSignature.Length))
            {
                FileVersionInfo fileVerInfo = FileVersionInfo.GetVersionInfo(filePath);

                if (fileVerInfo.OriginalFilename == null)
                {
                    return UpdatePackageType.ExeUnknown;
                }

                if (fileVerInfo.OriginalFilename.StartsWith(@"NDP", StringComparison.OrdinalIgnoreCase))
                {
                    return UpdatePackageType.ExeDotNetFramework;
                }
                else if (fileVerInfo.OriginalFilename.Equals(@"SFXCAB.EXE", StringComparison.OrdinalIgnoreCase))
                {
                    return UpdatePackageType.ExeMsxml;
                }
                else if (fileVerInfo.OriginalFilename.StartsWith(@"WindowsMedia", StringComparison.OrdinalIgnoreCase))
                {
                    return UpdatePackageType.ExeWindowsMedia;
                }
                else if (fileVerInfo.OriginalFilename.StartsWith(@"WindowsMovieMaker", StringComparison.OrdinalIgnoreCase))
                {
                    return UpdatePackageType.ExeWindowsMovieMaker;
                }

                return UpdatePackageType.ExeUnknown;
            }

            return UpdatePackageType.Unknown;
        }

        private static byte[] readSignature(string filePath)
        {
            const int bufferSize = 8;
            byte[] buffer = new byte[bufferSize];
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                stream.Read(buffer, 0, buffer.Length);
            }
            return buffer;
        }

        private static bool compareByteArray(byte[] a1, byte[] a2, int startIndex, int length)
        {
            if (a1 == null || a1.Length < (startIndex + length)) throw new ArgumentException(@"Invalid argument: a1", @"a1");
            if (a2 == null || a2.Length < (startIndex + length)) throw new ArgumentException(@"Invalid argument: a2", @"a2");

            for (int i = 0; i < length; i++)
            {
                if (a1[i] != a2[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
