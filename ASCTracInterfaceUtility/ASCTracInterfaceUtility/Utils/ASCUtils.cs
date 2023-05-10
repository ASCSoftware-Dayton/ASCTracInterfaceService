using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCTracInterfaceUtility.Utils
{
    internal class ASCUtils
    {
        internal static string ascGetNextWordTrim(ref string aInStr, string aDelim, bool TrimFlag)
        {
            string myStr = string.Empty;

            if (!String.IsNullOrEmpty(aInStr))
            {
                int i = aInStr.IndexOf(aDelim);

                if (i >= 0)
                {
                    myStr = aInStr.Substring(0, i);
                    aInStr = aInStr.Substring(i + aDelim.Length);
                }
                else
                {
                    myStr = aInStr;
                    aInStr = string.Empty;
                }
                if (TrimFlag)
                    myStr = myStr.Trim();
            }
            return myStr;
        }

        internal static string ascGetNextWord(ref string aInStr, string aDelim)
        {
            return (ascGetNextWordTrim(ref aInStr, aDelim, true));
        }
        public static string GetNextWord(ref string aInStr)
        {
            return ascGetNextWord(ref aInStr, "|");
        }

        public static long ascStrToInt(string aString, long defval)
        {
            long result = defval;
            if (aString != "")
            {
                try
                {
                    result = Convert.ToInt64(aString);
                }
                catch
                {
                    try
                    {
                        result = Convert.ToInt64(Convert.ToDouble(aString));
                    }
                    catch
                    {
                    }
                }
            }
            return result;
        }

    }
}
