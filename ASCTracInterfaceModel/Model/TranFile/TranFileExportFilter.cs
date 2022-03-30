using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.TranFile
{
    public class TranFileExportFilter
    {
        public TranFileExportFilter(string aExcludeTranType, string aCustID)
        {
            CustID = aCustID;
            ExcludeTranType = aExcludeTranType;
            ExportFilterList = new List<ModelExportFilter>();
        }
        public string ExcludeTranType { get; } // list of transactions to ignore
        public string CustID { get; }
        public List<ModelExportFilter> ExportFilterList { get; set; }
    }
}

