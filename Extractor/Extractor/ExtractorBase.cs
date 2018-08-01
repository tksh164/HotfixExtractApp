using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Extractor.Extractor
{
    public abstract class ExtractorBase : IExtractor
    {
        public string DestinationDirectoryPath { get; set; }
        public string SourcePackagePath { get; set; }

        public ExtractorBase()
        {}

        public abstract void Extract();
    }
}
