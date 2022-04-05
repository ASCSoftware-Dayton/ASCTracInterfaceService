using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.ASN
{
    public class ASNHdrImport
    {
        public ASNHdrImport()
        {
            DetailList = new List<ASNDetImport>();
        }
        public DateTime CREATE_DATETIME { get; set; }

        public string FACILITY { get; set; }

        public string ASN { get; set; }

        public string PONUMBER { get; set; }

        public string TRUCKNUM { get; set; }

        public string FROM_FACILITY { get; set; }

        public string REF_ORDERNUMBER { get; set; }

        public DateTime EXPECTED_RECEIPT_DATE { get; set; }

        public string VENDORID { get; set; }

        public string PACKINGSLIP { get; set; }

        public string ASN_TYPE { get; set; }


        public List<ASNDetImport> DetailList { get; set; }
    }
}
