using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using VsInterop = Microsoft.VisualStudio.OLE.Interop;  // STATSTG is colision to System.Runtime.InteropServices.
using System.ComponentModel;
using Extractor.Helper;
using Extractor.Win32Native;

namespace Extractor.Extractor
{
    public class MspExtractor : ExtractorBase
    {
        public override void Extract()
        {
            Trace.Assert(!string.IsNullOrWhiteSpace(SourcePackagePath));
            Trace.Assert(!string.IsNullOrWhiteSpace(DestinationDirectoryPath));

            // Create destination directory if not exist.
            if (!Directory.Exists(DestinationDirectoryPath)) Directory.CreateDirectory(DestinationDirectoryPath);

            // Extract *cab files from .msp file.
            extractCabFiles();

            // Retrieve inner cab file path.
            string innerCabFilePath = getInnerCabFilePath();

            // Extract inner *.cab file.
            extractInnerCabFile(innerCabFilePath);
        }

        private void extractCabFiles()
        {
            // Open the MSI database.
            NativeMethods.MsiOpenDbPersistenceMode persistenceMode = getMsiOpenDbPersistenceMode(SourcePackagePath);
            IntPtr msiDatabaseHandle = IntPtr.Zero;
            uint err = NativeMethods.MsiOpenDatabase(SourcePackagePath, new IntPtr((uint)persistenceMode), out msiDatabaseHandle);
            if (err != WinError.ERROR_SUCCESS) throw new Win32Exception((int)err, string.Format(@"Failed: MsiOpenDatabase(): Failed to open MSI database ""{0}""", SourcePackagePath));

            try
            {
                // Create the view from query.
                IntPtr msiViewHandle = IntPtr.Zero;
                err = NativeMethods.MsiDatabaseOpenView(msiDatabaseHandle, @"SELECT Name, Data FROM _Streams", out msiViewHandle);
                if (err != WinError.ERROR_SUCCESS) throw new Win32Exception((int)err, string.Format(@"Failed: MsiDatabaseOpenView(): Failed to open view ""{0}""", SourcePackagePath));

                try
                {
                    // Execute the view query for fetching streams.
                    err = NativeMethods.MsiViewExecute(msiViewHandle, IntPtr.Zero);
                    if (err != WinError.ERROR_SUCCESS) throw new Win32Exception((int)err, string.Format(@"Failed: MsiViewExecute(): ""{0}""", SourcePackagePath));

                    while (true)
                    {
                        // Fetch the stream from view.
                        IntPtr msiRecordHandle = IntPtr.Zero;
                        err = NativeMethods.MsiViewFetch(msiViewHandle, out msiRecordHandle);
                        if (err != WinError.ERROR_SUCCESS) break;

                        try
                        {
                            // Save stream to file.
                            saveStream(msiRecordHandle, DestinationDirectoryPath, true);
                        }
                        finally
                        {
                            // Close MSI record handle.
                            NativeMethods.MsiCloseHandle(msiRecordHandle);
                        }
                    }
                }
                finally
                {
                    // Close MSI view handle.
                    if (msiViewHandle != IntPtr.Zero) NativeMethods.MsiCloseHandle(msiViewHandle);
                }
            }
            finally
            {
                // Close MSI database handle.
                if (msiDatabaseHandle != IntPtr.Zero) NativeMethods.MsiCloseHandle(msiDatabaseHandle);
            }
        }

        private static NativeMethods.MsiOpenDbPersistenceMode getMsiOpenDbPersistenceMode(string mspFilePath)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(mspFilePath));

            // Open root storage.
            VsInterop.IStorage rootStorage = null;
            uint hr = NativeMethods.StgOpenStorage(mspFilePath, null, NativeMethods.STGM.STGM_READ | NativeMethods.STGM.STGM_SHARE_EXCLUSIVE, null, 0, out rootStorage);
            if (HResultHelper.IsFailed(hr) || rootStorage == null) throw new Win32Exception((int)hr, string.Format(@"Failed: StgOpenStorage(): Failed to open root storage ""{0}""", mspFilePath));

