using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileTime = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace Extractor.Helper
{
    internal static class NativeMethodHelper
    {
        public static uint GetHighWord(uint word)
        {
            return (word >> 16) & 0x0000ffff;
        }

        public static uint GetLowWord(uint word)
        {
            return word & 0x0000ffff;
        }

        public static ulong NativeUInt64HighLowToUInt64(ulong high, ulong low)
        {
            return ((high << 32) | low);
        }

        public static ulong NativeUInt32HighLowToUInt64(uint high, uint low)
        {
            return NativeUInt64HighLowToUInt64((ulong)high, (ulong)low);
        }

        public static long NativeInt32HighLowToInt64(int high, int low)
        {
            return (long)((ulong)high << 32 | (uint)low);
        }

        public static DateTime NativeFileTimeToDateTimeUtc(FileTime fileTime)
        {
            return DateTime.FromFileTimeUtc(NativeInt32HighLowToInt64(fileTime.dwHighDateTime, fileTime.dwLowDateTime));
        }
    }
}
