﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceDll.Model.CustOrder
{
    internal class COExportConfig
    {
        public string GatewayUserID { get; set; }
        public string postedFlagField { get; set; }
        public string posteddateField { get; set; }

        public bool GWCOUseCustItem { get; set; }
    }
}
