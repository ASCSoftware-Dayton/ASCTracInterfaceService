using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceDll.Configs
{
    internal class TranfileConfig
    {
        internal static Dictionary<string, Model.Tranfile.TranfileExportConfig> TranfileExportConfigList = new Dictionary<string, Model.Tranfile.TranfileExportConfig>();
        private static string siteid;
        private static ParseNet.GlobalClass Globals;
        internal static Model.Tranfile.TranfileExportConfig getExportSite(string aSiteID, ParseNet.GlobalClass aGlobals)
        {
            Globals = aGlobals;
            Model.Tranfile.TranfileExportConfig curConfig;
            if (!TranfileExportConfigList.ContainsKey(aSiteID))
            {
                curConfig = ReadExportConfig(aSiteID);
                TranfileExportConfigList.Add(aSiteID, curConfig);

            }
            else
                curConfig = TranfileExportConfigList[aSiteID];
            return (curConfig);
        }

        private static Model.Tranfile.TranfileExportConfig ReadExportConfig(string aSiteID)
        {
            siteid = aSiteID;
            var retval = new Model.Tranfile.TranfileExportConfig();
            retval.GatewayUserID = ConfigUtils.GetUserID("TRAN", Globals);
            retval.postedFlagField = ConfigUtils.ReadConfigSetting("GWExportTranfilepostedFlag", "POSTED", Globals);
            retval.posteddateField = "POSTEDDATE";
            if (retval.postedFlagField == "POSTED2") retval.posteddateField = "POSTEDDATE2";
            else if (retval.postedFlagField == "POSTED3") retval.posteddateField = "POSTEDDATE3";

            retval.exportUnreceivesAsInvAdj = ConfigUtils.ReadConfigSetting("GWExportUnreceiveAsInvAdjustment", "F", Globals) == "T";

            return (retval);
        }

    }
}
