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
            retval.GatewayUserID = ReadConfigSetting("GWPOUSERID", "GATEWAY");
            retval.createSkeletonItems = ReadConfigSetting("GWPOCreateSkeletonItems", false);

            retval.GWPurgePODetOnImport = ReadConfigSetting("GWPurgePODetOnImport", "F") == "T";
            retval.GWDeletePOLinesNotInInterface = ReadConfigSetting("GWDeletePOLinesNotInInterface", "N") == "Y";

            retval.GWPurgeRMADetOnImport = ReadConfigSetting("GWPurgeRMADetOnImport", false);
            retval.RMA_TYPE = ReadConfigSetting("RMA_TYPE", "C");
            retval.GWDeleteRMALinesNotInInterface = ReadConfigSetting("GWDeleteRMALinesNotInInterface", "N") == "Y";

            ReadTransationFields(retval.GWPOHdrTranslation, "POHDR");
            ReadTransationFields(retval.GWPODetTranslation, "PODET");

            return (retval);
        }

        private static Model.PO.POExportConfig ReadExportConfig(string aSiteID)
        {
            siteid = aSiteID;
            var retval = new Model.PO.POExportConfig();
            retval.GatewayUserID = ReadConfigSetting("GWPOUSERID", "GATEWAY");
            retval.ExportUnreceivesAsInvAdj = ReadConfigSetting("GWExportUnreceiveAsInvAdjustment", false);
            retval.postedFlagField = ReadConfigSetting("GWExportTranfilepostedFlag", "POSTED");
            retval.posteddateField = "POSTEDDATE";
            if (retval.postedFlagField == "POSTED2") retval.posteddateField = "POSTEDDATE2";
            else if (retval.postedFlagField == "POSTED3") retval.posteddateField = "POSTEDDATE3";
            /*
            retval.createSkeletonItems = ReadConfigSetting("GWPOCreateSkeletonItems", false);

            retval.GWPurgePODetOnImport = ReadConfigSetting("GWPurgePODetOnImport", "F") == "T";
            retval.GWDeletePOLinesNotInInterface = ReadConfigSetting("GWDeletePOLinesNotInInterface", "N") == "Y";

            retval.GWPurgeRMADetOnImport = ReadConfigSetting("GWPurgeRMADetOnImport", false);
            retval.RMA_TYPE = ReadConfigSetting("RMA_TYPE", "C");
            retval.GWDeleteRMALinesNotInInterface = ReadConfigSetting("GWDeleteRMALinesNotInInterface", "N") == "Y";

            ReadTransationFields(retval.GWPOHdrTranslation, "POHDR");
            ReadTransationFields(retval.GWPODetTranslation, "PODET");
            */
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

        private static void ReadTransationFields( Dictionary<string, List<string>> aTranList, string aTblName)
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
                    if (Globals.myDBUtils.IfFieldExists(aTblName, ascFieldname))
                    {
                        if (!aTranList.ContainsKey(apiFieldname))
                        {
                            var ascList = new List<string>();
                            ascList.Add(ascFieldname);
                            aTranList.Add(apiFieldname, ascList);
                        }
                        else
                        {
                            var ascList = aTranList[apiFieldname];
                            ascList.Add(ascFieldname);
                        }
                    }
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
