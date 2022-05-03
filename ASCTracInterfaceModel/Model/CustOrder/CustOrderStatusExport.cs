using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.CustOrder
{
    public class CustOrderStatusExport
    {
        public CustOrderStatusExport()
        {
            SUCCESSFUL = true;
            ERROR_MESSAGE = string.Empty;
        }
        public bool SUCCESSFUL { get; set; }
        public string ERROR_MESSAGE { get; set; }


        public DateTime CREATE_DATETIME { get; set; }

        public string FACILITY { get; set; }

        public string ORDERNUMBER { get; set; }

        public string RESCHEDULED_FLAG { get; set; }

        public string ORDER_TYPE { get; set; }

        public string PICKSTATUS { get; set; }

        public DateTime NEWDATETIME { get; set; }

        public DateTime ORIGINALDATETIME { get; set; }

        public string CUSTOMERID { get; set; }

        public string VENDORID { get; set; }

        public DateTime TRANSACTION_DATE { get; set; }

    }
}


