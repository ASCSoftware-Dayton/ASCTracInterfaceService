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

            lblInterfaceDB.Text = myDBUtils.fServer + ", " + myDBUtils.fDatabase;

            string tmp = string.Empty;
            if (!myASCDBUtils.ReadFieldFromDB("SELECT CFGDATA FROM CFGSETTINGS WHERE CFGFIELD='InterfaceTestUrl'", "", ref tmp))
                tmp = "https://localhost:44344/";
            // https://localhost:44344/
            // http://10.169.0.30/
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

            if (cbFunction.Text == "VendorImport")
                doVendorImport();
            if (cbFunction.Text == "POImport")
                doPOImport();
            if (cbFunction.Text == "POExport - Lines")
                doPOLinesExport();
            if (cbFunction.Text == "POExport - Licenses")
                doPOLicensesExport();
            if (cbFunction.Text == "CustOrderImport")
                doCOImport();
            if (cbFunction.Text == "CustOrderExport")
                doCOExport();
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
                foreach( var rec in mylist)
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

                    data.CUST_ID = dr["CUST_ID"].ToString();
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
                    data.PRIORITY_ID = ascLibrary.ascUtils.ascStrToDouble( dr["PRIORITY_ID"].ToString(), 0);
                    data.RECIPIENT_EMAIL = dr["RECIPIENT_EMAIL"].ToString();
                    data.BOL_NUMBER = dr["BOL_NUMBER"].ToString();
                    data.FREIGHT_ACCOUNT_NUMBER = dr["FREIGHT_ACCOUNT_NUMBER"].ToString();
                    data.REFERENCE_NUMBER = dr["REFERENCE_NUMBER"].ToString();
                    data.PREPAY_COLLECT = dr["PREPAY_COLLECT"].ToString();
                    data.CANCEL_DATE = ascLibrary.ascUtils.ascStrToDate( dr["CANCEL_DATE"].ToString(), DateTime.MinValue);
                    data.CARRIER_SERVICE_CODE = dr["CARRIER_SERVICE_CODE"].ToString();
                    data.DELIVERY_INSTRUCTIONS = dr["DELIVERY_INSTRUCTIONS"].ToString();

                    data.COD_AMT =ascLibrary.ascUtils.ascStrToDouble( dr["COD_AMT"].ToString(), 0);
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

                    rec.CREATE_DATETIME = ascLibrary.ascUtils.ascStrToDate( dr["CREATE_DATETIME"].ToString(), DateTime.MinValue);

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

    }
}