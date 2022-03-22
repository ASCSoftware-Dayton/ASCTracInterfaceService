using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.PO
{
    public class POExportFilter
    {
        public POExportFilter( bool aOnlySendCompletedReceipt)
        {
            //PostedFieldNumber = aPostedFieldNumber;
            OnlySendCompletedReceipts = aOnlySendCompletedReceipt;
            ExportFilterList = new List<ModelExportFilter>();
        }
        //public int PostedFieldNumber { get; }
        public bool OnlySendCompletedReceipts { get; }
        public List<ModelExportFilter> ExportFilterList { get; set; }
    }
}
