using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.PO
{
    public class POExportLines
    {
        public long ID { get; set; }
        public bool SUCCESSFUL { get; set; }
        public string ERROR_MESSAGE { get; set; }

        public string FACILITY { get; set; }

        public string RECEIVER_NO { get; set; }

        public long LINE_NUMBER { get; set; }

        public string ORDER_TYPE { get; set; }

        public string PONUMBER { get; set; }

        public string VENDOR_ITEM_ID { get; set; }

        public double QTY_RECEIVED { get; set; }

        public string PRODUCT_CODE { get; set; }

        public string STATUS { get; set; }

        public double CW_QTY { get; set; }

        public string CW_UOM { get; set; }

        public string BIN_LOCATION { get; set; }

        public string PACKING_SLIP_NBR { get; set; }

        public string CARRIER_NAME { get; set; }

        public string LOTID { get; set; }

        public string RECEIPT_ID { get; set; }

        public double QTY_ON_HOLD { get; set; }

        public string CLOSED_PO_FLAG { get; set; }

        public DateTime EXPIRED_DATE_BY_LOT { get; set; }

        public string CUSTOM_DATA1 { get; set; }

        public string CUSTOM_DATA2 { get; set; }

        public string CUSTOM_DATA3 { get; set; }

        public string CUSTOM_DATA4 { get; set; }

        public string CUSTOM_DATA5 { get; set; }

        public string CUSTOM_DATA6 { get; set; }

        public string CUSTOM_DATA7 { get; set; }

        public string CUSTOM_DATA8 { get; set; }

        public string CUSTOM_DATA9 { get; set; }

        public string CUSTOM_DATA10 { get; set; }

        public string CUST_ID { get; set; }

        public DateTime RECEIVED_DATE { get; set; }

        public string RELEASENUM { get; set; }

        public string CUSTOM_HDRDATA1 { get; set; }

        public string CUSTOM_HDRDATA2 { get; set; }

        public string CUSTOM_HDRDATA3 { get; set; }

        public string CUSTOM_HDRDATA4 { get; set; }

        public string CUSTOM_HDRDATA5 { get; set; }

        public string CUSTOM_HDRDATA6 { get; set; }

        public string CUSTOM_HDRDATA7 { get; set; }

        public string CUSTOM_HDRDATA8 { get; set; }

        public string RECV_OPR { get; set; }

        public string RMA_CUSTORDERNUM { get; set; }

        public string PROD_LINE { get; set; }

        public string VENDOR_ID { get; set; }

        public string ALT_LOTID { get; set; }

        public string CUSTOM_ORDER_TYPE { get; set; }

        public double QTY_ORDERED { get; set; }

        public string UOM { get; set; }

        public DateTime ARRIVAL_DATE { get; set; }

        public DateTime PROD_DATE { get; set; }

        public POExportLines()
        {
            SUCCESSFUL = true;
            ERROR_MESSAGE = string.Empty;
        }
    }
}