            try
            {
                // Decide to persistance mode.
                NativeMethods.MsiOpenDbPersistenceMode persistenceMode = NativeMethods.MsiOpenDbPersistenceMode.MSIDBOPEN_READONLY;
                if (isPatch(rootStorage)) persistenceMode = NativeMethods.MsiOpenDbPersistenceMode.MSIDBOPEN_READONLY | NativeMethods.MsiOpenDbPersistenceMode.MSIDBOPEN_PATCHFILE;
                return persistenceMode;
            }
            finally
            {
                // Release COM object of root storage.
                Marshal.ReleaseComObject(rootStorage);
            }
        }

        private static bool isPatch(VsInterop.IStorage storage)
        {
            Debug.Assert(storage != null);

            Guid CLSID_MsiPatch = new Guid(@"{000c1086-0000-0000-c000-000000000046}");  // MSI Patch CLSID

            VsInterop.STATSTG[] stg = new VsInterop.STATSTG[1];
            storage.Stat(stg, (uint)VsInterop.STATFLAG.STATFLAG_NONAME);
            return stg[0].clsid == CLSID_MsiPatch;
        }

        private static void saveStream(IntPtr msiRecordHandle, string saveDirectory, bool includeExtension)
        {
            Debug.Assert(msiRecordHandle != IntPtr.Zero);
            Debug.Assert(!string.IsNullOrWhiteSpace(saveDirectory));

            // Get the name of the stream.
            string streamName = getString(msiRecordHandle, 1);

            // Not used "\005SummaryInformation" and "\005DigitalSignature" streams.
            if (streamName.StartsWith("\u0005")) return;

            byte[] fileData;
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // Read data from MSI stream.
                while (true)
                {
                    // Read data to buffer from MSI stream.
                    byte[] buffer = new byte[4098];
                    int bufferLength = buffer.Length;
                    uint err = NativeMethods.MsiRecordReadStream(msiRecordHandle, 2, buffer, ref bufferLength);
                    if (err != WinError.ERROR_SUCCESS) throw new Win32Exception((int)err, @"Failed: MsiRecordReadStream(): Could not read from stream.");

                    // Reached the end of MSI stream.
                    if (bufferLength == 0) break;

                    // Write data to memory stream.
                    writer.Write(buffer);
                }

                // Get the entire data of file.
                fileData = stream.ToArray();
            }

            // Save to file.
            string filePath = saveDirectory + Path.DirectorySeparatorChar + streamName;
            if (includeExtension) filePath += getStreamFileExtension(fileData);
            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            using (BinaryWriter fileWriter = new BinaryWriter(fileStream))
            {
                fileWriter.Write(fileData);
            }
        }

        private static string getString(IntPtr msiRecordHandle, uint field)
        {
            Debug.Assert(msiRecordHandle != IntPtr.Zero);
            Debug.Assert(field > 0);

            int bufferLength = 0;
            uint err = NativeMethods.MsiRecordGetString(msiRecordHandle, field, new StringBuilder(0), ref bufferLength);
            if (err != WinError.ERROR_MORE_DATA) throw new Win32Exception((int)err, @"Failed: MsiRecordGetString(): First time.");

            bufferLength++;  // for null char. (need ?)
            StringBuilder buffer = new StringBuilder(bufferLength);
            err = NativeMethods.MsiRecordGetString(msiRecordHandle, field, buffer, ref bufferLength);
            if (err != WinError.ERROR_SUCCESS) throw new Win32Exception((int)err, @"Failed: MsiRecordGetString(): Second time.");

            return buffer.ToString();
        }

        private static string getStreamFileExtension(byte[] fileData)
        {
            Debug.Assert(fileData != null);

            // .cab, MSCF
            byte[] signature = new byte[] { 0x4d, 0x53, 0x43, 0x46 };
            if (compareArray<byte>(fileData, signature, signature.Length)) return @".cab";

            // .exe/.dll, MZ
            signature = new byte[] { 0x4d, 0x5a };
            if (compareArray<byte>(fileData, signature, signature.Length)) return @".dll";

            // .ico
            signature = new byte[] { 0x0, 0x0, 0x1, 0x0 };
            if (compareArray<byte>(fileData, signature, signature.Length)) return @".ico";

            // .bmp, BM
            signature = new byte[] { 0x42, 0x4d };
            if (compareArray<byte>(fileData, signature, signature.Length)) return @".bmp";

            // .gif, GIF
            signature = new byte[] { 0x47, 0x49, 0x46 };
            if (compareArray<byte>(fileData, signature, signature.Length)) return @".gif";

            // Unknown
            return string.Empty;
        }

        private static bool compareArray<T>(T[] a, T[] b, int length)
        {
            Debug.Assert(a.Length >= length);
            Debug.Assert(b.Length >= length);

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < length; i++)
            {
                if (!comparer.Equals(a[i], b[i])) return false;
            }
            return true;
        }
        
        private string getInnerCabFilePath()
        {
            List<string> resultPaths = new List<string>();
            IEnumerable<string> extractedCabFilePaths = Directory.EnumerateFiles(DestinationDirectoryPath, @"*.cab", SearchOption.TopDirectoryOnly);
            foreach (string extractedCabFilePath in extractedCabFilePaths)
            {
                resultPaths.Add(extractedCabFilePath);
            }

            // Expect one cab file path.
            if (resultPaths.Count != 1) throw new PackageStructureException(@"Unexpected package structure.");

            // Retrieve inner *.cab file path.
            string innerCabFilePath = null;
            foreach (string path in resultPaths) innerCabFilePath = path;
            return innerCabFilePath;
        }

        private void extractInnerCabFile(string innerCabFilePath)
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
