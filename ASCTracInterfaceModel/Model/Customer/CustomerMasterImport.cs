using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.Customer
{
    public class CustomerMasterImport
    {
        public CustomerMasterImport()
        {
            NotesList = new List<NotesImport>();
            CustomList = new List<ModelCustomData>();
        }

        public string FACILITY { get; set; }
        public string CUST_ID { get; set; }
        public string CUST_NAME { get; set; }
        public string TERMS { get; set; }
        public string HOST_COMMENT { get; set; }
        public string INACTIVE_FLAG { get; set; }
        public string SHIP_ADDR_LINE1 { get; set; }
        public string SHIP_ADDR_LINE2 { get; set; }
        public string SHIP_CITY { get; set; }
        public string SHIP_STATE { get; set; }
        public string SHIP_ZIP { get; set; }
        public string SHIP_COUNTRY { get; set; }
        public string SHIP_CONTACT_NAME { get; set; }
        public string SHIP_CONTACT_TEL { get; set; }
        public string SHIP_CONTACT_FAX { get; set; }
        public string BILL_ADDR_LINE1 { get; set; }
        public string BILL_ADDR_LINE2 { get; set; }
        public string BILL_CITY { get; set; }
        public string BILL_STATE { get; set; }
        public string BILL_ZIP { get; set; }
        public string BILL_COUNTRY { get; set; }
        public string BILL_CONTACT_NAME { get; set; }
        public string BILL_CONTACT_TEL { get; set; }
        public string BILL_CONTACT_FAX { get; set; }
        public string STATUS_FLAG { get; set; }
        public string TERMS_ID { get; set; }
        public string SHIP_TO_TITLE { get; set; }
        //public string SHIP_TO_EMAIL { get; set; }
        public string BILL_TO_NAME { get; set; }
        public string ROUTE_AREAID { get; set; }
        public double PAST_DUE_PERIOD1 { get; set; }
        public double PAST_DUE_PERIOD2 { get; set; }
        public double PAST_DUE_PERIOD3 { get; set; }
        public double PAST_DUE_PERIOD4 { get; set; }
        public string MASTER_CUSTID { get; set; }
        public string CREDIT_RISK_RATING { get; set; }
        public double CREDIT_LIMIT { get; set; }
        public double OPENAMOUNT { get; set; }
        public double PROMOTION_ALLOWANCE { get; set; }
        public double PROMOTION_ALLOWANCE_CONSUMED { get; set; }
        public string CNTR_GROUP_ID { get; set; }
        public string PREVENT_ORGANIC_FLAG { get; set; }
        public string CUST_CATEGORY { get; set; }

        public List<NotesImport> NotesList { get; set; }
        public List<ModelCustomData> CustomList { get; set; }

    }
}
