using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.CustOrder
{
    public class OrdrDetImport
    {
        public OrdrDetImport()
        {
            NotesList = new List<NotesImport>();
            CustomList = new List<ModelCustomData>();
        }

        public DateTime CREATE_DATETIME { get; set; }

        public long LINE_NUMBER { get; set; }

        public string PRODUCT_CODE { get; set; }

        public double QUANTITY { get; set; }

        public string COMMENT { get; set; }

        public double COSTEACH { get; set; }

        public double CW_NOT_BASE_UOM { get; set; }

        public string CW_UOM { get; set; }

        public string STATUS_FLAG { get; set; }

        public double LIST_PRICE { get; set; }

        public string ORDER_STATUS { get; set; }

        public string HOST_UOM { get; set; }

        public string REQUESTED_LOT { get; set; }

        public string CLIENTDEPT { get; set; }

        public string CLIENTDIVISION { get; set; }

        public string CLIENTGLACCT { get; set; }

        public string CLIENTPROFIT { get; set; }

        public string NOTES { get; set; }

        public string SHIPDESC { get; set; }

        public string CUST_ITEMID { get; set; }

        public double SOLD_PRICE { get; set; }

        public double QTYBACKORDERED { get; set; }

        public string COUNTRY_OF_DESTINATION { get; set; }

        public List<ModelCustomData> CustomList { get; set; }
        public List<NotesImport> NotesList { get; set; }

    }
}
