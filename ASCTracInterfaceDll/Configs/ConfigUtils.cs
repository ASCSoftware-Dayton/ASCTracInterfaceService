using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace ASCTracInterfaceDll.Configs
{
    internal class ConfigUtils
    {
        internal static string GetUserID(string aPrefix, ParseNet.GlobalClass Globals)
        {
            string retval = string.Empty;
            if( !String.IsNullOrEmpty( aPrefix))
                ReadConfigSetting("GW" + aPrefix + "USERID", "GATEWAY", Globals);
            if (string.IsNullOrEmpty(retval))
                retval = ReadConfigSetting("GWUSERID", "GATEWAY", Globals);
            if (string.IsNullOrEmpty(retval))
                retval= "GATEWAY";
            return (retval);
        }
        internal static bool ReadConfigSetting(string afield, bool aDefault, ParseNet.GlobalClass Globals)
        {
            bool retval = aDefault;
            string tmp = string.Empty;
            if (!Globals.myDBUtils.ReadFieldFromDB("SELECT CFGDATA from CFGSETTINGS WHERE SITE_ID='&&' AND CFGFIELD='" + afield + "'", "", ref tmp))
                Globals.myDBUtils.ReadFieldFromDB("SELECT CFGDATA from CFGSETTINGS WHERE SITE_ID='" + Globals.curSiteID + "' AND CFGFIELD='" + afield + "'", "", ref tmp);
            if (!String.IsNullOrEmpty(tmp))
                retval = tmp.Equals("T");
            return (retval);
        }
        internal static string ReadConfigSetting(string afield, string aDefault, ParseNet.GlobalClass Globals)
        {
            string retval = aDefault;
            string tmp = string.Empty;
            if (!Globals.myDBUtils.ReadFieldFromDB("SELECT CFGDATA from CFGSETTINGS WHERE SITE_ID='&&' AND CFGFIELD='" + afield + "'", "", ref tmp))
                Globals.myDBUtils.ReadFieldFromDB("SELECT CFGDATA from CFGSETTINGS WHERE SITE_ID='" + Globals.curSiteID + "' AND CFGFIELD='" + afield + "'", "", ref tmp);
            if (!String.IsNullOrEmpty(tmp))
                retval = tmp;
            return (retval);
        }

        internal static void ReadTransationFields(Dictionary<string, List<string>> aTranList, string aTblName, ParseNet.GlobalClass Globals)
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
