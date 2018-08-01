using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extractor.Helper
{
    internal static class UtilityHelper
    {
        public static string ConvertByteArrayToHexString(byte[] data)
        {
            return BitConverter.ToString(data).Replace(@"-", string.Empty);
        }
    }
}
