using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Extractor.Helper;
using System.Diagnostics;
using System.IO;
using Extractor.Win32Native;

namespace Extractor
{
    public sealed class DetailedFileInfo
    {
        public string OriginalFilePath { get; private set; }
        public string UncFilePath { get; private set; }
        public ulong FileSize { get; private set; }
        public DateTime CreationTimeUtc { get; private set; }
        public DateTime LastAccessTimeUtc { get; private set; }
        public DateTime LastWriteTimeUtc { get; private set; }
        public string FileDescription { get; private set; }
        public string FileVersion { get; private set; }
        public string FileLanguage { get; private set; }
        public ushort FileLanguageId { get; private set; }
        public ushort FileLanguageCodePage { get; private set; }
        public string OriginalFileName { get; private set; }
        public string ProductVersion { get; private set; }
        public string ProductName { get; private set; }
        public string Platform { get; private set; }
        public string LegalTrademarks { get; private set; }
        public string LegalCopyright { get; private set; }
        public string InternalName { get; private set; }
        public string CompanyName { get; private set; }

        public DetailedFileInfo(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException(@"The file path is null or whitespace.", @"filePath");

            OriginalFilePath = filePath;
            UncFilePath = getNormalizedPathAsUncPath(Path.GetFullPath(filePath));

            initializePropertyFromFileAttributes();
            initializePropertyFromFileVersionInformation();
        }

        private void initializePropertyFromFileAttributes()
        {
            // Get the file attributes by the native function.
            NativeMethods.WIN32_FILE_ATTRIBUTE_DATA fileAttributeData;
            if (!NativeMethods.GetFileAttributesExW(UncFilePath, NativeMethods.GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out fileAttributeData))
            {
                int err = Marshal.GetLastWin32Error();
                string message = string.Format(@"Failed GetFileAttributesExW function with {0}. The file path is ""{1}""", err, UncFilePath);
                throw new Win32Exception(err, message);
            }

            // Initialize the properties.
            FileSize = NativeMethodHelper.NativeUInt32HighLowToUInt64(fileAttributeData.FileSizeHigh, fileAttributeData.FileSizeLow);
            CreationTimeUtc = NativeMethodHelper.NativeFileTimeToDateTimeUtc(fileAttributeData.CreationTime);
            LastAccessTimeUtc = NativeMethodHelper.NativeFileTimeToDateTimeUtc(fileAttributeData.LastAccessTime);
            LastWriteTimeUtc = NativeMethodHelper.NativeFileTimeToDateTimeUtc(fileAttributeData.LastWriteTime);
        }

        private void initializePropertyFromFileVersionInformation()
        {
            try
            {
                // Read the localised file version information data.
                byte[] localisedData = readFileVersionInfoData(UncFilePath, NativeMethods.FileVersionInfoFlags.FILE_VER_GET_LOCALISED | NativeMethods.FileVersionInfoFlags.FILE_VER_GET_NEUTRAL);
                if (localisedData == null)
                {
                    fillPropertiesByEmpty();
                    return;
                }

                // Read the neutral file version information data.
                byte[] neutralData = readFileVersionInfoData(UncFilePath, NativeMethods.FileVersionInfoFlags.FILE_VER_GET_NEUTRAL);
                if (neutralData == null)
                {
                    fillPropertiesByEmpty();
                    return;
                }

                // File version
                FileVersion = extractFileVersion(neutralData);

                // Get the languages and code pages.
                NativeMethods.LangAndCodePage[] localisedLangsAndCodePages = getLanguageCodePagePairs(localisedData);
                if (localisedLangsAndCodePages != null)
                {
                    // Language ID
                    FileLanguageId = localisedLangsAndCodePages[0].Language;

                    // Code page
                    FileLanguageCodePage = localisedLangsAndCodePages[0].CodePage;

                    // File language
                    FileLanguage = getLanguageString(localisedLangsAndCodePages);
                }
                else
                {
                    FileLanguageId = 0;
                    FileLanguageCodePage = 0;
                    FileLanguage = string.Empty;
                }

                // File description
                FileDescription = extractFileDescription(localisedData);  // Use localise's data
                //FileDescription = extractFileDescription(neutralData);  // Use neutral's data

                OriginalFileName = extractOriginalFileName(neutralData);
                ProductVersion = extractProductVersion(neutralData);
                ProductName = extractProductName(localisedData);
                Platform = extractPlatform(localisedData);
                LegalTrademarks = extractLegalTrademarks(localisedData);
                LegalCopyright = extractLegalCopyright(localisedData);
                InternalName = extractInternalName(localisedData);
                CompanyName = extractCompanyName(localisedData);
            }
            catch (Exception ex)
            {
                string message = string.Format(@"An exception was thrown by the ""{0}""", UncFilePath);
                throw new DetailedFileInfoException(UncFilePath, message, ex);
            }
        }

