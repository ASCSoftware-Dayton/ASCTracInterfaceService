using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceDll.Model.PO
{
    internal class POExportConfig
    {
        public string GatewayUserID { get; set; }
        public bool ExportUnreceivesAsInvAdj { get; set; }
        public string postedFlagField { get; set; }
        public string posteddateField { get; set; }
    }
}
