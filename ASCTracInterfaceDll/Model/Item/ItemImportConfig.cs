using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceDll.Model.Item
{
    public class ItemImportConfig
    {
        public ItemImportConfig()
        {
            GWTranslation = new Dictionary<string, List<string>>();
        }
        public string GatewayUserID { get; set; }
        public bool doNotUpdateUOMValues { get; set; }

        public string defLabelUOM { get; set; }
        public string defFreightClass { get; set; }

        public string defItemType { get; set; }
        public string defTrackBy { get; set; }
        public bool allowMultiSiteItemImport { get; set; }

        public bool InvAuditExportLotId { get; set; }
        public bool InvAuditExportExcludeNonPickable { get; set; }
        public bool InvAuditExportExcludeZeroQuantity { get; set; }

        public Dictionary<string, List<string>> GWTranslation { get; set; }

    }
}

