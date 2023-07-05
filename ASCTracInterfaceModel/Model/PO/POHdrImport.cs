using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel
	.Model.PO
{
	public class POHdrImport
	{
		public POHdrImport()
        {
			PODetList = new List<PODetImport>();
			NotesList = new List<NotesImport>();
			CustomList = new List<ModelCustomData>();
		}

		public string FACILITY { get; set; }
		public string ORDER_TYPE{ get; set; }
		public string PONUMBER { get; set; }
		public string HOST_PONUMBER { get; set; }
		public string VENDOR_CODE{ get; set; }
		public string TO_FACILITY { get; set; }
		//public DateTime LEAVES_DATE { get; set; }
		public DateTime ARRIVAL_DATE { get; set; }
		public DateTime ENTRY_DATE { get; set; }
		public string CARRIER{ get; set; }
		public string DELIVERY_INSTRUCTIONS { get; set; }
		public string ADDR_LINE1{ get; set; }
		public string ADDR_LINE2{ get; set; }
		public string CITY { get; set; }
		public string STATE{ get; set; }
		public string ZIP{ get; set; }
		public string COUNTRY { get; set; }
		public string CONTACT_NAME { get; set; }
		public string CONTACT_TEL{ get; set; }
		//public string CONTACT_FAX{ get; set; }
		public string STATUS_FLAG { get; set; }
		public string RMA_TYPE { get; set; }
		public string LINKED_ORDERNUMBER{ get; set; }
		public string BUYER_CODE_ID{ get; set; }
		public string ADDR_LINE3{ get; set; }
		public string TERMS_ID { get; set; }
		public string RELEASENUM{ get; set; }
		public string REQ_NUM{ get; set; }
		public string BILL_ADDR_LINE1{ get; set; }
		public string BILL_ADDR_LINE2{ get; set; }
		public string BILL_ADDR_LINE3{ get; set; }
		public string BILL_CITY_LINE3 { get; set; }
		public string BILL_CITY { get; set; }
		public string BILL_STATE{ get; set; }
		public string BILL_ZIP{ get; set; }
		public string BILL_COUNTRY { get; set; }
		public string BILL_CONTACT_NAME { get; set; }
		public string BILL_CONTACT_TEL{ get; set; }
		//public string BILL_CONTACT_FAX{ get; set; }
		public string DIRECT_SHIP_ORDERNUMBER{ get; set; }
		public string SHIP_TO_NAME { get; set; }
		public string BILL_TO_NAME { get; set; }
		public string SEAL_NUM{ get; set; }
		public string VMI_CUSTID{ get; set; }
		//public string ASN{ get; set; }
		public string PROMO_CODE{ get; set; }

		public List<PODetImport> PODetList { get; set; }
		public List<ModelCustomData> CustomList { get; set; }
		public List<NotesImport> NotesList { get; set; }
	}
}
