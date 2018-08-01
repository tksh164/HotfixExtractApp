using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extractor
{
    public class PackageStructureException : Exception
    {
        public PackageStructureException() : base()
        {
        }

        public PackageStructureException(string message) : base(message)
        {
        }
    }
}
