using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceDll.Utils
{

    internal class ASCUtils
    {
        private static Dictionary<string, Dictionary<string, int>> myTableList = new Dictionary<string, Dictionary<string, int>>();
        internal static string GetTrimString( string aValue, string aDefaultValue)
        {
            string retval = aDefaultValue;
            if (!String.IsNullOrEmpty(aValue))
                retval = aValue.Trim();
            return (retval);
        }

        internal static void CheckAndAppend(ref string updstr, string aTblName, string aFieldname, string aValue)
        {
            Dictionary<String, int> myFieldList; //
            if( myTableList.ContainsKey( aTblName))
                myFieldList = myTableList[aTblName];
            else
            {
                myFieldList = new Dictionary<string, int>();
                myTableList.Add(aTblName, myFieldList);
            }
            int maxlen = 0;
            if( myFieldList.ContainsKey( aFieldname))
            {
                maxlen = myFieldList[aFieldname];
            }
            else
            {
                string tmp = string.Empty;
                ascLibrary.ascDBUtils.libASCDBUtils.ReadFieldFromDB("SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='" + aTblName + "' AND COLUMN_NAME='" + aFieldname + "' ", "", ref tmp);
                maxlen = Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(tmp, 0));
                myFieldList.Add(aFieldname, maxlen);
            }
            if (!String.IsNullOrEmpty(aValue))
            {
                if ((maxlen > 0) && (maxlen < aValue.Length))
                    throw new Exception("Value for Column " + aFieldname + " in table " + aTblName + " is too large.");

                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, aFieldname, aValue);
            }
        }
        

    }
}
