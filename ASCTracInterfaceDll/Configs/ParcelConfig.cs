using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace ASCTracInterfaceDll.Configs
{
    internal class ParcelConfig
    {
        internal static Dictionary<string, Model.CustOrder.ParcelExportConfig> ParcelExportConfigList = new Dictionary<string, Model.CustOrder.ParcelExportConfig>();
        private static string siteid;
        private static ParseNet.GlobalClass Globals;
        internal static Model.CustOrder.ParcelExportConfig getExportSite(string aSiteID, ParseNet.GlobalClass aGlobals)
        {
            Globals = aGlobals;
            Model.CustOrder.ParcelExportConfig curConfig;
            if (!ParcelExportConfigList.ContainsKey(aSiteID))
            {
                curConfig = ReadExportConfig(aSiteID);
                ParcelExportConfigList.Add(aSiteID, curConfig);

            }
            else
                curConfig = ParcelExportConfigList[aSiteID];
            return (curConfig);
        }

        private static Model.CustOrder.ParcelExportConfig ReadExportConfig(string aSiteID)
        {
            siteid = aSiteID;
            var retval = new Model.CustOrder.ParcelExportConfig();
            retval.GatewayUserID = ConfigUtils.GetUserID("Parcel", Globals);
            retval.postedFlagField = ConfigUtils.ReadConfigSetting("GWExportpostedFlag", "POSTED", Globals);
            retval.posteddateField = "POSTEDDATE";
            if (retval.postedFlagField == "POSTED2") retval.posteddateField = "POSTEDDATE2";
            else if (retval.postedFlagField == "POSTED3") retval.posteddateField = "POSTEDDATE3";

            retval.includePackoutItemsInParcelsExport = ConfigUtils.ReadConfigSetting("GWIncludePackoutItemsInParcelsExport", false, Globals);  //added 05-10-16 (JXG) for Alto Systems
            retval.exportProNumAsTrackNumWhenBlank = ConfigUtils.ReadConfigSetting("GWExportProNumAsTrackNumWhenBlank", true, Globals);  //added 08-16-21 (JXG) 

            return (retval);
        }


    }
}