        private static byte[] readFileVersionInfoData(string filePath, NativeMethods.FileVersionInfoFlags flags)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException(@"The file path is null or whitespace.", @"filePath");

            // Get size of version data.
            uint handleIgnored;
            uint fileVersionInfoDataSizeInBytes = NativeMethods.GetFileVersionInfoSizeExW(flags, filePath, out handleIgnored);
            if (fileVersionInfoDataSizeInBytes == 0)
            {
                int err = Marshal.GetLastWin32Error();
                switch (err)
                {
                    case WinError.ERROR_RESOURCE_DATA_NOT_FOUND:
                    case WinError.ERROR_RESOURCE_TYPE_NOT_FOUND:
                        // The file do not have file version resource.
                        return null;

                    default:
                        string message = string.Format(@"Failed GetFileVersionInfoSizeExW function with {0}. The file path is ""{1}""", err, filePath);
                        throw new Win32Exception(err, message);
                }
            }

            // Read the file version info data.
            handleIgnored = 0;
            byte[] fileVersionInfoData = new byte[fileVersionInfoDataSizeInBytes];
            flags |= NativeMethods.FileVersionInfoFlags.FILE_VER_GET_PREFETCHED;
            if (!NativeMethods.GetFileVersionInfoExW(flags, filePath, handleIgnored, (uint)fileVersionInfoData.Length, fileVersionInfoData))
            {
                int err = Marshal.GetLastWin32Error();
                string message = string.Format(@"Failed GetFileVersionInfoExW function with {0}. The file path is ""{1}""", err, filePath);
                throw new Win32Exception(err, message);
            }

            return fileVersionInfoData;
        }

        private void fillPropertiesByEmpty()
        {
            FileDescription = string.Empty;
            FileVersion = string.Empty;
            FileLanguage = string.Empty;
            FileLanguageId = 0;
            FileLanguageCodePage = 0;
            OriginalFileName = string.Empty;
            ProductVersion = string.Empty;
            ProductName = string.Empty;
            Platform = string.Empty;
            LegalTrademarks = string.Empty;
            LegalCopyright = string.Empty;
            InternalName = string.Empty;
            CompanyName = string.Empty;
        }

