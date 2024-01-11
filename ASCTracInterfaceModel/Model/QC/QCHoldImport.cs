using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.QC
{
    public class QCHoldImport
    {
        public DateTime datetime { get; set; }
        public string Transaction { get; set; }
        public string Add_Reason_Code { get; set; }
        public string Remove_Reason_Code { get; set; }
        public string User_ID { get; set; }
        public string Facility { get; set; }
        public string Product_Code { get; set; }
        public string VMI_Cust_ID { get; set; }

        public DateTime Expiration_Date { get; set; }
        public string Lot_ID { get; set; }
        public string Alt_Lot_ID { get; set; }
        public string Workorder_ID { get; set; }
        public string Recv_PO_Num { get; set; }
        public string Receiver_ID { get; set; }
        public string License_ID { get; set; }
        public DateTime RecvDateTime { get; set; }
        public string Host_String_ID { get; set; }

        public int Hold_Ref_Num { get; set; }
        public string Hold_Comments { get; set; }
        public int MafNum { get; set; }

        public string Inventory_Custom_Data1 { get; set; }
        public string Inventory_Custom_Data2 { get; set; }
        public string Inventory_Custom_Data3 { get; set; }
        public string Exception_Message { get; set; }
    }
}
