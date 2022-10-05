using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceDll.Model.Tranfile
{
    internal class TranfileExportConfig
    {
        public string GatewayUserID { get; set; }
        public string postedFlagField { get; set; }
        public string posteddateField { get; set; }
        public bool exportUnreceivesAsInvAdj { get; set; }
        public bool APIIncludeProcessingStatus { get; set; }
        public string FilterPostedValues { get; set; }

    }
}
