using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.Vendor
{
    public class VendorImport
    {
        //public int ID { get; set; }

        //public string PROCESS_FLAG { get; set; }

        //public DateTime? PROCESS_DATETIME { get; set; }

        //public DateTime CREATE_DATETIME { get; set; }

        public string VENDOR_CODE { get; set; }

        public string VENDOR_DESC { get; set; }

        public string ADDR_LINE1 { get; set; }

        public string ADDR_LINE2 { get; set; }

        public string CITY { get; set; }

        public string STATE { get; set; }

        public string ZIP { get; set; }

        public string COUNTRY { get; set; }

        public string CONTACT_NAME { get; set; }

        public string CONTACT_TEL { get; set; }

        public string CONTACT_FAX { get; set; }

        public string REMIT_TO_ADDR_LINE1 { get; set; }

        public string REMIT_TO_ADDR_LINE2 { get; set; }

        public string REMIT_TO_CITY { get; set; }

        public string REMIT_TO_STATE { get; set; }

        public string REMIT_TO_ZIP { get; set; }

        public string REMIT_TO_COUNTRY { get; set; }

        public string REMIT_TO_CONTACT_NAME { get; set; }

        public string REMIT_TO_CONTACT_TEL { get; set; }

        public string REMIT_TO_CONTACT_FAX { get; set; }

        public string TERMS_ID { get; set; }

        public string STATUS { get; set; }

        public string MASTER_VENDORID { get; set; }

        public string ORGANIC_FLAG { get; set; }

        public string ORGANIC_REG_NUM { get; set; }

        public string AUTOCLOSEPO { get; set; }

        public string ENABLE_LICENSE_LEVEL_VALIDATION { get; set; }

    }
}
