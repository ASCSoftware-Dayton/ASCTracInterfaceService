using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.CustOrder
{
    public class ParcelExporFilter
    {
        public ParcelExporFilter(string aCustID, string aEDIMasterCustId)
        {
            CustID = aCustID;
            ExportFilterList = new List<ModelExportFilter>();
        }

        public string CustID { get; }
        public List<ModelExportFilter> ExportFilterList { get; set; }

    }
}
