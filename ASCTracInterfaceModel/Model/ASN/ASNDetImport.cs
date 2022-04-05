using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.ASN
{
    public class ASNDetImport
    {
        public ASNDetImport()
        {
            CustomList = new List<ModelCustomData>();
        }

        public string ITEMID { get; set; }

        public string LOTID { get; set; }

        public string SKIDID { get; set; }

        public double QUANTITY { get; set; }

        public DateTime EXPIRE_DATE { get; set; }

        public string PONUMBER { get; set; }

        public string RELEASENUM { get; set; }

        public double LINENUMBER { get; set; }

        public string VMI_CUSTID { get; set; }

        public double ACTUAL_WEIGHT { get; set; }

        public double CW_QTY { get; set; }

        public string VENDORID { get; set; }

        public string CONTAINER_ID { get; set; }

        public string PALLET_TYPE { get; set; }

        public DateTime DATETIMEPROD { get; set; }

        public string ALT_SKIDID { get; set; }

        public string ALT_LOTID { get; set; }

        public List<ModelCustomData> CustomList { get; set; }
    }
}
