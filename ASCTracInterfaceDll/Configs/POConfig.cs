using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace ASCTracInterfaceDll.Configs
{
    internal class POConfig
    {
        //internal static Model.PO.POImportConfig currPOImportConfig = null;
        internal static Dictionary<string, Model.PO.POImportConfig> POImportConfigList = new Dictionary<string, Model.PO.POImportConfig>();
        internal static Dictionary<string, Model.PO.POExportConfig> POExportConfigList = new Dictionary<string, Model.PO.POExportConfig>();
        private static string siteid;
        private static ParseNet.GlobalClass Globals;
        internal static Model.PO.POImportConfig getPOImportSite(string aSiteID, ParseNet.GlobalClass aGlobals)
        {
            Globals = aGlobals;
            Model.PO.POImportConfig currPOImportConfig;
            if (!POImportConfigList.ContainsKey(aSiteID))
            {
                currPOImportConfig = ReadImportConfig(aSiteID);
                POImportConfigList.Add(aSiteID, currPOImportConfig);

            }
            else
                currPOImportConfig = POImportConfigList[aSiteID];
            return (currPOImportConfig);
        }
        internal static Model.PO.POExportConfig getPOExportSite(string aSiteID, ParseNet.GlobalClass aGlobals)
        {
            Globals = aGlobals;
            Model.PO.POExportConfig currPOConfig;
            if (!POExportConfigList.ContainsKey(aSiteID))
            {
                currPOConfig = ReadExportConfig(aSiteID);
                POExportConfigList.Add(aSiteID, currPOConfig);

            }
            else
                currPOConfig = POExportConfigList[aSiteID];
            return (currPOConfig);
        }

        private static Model.PO.POImportConfig ReadImportConfig(string aSiteID)
        {
            siteid = aSiteID;
            var retval = new Model.PO.POImportConfig();
            retval.GatewayUserID = ConfigUtils.GetUserID("PO", Globals);
            retval.createSkeletonItems = ConfigUtils.ReadConfigSetting("GWPOCreateSkeletonItems", false, Globals);

            retval.GWPurgePODetOnImport = ConfigUtils.ReadConfigSetting("GWPurgePODetOnImport", "F", Globals) == "T";
            retval.GWDeletePOLinesNotInInterface = ConfigUtils.ReadConfigSetting("GWDeletePOLinesNotInInterface", "N", Globals) == "Y";

            retval.GWPurgeRMADetOnImport = ConfigUtils.ReadConfigSetting("GWPurgeRMADetOnImport", false, Globals);
            retval.RMA_TYPE = ConfigUtils.ReadConfigSetting("RMA_TYPE", "C", Globals);
            retval.GWDeleteRMALinesNotInInterface = ConfigUtils.ReadConfigSetting("GWDeleteRMALinesNotInInterface", "N", Globals) == "Y";

            ConfigUtils.ReadTransationFields(retval.GWPOHdrTranslation, "POHDR", Globals);
            ConfigUtils.ReadTransationFields(retval.GWPODetTranslation, "PODET", Globals);

            return (retval);
        }

        private static Model.PO.POExportConfig ReadExportConfig(string aSiteID)
        {
            siteid = aSiteID;
            var retval = new Model.PO.POExportConfig();
            retval.GatewayUserID = ConfigUtils.GetUserID("PO", Globals);
            retval.ExportUnreceivesAsInvAdj = ConfigUtils.ReadConfigSetting("GWExportUnreceiveAsInvAdjustment", false, Globals);
            retval.postedFlagField = ConfigUtils.ReadConfigSetting("GWExportTranfilepostedFlag", "POSTED", Globals);
            retval.posteddateField = "POSTEDDATE";
            if (retval.postedFlagField == "POSTED2") retval.posteddateField = "POSTEDDATE2";
            else if (retval.postedFlagField == "POSTED3") retval.posteddateField = "POSTEDDATE3";
            return (retval);
        }

    }
}