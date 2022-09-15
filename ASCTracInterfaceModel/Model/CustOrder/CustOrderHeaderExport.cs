using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.CustOrder
{
    public class CustOrderHeaderExport
    {
        public CustOrderHeaderExport()
        {
            SUCCESSFUL = true;
            ERROR_MESSAGE = string.Empty;
            PicksList = new List<CustOrderPicksExport>();
            ContainersList = new List<CustOrderContainersExport>();
            SerialsList = new List<CustOrderSerNumExport>();
        }
        public bool SUCCESSFUL { get; set; }
        public string ERROR_MESSAGE { get; set; }

        public DateTime CREATE_DATETIME { get; set; }

        public string FROM_FACILITY { get; set; }

        public string TO_FACILITY { get; set; }

        public string SHIPMENT_NUMBER { get; set; }

        public string ORDER_TYPE { get; set; }

        public string ORDERNUMBER { get; set; }

        public string CUST_ID { get; set; }

        public DateTime INVOICE_DATE { get; set; }

        public string CUST_PO_NUM { get; set; }

        public double FREIGHT_COST { get; set; }

        public string PRO_NUMBER { get; set; }

        public string DOCUMENT_TYPE { get; set; }

        public string DOC_TYPE { get; set; }

        public string SALESORDERNUMBER { get; set; }

        public string FACILITY { get; set; }

        public string LOCKED_FLAG { get; set; }

        public string ORDER_SOURCE { get; set; }

        public string SHIP_VIA_CODE { get; set; }

        public string USERID { get; set; }

        public string CUSTOM_ORDER_TYPE { get; set; }

        public string ORDER_SOURCE_SYSTEM { get; set; }

        public string CARRIER { get; set; }

        public string CARRIER_SERVICE_CODE { get; set; }


        public string CUSTOM_DATA1 { get; set; }

        public string CUSTOM_DATA2 { get; set; }

        public string CUSTOM_DATA3 { get; set; }

        public string CUSTOM_DATA4 { get; set; }

        public string CUSTOM_DATA5 { get; set; }

        public string CUSTOM_DATA6 { get; set; }

        public string CUSTOM_DATA7 { get; set; }

        public string CUSTOM_DATA8 { get; set; }

        public string CUSTOM_DATA9 { get; set; }

        public string CUSTOM_DATA10 { get; set; }

        public string CUSTOM_DATA11 { get; set; }

        public string CUSTOM_DATA12 { get; set; }


        public List<CustOrderPicksExport> PicksList { get; set; }
        public List<CustOrderContainersExport> ContainersList { get; set; }
        public List<CustOrderSerNumExport> SerialsList { get; set; }
    }
}
