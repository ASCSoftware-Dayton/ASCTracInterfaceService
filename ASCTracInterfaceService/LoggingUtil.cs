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
                EventLog elog = new EventLog();
                elog.Source = "ASCTracInterfaceServiceSource";
                elog.Log = "ASCTracInterfaceServiceLog";
                if (!System.Diagnostics.EventLog.SourceExists(elog.Source))
                    EventLog.CreateEventSource( elog.Source, elog.Log);
                elog.WriteEntry( TranType + " " + aTranID + "\r\n" + aException);
            }
            catch (Exception ex1)
            {
                errMsg = aException + "\r\n" + "Write Event Log: " + ex1.Message;
            }

        }
    }
}