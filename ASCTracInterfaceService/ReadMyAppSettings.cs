using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASCTracInterfaceService
{
    internal class ReadMyAppSettings
    {

        internal static void ReadAppSettings(string aFuncName)
        {
            //string myConnStr = ConfigurationSettings.AppSettings.Get("ASCTracConnectionString");
            //string myConnStr = System.Configuration!System.Configuration.ConfigurationManager.AppSettings.Get("ASCTracConnectionString");
            string myConnStr = System.Configuration.ConfigurationManager.AppSettings.Get("ASCTracConnectionString");
            if (!String.IsNullOrEmpty(myConnStr))
                ASCTracInterfaceDll.Class1.fDefaultConnectionStr = myConnStr;

            string fFlag = System.Configuration.ConfigurationManager.AppSettings.Get("Logging_" + aFuncName);
            if (String.IsNullOrEmpty(fFlag))
                fFlag = System.Configuration.ConfigurationManager.AppSettings.Get("Logging");
            if (String.IsNullOrEmpty(fFlag))
                fFlag = "X";
            if (!String.IsNullOrEmpty(fFlag))
                ASCTracInterfaceDll.Class1.fLogging = fFlag;
        }
    }
}