        private string extractFileVersion(byte[] fileVerInfoData)
        {
            // Read the file version from the version information resource.
            IntPtr value;
            uint valueLength;
            if (NativeMethods.VerQueryValueW(fileVerInfoData, @"\", out value, out valueLength))
            {
                NativeMethods.VS_FIXEDFILEINFO fixedFileInfo;
                fixedFileInfo = (NativeMethods.VS_FIXEDFILEINFO)Marshal.PtrToStructure(value, typeof(NativeMethods.VS_FIXEDFILEINFO));

                return string.Format(@"{0}.{1}.{2}.{3}",
                        NativeMethodHelper.GetHighWord(fixedFileInfo.FileVersionMS), NativeMethodHelper.GetLowWord(fixedFileInfo.FileVersionMS),
                        NativeMethodHelper.GetHighWord(fixedFileInfo.FileVersionLS), NativeMethodHelper.GetLowWord(fixedFileInfo.FileVersionLS));
            }

            // Try with language and code page if above failed.
            NativeMethods.LangAndCodePage[] langsAndCodePages = getLanguageCodePagePairs(fileVerInfoData);
            if (langsAndCodePages == null) return string.Empty;

            // Get file version information value.
            return getFileVersionInfoValue(fileVerInfoData, langsAndCodePages, @"FileVersion");
        }

        private static string getLanguageString(NativeMethods.LangAndCodePage[] langsAndCodePages)
        {
            string[] languageStrings = new string[langsAndCodePages.Length];

            for (int i = 0; i < langsAndCodePages.Length; i++)
            {
                // Get the language description string.
                StringBuilder languageString = new StringBuilder(1024);
                uint err = NativeMethods.VerLanguageNameW(langsAndCodePages[i].Language, languageString, (uint)languageString.Capacity);
                if (err == 0)
                {
                    string message = string.Format(@"Failed VerLanguageNameW function.");
                    throw new Win32Exception((int)err, message);
                }

                // Store the language description string to array.
                languageStrings[i] = languageString.ToString();
            }

            // Join the language description strings.
            return string.Join(@", ", languageStrings);
        }

        private static string extractFileDescription(byte[] fileVerInfoData)
        {
            if (fileVerInfoData == null) throw new ArgumentException(@"Null reference.", @"fileVerInfoData");
            if (fileVerInfoData.Length == 0) throw new ArgumentOutOfRangeException(@"fileVerInfoData", fileVerInfoData.Length, @"The length is zero.");

            // Get the languages and code pages.
            NativeMethods.LangAndCodePage[] langsAndCodePages = getLanguageCodePagePairs(fileVerInfoData);
            if (langsAndCodePages == null) return string.Empty;

            // Get file version information value.
            return getFileVersionInfoValue(fileVerInfoData, langsAndCodePages, @"FileDescription");
        }

        private string extractOriginalFileName(byte[] fileVerInfoData)
        {
            if (fileVerInfoData == null) throw new ArgumentException(@"Null reference.", @"fileVerInfoData");
            if (fileVerInfoData.Length == 0) throw new ArgumentOutOfRangeException(@"fileVerInfoData", fileVerInfoData.Length, @"The length is zero.");

            // Get the languages and code pages.
            NativeMethods.LangAndCodePage[] langsAndCodePages = getLanguageCodePagePairs(fileVerInfoData);
            if (langsAndCodePages == null) return string.Empty;

            // Get file version information value.
            return getFileVersionInfoValue(fileVerInfoData, langsAndCodePages, @"OriginalFileName");
        }

        private string extractInternalName(byte[] fileVerInfoData)
        {
            if (fileVerInfoData == null) throw new ArgumentException(@"Null reference.", @"fileVerInfoData");
            if (fileVerInfoData.Length == 0) throw new ArgumentOutOfRangeException(@"fileVerInfoData", fileVerInfoData.Length, @"The length is zero.");

            // Get the languages and code pages.
            NativeMethods.LangAndCodePage[] langsAndCodePages = getLanguageCodePagePairs(fileVerInfoData);
            if (langsAndCodePages == null) return string.Empty;

            // Get file version information value.
            return getFileVersionInfoValue(fileVerInfoData, langsAndCodePages, @"InternalName");
        }

        private string extractProductName(byte[] fileVerInfoData)
        {
            if (fileVerInfoData == null) throw new ArgumentException(@"Null reference.", @"fileVerInfoData");
            if (fileVerInfoData.Length == 0) throw new ArgumentOutOfRangeException(@"fileVerInfoData", fileVerInfoData.Length, @"The length is zero.");

            // Get the languages and code pages.
            NativeMethods.LangAndCodePage[] langsAndCodePages = getLanguageCodePagePairs(fileVerInfoData);
            if (langsAndCodePages == null) return string.Empty;

            // Get file version information value.
            return getFileVersionInfoValue(fileVerInfoData, langsAndCodePages, @"ProductName");
        }

        private string extractProductVersion(byte[] fileVerInfoData)
        {
            if (fileVerInfoData == null) throw new ArgumentException(@"Null reference.", @"fileVerInfoData");
            if (fileVerInfoData.Length == 0) throw new ArgumentOutOfRangeException(@"fileVerInfoData", fileVerInfoData.Length, @"The length is zero.");

            // Get the languages and code pages.
            NativeMethods.LangAndCodePage[] langsAndCodePages = getLanguageCodePagePairs(fileVerInfoData);
            if (langsAndCodePages == null) return string.Empty;

            // Get file version information value.
            return getFileVersionInfoValue(fileVerInfoData, langsAndCodePages, @"ProductVersion");
        }

        private string extractPlatform(byte[] fileVerInfoData)
        {
            if (fileVerInfoData == null) throw new ArgumentException(@"Null reference.", @"fileVerInfoData");
            if (fileVerInfoData.Length == 0) throw new ArgumentOutOfRangeException(@"fileVerInfoData", fileVerInfoData.Length, @"The length is zero.");

            // Get the languages and code pages.
            NativeMethods.LangAndCodePage[] langsAndCodePages = getLanguageCodePagePairs(fileVerInfoData);
            if (langsAndCodePages == null) return string.Empty;

            // Get file version information value.
            return getFileVersionInfoValue(fileVerInfoData, langsAndCodePages, @"Platform");
        }

        private string extractCompanyName(byte[] fileVerInfoData)
        {
            if (fileVerInfoData == null) throw new ArgumentException(@"Null reference.", @"fileVerInfoData");
            if (fileVerInfoData.Length == 0) throw new ArgumentOutOfRangeException(@"fileVerInfoData", fileVerInfoData.Length, @"The length is zero.");

            // Get the languages and code pages.
            NativeMethods.LangAndCodePage[] langsAndCodePages = getLanguageCodePagePairs(fileVerInfoData);
            if (langsAndCodePages == null) return string.Empty;

            // Get file version information value.
            return getFileVersionInfoValue(fileVerInfoData, langsAndCodePages, @"CompanyName");
        }

        private string extractLegalCopyright(byte[] fileVerInfoData)
        {
            if (fileVerInfoData == null) throw new ArgumentException(@"Null reference.", @"fileVerInfoData");
            if (fileVerInfoData.Length == 0) throw new ArgumentOutOfRangeException(@"fileVerInfoData", fileVerInfoData.Length, @"The length is zero.");

            // Get the languages and code pages.
            NativeMethods.LangAndCodePage[] langsAndCodePages = getLanguageCodePagePairs(fileVerInfoData);
            if (langsAndCodePages == null) return string.Empty;

            // Get file version information value.
            return getFileVersionInfoValue(fileVerInfoData, langsAndCodePages, @"LegalCopyright");
        }

        private string extractLegalTrademarks(byte[] fileVerInfoData)
        {
            if (fileVerInfoData == null) throw new ArgumentException(@"Null reference.", @"fileVerInfoData");
            if (fileVerInfoData.Length == 0) throw new ArgumentOutOfRangeException(@"fileVerInfoData", fileVerInfoData.Length, @"The length is zero.");

            // Get the languages and code pages.
            NativeMethods.LangAndCodePage[] langsAndCodePages = getLanguageCodePagePairs(fileVerInfoData);
            if (langsAndCodePages == null) return string.Empty;

            // Get file version information value.
            return getFileVersionInfoValue(fileVerInfoData, langsAndCodePages, @"LegalTrademarks");
        }

        private static NativeMethods.LangAndCodePage[] getLanguageCodePagePairs(byte[] fileVerInfoData)
        {
            if (fileVerInfoData == null) throw new ArgumentException(@"Null reference.", @"fileVerInfoData");
            if (fileVerInfoData.Length == 0) throw new ArgumentOutOfRangeException(@"fileVerInfoData", fileVerInfoData.Length, @"The length is zero.");

            // Read the buffer of languages and code pages.
            IntPtr langsAndCodePagesBuffer;
            uint bufferLength;
            if (!NativeMethods.VerQueryValueW(fileVerInfoData, @"\VarFileInfo\Translation", out langsAndCodePagesBuffer, out bufferLength))
            {
                int err = Marshal.GetLastWin32Error();
                string message = string.Format(@"Failed VerQueryValueW function with {0}.", err);
                throw new Win32Exception(err, message);
            }

            // Do not contain the languages and code pages.
            if (bufferLength == 0) return null;

            // Allocate the languages and code pages array.
            long numElements = bufferLength / Marshal.SizeOf(typeof(NativeMethods.LangAndCodePage));
            NativeMethods.LangAndCodePage[] langsAndCodePages = new NativeMethods.LangAndCodePage[numElements];

            // Extract the languages and code pages as array.
            int langsAndCodePagesBufferStartAddress = langsAndCodePagesBuffer.ToInt32();
            for (int i = 0; i < langsAndCodePages.Length; i++)
            {
                // Caluculate the pointer of next language and code page structure.
                int offsetNextElement = i * Marshal.SizeOf(typeof(NativeMethods.LangAndCodePage));
                IntPtr langAndCodePagePtr = new IntPtr(langsAndCodePagesBufferStartAddress + offsetNextElement);

                // Get structure from the languages and code pages buffer.
                NativeMethods.LangAndCodePage langAndCodePage = (NativeMethods.LangAndCodePage)Marshal.PtrToStructure(langAndCodePagePtr, typeof(NativeMethods.LangAndCodePage));

                // Store to the array.
                langsAndCodePages[i] = langAndCodePage;
            }

            return langsAndCodePages;
        }

        private static string getFileVersionInfoValue(byte[] fileVerInfoData, NativeMethods.LangAndCodePage[] langsAndCodePages, string fieldName)
        {
            if (fileVerInfoData == null) throw new ArgumentException(@"Null reference.", @"fileVerInfoData");
            if (fileVerInfoData.Length == 0) throw new ArgumentOutOfRangeException(@"fileVerInfoData", fileVerInfoData.Length, @"The length is zero.");
            if (langsAndCodePages == null) throw new ArgumentException(@"Null reference.", @"langsAndCodePages");
            if (langsAndCodePages.Length == 0) throw new ArgumentOutOfRangeException(@"langsAndCodePages", langsAndCodePages.Length, @"The length is zero.");
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentException(@"The field name is null or whitespace.", @"fieldName");

            // Read the file version information value with language and code page.
            string subBlock = string.Format(@"\StringFileInfo\{0:x04}{1:x04}\{2}", langsAndCodePages[0].Language, langsAndCodePages[0].CodePage, fieldName);
            IntPtr value;
            uint valueLength;
            if (NativeMethods.VerQueryValueW(fileVerInfoData, subBlock, out value, out valueLength))
            {
                return Marshal.PtrToStringUni(value);
            }

            // Try with English(United States) & Unicode (BMP of ISO 10646) - 0409 04B0
            subBlock = string.Format(@"\StringFileInfo\040904B0\{0}", fieldName);
            if (NativeMethods.VerQueryValueW(fileVerInfoData, subBlock, out value, out valueLength))
            {
                return Marshal.PtrToStringUni(value);
            }

            // Try with English(United States) & Windows 3.1 US - 0409 04E4
            subBlock = string.Format(@"\StringFileInfo\040904E4\{0}", fieldName);
            if (NativeMethods.VerQueryValueW(fileVerInfoData, subBlock, out value, out valueLength))
            {
                return Marshal.PtrToStringUni(value);
            }

            // Try with English(United States) & no code page - 0409 0000
            subBlock = string.Format(@"\StringFileInfo\04090000\{0}", fieldName);
            if (NativeMethods.VerQueryValueW(fileVerInfoData, subBlock, out value, out valueLength))
            {
                return Marshal.PtrToStringUni(value);
            }

            // Give up
            return string.Empty;
        }

        private static string getNormalizedPathAsUncPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException(@"The file path is null or whitespace.", @"filePath");

            // The file path is already the UNC path.
            if (isUncPath(filePath)) return filePath;

            // The file path is the share path.
            if (filePath.StartsWith(@"\\")) return @"\\?\UNC\" + filePath.Substring(2);

            // The file path is the local path.
            return @"\\?\" + filePath;
        }

        private static bool isUncPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException(@"The file path is null or whitespace.", @"filePath");

            return filePath.StartsWith(@"\\?\");
        }
    }
}
