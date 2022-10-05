using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceDll.Model.CustOrder
{
    internal class COExportConfig
    {
        public string GatewayUserID { get; set; }
        public string postedFlagField { get; set; }
        public string posteddateField { get; set; }
        public string StatusPostedFlagField { get; set; }
        public string StatusPosteddateField { get; set; }

        public bool GWCOUseCustItem { get; set; }
        public bool APIIncludeProcessingStatus { get; set; }
        public string FilterPostedValues { get; set; }

    }
}
