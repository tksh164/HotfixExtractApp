using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extractor.Extractor
{
    public interface IExtractor
    {
        string SourcePackagePath { get; set; }
        string DestinationDirectoryPath { get; set; }

        void Extract();
    }
}
