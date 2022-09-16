using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceDll.Utils
{
    internal class ASCUtils
    {
        internal static string GetTrimString( string aValue, string aDefaultValue)
        {
            string retval = aDefaultValue;
            if (!String.IsNullOrEmpty(aValue))
                retval = aValue.Trim();
            return (retval);
        }

    }
}
