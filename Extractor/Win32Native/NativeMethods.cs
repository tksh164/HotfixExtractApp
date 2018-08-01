using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using FileTime = System.Runtime.InteropServices.ComTypes.FILETIME;
using VsInterop = Microsoft.VisualStudio.OLE.Interop;  // STATSTG is colision to System.Runtime.InteropServices.

namespace Extractor.Win32Native
{
    internal static class NativeMethods
    {
        #region --- The File information related ---

        // WIN32_FILE_ATTRIBUTE_DATA structure
        [StructLayout(LayoutKind.Sequential)]
        public struct WIN32_FILE_ATTRIBUTE_DATA
        {
            public uint FileAttributes;
            public FileTime CreationTime;
            public FileTime LastAccessTime;
            public FileTime LastWriteTime;
            public uint FileSizeHigh;
            public uint FileSizeLow;
        };

        // GET_FILEEX_INFO_LEVELS enumeration
        public enum GET_FILEEX_INFO_LEVELS : int
        {
            GetFileExInfoStandard = 0,
            GetFileExMaxInfoLevel = 1
        };

        // Flags for GetFileVersionInfoSizeEx, GetFileVersionInfoExW
        [Flags]
        public enum FileVersionInfoFlags : uint
        {
            FILE_VER_GET_LOCALISED = 0x01U,
            FILE_VER_GET_NEUTRAL = 0x02U,
            FILE_VER_GET_PREFETCHED = 0x04U
        };

        // VS_FIXEDFILEINFO structure
        [StructLayout(LayoutKind.Sequential)]
        public struct VS_FIXEDFILEINFO
        {
            public uint Signature;         // e.g. 0xfeef04bd
            public uint StrucVersion;      // e.g. 0x00000042 = "0.42"
            public uint FileVersionMS;     // e.g. 0x00030075 = "3.75"
            public uint FileVersionLS;     // e.g. 0x00000031 = "0.31"
            public uint ProductVersionMS;  // e.g. 0x00030010 = "3.10"
            public uint ProductVersionLS;  // e.g. 0x00000031 = "0.31"
            public uint FileFlagsMask;     // = 0x3F for version "0.42"
            public uint FileFlags;         // e.g. VFF_DEBUG | VFF_PRERELEASE
            public uint FileOS;            // e.g. VOS_DOS_WINDOWS16
            public uint FileType;          // e.g. VFT_DRIVER
            public uint FileSubtype;       // e.g. VFT2_DRV_KEYBOARD
            public uint FileDateMS;        // e.g. 0
            public uint FileDateLS;        // e.g. 0
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct LangAndCodePage
        {
            public ushort Language;
            public ushort CodePage;
        };

        [DllImport(@"kernel32.dll", EntryPoint = @"GetFileAttributesExW", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetFileAttributesExW(string fileName, NativeMethods.GET_FILEEX_INFO_LEVELS fileInfoLevel, out NativeMethods.WIN32_FILE_ATTRIBUTE_DATA fileInformation);

        [DllImport(@"version.dll", EntryPoint = @"GetFileVersionInfoSizeExW", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.U4)]
        public static extern uint GetFileVersionInfoSizeExW(FileVersionInfoFlags flag, string fileName, out uint handleIgnored);

        [DllImport(@"version.dll", EntryPoint = @"GetFileVersionInfoExW", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetFileVersionInfoExW(FileVersionInfoFlags flag, string fileName, uint handleIgnored, uint fileVersionInfoDataSizeInBytes, byte[] fileVersionInfoData);

        [DllImport(@"version.dll", EntryPoint = @"VerQueryValueW", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VerQueryValueW(byte[] fileVersionInfoData, string resourcePath, out IntPtr buffer, out uint bufferSizeInBytes);

        [DllImport(@"kernel32.dll", EntryPoint = @"VerLanguageNameW", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.U4)]
        public static extern uint VerLanguageNameW(uint languageID, StringBuilder languageString, uint languageStringSizeInCharacters);

        #endregion --- The File information related ---

        #region --- The MSP package related ---

        // STGM Constants
        [Flags()]
        public enum STGM : uint
        {
            // Access
            STGM_READ = 0x0,
            STGM_WRITE = 0x1,
            STGM_READWRITE = 0x2,
            // Sharing
            STGM_SHARE_DENY_NONE = 0x40,
            STGM_SHARE_DENY_READ = 0x30,
            STGM_SHARE_DENY_WRITE = 0x20,
            STGM_SHARE_EXCLUSIVE = 0x10,
            STGM_PRIORITY = 0x40000,
            // Creation
            STGM_CREATE = 0x1000,
            STGM_CONVERT = 0x20000,
            STGM_FAILIFTHERE = 0x0,
            // Transactioning
            STGM_DIRECT = 0x0,
            STGM_TRANSACTED = 0x10000,
            // Transactioning Performance
            STGM_NOSCRATCH = 0x100000,
            STGM_NOSNAPSHOT = 0x200000,
            // Direct SWMR and Simple
            STGM_SIMPLE = 0x8000000,
            STGM_DIRECT_SWMR = 0x400000,
            // Delete On Release
            STGM_DELETEONRELEASE = 0x4000000,
        };

        [Flags()]
        public enum MsiOpenDbPersistenceMode : uint
        {
            MSIDBOPEN_READONLY = 0,  // database open read-only, no persistent changes
            MSIDBOPEN_TRANSACT = 1,  // database read/write in transaction mode
            MSIDBOPEN_DIRECT = 2,  // database direct read/write without transaction
            MSIDBOPEN_CREATE = 3,  // create new database, transact mode read/write
            MSIDBOPEN_CREATEDIRECT = 4,  // create new database, direct mode read/write
            MSIDBOPEN_PATCHFILE = 32, //32/sizeof(*MSIDBOPEN_READONLY) // add flag to indicate patch file
        };

        [DllImport(@"ole32.dll", EntryPoint = @"StgOpenStorage", SetLastError = false, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static uint StgOpenStorage(string pwcsName, VsInterop.IStorage pstgPriority, STGM grfMode, string[] snbExclude, int reserved, out VsInterop.IStorage ppstgOpen);

        [DllImport(@"msi.dll", EntryPoint = @"MsiOpenDatabaseW", SetLastError = false, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static uint MsiOpenDatabase(string szDatabasePath, IntPtr phPersist, out IntPtr phDatabase);

        [DllImport(@"msi.dll", EntryPoint = @"MsiDatabaseOpenViewW", SetLastError = false, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static uint MsiDatabaseOpenView(IntPtr hDatabase, string szQuery, out IntPtr phView);

        [DllImport(@"msi.dll", EntryPoint = @"MsiViewExecute", SetLastError = false, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static uint MsiViewExecute(IntPtr hView, IntPtr hRecord);

        [DllImport(@"msi.dll", EntryPoint = @"MsiViewFetch", SetLastError = false, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern uint MsiViewFetch(IntPtr hView, out IntPtr hRecord);

        [DllImport(@"msi.dll", EntryPoint = @"MsiCloseHandle", SetLastError = false, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static uint MsiCloseHandle(IntPtr hAny);

        [DllImport(@"msi.dll", EntryPoint = @"MsiRecordGetStringW", SetLastError = false, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static uint MsiRecordGetString(IntPtr hRecord, uint iField, StringBuilder szValueBuf, ref int pcchValueBuf);

        [DllImport(@"msi.dll", EntryPoint = @"MsiRecordReadStream", SetLastError = false, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static uint MsiRecordReadStream(IntPtr hRecord, uint iField, [Out] byte[] szDataBuf, ref int pcbDataBuf);

        #endregion --- The MSP package related ---
    }
}
