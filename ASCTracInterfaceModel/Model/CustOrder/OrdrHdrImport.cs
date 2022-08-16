using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.CustOrder
{
    public class OrdrHdrImport
    {
        public OrdrHdrImport()
        {
            DetailList = new List<OrdrDetImport>();
            NotesList = new List<NotesImport>();
            CustomList = new List<ModelCustomData>();
        }
        public DateTime ORDER_CREATE_DATE { get; set; }

        public string FACILITY { get; set; }

        public string ORDER_TYPE { get; set; }

        public string ORDERNUMBER { get; set; }

        //public string CUST_ID { get; set; }

        public string FROM_FACILITY { get; set; }

        public DateTime LEAVES_DATE { get; set; }

        public DateTime ENTRY_DATE { get; set; }

        public string CARRIER { get; set; }

        public string PAYMENT_TYPE { get; set; }

        public string SHIP_TO_NAME { get; set; }
        public string SHIP_TO_ADDR_LINE1 { get; set; }
        public string SHIP_TO_ADDR_LINE2 { get; set; }
        public string SHIP_TO_ADDR_LINE3 { get; set; }
        public string SHIP_TO_CITY { get; set; }
        public string SHIP_TO_STATE { get; set; }
        public string SHIP_TO_ZIP { get; set; }
        public string SHIP_TO_COUNTRY { get; set; }
        public string SHIP_TO_CONTACT_NAME { get; set; }
        public string SHIP_TO_CONTACT_TEL { get; set; }
        public string SHIP_TO_CONTACT_FAX { get; set; }

        public string BILL_TO_NAME { get; set; }
        public string BILL_TO_ADDR_LINE1 { get; set; }
        public string BILL_TO_ADDR_LINE2 { get; set; }
        public string BILL_TO_ADDR_LINE3 { get; set; }
        public string BILL_TO_CITY { get; set; }
        public string BILL_TO_STATE { get; set; }
        public string BILL_TO_ZIP { get; set; }
        public string BILL_TO_COUNTRY { get; set; }
        public string BILL_TO_CONTACT_NAME { get; set; }
        public string BILL_TO_CONTACT_TEL { get; set; }
        public string BILL_TO_CONTACT_FAX { get; set; }

        public string CUST_PO_NUM { get; set; }

        public string STATUS_FLAG { get; set; }

        public string LOAD_PLAN_NUM { get; set; }

        public long LOAD_STOP_SEQ { get; set; }

        public double PRIORITY_ID { get; set; }

        public string RECIPIENT_EMAIL { get; set; }

        public string BOL_NUMBER { get; set; }

        public string FREIGHT_ACCOUNT_NUMBER { get; set; }

        public string REFERENCE_NUMBER { get; set; }

        public string SHIP_TO_CUST_ID { get; set; }

        public string BILL_TO_CUST_ID { get; set; }

        public string PREPAY_COLLECT { get; set; }

        public DateTime CANCEL_DATE { get; set; }

        public string CARRIER_SERVICE_CODE { get; set; }

        public string DELIVERY_INSTRUCTIONS { get; set; }

        public string CUST_SHIPTO_PO_NUM { get; set; }

        public double COD_AMT { get; set; }

        public DateTime MUST_ARRIVE_BY_DATE { get; set; }

        public string SALESPERSON { get; set; }

        public string TERMS_ID { get; set; }

        public string LINKED_PONUMBER { get; set; }

        public string CREDIT_HOLD_STATUS { get; set; }

        public string CLIENTDEPT { get; set; }

        public string CLIENTDIVISION { get; set; }

        public string CLIENTGLACCT { get; set; }

        public string CLIENTPROFIT { get; set; }

        public string ALLOW_SHORT_SHIP { get; set; }

        public string RESIDENTIAL_FLAG { get; set; }

        public string SHIP_VIA { get; set; }

        public string AREA { get; set; }

        public string ALLOW_OVER_SHIP { get; set; }

        public string SALESORDERNUMBER { get; set; }

        public string THIRDPARTYCUSTID { get; set; }

        public string THIRDPARTYNAME { get; set; }

        public string THIRDPARTYADDRESS1 { get; set; }

        public string THIRDPARTYADDRESS2 { get; set; }

        public string THIRDPARTYADDRESS3 { get; set; }

        public string THIRDPARTYCITY { get; set; }

        public string THIRDPARTYSTATE { get; set; }

        public string THIRDPARTYZIPCODE { get; set; }

        public string THIRDPARTYCOUNTRY { get; set; }

        
        public string STORE_NUM { get; set; }

        public string DEPT { get; set; }

        public string PACKLIST_REQ { get; set; }

        public string DROP_SHIP { get; set; }

        public string BATCH_NUM { get; set; }

        public string ROUTEID { get; set; }

        public string PROMO_CODE { get; set; }

        public string CUSTORDERCAT { get; set; }

        public string FOB { get; set; }

        public string CUST_BILLTO_PO_NUM { get; set; }

        public string COMPLIANCE_LABEL { get; set; }

        public string VMI_GROUPID { get; set; }

        public string ORDER_SOURCE_SYSTEM { get; set; }

        public string FREIGHTBILLTONAME { get; set; }

        public string FREIGHTBILLTOCONTACT { get; set; }

        public string FREIGHTBILLTOADDRESS1 { get; set; }

        public string FREIGHTBILLTOADDRESS2 { get; set; }

        public string FREIGHTBILLTOADDRESS3 { get; set; }

        public string FREIGHTBILLTOADDRESS4 { get; set; }

        public string FREIGHTBILLTOCITY { get; set; }

        public string FREIGHTBILLTOSTATE { get; set; }

        public string FREIGHTBILLTOZIPCODE { get; set; }

        public string FREIGHTBILLTOCOUNTRY { get; set; }

        public string FREIGHTBILLTOTELEPHONE { get; set; }

        public string FREIGHTBILLTOALTTEL { get; set; }

        public string FREIGHTBILLTOFAX { get; set; }

        public List<OrdrDetImport> DetailList { get; set; }

        public List<ModelCustomData> CustomList { get; set; }
        public List<NotesImport> NotesList { get; set; }

    }
}
