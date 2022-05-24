using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceDll.Model.CustOrder
{
    internal class COImportConfig
    {
        public COImportConfig()
        {
            GWCOHdrTranslation = new Dictionary<string, List<string>>();
            GWCODetTranslation = new Dictionary<string, List<string>>();
        }
        public string GatewayUserID { get; set; }
        public bool GWCOPurgeHeaderWithNoLines { get; set; }
        public bool GWPurgeCODetOnImport { get; set; }
        public bool GWDeleteCOLinesNotInInterface { get; set; }
        public bool GWCreateTransferPOFromCO { get; set; }
        public bool GWCreateWOFromCO { get; set; }
        public bool GWEngageBatchPickingOnImport { get; set; }
        public bool GWUpdateInProgressOrders { get; set; }
        public string GWLineErrorHandling { get; set; }
        public bool useB2BLogic { get; set; }
        public bool GWUseAddrFromCustTable { get; set; }
        public bool GWUseFreightBillToAddrFromCustTable { get; set; }
        public bool useCustBillToNameIfBlank { get; set; }
        public bool useBillToAsShipToNameIfBlank { get; set; }

        public bool GWWillCallCarrierFlag { get; set; }
        public string GWWillCallCarrier { get; set; }
        public bool GWAllowCancelOfPickedOrder { get; set; }

        // detail flags
        public bool GWCOUseCustItem { get; set; }
        public bool GWImportVMIItemIfActive { get; set; }

        public bool GWLogChangedOrderTranfile { get; set; }
        public bool CPSetORDRDETPickLocOnImport { get; set; }
        public bool GWUseStandardKitExplosion { get; set; }


        // from FILEXFER
        public bool isActiveWOFROMCO { get; set; }
        public Dictionary<string, List<string>> GWCOHdrTranslation { get; set; }
        public Dictionary<string, List<string>> GWCODetTranslation { get; set; }


    }
}
