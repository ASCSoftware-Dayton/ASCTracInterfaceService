using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace ASCTracInterfaceDll.Configs
{
    internal class CustOrderConfig
    {
        //internal static Model.PO.POImportConfig currCOImportConfig = null;
        internal static Dictionary<string, Model.CustOrder.COImportConfig> COImportConfigList = new Dictionary<string, Model.CustOrder.COImportConfig>();
        internal static Dictionary<string, Model.CustOrder.COExportConfig> COExportConfigList = new Dictionary<string, Model.CustOrder.COExportConfig>();
        private static string siteid;
        private static ParseNet.GlobalClass Globals;
        internal static Model.CustOrder.COImportConfig getCOImportSite(string aSiteID, ParseNet.GlobalClass aGlobals)
        {
            Globals = aGlobals;
            Model.CustOrder.COImportConfig currCOImportConfig;
            if (!COImportConfigList.ContainsKey(aSiteID))
            {
                currCOImportConfig = ReadImportConfig(aSiteID);
                COImportConfigList.Add(aSiteID, currCOImportConfig);

            }
            else
                currCOImportConfig = COImportConfigList[aSiteID];
            return (currCOImportConfig);
        }
        internal static Model.CustOrder.COExportConfig getCOExportSite(string aSiteID, ParseNet.GlobalClass aGlobals)
        {
            Globals = aGlobals;
            Model.CustOrder.COExportConfig curConfig;
            if (!COExportConfigList.ContainsKey(aSiteID))
            {
                curConfig = ReadExportConfig(aSiteID);
                COExportConfigList.Add(aSiteID, curConfig);
            }
            else
                curConfig = COExportConfigList[aSiteID];
            return (curConfig);
        }

        private static Model.CustOrder.COImportConfig ReadImportConfig(string aSiteID)
        {
            siteid = aSiteID;
            var retval = new Model.CustOrder.COImportConfig();
            retval.GatewayUserID = ConfigUtils.GetUserID("CO", Globals);
            string sql = "SELECT ID FROM FILEXFER (NOLOCK) WHERE ID='WOFROMCO' AND STATUS<>'I'";
            retval.isActiveWOFROMCO = Globals.myDBUtils.ifRecExists(sql);
            string tmp = string.Empty;
            retval.GWCreateTransferPOFromCO = ConfigUtils.ReadConfigSetting("GWCreateTransferPOFromCO", "F", Globals) == "T";
            retval.GWCreateWOFromCO = ConfigUtils.ReadConfigSetting("GWCreateWOFromCO", "F", Globals) == "T";
            retval.GWEngageBatchPickingOnImport = ConfigUtils.ReadConfigSetting("GWEngageBatchPickingOnImport", "F", Globals) == "T";
            retval.GWUpdateInProgressOrders = ConfigUtils.ReadConfigSetting("GWUpdateInProgressOrders", "F", Globals)  == "T"; 
            retval.GWLineErrorHandling = ConfigUtils.ReadConfigSetting("GWLineErrorHandling", "C", Globals);
            retval.useB2BLogic = ConfigUtils.ReadConfigSetting("GWOrderImportUseB2Logic", "F", Globals) == "T";
            retval.GWUseAddrFromCustTable = ConfigUtils.ReadConfigSetting("GWUseAddrFromCustTable", "F", Globals) == "T";
            retval.GWUseFreightBillToAddrFromCustTable = ConfigUtils.ReadConfigSetting("GWUseFreightBillToAddrFromCustTable", "F", Globals) == "T";
            retval.useCustBillToNameIfBlank = ConfigUtils.ReadConfigSetting("GWUseCustBillToNameIfBlank", "F", Globals) == "T"; 
            retval.useBillToAsShipToNameIfBlank = ConfigUtils.ReadConfigSetting("GWUseBillToAsShipToNameIfBlank", "F", Globals) == "T";
            retval.GWWillCallCarrierFlag = ConfigUtils.ReadConfigSetting("GWWillCallCarrierFlag", "F", Globals) == "T";
            retval.GWWillCallCarrier = ConfigUtils.ReadConfigSetting("GWWillCallCarrier", "", Globals);
            retval.GWAllowCancelOfPickedOrder = ConfigUtils.ReadConfigSetting("GWAllowCancelOfPickedOrder", false, Globals);

            retval.GWCOUseCustItem = ConfigUtils.ReadConfigSetting("GWCOUseCustItem", "F", Globals) == "T";
            retval.GWImportVMIItemIfActive = ConfigUtils.ReadConfigSetting("GWImportVMIItemIfActive", "F", Globals) == "T";

            retval.GWCOPurgeHeaderWithNoLines = ConfigUtils.ReadConfigSetting("GWCOPurgeHeaderWithNoLines", "F", Globals) == "T";
            retval.GWPurgeCODetOnImport = ConfigUtils.ReadConfigSetting("GWPurgeOrderDetOnImport", "F", Globals) == "T";
            retval.GWDeleteCOLinesNotInInterface = ConfigUtils.ReadConfigSetting("GWDeleteOrderLinesNotInInterface", "N", Globals) == "Y";

            retval.GWLogChangedOrderTranfile = ConfigUtils.ReadConfigSetting("GWLogChangedOrderTranfile", "F", Globals) == "T";
            retval.CPSetORDRDETPickLocOnImport = ConfigUtils.ReadConfigSetting("CPSetORDRDETPickLocOnImport", "F", Globals) == "T";
            retval.GWUseStandardKitExplosion = ConfigUtils.ReadConfigSetting("GWUseStandardKitExplosion", "F", Globals) == "T";

            ConfigUtils.ReadTransationFields(retval.GWCOHdrTranslation, "ORDRHDR", Globals);
            ConfigUtils.ReadTransationFields(retval.GWCODetTranslation, "ORDRDET", Globals);

            return (retval);
        }

        private static Model.CustOrder.COExportConfig ReadExportConfig(string aSiteID)
        {
            siteid = aSiteID;
            var retval = new Model.CustOrder.COExportConfig();
            retval.GatewayUserID = ConfigUtils.GetUserID("CO", Globals);
            retval.postedFlagField = ConfigUtils.ReadConfigSetting("GWExportTranfilepostedFlag", "POSTED", Globals);
            retval.posteddateField = "POSTEDDATE";
            if (retval.postedFlagField == "POSTED2") retval.posteddateField = "POSTEDDATE2";
            else if (retval.postedFlagField == "POSTED3") retval.posteddateField = "POSTEDDATE3";

            retval.StatusPostedFlagField = ConfigUtils.ReadConfigSetting("GWExportTranfileStatusPostedFlag", "POSTED3", Globals);
            retval.StatusPosteddateField = "POSTEDDATE";
            if (retval.StatusPostedFlagField == "POSTED2") retval.StatusPosteddateField = "POSTEDDATE2";
            else if (retval.StatusPostedFlagField == "POSTED3") retval.StatusPosteddateField = "POSTEDDATE3";

            retval.GWCOUseCustItem = ConfigUtils.ReadConfigSetting("GWCOUseCustItem", "F", Globals) == "T";
            retval.APIIncludeProcessingStatus = ConfigUtils.ReadConfigSetting("GWAPIIncludeProcessingStatus", "F", Globals) == "T";
            if (retval.APIIncludeProcessingStatus)
                retval.FilterPostedValues = "'F','S'";
            else
                retval.FilterPostedValues = "'F'";


            return (retval);
        }
    }
}
