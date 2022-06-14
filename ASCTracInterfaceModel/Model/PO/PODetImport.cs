using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.PO
{
    public class PODetImport
    {
		public PODetImport()
        {
            NotesList = new List<NotesImport>();
			CustomList = new List<ModelCustomData>();
		}

		//public string FACILITY { get; set; }
		//public string ORDER_TYPE { get; set; }
		//public string PONUMBER { get; set; }
		public long LINE_NUMBER { get; set; }
		public string VENDOR_ITEM_ID { get; set; }
		public string PRODUCT_CODE { get; set; }
		public double QUANTITY { get; set; }
		public DateTime EXPECTED_RECEIPT_DATE { get; set; }
		public string COMMENT { get; set; }
		public double COSTEACH { get; set; }
		public string CW_UOM { get; set; }
		public string STATUS_FLAG { get; set; }
		public string UPC_CODE { get; set; }
		public string ITEM_DESCRIPTION { get; set; }
		public string LOTID { get; set; }
		public string UOM { get; set; }
		public double BUY_TO_STOCK_CONV_FACTOR { get; set; }
		public string DIRECT_SHIP_ORDERNUMBER { get; set; }
		public string LINKED_ORDERNUMBER { get; set; }
		public string SKIDID { get; set; }
		public string SERIAL_NUM { get; set; }
		public string PROMO_CODE { get; set; }
		public string RELEASENUM { get; set; }
		public DateTime EXPECTED_SHIP_DATE { get; set; }
		public DateTime USER_REQUIRED_DATE { get; set; }
		public string ORIG_ORDERNUMBER { get; set; }
		public double CW_QTY { get; set; }
		public string VEND_PRODLINE { get; set; }
		public string QC_REASON { get; set; }
		public string ALT_LOTID { get; set; }
		public double HOST_LINENUMBER { get; set; }
		public string PROJECT_NUMBER { get; set; }

		public List<NotesImport> NotesList { get; set; }
		public List<ModelCustomData> CustomList { get; set; }
	}
}
