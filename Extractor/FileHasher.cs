using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace Extractor
{
    public static class FileHasher
    {
        internal static byte[] ComputeHash(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException(@"The file path is null or whitespace.", @"filePath");

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                SHA1CryptoServiceProvider sha1Provider = new SHA1CryptoServiceProvider();
                return sha1Provider.ComputeHash(stream);
            }
        }
    }
}
