using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace ASCTracInterfaceDll.Configs
{
    internal class ItemConfig
    {
        internal static Dictionary<string, Model.Item.ItemImportConfig> ImportConfigList = new Dictionary<string, Model.Item.ItemImportConfig>();
        private static string siteid;
        private static ParseNet.GlobalClass Globals;
        internal static Model.Item.ItemImportConfig getImportSite(string aSiteID, ParseNet.GlobalClass aGlobals)
        {
            Globals = aGlobals;
            Model.Item.ItemImportConfig currImportConfig;
            if (!ImportConfigList.ContainsKey(aSiteID))
            {
                currImportConfig = ReadImportConfig(aSiteID);
                ImportConfigList.Add(aSiteID, currImportConfig);

            }
            else
                currImportConfig = ImportConfigList[aSiteID];
            return (currImportConfig);
        }

        private static Model.Item.ItemImportConfig ReadImportConfig(string aSiteID)
        {
            siteid = aSiteID;
            var retval = new Model.Item.ItemImportConfig();
            retval.GatewayUserID = ConfigUtils.GetUserID("", Globals);
            retval.doNotUpdateUOMValues = ConfigUtils.ReadConfigSetting( "GWDontUpdateUOMValues", false, Globals);
            retval.defLabelUOM = ConfigUtils.ReadConfigSetting( "GWDefaultLabelUOM", "PL", Globals);
            retval.defFreightClass = ConfigUtils.ReadConfigSetting( "GWDefaultFreightClassCode", "", Globals);
            retval.defItemType = ConfigUtils.ReadConfigSetting( "GWDefaultItemType", "F", Globals);
            retval.defTrackBy = ConfigUtils.ReadConfigSetting( "GWDefaultTrackBy", "T", Globals);
            retval.allowMultiSiteItemImport = ConfigUtils.ReadConfigSetting( "GWAllowMultiSiteItemImport", false, Globals);  //added 08-26-15 (JXG)

            ConfigUtils.ReadTransationFields(retval.GWTranslation, "ITEMMSTR", Globals);

            return (retval);
        }

    }
}
