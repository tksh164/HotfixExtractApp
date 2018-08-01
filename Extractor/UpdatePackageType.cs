using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extractor
{
    public enum UpdatePackageType
    {
        Unknown,
        Msu,
        MsiMsp,
        ExeUnknown,
        ExeDotNetFramework,
        ExeMsxml,
        ExeWindowsMedia,
        ExeWindowsMovieMaker,
    }
}
