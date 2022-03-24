using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.CustOrder
{
    public class CustOrderPicksExport
    {
        public DateTime CREATE_DATETIME { get; set; }

        public string SHIPMENT_NUMBER { get; set; }

        public long LINE_NUMBER { get; set; }

        public string PRODUCT_CODE { get; set; }

        public double EXT_PRICE { get; set; }

        public double QTY_ORDERED { get; set; }

        public double QTY_SHIPPED { get; set; }

        public double CW_QTY { get; set; }

        public string CW_UOM { get; set; }

        public string ITEM_NUMBER { get; set; }

        public string LOTID { get; set; }

        public string HOST_UOM { get; set; }

        public string PICK_OPR { get; set; }

        public string ALT_LOTID { get; set; }

        public double SOLD_PRICE { get; set; }


        public string CUSTOM_DATA1 { get; set; }
        public string CUSTOM_DATA2 { get; set; }
        public string CUSTOM_DATA3 { get; set; }
        public string CUSTOM_DATA4 { get; set; }
        public string CUSTOM_DATA5 { get; set; }


    }
}
