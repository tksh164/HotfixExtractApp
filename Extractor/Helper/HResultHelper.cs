using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extractor.Helper
{
    internal static class HResultHelper
    {
        public static bool IsSucceeded(uint hr)
        {
            return hr >= 0;
        }

        public static bool IsFailed(uint hr)
        {
            return hr < 0;
        }
    }
}
