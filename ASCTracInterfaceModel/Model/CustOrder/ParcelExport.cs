using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.CustOrder
{
    public class ParcelExport
    {
        public ParcelExport( )
        {
            Successful = true;
            ErrorMessage = string.Empty;
        }
        public bool Successful { get; set; }
        public string ErrorMessage { get; set; }

        public DateTime CREATE_DATETIME { get; set; }

        public string FACILITY { get; set; }

        public string ORDERNUMBER { get; set; }

        public string PARCEL_NUMBER { get; set; }

        public string CARRIER { get; set; }

        public double FREIGHT_COST { get; set; }

        public double WEIGHT { get; set; }

        public DateTime SHIPDATE { get; set; }

        public string CARRIER_SERVICE_CODE { get; set; }

        public string TRACKING_NUMBER { get; set; }

        public string SWOG { get; set; }

        public string CUST_ID { get; set; }

        public string SALESORDERNUMBER { get; set; }

        public double CUST_FREIGHT_COST { get; set; }

        public string USERID { get; set; }

        public string SEAL_NUMBERS { get; set; }

        public string ITEMID { get; set; }

        public string ASCITEMID { get; set; }

        public string ITEM_DESC { get; set; }

        public double LINENUMBER { get; set; }

        public string LOT_ID { get; set; }

        public double QTY { get; set; }

    }
}
