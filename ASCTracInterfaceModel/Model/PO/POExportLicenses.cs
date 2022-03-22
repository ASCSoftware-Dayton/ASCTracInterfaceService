using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.PO
{
    public class POExportLicenses
    {
        public long ID { get; set; }
        public bool SUCCESSFUL { get; set; }
        public string ERROR_MESSAGE { get; set; }


        public string FACILITY { get; set; }

        public string PRODUCT_CODE { get; set; }

        public double QUANTITY { get; set; }
        public string UOM { get; set; }

        public DateTime DATE_TIME { get; set; }

        public string TRANS_TYPE { get; set; }

        public double CW_QTY { get; set; }

        public string CW_UOM { get; set; }

        public string REASON_CODE { get; set; }

        public string LOTID { get; set; }

        public string ACCT_NUM_OR_CUSTID { get; set; }

        public string HOST_UOM { get; set; }

        public string CA_FILENAME { get; set; }

        public string ORDERNUMBER { get; set; }

        public double LINE_NUMBER { get; set; }

        public DateTime EXPDATE { get; set; }

        public string VMI_CUSTID { get; set; }

        public string CUSTOM_DATA1 { get; set; }

        public string CUSTOM_DATA2 { get; set; }

        public string CUSTOM_DATA3 { get; set; }

        public string CUSTOM_DATA4 { get; set; }

        public string CUSTOM_DATA5 { get; set; }

        public string CUSTOM_DATA6 { get; set; }

        public double CUSTOM_NUM1 { get; set; }

        public DateTime CUSTOM_DATE { get; set; }

        public string REF_NUMBER { get; set; }

        public string SKIDID { get; set; }

        public string CONTAINER_ID { get; set; }

        public string WHSE_ID { get; set; }

        public string FROM_LOCATION { get; set; }

        public string TO_LOCATION { get; set; }

        public string PROD_LINE { get; set; }

        public DateTime RECVDATETIME { get; set; }

        public DateTime DATETIMEPROD { get; set; }

        public string VENDORID { get; set; }

        public string COST_CENTER_ID { get; set; }

        public string RESP_SITE_ID { get; set; }

        public string COMMENTS { get; set; }

        public string RECEIVER_NO { get; set; }

        public string STOCK_UOM { get; set; }

        public string USERID { get; set; }

        public string ALT_LOTID { get; set; }

        public string CATID { get; set; }

        public string CAT2ID { get; set; }

        public string RELEASENUM { get; set; }

        public string ALT_SKIDID { get; set; }

        public string MFG_ID { get; set; }

        public double COST_EACH { get; set; }


        public POExportLicenses()
        {
            SUCCESSFUL = true;
            ERROR_MESSAGE = string.Empty;
        }
    }
}
