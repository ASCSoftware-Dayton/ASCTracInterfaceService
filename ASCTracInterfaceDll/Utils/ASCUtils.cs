using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceDll.Utils
{

    internal class ASCUtils
    {
        private static Dictionary<string, Dictionary<string, string>> myTableList = new Dictionary<string, Dictionary<string, string>>();
        internal static string GetTrimString( string aValue, string aDefaultValue)
        {
            string retval = aDefaultValue;
            if (!String.IsNullOrEmpty(aValue))
                retval = aValue.Trim();
            return (retval);
        }

        internal static void CheckAndAppend(ref string updstr, string aTblName, string aFieldname, string aValue)
        {
            string errmsg = string.Empty;
            CheckAndAppend(ref updstr, aTblName, aFieldname, aValue, ref errmsg);
            if (!String.IsNullOrEmpty(errmsg))
                throw new Exception(errmsg);
        }
            internal static void CheckAndAppend(ref string updstr, string aTblName, string aFieldname, string aValue, ref string errmsg)
        {
            Dictionary<String, string> myFieldList; //
            if( myTableList.ContainsKey( aTblName))
                myFieldList = myTableList[aTblName];
            else
            {
                myFieldList = new Dictionary<string, string>();
                myTableList.Add(aTblName, myFieldList);
            }
            long maxlen = 0;
            bool fISNullable;
            if( myFieldList.ContainsKey( aFieldname))
            {
                string tmp = myFieldList[aFieldname];
                maxlen = ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0);
                fISNullable = tmp.Equals("YES");
            }
            else
            {
                string tmp = string.Empty;
                ascLibrary.ascDBUtils.libASCDBUtils.ReadFieldFromDB("SELECT CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='" + aTblName + "' AND COLUMN_NAME='" + aFieldname + "' ", "", ref tmp);
                myFieldList.Add(aFieldname, tmp);

                maxlen = ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0);
                fISNullable = tmp.Equals("YES");
            }
            if (!String.IsNullOrEmpty(aValue))
            {
                if ((maxlen > 0) && (maxlen < aValue.Length))
                    errmsg += "Value for Column " + aFieldname + " in table " + aTblName + " is too large.\r\n";

                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, aFieldname, aValue);
            }
            else if ( !fISNullable)
                errmsg += "Value for Column " + aFieldname + " in table " + aTblName + " is required.\r\n";
        }


    }
}
