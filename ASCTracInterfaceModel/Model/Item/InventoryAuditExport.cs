using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.Item
{
    public class InventoryAuditExport
    {

            public string FACILITY { get; set; }

            public string PRODUCT_CODE { get; set; }

            public double QUANTITY { get; set; }

            public double CW_QTY { get; set; }

            public string CW_UOM { get; set; }

            public string LOTID { get; set; }

            public double QTY_ALLOCATED { get; set; }

            public double QTY_ONHOLD { get; set; }

            public DateTime DATE_TIME { get; set; }

            public string UOM { get; set; }

            public string VMI_CUSTID { get; set; }

            public DateTime EXPDATE { get; set; }

            public string ALT_LOTID { get; set; }

            public double COST_EACH { get; set; }

            public string MFG_ID { get; set; }


    }
}
