using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extractor
{
    public sealed class DetailedFileInfoException : Exception
    {
        public string FilePath { get; private set; }

        public DetailedFileInfoException(string filePath) : base()
        {
            FilePath = filePath;
        }

        public DetailedFileInfoException(string filePath, string message) : base(message)
        {
            FilePath = filePath;
        }

        public DetailedFileInfoException(string filePath, string message, Exception innerException) : base(message, innerException)
        {
            FilePath = filePath;
        }
    }
}
