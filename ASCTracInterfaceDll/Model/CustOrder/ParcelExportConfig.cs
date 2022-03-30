using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceDll.Model.CustOrder
{
    internal class ParcelExportConfig
    {
        public string GatewayUserID { get; set; }
        public string postedFlagField { get; set; }
        public string posteddateField { get; set; }

        public bool includePackoutItemsInParcelsExport { get; set; }
        public bool exportProNumAsTrackNumWhenBlank { get; set; }
    }
}
