using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extractor.Win32Native
{
    internal sealed class WinError
    {
        public const int ERROR_SUCCESS = 0;
        //public const int ERROR_INVALID_HANDLE = 6;
        //public const int ERROR_INVALID_PARAMETER = 87;
        //public const int ERROR_OPEN_FAILED = 110;
        public const int ERROR_MORE_DATA = 234;
        //public const int ERROR_NO_MORE_ITEMS = 259;
        //public const int ERROR_INVALID_HANDLE_STATE = 1609;
        //public const int ERROR_BAD_QUERY_SYNTAX = 1615;
        //public const int ERROR_FUNCTION_FAILED = 1627;
        //public const int ERROR_CREATE_FAILED = 1631;
        //public const int ERROR_INVALID_DATATYPE = 1804;
        public const int ERROR_RESOURCE_DATA_NOT_FOUND = 1812;
        public const int ERROR_RESOURCE_TYPE_NOT_FOUND = 1813;
    };
}
