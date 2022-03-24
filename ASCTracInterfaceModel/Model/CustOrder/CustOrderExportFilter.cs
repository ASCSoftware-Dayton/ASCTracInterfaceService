using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.CustOrder
{
    public class CustOrderExportFilter
    {
        public CustOrderExportFilter( string aExportShipmentType, string aCustID, string aEDIMasterCustId)
        {
            ExportShipmentType = aExportShipmentType;
            CustID = aCustID;
            EDIMasterCustId = aEDIMasterCustId;
            ExportFilterList = new List<ModelExportFilter>();
        }
        public string ExportShipmentType { get;  } // C=Confirm shipped, P=Picked complete
        public string CustID { get;  }
        public string EDIMasterCustId { get; }
        public List<ModelExportFilter> ExportFilterList { get; set; }
    }
}
