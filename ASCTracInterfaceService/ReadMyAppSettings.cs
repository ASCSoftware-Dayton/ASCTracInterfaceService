using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASCTracInterfaceService
{
    internal class ReadMyAppSettings
    {

        internal static void ReadAppSettings()
        {
            //string myConnStr = ConfigurationSettings.AppSettings.Get("ASCTracConnectionString");
            //string myConnStr = System.Configuration!System.Configuration.ConfigurationManager.AppSettings.Get("ASCTracConnectionString");
            string myConnStr = System.Configuration.ConfigurationManager.AppSettings.Get("ASCTracConnectionString");
            if (!String.IsNullOrEmpty(myConnStr))
                ASCTracInterfaceDll.Class1.fDefaultConnectionStr = myConnStr;
        }
    }
}