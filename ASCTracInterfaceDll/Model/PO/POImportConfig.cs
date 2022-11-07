using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceDll.Model.PO
{
    public class POImportConfig
    {
        public POImportConfig()
        {
            GWPOHdrTranslation = new Dictionary<string, List<string>>();
            GWPODetTranslation = new Dictionary<string, List<string>>();
        }
        public string GatewayUserID { get; set; }
        public bool createSkeletonItems { get; set; }

        public bool GWPurgePODetOnImport { get; set; }
        public bool GWDeletePOLinesNotInInterface { get; set; }
        public Dictionary<string, List<string>> GWPOHdrTranslation { get; set; }
        public Dictionary<string, List<string>> GWPODetTranslation { get; set; }

        public bool GWPurgeRMADetOnImport { get; set; }
        public string RMA_TYPE { get; set; }
        public bool GWDeleteRMALinesNotInInterface { get; set; }
        public bool GWPOUseHostLineToCalcLineNum { get; set; }
        public long GWPOMissingItemAction { get; set; } // 0=Abort Import, 1=Import Existing Items, 2=Add missing items
    }
}
