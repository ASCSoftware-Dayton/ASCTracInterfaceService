using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.CustOrder
{
    public class CustOrderSerNumExport
    {

        public DateTime CREATE_DATETIME { get; set; }


        public long ORDER_LINENUM { get; set; }

        public string SER_NUM { get; set; }

        public string ITEMID { get; set; }

        public string LOTID { get; set; }

        public double QTY { get; set; }

    }

}

