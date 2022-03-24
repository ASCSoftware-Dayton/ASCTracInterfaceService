using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.CustOrder
{
    public class CustOrderContainersExport
    {
        public DateTime CREATE_DATETIME { get; set; }
        public string SHIPMENT_NUMBER { get; set; }
        public string CONTAINER_ID { get; set; }
        public string PALLET_TYPE { get; set; }
        public double TOTAL_WEIGHT { get; set; }
    }
}
