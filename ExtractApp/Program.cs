using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extractor;
using System.IO;

namespace ExtractApp
{
    class Program
    {
        static int Main(string[] args)
        {
            #region --- Extract MSU ---
            //MsuExtractor extractor = new MsuExtractor()
            //{
            //    SourcePackagePath = @"D:\HotfixDBv3\IE8-Windows6.0-xxxx-KB2744842-x64.msu",
            //    DestinationDirectoryPath = @"D:\HotfixDBv3\Dest",
            //};
            //extractor.Extract();
            #endregion --- Extract MSU ---

            #region --- Extract MSP ---
            //MspExtractor extractor = new MspExtractor()
            //{
            //    SourcePackagePath = @"D:\HotfixDBv3\ftp75_en_x64_kb2716513.msp",
            //    DestinationDirectoryPath = @"D:\HotfixDBv3\msp-dest",
            //};
            //extractor.Extract();
            #endregion

            #region --- Detailed file information ---

            if (args.Length < 1)
            {
                Console.WriteLine(@"Usage: ExeName.exe <FilePath>");
                return -1;
            }

            string filePath = args[0];

            DetailedFileInfo fileInfo = new DetailedFileInfo(filePath);

            int exitCode = 0;

            if (!string.IsNullOrWhiteSpace(fileInfo.OriginalFileName))
            {
                string sourcePath = fileInfo.OriginalFilePath;
                string destPath = Path.GetDirectoryName(fileInfo.OriginalFilePath) + Path.DirectorySeparatorChar + fileInfo.OriginalFileName;

                int duplicationNumber = 1;
                while (File.Exists(destPath))
                {
                    destPath = Path.GetDirectoryName(fileInfo.OriginalFilePath) + Path.DirectorySeparatorChar + fileInfo.OriginalFileName + '.' + duplicationNumber.ToString();
                    duplicationNumber++;
                }

                //if (!File.Exists(destPath))
                //{
                    File.Move(sourcePath, destPath);
                    Console.WriteLine(@"Before: ""{0}"", After: ""{0}""", sourcePath, destPath);
                //}
                //else
                //{
                //    Console.WriteLine(@"Source: ""{0}"", Destination: ""{0}"" is already exists.", sourcePath, destPath);
                //    exitCode = 1;
                //}
            }
            else
            {
                Console.WriteLine(@"The original file name is not present.");
                exitCode = 2;
            }

            #endregion --- Detailed file information ---

            return exitCode;
        }
    }
}
