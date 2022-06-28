using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.Item
{
    public class ItemMasterImport
    {
        public ItemMasterImport()
        {
            ExtDataList = new Dictionary<string, string>();
            NotesList = new List<NotesImport>();
            CustomList = new List<ModelCustomData>();
        }
        public DateTime CREATE_DATETIME { get; set; }

        public string FACILITY { get; set; }

        public string PRODUCT_CODE { get; set; }

        public string CATEGORY { get; set; }

        public string DESCRIPTION { get; set; }

        public string PROD_ALTDESC { get; set; }

        public double STD_COST { get; set; }

        public string RECEIVING_UOM { get; set; }

        public double PRODUCT_WEIGHT { get; set; }

        public string CW_UOM { get; set; }

        public double BASE_TO_RECV_CONV_FACTOR { get; set; }

        public string STATUS_FLAG { get; set; }

        public string ITEM_CUSTOMDATA1 { get; set; }

        public string ITEM_CUSTOMDATA2 { get; set; }

        public string ITEM_CUSTOMDATA3 { get; set; }

        public string UPC_CODE { get; set; }

        public string ITEM_TYPE { get; set; }

        public string UNIT1_UOM { get; set; }

        public double CONVERSION_UNIT_1 { get; set; }

        public string UNIT2_UOM { get; set; }

        public double CONVERSION_UNIT_2 { get; set; }

        public string UNIT3_UOM { get; set; }

        public double CONVERSION_UNIT_3 { get; set; }

        public string UNIT4_UOM { get; set; }

        public double CONVERSION_UNIT_4 { get; set; }

        public string GTIN_CODE_1 { get; set; }

        public string GTIN_CODE_2 { get; set; }

        public string GTIN_CODE_3 { get; set; }

        public string GTIN_CODE_4 { get; set; }

        public double UNITWIDTH { get; set; }

        public double UNITLENGTH { get; set; }

        public double UNITHEIGHT { get; set; }

        public double UNITWEIGHT { get; set; }

        public string CATEGORY_2 { get; set; }

        public string STOCK_UOM { get; set; }

        public string BUY_UOM { get; set; }

        public string BUYER { get; set; }

        public string VENDORID { get; set; }

        public string SCC14 { get; set; }

        public double CUBIC_PER_EACH { get; set; }

        public string ABC_ZONE { get; set; }

        public double SHELF_LIFE { get; set; }

        public string AUTO_QC_REASON { get; set; }

        public double RETAIL_PRICE { get; set; }

        public string COUNTRY_OF_ORIGIN { get; set; }

        public string SKID_TRACKED { get; set; }

        public string SERIAL_TRACKED { get; set; }

        public double TARE_WEIGHT { get; set; }

        public double BULK_TARE_WEIGHT { get; set; }

        public string BILL_UOM { get; set; }

        public string HAZMAT_FLAG { get; set; }
        public string LOT_FLAG { get; set; }
        public string LOT_PROD_FLAG { get; set; }

        public double EXPIRE_DAYS { get; set; }

        public string EXP_DATE_REQ_FLAG { get; set; }

        public double RESTOCK_QTY { get; set; }

        public double LEADTIME { get; set; }

        public double MINIMUM { get; set; }

        public double MAXIMUM { get; set; }

        public string REF_NOTES { get; set; }

        public string LABEL_UOM { get; set; }

        public double INHOUSE_TIME { get; set; }

        public double HOST_QTY { get; set; }

        public string FREIGHT_CLASS_CODE { get; set; }

        public string BUNDLE_SIZE { get; set; }

        public string VMI_CUSTID { get; set; }

        public string VMI_RESPID { get; set; }

        public string THUMBNAIL_FILENAME { get; set; }

        public string ORGANIC_FLAG { get; set; }

        public string POST_LOT_TO_HOST_FLAG { get; set; }

        public string PKG_MATERIAL_FLAG { get; set; }

        public string MFG_ID { get; set; }

        public string VENDOR1ITEMNUM { get; set; }
        public string ZONEID { get; set; }
        public double ORDERMINIMUM { get; set; }
        public double MRP_SAFETY_STOCK { get; set; }
        public Dictionary<string,string> ExtDataList { get; set; }
        public List<NotesImport> NotesList { get; set; }
        public List<ModelCustomData> CustomList { get; set; }
    }

}

