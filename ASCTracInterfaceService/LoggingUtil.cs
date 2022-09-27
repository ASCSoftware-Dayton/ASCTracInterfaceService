using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace ASCTracInterfaceService
{
    internal class LoggingUtil
    {
        internal static void LogEventView( string TranType, string aTranID, string aException, ref string errMsg)
        {
            try
            {
                EventLog.WriteEntry( TranType + " " + aTranID, aException, EventLogEntryType.Error);
            }
            catch (Exception ex1)
            {
                errMsg += "\r\nWrite Event Log: " + ex1.Message;
            }

        }
    }
}