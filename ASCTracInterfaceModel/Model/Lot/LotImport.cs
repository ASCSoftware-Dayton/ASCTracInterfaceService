using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.Lot
{
    public class LotImport
    {
        public LotImport()
        {
        }
        public DateTime CREATE_DATETIME { get; set; }

        public string FACILITY { get; set; }

        public string LOTID { get; set; }

        public string PRODUCT_CODE { get; set; }

        public double STD_COST { get; set; }

        public DateTime MFG_DATE { get; set; }

        public string QAHOLD { get; set; }

        public string QAREASON { get; set; }

        public string CA_FILENAME { get; set; }

        public string WORKORDER_ID { get; set; }

        public double LANDED_COST { get; set; }
    }

}

