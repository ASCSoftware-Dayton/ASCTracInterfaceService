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
        //internal static Dictionary<string, Model.PO.POExportConfig> POExportConfigList = new Dictionary<string, Model.PO.POExportConfig>();
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

        private static Model.CustOrder.COImportConfig ReadImportConfig(string aSiteID)
        {
            siteid = aSiteID;
            var retval = new Model.CustOrder.COImportConfig();
            retval.GatewayUserID = ReadConfigSetting("GWPOUSERID", "GATEWAY");
            //retval.createSkeletonItems = ReadConfigSetting("GWPOCreateSkeletonItems", false);
            string sql = "SELECT ID FROM FILEXFER (NOLOCK) WHERE ID='WOFROMCO' AND STATUS<>'I'";
            retval.isActiveWOFROMCO = Globals.myDBUtils.ifRecExists(sql);
            string tmp = string.Empty;
            retval.GWCreateTransferPOFromCO = ReadConfigSetting("GWCreateTransferPOFromCO", "F") == "T";
            retval.GWCreateWOFromCO = ReadConfigSetting("GWCreateWOFromCO", "F") == "T";
            retval.GWEngageBatchPickingOnImport = ReadConfigSetting("GWEngageBatchPickingOnImport", "F") == "T";
            retval.GWUpdateInProgressOrders = ReadConfigSetting("GWUpdateInProgressOrders", "F") == "T"; 
            retval.GWLineErrorHandling = ReadConfigSetting("GWLineErrorHandling", "C");
            retval.useB2BLogic = ReadConfigSetting("GWOrderImportUseB2Logic", "F") == "T";
            retval.GWUseAddrFromCustTable = ReadConfigSetting("GWUseAddrFromCustTable", "F") == "T";
            retval.GWUseFreightBillToAddrFromCustTable = ReadConfigSetting("GWUseFreightBillToAddrFromCustTable", "F") == "T";
            retval.useCustBillToNameIfBlank = ReadConfigSetting("GWUseCustBillToNameIfBlank", "F") == "T"; 
            retval.useBillToAsShipToNameIfBlank = ReadConfigSetting("GWUseBillToAsShipToNameIfBlank", "F") == "T";
            retval.GWWillCallCarrierFlag = ReadConfigSetting("GWWillCallCarrierFlag", "F") == "T";
            retval.GWWillCallCarrier = ReadConfigSetting("GWWillCallCarrier", "");

            retval.GWCOUseCustItem = ReadConfigSetting("GWCOUseCustItem", "F") == "T";
            
            retval.GWCOPurgeHeaderWithNoLines = ReadConfigSetting("GWCOPurgeHeaderWithNoLines", "F") == "T";
            retval.GWPurgeCODetOnImport = ReadConfigSetting("GWPurgeOrderDetOnImport", "F") == "T";
            retval.GWDeleteCOLinesNotInInterface = ReadConfigSetting("GWDeleteOrderLinesNotInInterface", "N") == "Y";

            retval.GWLogChangedOrderTranfile = ReadConfigSetting("GWLogChangedOrderTranfile", "F") == "T";
            retval.CPSetORDRDETPickLocOnImport = ReadConfigSetting("CPSetORDRDETPickLocOnImport", "F") == "T";
            retval.GWUseStandardKitExplosion = ReadConfigSetting("GWUseStandardKitExplosion", "F") == "T";

            ReadTransationFields(retval.GWCOHdrTranslation, "ORDRHDR");
            ReadTransationFields(retval.GWCODetTranslation, "ORDRDET");

            return (retval);
        }

        private static bool ReadConfigSetting(string afield, bool aDefault)
        {
            bool retval = aDefault;
            string tmp = string.Empty;
            if (!Globals.myDBUtils.ReadFieldFromDB("SELECT CFGDATA from CFGSETTINGS WHERE SITE_ID='&&' AND CFGFIELD='" + afield + "'", "", ref tmp))
                Globals.myDBUtils.ReadFieldFromDB("SELECT CFGDATA from CFGSETTINGS WHERE SITE_ID='" + siteid + "' AND CFGFIELD='" + afield + "'", "", ref tmp);
            if (!String.IsNullOrEmpty(tmp))
                retval = tmp.Equals("T");
            return (retval);
        }
        private static string ReadConfigSetting(string afield, string aDefault)
        {
            string retval = aDefault;
            string tmp = string.Empty;
            if (!Globals.myDBUtils.ReadFieldFromDB("SELECT CFGDATA from CFGSETTINGS WHERE SITE_ID='&&' AND CFGFIELD='" + afield + "'", "", ref tmp))
                Globals.myDBUtils.ReadFieldFromDB("SELECT CFGDATA from CFGSETTINGS WHERE SITE_ID='" + siteid + "' AND CFGFIELD='" + afield + "'", "", ref tmp);
            if (!String.IsNullOrEmpty(tmp))
                retval = tmp;
            return (retval);
        }

        private static void ReadTransationFields(Dictionary<string, string> aList, string aTblName)
        {
            string sqlStr = "SELECT API_FIELDNAME, ASCTRAC_FIELDNAME FROM API_FIELD_TRANSLATE WHERE TBLNAME='" + aTblName + "'";
            SqlConnection customConnection = new SqlConnection(Globals.myDBUtils.myConnString);
            SqlCommand customCommand = new SqlCommand(sqlStr, customConnection);
            customConnection.Open();
            SqlDataReader customReader = customCommand.ExecuteReader();

            try
            {
                string updStr = string.Empty;
                while (customReader.Read())
                {
                    string apiFieldname = customReader["API_FIELDNAME"].ToString().ToUpper();
                    string ascFieldname = customReader["ASCTRAC_FIELDNAME"].ToString().ToUpper();
                    if (!aList.ContainsKey(apiFieldname))
                        aList.Add(apiFieldname, ascFieldname);
                }
            }
            finally
            {
                customReader.Close();
                customCommand.Dispose();
                customConnection.Close();
                customConnection.Dispose();
            }
        }
    }
}
