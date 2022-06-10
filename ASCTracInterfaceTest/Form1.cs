using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ASCTracInterfaceTest
{
    public partial class Form1 : Form
    {
        ascLibrary.ascDBUtils myDBUtils;
        ascLibrary.ascDBUtils myASCDBUtils;
        ascLibrary.ascDBUtils myAWCSBUtils;
        RestService myRestService;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            myDBUtils = new ascLibrary.ascDBUtils();
            myDBUtils.BuildConnectString("AliasASCTracInterface");
            myASCDBUtils = new ascLibrary.ascDBUtils();
            myASCDBUtils.BuildConnectString("AliasASCTrac");

            myAWCSBUtils = new ascLibrary.ascDBUtils();
            myAWCSBUtils.BuildConnectString("AliasWCS");

            lblInterfaceDB.Text = myDBUtils.fServer + ", " + myDBUtils.fDatabase;

            string tmp = string.Empty;
            if (!myASCDBUtils.ReadFieldFromDB("SELECT CFGDATA FROM CFGSETTINGS WHERE CFGFIELD='InterfaceTestUrl'", "", ref tmp))
                tmp = "https://localhost:44344/";
            // https://localhost:44344/
            // http://10.169.0.30/
            // Example of getting structure
            // http://10.169.0.30/Help/Api/POST-api-VendorImport
            // https://localhost:44344/Help/Api/POST-api-VendorImport

            edURL.Text = tmp;

            myRestService = new RestService();
        }

        private void SetURL()
        {
            string tmp = "";
            if (myASCDBUtils.ReadFieldFromDB("SELECT CFGDATA FROM CFGSETTINGS WHERE CFGFIELD='InterfaceTestUrl'", "", ref tmp))
            {
                myASCDBUtils.RunSqlCommand("UPDATE CFGSETTINGS SET CFGDATA='" + edURL.Text + "' WHERE CFGFIELD='InterfaceTestUrl'");
            }
            else
            {
                myASCDBUtils.RunSqlCommand("INSERT INTO CFGSETTINGS" +
                    " (SITE_ID, USERID, CFGFIELD, CFGDATA, SECTION, DESCRIPTION)" +
                    " VALUES ( '1', 'GW', 'InterfaceTestUrl', '" + edURL.Text + "', '[INTERFACE]', 'Test Interface URL')");

            }

            myRestService.fURL = edURL.Text;
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            SetURL();

            if (cbFunction.Text == "ItemImport")
                doItemImport();
            else if (cbFunction.Text == "VendorImport")
                doVendorImport();
            else if (cbFunction.Text == "POImport")
                doPOImport();
            else if (cbFunction.Text == "POExport - Lines")
                doPOLinesExport();
            else if (cbFunction.Text == "POExport - Licenses")
                doPOLicensesExport();
            else if (cbFunction.Text == "ASNImport")
                doASNImport();
            else if (cbFunction.Text == "CustOrderImport")
                doCOImport();
            else if (cbFunction.Text == "CustOrderExport")
                doCOExport();
            else if (cbFunction.Text == "ParcelExport")
                doParcelExport();
            else if (cbFunction.Text == "TranfileExport")
                doTranfileExport();
            else if (cbFunction.Text == "WCS-Pick")
                doWCSExport("C");
            else if (cbFunction.Text == "WCS-Repick")
                doWCSExport("R");
            else if (cbFunction.Text == "WCS-Unpick")
                doWCSExport("N");
            else
                MessageBox.Show("Unrecognized function " + cbFunction.Text + ".");
        }

        private async void doItemImport()
        {
            string sql = "SELECT * FROM TBL_TOASC_ITEMMSTR WHERE PROCESS_FLAG = 'R'";
            SqlConnection myConnection = new SqlConnection(myDBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sql, myConnection);
            myConnection.Open();
            try
            {
                SqlDataReader dr = myCommand.ExecuteReader();
                while (dr.Read())
                {
                    var data = new ASCTracInterfaceModel.Model.Item.ItemMasterImport();

                    data.CREATE_DATETIME = DateTime.Now;
                    data.FACILITY = dr["FACILITY"].ToString();
                    data.PRODUCT_CODE = dr["PRODUCT_CODE"].ToString();
                    data.CATEGORY = dr["CATEGORY"].ToString();
                    data.DESCRIPTION = dr["DESCRIPTION"].ToString();
                    data.PROD_ALTDESC = dr["PROD_ALTDESC"].ToString();
                    data.STD_COST = ascLibrary.ascUtils.ascStrToDouble(dr["STD_COST"].ToString(), 0);
                    data.RECEIVING_UOM = dr["RECEIVING_UOM"].ToString();
                    data.PRODUCT_WEIGHT = ascLibrary.ascUtils.ascStrToDouble(dr["PRODUCT_WEIGHT"].ToString(), 0);
                    data.CW_UOM = dr["CW_UOM"].ToString();
                    data.BASE_TO_RECV_CONV_FACTOR = ascLibrary.ascUtils.ascStrToDouble(dr["BASE_TO_RECV_CONV_FACTOR"].ToString(), 0);
                    data.STATUS_FLAG = dr["STATUS_FLAG"].ToString();
                    /*
                    data.ITEM_CUSTOMDATA1 = dr["ITEM_CUSTOMDATA1"].ToString();

        public string ITEM_CUSTOMDATA2 { get; set; }

        public string ITEM_CUSTOMDATA3 { get; set; }
                    */
                    data.UPC_CODE = dr["UPC_CODE"].ToString();
                    data.ITEM_TYPE = dr["ITEM_TYPE"].ToString();
                    data.UNIT1_UOM = dr["UNIT1_UOM"].ToString();
                    data.CONVERSION_UNIT_1 = ascLibrary.ascUtils.ascStrToDouble(dr["CONVERSION_UNIT_1"].ToString(), 1);
                    data.UNIT2_UOM = dr["UNIT2_UOM"].ToString();
                    data.CONVERSION_UNIT_2 = ascLibrary.ascUtils.ascStrToDouble(dr["CONVERSION_UNIT_2"].ToString(), 0);
                    data.UNIT3_UOM = dr["UNIT3_UOM"].ToString();
                    data.CONVERSION_UNIT_3 = ascLibrary.ascUtils.ascStrToDouble(dr["CONVERSION_UNIT_3"].ToString(), 0);
                    data.UNIT4_UOM = dr["UNIT4_UOM"].ToString();
                    data.CONVERSION_UNIT_4 = ascLibrary.ascUtils.ascStrToDouble(dr["CONVERSION_UNIT_4"].ToString(), 0);

                    data.GTIN_CODE_1 = dr["GTIN_CODE_1"].ToString();
                    data.GTIN_CODE_2 = dr["GTIN_CODE_2"].ToString();
                    data.GTIN_CODE_3 = dr["GTIN_CODE_3"].ToString();
                    data.GTIN_CODE_4 = dr["GTIN_CODE_4"].ToString();

                    data.UNITWIDTH = ascLibrary.ascUtils.ascStrToDouble(dr["UNITWIDTH"].ToString(), 0);
                    data.UNITLENGTH = ascLibrary.ascUtils.ascStrToDouble(dr["UNITLENGTH"].ToString(), 0);
                    data.UNITHEIGHT = ascLibrary.ascUtils.ascStrToDouble(dr["UNITHEIGHT"].ToString(), 0);
                    data.UNITWEIGHT = ascLibrary.ascUtils.ascStrToDouble(dr["UNITWEIGHT"].ToString(), 0);

                    data.CATEGORY_2 = dr["CATEGORY_2"].ToString();
                    data.STOCK_UOM = dr["STOCK_UOM"].ToString();
                    data.BUY_UOM = dr["BUY_UOM"].ToString();
                    data.BUYER = dr["BUYER"].ToString();
                    data.VENDORID = dr["VENDORID"].ToString();
                    data.SCC14 = dr["SCC14"].ToString();
                    data.CUBIC_PER_EACH = ascLibrary.ascUtils.ascStrToDouble(dr["CUBIC_PER_EACH"].ToString(), 0);
                    data.ABC_ZONE = dr["ABC_ZONE"].ToString();
                    data.SHELF_LIFE = ascLibrary.ascUtils.ascStrToDouble(dr["SHELF_LIFE"].ToString(), 0);
                    data.AUTO_QC_REASON = dr["AUTO_QC_REASON"].ToString();
                    data.RETAIL_PRICE = ascLibrary.ascUtils.ascStrToDouble(dr["RETAIL_PRICE"].ToString(), 0);
                    data.COUNTRY_OF_ORIGIN = dr["COUNTRY_OF_ORIGIN"].ToString();
                    data.SKID_TRACKED = dr["SKID_TRACKED"].ToString();

                    data.SERIAL_TRACKED = dr["SERIAL_TRACKED"].ToString();

                    data.TARE_WEIGHT = ascLibrary.ascUtils.ascStrToDouble(dr["TARE_WEIGHT"].ToString(), 0);
                    data.BULK_TARE_WEIGHT = ascLibrary.ascUtils.ascStrToDouble(dr["BULK_TARE_WEIGHT"].ToString(), 0);
                    data.BILL_UOM = dr["BILL_UOM"].ToString();

                    data.HAZMAT_FLAG = dr["HAZMAT_FLAG"].ToString();
                    data.LOT_FLAG = dr["LOT_FLAG"].ToString();
                    data.LOT_PROD_FLAG = dr["LOT_PROD_FLAG"].ToString();

                    data.EXPIRE_DAYS = ascLibrary.ascUtils.ascStrToDouble(dr["EXPIRE_DAYS"].ToString(), 0);
                    data.EXP_DATE_REQ_FLAG = dr["EXP_DATE_REQ_FLAG"].ToString();

                    data.RESTOCK_QTY = ascLibrary.ascUtils.ascStrToDouble(dr["RESTOCK_QTY"].ToString(), 0);
                    data.LEADTIME = ascLibrary.ascUtils.ascStrToDouble(dr["LEADTIME"].ToString(), 0);

                    data.MINIMUM = ascLibrary.ascUtils.ascStrToDouble(dr["MINIMUM"].ToString(), 0);

                    data.MAXIMUM = ascLibrary.ascUtils.ascStrToDouble(dr["MAXIMUM"].ToString(), 0);

                    data.REF_NOTES = dr["REF_NOTES"].ToString();

                    data.LABEL_UOM = dr["LABEL_UOM"].ToString();

                    data.INHOUSE_TIME = ascLibrary.ascUtils.ascStrToDouble(dr["INHOUSE_TIME"].ToString(), 0);

                    data.HOST_QTY = ascLibrary.ascUtils.ascStrToDouble(dr["HOST_QTY"].ToString(), 0);

                    data.FREIGHT_CLASS_CODE = dr["FREIGHT_CLASS_CODE"].ToString();

                    data.BUNDLE_SIZE = dr["BUNDLE_SIZE"].ToString();

                    data.VMI_CUSTID = dr["VMI_CUSTID"].ToString();

                    data.VMI_RESPID = dr["VMI_RESPID"].ToString();

                    /*
                    data.ITEM_CUSTOMDATA8 = dr["ITEM_CUSTOMDATA8"].ToString(); 

        public string ITEM_CUSTOMDATA7 { get; set; }

        public string ITEM_CUSTOMDATA6 { get; set; }

        public string ITEM_CUSTOMDATA5 { get; set; }

        public string ITEM_CUSTOMDATA4 { get; set; }
                    */
                    data.THUMBNAIL_FILENAME = dr["THUMBNAIL_FILENAME"].ToString();
                    data.ORGANIC_FLAG = dr["ORGANIC_FLAG"].ToString();
                    data.POST_LOT_TO_HOST_FLAG = dr["POST_LOT_TO_HOST_FLAG"].ToString();
                    data.PKG_MATERIAL_FLAG = dr["PKG_MATERIAL_FLAG"].ToString();
                    data.MFG_ID = dr["MFG_ID"].ToString();
                    data.VENDOR1ITEMNUM = dr["VENDOR1ITEMNUM"].ToString();

                    for (int i = 1; i <= 8; i++)
                    {
                        string fieldname = "ITEM_CUSTOMDATA" + i.ToString();
                        if (!String.IsNullOrEmpty(dr[fieldname].ToString()))
                            data.CustomList.Add(new ASCTracInterfaceModel.Model.ModelCustomData(fieldname, dr[fieldname].ToString()));
                    }

                    AddItemExtData(data);

                    var myResult = myRestService.doItemImport(data).Result;

                    lblResultCode.Text = myResult.StatusCode.ToString();
                    tbContent.Text = await myResult.Content.ReadAsStringAsync();
                }
            }
            finally
            {
                myConnection.Close();
            }
        }


        private void AddItemExtData(ASCTracInterfaceModel.Model.Item.ItemMasterImport data)
        {
            string sql = "SELECT PROMPT_NUM, VALUE FROM TBL_TOASC_ITEMMSTR_EXTDATA WHERE TBLNAME='ITEMMSTR' AND FACILITY='" + data.FACILITY + "' AND PRODUCT_CODE='" + data.PRODUCT_CODE + "'";
            SqlConnection myConnection = new SqlConnection(myDBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sql, myConnection);
            myConnection.Open();
            try
            {
                SqlDataReader dr = myCommand.ExecuteReader();
                while (dr.Read())
                {
                    string prompt = string.Empty;
                    if (myASCDBUtils.ReadFieldFromDB("SELECT DISPLAY_STRING FROM EXTDATA_SETUP WHERE TBLNAME='ITEMMSTR' AND PROMPT_NUM=" + dr["PROMPT_NUM"].ToString(), "", ref prompt))
                        data.ExtDataList.Add(prompt, dr["VALUE"].ToString());
                }
            }
            finally
            {
                myConnection.Close();
            }
        }

        private async void doVendorImport()
        {
            string sql = "SELECT * FROM TBL_TOASC_VENDOR_MSTR WHERE PROCESS_FLAG = 'R'";
            SqlConnection myConnection = new SqlConnection(myDBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sql, myConnection);
            myConnection.Open();
            try
            {
                SqlDataReader dr = myCommand.ExecuteReader();
                while ((dr.Read()))
                {
                    var data = new ASCTracInterfaceModel.Model.Vendor.VendorImport();
                    data.VENDOR_CODE = dr["VENDOR_CODE"].ToString();
                    data.VENDOR_DESC = dr["VENDOR_DESC"].ToString();

                    data.ADDR_LINE1 = dr["ADDR_LINE1"].ToString();
                    data.ADDR_LINE2 = dr["ADDR_LINE2"].ToString();
                    data.CITY = dr["CITY"].ToString();
                    data.STATE = dr["STATE"].ToString();
                    data.ZIP = dr["ZIP"].ToString();
                    data.COUNTRY = dr["COUNTRY"].ToString();
                    data.CONTACT_NAME = dr["CONTACT_NAME"].ToString();
                    data.CONTACT_TEL = dr["CONTACT_TEL"].ToString();
                    data.CONTACT_FAX = dr["CONTACT_FAX"].ToString();

                    data.REMIT_TO_ADDR_LINE1 = dr["REMIT_TO_ADDR_LINE1"].ToString();
                    data.REMIT_TO_ADDR_LINE2 = dr["REMIT_TO_ADDR_LINE2"].ToString();
                    data.REMIT_TO_CITY = dr["REMIT_TO_CITY"].ToString();
                    data.REMIT_TO_STATE = dr["REMIT_TO_STATE"].ToString();
                    data.REMIT_TO_ZIP = dr["REMIT_TO_ZIP"].ToString();
                    data.REMIT_TO_COUNTRY = dr["REMIT_TO_COUNTRY"].ToString();
                    data.REMIT_TO_CONTACT_NAME = dr["REMIT_TO_CONTACT_NAME"].ToString();
                    data.REMIT_TO_CONTACT_TEL = dr["REMIT_TO_CONTACT_TEL"].ToString();
                    data.REMIT_TO_CONTACT_FAX = dr["REMIT_TO_CONTACT_FAX"].ToString();

                    data.TERMS_ID = dr["TERMS_ID"].ToString();
                    data.STATUS = dr["STATUS"].ToString();
                    data.MASTER_VENDORID = dr["MASTER_VENDORID"].ToString();
                    data.ORGANIC_FLAG = dr["ORGANIC_FLAG"].ToString();
                    data.ORGANIC_REG_NUM = dr["ORGANIC_REG_NUM"].ToString();
                    data.AUTOCLOSEPO = dr["AUTOCLOSEPO"].ToString();
                    data.ENABLE_LICENSE_LEVEL_VALIDATION = dr["ENABLE_LICENSE_LEVEL_VALIDATION"].ToString();
                    var myResult = myRestService.doVendorImport(data).Result;

                    lblResultCode.Text = myResult.StatusCode.ToString();
                    tbContent.Text = await myResult.Content.ReadAsStringAsync();
                }
            }
            finally
            {
                myConnection.Close();
            }

        }
        private async void doPOImport()
        {
            string sql = "SELECT * FROM TBL_TOASC_PO_HEADER WHERE PROCESS_FLAG = 'R'";
            SqlConnection myConnection = new SqlConnection(myDBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sql, myConnection);
            myConnection.Open();
            try
            {
                SqlDataReader dr = myCommand.ExecuteReader();
                while ((dr.Read()))
                {
                    var data = new ASCTracInterfaceModel.Model.PO.POHdrImport();

                    data.FACILITY = dr["FACILITY"].ToString();
                    data.ORDER_TYPE = dr["ORDER_TYPE"].ToString();
                    data.PONUMBER = dr["PONUMBER"].ToString();
                    data.VENDOR_CODE = dr["VENDOR_CODE"].ToString();
                    data.TO_FACILITY = dr["TO_FACILITY"].ToString();
                    data.LEAVES_DATE = ascLibrary.ascUtils.ascStrToDate(dr["LEAVES_DATE"].ToString(), DateTime.MinValue);
                    data.ARRIVAL_DATE = ascLibrary.ascUtils.ascStrToDate(dr["ARRIVAL_DATE"].ToString(), DateTime.MinValue);
                    data.ENTRY_DATE = ascLibrary.ascUtils.ascStrToDate(dr["ENTRY_DATE"].ToString(), DateTime.MinValue);
                    data.CARRIER = dr["CARRIER"].ToString();
                    data.DELIVERY_INSTRUCTIONS = dr["DELIVERY_INSTRUCTIONS"].ToString();
                    data.ADDR_LINE1 = dr["ADDR_LINE1"].ToString();
                    data.ADDR_LINE2 = dr["ADDR_LINE2"].ToString();
                    data.CITY = dr["CITY"].ToString();
                    data.STATE = dr["STATE"].ToString();
                    data.ZIP = dr["ZIP"].ToString();
                    data.COUNTRY = dr["COUNTRY"].ToString();
                    data.CONTACT_NAME = dr["CONTACT_NAME"].ToString();
                    data.CONTACT_TEL = dr["CONTACT_TEL"].ToString();
                    data.CONTACT_FAX = dr["CONTACT_FAX"].ToString();
                    data.STATUS_FLAG = dr["STATUS_FLAG"].ToString();
                    for (int i = 1; i <= 10; i++)
                    {
                        string fieldname = "CUSTOM_DATA" + i.ToString();
                        string value = dr[fieldname].ToString();
                        if (!String.IsNullOrEmpty(value))
                            data.CustomList.Add(new ASCTracInterfaceModel.Model.ModelCustomData(fieldname, value));
                    }
                    data.RMA_TYPE = dr["RMA_TYPE"].ToString();
                    data.LINKED_ORDERNUMBER = dr["LINKED_ORDERNUMBER"].ToString();
                    data.BUYER_CODE_ID = dr["BUYER_CODE_ID"].ToString();
                    data.ADDR_LINE3 = dr["ADDR_LINE3"].ToString();
                    data.TERMS_ID = dr["TERMS_ID"].ToString();
                    data.RELEASENUM = dr["RELEASENUM"].ToString();
                    data.REQ_NUM = dr["REQ_NUM"].ToString();
                    data.BILL_ADDR_LINE1 = dr["BILL_ADDR_LINE1"].ToString();
                    data.BILL_ADDR_LINE2 = dr["BILL_ADDR_LINE2"].ToString();
                    data.BILL_ADDR_LINE3 = dr["BILL_ADDR_LINE3"].ToString();
                    //data.BILL_CITY_LINE3 = dr["BILL_CITY_LINE3"].ToString();
                    data.BILL_CITY = dr["BILL_CITY"].ToString();
                    data.BILL_STATE = dr["BILL_STATE"].ToString();
                    data.BILL_ZIP = dr["BILL_ZIP"].ToString();
                    data.BILL_COUNTRY = dr["BILL_COUNTRY"].ToString();
                    data.BILL_CONTACT_NAME = dr["BILL_CONTACT_NAME"].ToString();
                    data.BILL_CONTACT_TEL = dr["BILL_CONTACT_TEL"].ToString();
                    data.BILL_CONTACT_FAX = dr["BILL_CONTACT_FAX"].ToString();
                    data.DIRECT_SHIP_ORDERNUMBER = dr["DIRECT_SHIP_ORDERNUMBER"].ToString();
                    data.SHIP_TO_NAME = dr["SHIP_TO_NAME"].ToString();
                    data.BILL_TO_NAME = dr["BILL_TO_NAME"].ToString();
                    data.SEAL_NUM = dr["SEAL_NUM"].ToString();
                    data.VMI_CUSTID = dr["VMI_CUSTID"].ToString();
                    data.ASN = dr["ASN"].ToString();
                    data.PROMO_CODE = dr["PROMO_CODE"].ToString();

                    AddPODet(data);
                    AddPONotes(data);

                    var myResult = myRestService.doPOImport(data).Result;

                    lblResultCode.Text = myResult.StatusCode.ToString();
                    tbContent.Text = await myResult.Content.ReadAsStringAsync();
                }
            }
            finally
            {
                myConnection.Close();
            }
        }

        private void AddPODet(ASCTracInterfaceModel.Model.PO.POHdrImport data)
        {
            string sql = "SELECT * FROM TBL_TOASC_PO_DETAIL WHERE PROCESS_FLAG = 'R' AND PONUMBER='" + data.PONUMBER + "'";
            if (!String.IsNullOrEmpty(data.RELEASENUM))
                sql += " AND RELEASENUM='" + data.RELEASENUM + "'";
            SqlConnection myConnection = new SqlConnection(myDBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sql, myConnection);
            myConnection.Open();
            try
            {
                SqlDataReader dr = myCommand.ExecuteReader();
                while ((dr.Read()))
                {
                    var rec = new ASCTracInterfaceModel.Model.PO.PODetImport();


                    rec.LINE_NUMBER = ascLibrary.ascUtils.ascStrToInt(dr["LINE_NUMBER"].ToString(), 0);
                    rec.VENDOR_ITEM_ID = dr["VENDOR_ITEM_ID"].ToString();
                    rec.PRODUCT_CODE = dr["PRODUCT_CODE"].ToString();
                    rec.QUANTITY = ascLibrary.ascUtils.ascStrToDouble(dr["QUANTITY"].ToString(), 0);

                    rec.EXPECTED_RECEIPT_DATE = ascLibrary.ascUtils.ascStrToDate(dr["EXPECTED_RECEIPT_DATE"].ToString(), DateTime.MinValue);
                    rec.COMMENT = dr["COMMENT"].ToString();
                    rec.COSTEACH = ascLibrary.ascUtils.ascStrToDouble(dr["COSTEACH"].ToString(), 0);
                    rec.CW_UOM = dr["CW_UOM"].ToString();
                    rec.STATUS_FLAG = dr["STATUS_FLAG"].ToString();
                    rec.UPC_CODE = dr["UPC_CODE"].ToString();
                    rec.ITEM_DESCRIPTION = dr["ITEM_DESCRIPTION"].ToString();
                    rec.LOTID = dr["LOTID"].ToString();
                    rec.UOM = dr["UOM"].ToString();
                    rec.DIRECT_SHIP_ORDERNUMBER = dr["DIRECT_SHIP_ORDERNUMBER"].ToString();
                    rec.LINKED_ORDERNUMBER = dr["LINKED_ORDERNUMBER"].ToString();
                    for (int i = 1; i <= 10; i++)
                    {
                        string fieldname = "CUSTOM_DATA" + i.ToString();
                        string value = dr[fieldname].ToString();
                        if (!String.IsNullOrEmpty(value))
                            rec.CustomList.Add(new ASCTracInterfaceModel.Model.ModelCustomData(fieldname, value));
                    }
                    rec.SKIDID = dr["SKIDID"].ToString();
                    rec.SERIAL_NUM = dr["SERIAL_NUM"].ToString();
                    rec.PROMO_CODE = dr["PROMO_CODE"].ToString();
                    rec.RELEASENUM = dr["RELEASENUM"].ToString();
                    rec.EXPECTED_SHIP_DATE = ascLibrary.ascUtils.ascStrToDate(dr["EXPECTED_SHIP_DATE"].ToString(), DateTime.MinValue);
                    rec.USER_REQUIRED_DATE = ascLibrary.ascUtils.ascStrToDate(dr["USER_REQUIRED_DATE"].ToString(), DateTime.MinValue);
                    rec.ORIG_ORDERNUMBER = dr["ORIG_ORDERNUMBER"].ToString();
                    rec.CW_QTY = ascLibrary.ascUtils.ascStrToDouble(dr["CW_QTY"].ToString(), 0);
                    rec.VEND_PRODLINE = dr["VEND_PRODLINE"].ToString();
                    rec.QC_REASON = dr["QC_REASON"].ToString();
                    rec.ALT_LOTID = dr["ALT_LOTID"].ToString();
                    rec.HOST_LINENUMBER = ascLibrary.ascUtils.ascStrToInt(dr["HOST_LINENUMBER"].ToString(), 0);



                    data.PODetList.Add(rec);
                }
            }
            finally
            {
                myConnection.Close();
            }
        }

        private void AddPONotes(ASCTracInterfaceModel.Model.PO.POHdrImport data)
        {
        }

        async private void doPOLinesExport()
        {
            ASCTracInterfaceModel.Model.PO.POExportFilter poExportFilter = new ASCTracInterfaceModel.Model.PO.POExportFilter(false);
            var myResult = myRestService.doPOLinesExport(poExportFilter).Result;

            lblResultCode.Text = myResult.StatusCode.ToString();
            tbContent.Text = await myResult.Content.ReadAsStringAsync();

            if (MessageBox.Show("Update PO Results", "PO Export", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
            {
                var mylist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ASCTracInterfaceModel.Model.PO.POExportLines>>(tbContent.Text);
                foreach (var rec in mylist)
                {
                    rec.SUCCESSFUL = true;
                }
                myResult = myRestService.updatePOLinesExport(mylist).Result;

                lblResultCode.Text = myResult.StatusCode.ToString();
                tbContent.Text = await myResult.Content.ReadAsStringAsync();

            }

        }
        async private void doPOLicensesExport()
        {
            ASCTracInterfaceModel.Model.PO.POExportFilter poExportFilter = new ASCTracInterfaceModel.Model.PO.POExportFilter(false);
            var myResult = myRestService.doPOLicensesExport(poExportFilter).Result;

            lblResultCode.Text = myResult.StatusCode.ToString();
            tbContent.Text = await myResult.Content.ReadAsStringAsync();

            if (MessageBox.Show("Update PO Results", "PO Export", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
            {
                var mylist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ASCTracInterfaceModel.Model.PO.POExportLicenses>>(tbContent.Text);
                foreach (var rec in mylist)
                {
                    rec.SUCCESSFUL = true;
                }
                myResult = myRestService.UpdatePOLicensesExport(mylist).Result;

                lblResultCode.Text = myResult.StatusCode.ToString();
                tbContent.Text = await myResult.Content.ReadAsStringAsync();

            }
        }

        async private void doASNImport()
        {
            string sql = "SELECT * FROM TBL_TOASC_ASN_HEADER WHERE PROCESS_FLAG = 'R'";
            SqlConnection myConnection = new SqlConnection(myDBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sql, myConnection);
            myConnection.Open();
            try
            {
                SqlDataReader dr = myCommand.ExecuteReader();
                while ((dr.Read()))
                {
                    var data = new ASCTracInterfaceModel.Model.ASN.ASNHdrImport();

                    data.FACILITY = dr["FACILITY"].ToString();
                    data.CREATE_DATETIME = ascLibrary.ascUtils.ascStrToDate(dr["CREATE_DATETIME"].ToString(), DateTime.Now);
                    data.FACILITY = dr["FACILITY"].ToString();
                    data.ASN = dr["ASN"].ToString();
                    data.PONUMBER = dr["PONUMBER"].ToString();
                    data.TRUCKNUM = dr["TRUCKNUM"].ToString();
                    data.FROM_FACILITY = dr["FROM_FACILITY"].ToString();
                    data.REF_ORDERNUMBER = dr["REF_ORDERNUMBER"].ToString();
                    data.EXPECTED_RECEIPT_DATE = ascLibrary.ascUtils.ascStrToDate(dr["EXPECTED_RECEIPT_DATE"].ToString(), DateTime.MinValue);
                    data.VENDORID = dr["VENDORID"].ToString();
                    data.PACKINGSLIP = dr["PACKINGSLIP"].ToString();

                    data.ASN_TYPE = dr["ASN_TYPE"].ToString();
                    AddASNDet(data);

                    var myResult = myRestService.doASNImport(data).Result;

                    lblResultCode.Text = myResult.StatusCode.ToString();
                    tbContent.Text = await myResult.Content.ReadAsStringAsync();
                }
            }
            finally
            {
                myConnection.Close();
            }
        }

        private void AddASNDet(ASCTracInterfaceModel.Model.ASN.ASNHdrImport HDRdata)
        {
            string sql = "SELECT * FROM TBL_TOASC_ASN_DETAIL WHERE PROCESS_FLAG = 'R' AND ASN='" + HDRdata.ASN + "'";
            SqlConnection myConnection = new SqlConnection(myDBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sql, myConnection);
            myConnection.Open();
            try
            {
                SqlDataReader dr = myCommand.ExecuteReader();
                while ((dr.Read()))
                {
                    var data = new ASCTracInterfaceModel.Model.ASN.ASNDetImport();

                    data.ITEMID = dr["ITEMID"].ToString();
                    data.LOTID = dr["LOTID"].ToString();
                    data.SKIDID = dr["SKIDID"].ToString();
                    data.QUANTITY = ascLibrary.ascUtils.ascStrToDouble(dr["QUANTITY"].ToString(), 0);
                    data.EXPIRE_DATE = ascLibrary.ascUtils.ascStrToDate(dr["EXPIRE_DATE"].ToString(), DateTime.MinValue);
                    data.PONUMBER = dr["PONUMBER"].ToString();
                    data.RELEASENUM = dr["RELEASENUM"].ToString();
                    data.LINENUMBER = ascLibrary.ascUtils.ascStrToDouble(dr["LINENUMBER"].ToString(), 0);
                    data.VMI_CUSTID = dr["VMI_CUSTID"].ToString();
                    data.ACTUAL_WEIGHT = ascLibrary.ascUtils.ascStrToDouble(dr["ACTUAL_WEIGHT"].ToString(), 0);
                    data.CW_QTY = ascLibrary.ascUtils.ascStrToDouble(dr["CW_QTY"].ToString(), 0);
                    data.VENDORID = dr["VENDORID"].ToString();
                    data.CONTAINER_ID = dr["CONTAINER_ID"].ToString();
                    data.PALLET_TYPE = dr["PALLET_TYPE"].ToString();
                    data.DATETIMEPROD = ascLibrary.ascUtils.ascStrToDate(dr["DATETIMEPROD"].ToString(), DateTime.MinValue);
                    data.ALT_SKIDID = dr["ALT_SKIDID"].ToString();
                    data.ALT_LOTID = dr["ALT_LOTID"].ToString();
                    for (int i = 1; i <= 6; i++)
                    {
                        string fieldname = "CUSTOM_DATA" + i.ToString();
                        if (!String.IsNullOrEmpty(dr[fieldname].ToString()))
                            data.CustomList.Add(new ASCTracInterfaceModel.Model.ModelCustomData(fieldname, dr[fieldname].ToString()));
                    }

                    HDRdata.DetailList.Add(data);
                }
            }
            finally
            {
                myConnection.Close();
            }

        }

        async private void doCOImport()
        {
            string sql = "SELECT * FROM TBL_TOASC_CUST_ORDR_HEADER WHERE PROCESS_FLAG = 'R'";
            SqlConnection myConnection = new SqlConnection(myDBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sql, myConnection);
            myConnection.Open();
            try
            {
                SqlDataReader dr = myCommand.ExecuteReader();
                while ((dr.Read()))
                {
                    var data = new ASCTracInterfaceModel.Model.CustOrder.OrdrHdrImport();

                    data.FACILITY = dr["FACILITY"].ToString();
                    data.ORDER_TYPE = dr["ORDER_TYPE"].ToString();
                    data.ORDERNUMBER = dr["ORDERNUMBER"].ToString();
                    data.ORDER_CREATE_DATE = ascLibrary.ascUtils.ascStrToDate(dr["ORDER_CREATE_DATE"].ToString(), DateTime.MinValue);

                    //data.CUST_ID = dr["CUST_ID"].ToString();
                    data.FROM_FACILITY = dr["FROM_FACILITY"].ToString();
                    data.LEAVES_DATE = ascLibrary.ascUtils.ascStrToDate(dr["LEAVES_DATE"].ToString(), DateTime.MinValue);
                    data.ENTRY_DATE = ascLibrary.ascUtils.ascStrToDate(dr["ENTRY_DATE"].ToString(), DateTime.MinValue);
                    data.CARRIER = dr["CARRIER"].ToString();
                    data.PAYMENT_TYPE = dr["PAYMENT_TYPE"].ToString();

                    data.SHIP_TO_CUST_ID = dr["SHIP_TO_CUST_ID"].ToString();
                    data.SHIP_TO_NAME = dr["SHIP_TO_NAME"].ToString();
                    data.SHIP_TO_ADDR_LINE1 = dr["SHIP_TO_ADDR_LINE1"].ToString();
                    data.SHIP_TO_ADDR_LINE2 = dr["SHIP_TO_ADDR_LINE2"].ToString();
                    data.SHIP_TO_ADDR_LINE3 = dr["SHIP_TO_ADDR_LINE3"].ToString();
                    data.SHIP_TO_CITY = dr["SHIP_TO_CITY"].ToString();
                    data.SHIP_TO_STATE = dr["SHIP_TO_STATE"].ToString();
                    data.SHIP_TO_ZIP = dr["SHIP_TO_ZIP"].ToString();
                    data.SHIP_TO_COUNTRY = dr["SHIP_TO_COUNTRY"].ToString();
                    data.SHIP_TO_CONTACT_NAME = dr["SHIP_TO_CONTACT_NAME"].ToString();
                    data.SHIP_TO_CONTACT_TEL = dr["SHIP_TO_CONTACT_TEL"].ToString();
                    data.SHIP_TO_CONTACT_FAX = dr["SHIP_TO_CONTACT_FAX"].ToString();

                    data.BILL_TO_CUST_ID = dr["BILL_TO_CUST_ID"].ToString();
                    data.BILL_TO_NAME = dr["BILL_TO_NAME"].ToString();
                    data.BILL_TO_ADDR_LINE1 = dr["BILL_TO_ADDR_LINE1"].ToString();
                    data.BILL_TO_ADDR_LINE2 = dr["BILL_TO_ADDR_LINE2"].ToString();
                    data.BILL_TO_ADDR_LINE3 = dr["BILL_TO_ADDR_LINE3"].ToString();
                    data.BILL_TO_CITY = dr["BILL_TO_CITY"].ToString();
                    data.BILL_TO_STATE = dr["BILL_TO_STATE"].ToString();
                    data.BILL_TO_ZIP = dr["BILL_TO_ZIP"].ToString();
                    data.BILL_TO_COUNTRY = dr["BILL_TO_COUNTRY"].ToString();
                    data.BILL_TO_CONTACT_NAME = dr["BILL_TO_CONTACT_NAME"].ToString();
                    data.BILL_TO_CONTACT_TEL = dr["BILL_TO_CONTACT_TEL"].ToString();
                    data.BILL_TO_CONTACT_FAX = dr["BILL_TO_CONTACT_FAX"].ToString();

                    data.CUST_PO_NUM = dr["CUST_PO_NUM"].ToString();
                    data.CUST_BILLTO_PO_NUM = dr["CUST_BILLTO_PO_NUM"].ToString();
                    data.CUST_SHIPTO_PO_NUM = dr["CUST_SHIPTO_PO_NUM"].ToString();
                    data.STATUS_FLAG = dr["STATUS_FLAG"].ToString();
                    data.LOAD_PLAN_NUM = dr["LOAD_PLAN_NUM"].ToString();
                    data.LOAD_STOP_SEQ = dr["LOAD_STOP_SEQ"].ToString();
                    data.PRIORITY_ID = ascLibrary.ascUtils.ascStrToDouble(dr["PRIORITY_ID"].ToString(), 0);
                    data.RECIPIENT_EMAIL = dr["RECIPIENT_EMAIL"].ToString();
                    data.BOL_NUMBER = dr["BOL_NUMBER"].ToString();
                    data.FREIGHT_ACCOUNT_NUMBER = dr["FREIGHT_ACCOUNT_NUMBER"].ToString();
                    data.REFERENCE_NUMBER = dr["REFERENCE_NUMBER"].ToString();
                    data.PREPAY_COLLECT = dr["PREPAY_COLLECT"].ToString();
                    data.CANCEL_DATE = ascLibrary.ascUtils.ascStrToDate(dr["CANCEL_DATE"].ToString(), DateTime.MinValue);
                    data.CARRIER_SERVICE_CODE = dr["CARRIER_SERVICE_CODE"].ToString();
                    data.DELIVERY_INSTRUCTIONS = dr["DELIVERY_INSTRUCTIONS"].ToString();

                    data.COD_AMT = ascLibrary.ascUtils.ascStrToDouble(dr["COD_AMT"].ToString(), 0);
                    data.MUST_ARRIVE_BY_DATE = ascLibrary.ascUtils.ascStrToDate(dr["MUST_ARRIVE_BY_DATE"].ToString(), DateTime.MinValue);
                    data.SALESPERSON = dr["SALESPERSON"].ToString();
                    data.TERMS_ID = dr["TERMS_ID"].ToString();
                    data.LINKED_PONUMBER = dr["LINKED_PONUMBER"].ToString();
                    data.CREDIT_HOLD_STATUS = dr["CREDIT_HOLD_STATUS"].ToString();
                    data.CLIENTDEPT = dr["CLIENTDEPT"].ToString();
                    data.CLIENTDIVISION = dr["CLIENTDIVISION"].ToString();
                    data.CLIENTGLACCT = dr["CLIENTGLACCT"].ToString();
                    data.CLIENTPROFIT = dr["CLIENTPROFIT"].ToString();
                    data.ALLOW_SHORT_SHIP = dr["ALLOW_SHORT_SHIP"].ToString();
                    data.RESIDENTIAL_FLAG = dr["RESIDENTIAL_FLAG"].ToString();
                    data.SHIP_VIA = dr["SHIP_VIA"].ToString();
                    data.AREA = dr["AREA"].ToString();
                    data.ALLOW_OVER_SHIP = dr["ALLOW_OVER_SHIP"].ToString();
                    data.SALESORDERNUMBER = dr["SALESORDERNUMBER"].ToString();

                    data.THIRDPARTYCUSTID = dr["THIRDPARTYCUSTID"].ToString();
                    data.THIRDPARTYNAME = dr["THIRDPARTYNAME"].ToString();
                    data.THIRDPARTYADDRESS1 = dr["THIRDPARTYADDRESS1"].ToString();
                    data.THIRDPARTYADDRESS2 = dr["THIRDPARTYADDRESS2"].ToString();
                    data.THIRDPARTYADDRESS3 = dr["THIRDPARTYADDRESS3"].ToString();
                    data.THIRDPARTYCITY = dr["THIRDPARTYCITY"].ToString();
                    data.THIRDPARTYSTATE = dr["THIRDPARTYSTATE"].ToString();
                    data.THIRDPARTYZIPCODE = dr["THIRDPARTYZIPCODE"].ToString();
                    data.THIRDPARTYCOUNTRY = dr["THIRDPARTYCOUNTRY"].ToString();

                    data.STORE_NUM = dr["STORE_NUM"].ToString();
                    data.DEPT = dr["DEPT"].ToString();
                    data.PACKLIST_REQ = dr["PACKLIST_REQ"].ToString();
                    data.DROP_SHIP = dr["DROP_SHIP"].ToString();
                    data.BATCH_NUM = dr["BATCH_NUM"].ToString();
                    data.ROUTEID = dr["ROUTEID"].ToString();
                    data.PROMO_CODE = dr["PROMO_CODE"].ToString();
                    data.CUSTORDERCAT = dr["CUSTORDERCAT"].ToString();
                    data.FOB = dr["FOB"].ToString();
                    data.COMPLIANCE_LABEL = dr["COMPLIANCE_LABEL"].ToString();
                    data.VMI_GROUPID = dr["VMI_GROUPID"].ToString();
                    data.ORDER_SOURCE_SYSTEM = dr["ORDER_SOURCE_SYSTEM"].ToString();

                    data.FREIGHTBILLTONAME = dr["FREIGHTBILLTONAME"].ToString();
                    data.FREIGHTBILLTOCONTACT = dr["FREIGHTBILLTOCONTACT"].ToString();
                    data.FREIGHTBILLTOADDRESS1 = dr["FREIGHTBILLTOADDRESS1"].ToString();
                    data.FREIGHTBILLTOADDRESS2 = dr["FREIGHTBILLTOADDRESS2"].ToString();
                    data.FREIGHTBILLTOADDRESS3 = dr["FREIGHTBILLTOADDRESS3"].ToString();
                    data.FREIGHTBILLTOADDRESS4 = dr["FREIGHTBILLTOADDRESS4"].ToString();
                    data.FREIGHTBILLTOCITY = dr["FREIGHTBILLTOCITY"].ToString();
                    data.FREIGHTBILLTOSTATE = dr["FREIGHTBILLTOSTATE"].ToString();
                    data.FREIGHTBILLTOZIPCODE = dr["FREIGHTBILLTOZIPCODE"].ToString();
                    data.FREIGHTBILLTOCOUNTRY = dr["FREIGHTBILLTOCOUNTRY"].ToString();
                    data.FREIGHTBILLTOTELEPHONE = dr["FREIGHTBILLTOTELEPHONE"].ToString();
                    data.FREIGHTBILLTOALTTEL = dr["FREIGHTBILLTOALTTEL"].ToString();
                    data.FREIGHTBILLTOFAX = dr["FREIGHTBILLTOFAX"].ToString();

                    AddCODet(data);
                    AddCONotes(data);

                    var myResult = myRestService.doCOImport(data).Result;

                    lblResultCode.Text = myResult.StatusCode.ToString();
                    tbContent.Text = await myResult.Content.ReadAsStringAsync();
                }
            }
            finally
            {
                myConnection.Close();
            }
        }

        private void AddCODet(ASCTracInterfaceModel.Model.CustOrder.OrdrHdrImport data)
        {
            string sql = "SELECT * FROM TBL_TOASC_CUST_ORDR_DETAIL WHERE PROCESS_FLAG = 'R' AND ORDERNUMBER='" + data.ORDERNUMBER + "'";
            SqlConnection myConnection = new SqlConnection(myDBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sql, myConnection);
            myConnection.Open();
            try
            {
                SqlDataReader dr = myCommand.ExecuteReader();
                while ((dr.Read()))
                {
                    var rec = new ASCTracInterfaceModel.Model.CustOrder.OrdrDetImport();


                    rec.LINE_NUMBER = ascLibrary.ascUtils.ascStrToInt(dr["LINE_NUMBER"].ToString(), 0);
                    rec.CUST_ITEMID = dr["CUST_ITEMID"].ToString();
                    rec.PRODUCT_CODE = dr["PRODUCT_CODE"].ToString();
                    rec.QUANTITY = ascLibrary.ascUtils.ascStrToDouble(dr["QUANTITY"].ToString(), 0);

                    rec.CREATE_DATETIME = ascLibrary.ascUtils.ascStrToDate(dr["CREATE_DATETIME"].ToString(), DateTime.MinValue);

                    rec.COMMENT = dr["COMMENT"].ToString();
                    rec.COSTEACH = ascLibrary.ascUtils.ascStrToDouble(dr["COSTEACH"].ToString(), 0);
                    rec.CW_NOT_BASE_UOM = ascLibrary.ascUtils.ascStrToDouble(dr["CW_NOT_BASE_UOM"].ToString(), 0);
                    rec.CW_UOM = dr["CW_UOM"].ToString();
                    rec.STATUS_FLAG = dr["STATUS_FLAG"].ToString();
                    rec.LIST_PRICE = ascLibrary.ascUtils.ascStrToDouble(dr["LIST_PRICE"].ToString(), 0);
                    rec.ORDER_STATUS = dr["ORDER_STATUS"].ToString();
                    rec.HOST_UOM = dr["HOST_UOM"].ToString();
                    rec.REQUESTED_LOT = dr["REQUESTED_LOT"].ToString();
                    rec.CLIENTDEPT = dr["CLIENTDEPT"].ToString();
                    rec.CLIENTDIVISION = dr["CLIENTDIVISION"].ToString();
                    rec.CLIENTGLACCT = dr["CLIENTGLACCT"].ToString();
                    rec.CLIENTPROFIT = dr["CLIENTPROFIT"].ToString();
                    rec.NOTES = dr["NOTES"].ToString();
                    rec.SHIPDESC = dr["SHIPDESC"].ToString();

                    rec.SOLD_PRICE = ascLibrary.ascUtils.ascStrToDouble(dr["SOLD_PRICE"].ToString(), 0);
                    rec.QTYBACKORDERED = ascLibrary.ascUtils.ascStrToDouble(dr["QTYBACKORDERED"].ToString(), 0);
                    rec.COUNTRY_OF_DESTINATION = dr["COUNTRY_OF_DESTINATION"].ToString();

                    data.DetailList.Add(rec);
                }
            }
            finally
            {
                myConnection.Close();
            }
        }

        private void AddCONotes(ASCTracInterfaceModel.Model.CustOrder.OrdrHdrImport data)
        {

        }


        async private void doCOExport()
        {
            ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter coExportFilter = new ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter("C", "", "");
            var myResult = myRestService.doCOsExport(coExportFilter).Result;

            lblResultCode.Text = myResult.StatusCode.ToString();
            tbContent.Text = await myResult.Content.ReadAsStringAsync();

            if (myResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (MessageBox.Show("Update Cust Order Results", "Cust Order Export", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    var mylist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport>>(tbContent.Text);
                    foreach (var rec in mylist)
                    {
                        rec.SUCCESSFUL = true;
                    }
                    myResult = myRestService.updateCOLinesExport(mylist).Result;

                    lblResultCode.Text = myResult.StatusCode.ToString();
                    tbContent.Text = await myResult.Content.ReadAsStringAsync();

                }
            }
        }



        async private void doParcelExport()
        {
            ASCTracInterfaceModel.Model.CustOrder.ParcelExporFilter ExportFilter = new ASCTracInterfaceModel.Model.CustOrder.ParcelExporFilter("", "");
            var myResult = myRestService.doParcelExport(ExportFilter).Result;

            lblResultCode.Text = myResult.StatusCode.ToString();
            tbContent.Text = await myResult.Content.ReadAsStringAsync();

            if (myResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (MessageBox.Show("Update Parcel Results", "PArcel Export", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    var mylist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ASCTracInterfaceModel.Model.CustOrder.ParcelExport>>(tbContent.Text);
                    foreach (var rec in mylist)
                    {
                        rec.Successful = true;
                    }
                    myResult = myRestService.updateParcelExport(mylist).Result;

                    lblResultCode.Text = myResult.StatusCode.ToString();
                    tbContent.Text = await myResult.Content.ReadAsStringAsync();

                }
            }
        }

        async private void doTranfileExport()
        {
            ASCTracInterfaceModel.Model.TranFile.TranFileExportFilter ExportFilter = new ASCTracInterfaceModel.Model.TranFile.TranFileExportFilter("", "");
            var myResult = myRestService.doTranfileExport(ExportFilter).Result;

            lblResultCode.Text = myResult.StatusCode.ToString();
            tbContent.Text = await myResult.Content.ReadAsStringAsync();

            if (myResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (MessageBox.Show("Update Tranfile Results", "Tran File Export", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    var mylist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ASCTracInterfaceModel.Model.TranFile.TranfileExport>>(tbContent.Text);
                    foreach (var rec in mylist)
                    {
                        rec.SUCCESSFUL = true;
                    }
                    myResult = myRestService.updateTranfileExport(mylist).Result;

                    lblResultCode.Text = myResult.StatusCode.ToString();
                    tbContent.Text = await myResult.Content.ReadAsStringAsync();

                }
            }
        }


        async private void doWCSExport(string aOrderType)
        {
            bool retval = true;
            int count = 0;
            string sqlstr = "SELECT * FROM TBL_ASC_WCS_PICK";
            sqlstr += " WHERE PROCESS_FLAG='R' AND PROCESS_RECIPIENT='A'";
            sqlstr += " AND ORDERTYPE='" + aOrderType + "'";
            sqlstr += " ORDER BY CREATE_DATETIME, ID";
            SqlConnection myConnection = new SqlConnection(myAWCSBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sqlstr, myConnection);

            myConnection.Open();
            SqlDataReader myReader = myCommand.ExecuteReader();
            try
            {
                while (myReader.Read() && retval)
                {
                    ASCTracInterfaceModel.Model.WCS.WCSPick data = new ASCTracInterfaceModel.Model.WCS.WCSPick();
                    data.ORDERTYPE = aOrderType;
                    data.ORDERNUMBER = myReader["ORDERNUMBER"].ToString();
                    data.TYPE_OF_PICK = myReader["TYPE_OF_PICK"].ToString();
                    data.SITE_ID = myReader["SITE_ID"].ToString();
                    data.PICK_SEQUENCE_NO = ascLibrary.ascUtils.ascStrToDouble(myReader["PICK_SEQUENCE_NO"].ToString(), 0);
                    data.QTY_PICKED = ascLibrary.ascUtils.ascStrToDouble(myReader["QTY_PICKED"].ToString(), 0);

                    data.ITEMID = myReader["ITEMID"].ToString();
                    data.LOTID = myReader["LOTID"].ToString();
                    data.LOCATIONID = myReader["LOCATIONID"].ToString();
                    data.SKIDID = myReader["SKIDID"].ToString();
                    data.CONTAINER_ID = myReader["CONTAINER_ID"].ToString();
                    data.SER_NUM = myReader["SER_NUM"].ToString();
                    data.DATETIME_PICKED = ascLibrary.ascUtils.ascStrToDate(myReader["DATETIME_PICKED"].ToString(), DateTime.Now);
                    data.USERID = myReader["USERID"].ToString();
                    data.SER_NUM = myReader["SER_NUM"].ToString();

                    var myResult = myRestService.CallWCSPostPick(data).Result;

                    string errmsg = myResult.Content.ReadAsStringAsync().Result;
                    lblResultCode.Text = myResult.StatusCode.ToString();
                    tbContent.Text = errmsg;
                    string updstr = string.Empty;
                    if (myResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {

                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "PROCESS_FLAG", "P");
                        ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "PROCESS_DATETIME", "GetDate()");
                    }
                    else
                    {
                        retval = false;
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "PROCESS_FLAG", "E");
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "ERR_MESSAGE", ascLibrary.ascStrUtils.ascSubString(errmsg.Replace("'", ""), 0, 80));
                    }
                    myAWCSBUtils.UpdateFields("TBL_ASC_WCS_PICK", updstr, "ID=" + myReader["ID"].ToString());
                    count += 1;
                }
                if (retval)
                {
                    if (count == 0)
                        tbContent.Text = "No Records Found";
                    else
                        tbContent.Text = count.ToString() + " Records processed.";

                }
            }
            catch (Exception ex)
            {
                tbContent.Text = "Exception: " + ex.Message;
            }
        }

    }
}