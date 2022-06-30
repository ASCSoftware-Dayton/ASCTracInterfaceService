using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ASCTracInterfaceSample
{
    public partial class Form1 : Form
    {
        RestService myRestService;

        public Form1()
        {
            InitializeComponent();
            edURL.Text = ConfigurationManager.AppSettings["HostURL"];
            myRestService = new RestService();
        }


        private void SetURL()
        {
            myRestService.fURL = edURL.Text;
            ConfigurationManager.AppSettings["HostURL"] = myRestService.fURL;
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
            else if (cbFunction.Text == "WCS-GetPicks")
                doWCSGetPicks();
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
            var data = new ASCTracInterfaceModel.Model.Item.ItemMasterImport();

            data.CREATE_DATETIME = DateTime.Now;
            data.FACILITY = "HostSiteID";
            data.PRODUCT_CODE = "itemid";
            data.CATEGORY = "CatID";
            data.DESCRIPTION = "Item Description";
            data.PROD_ALTDESC = "Item Description 2";
            data.STD_COST = 1.25;
            data.RECEIVING_UOM = "EA";
            data.PRODUCT_WEIGHT = 2.5;
            data.CW_UOM = string.Empty; // set for Dual unit items
            data.BASE_TO_RECV_CONV_FACTOR = 0;
            data.STATUS_FLAG = "A";
            data.UPC_CODE = "UPCCode";
            data.ITEM_TYPE = "F";
            data.UNIT1_UOM = "CA";
            data.CONVERSION_UNIT_1 = 10;
            data.UNIT2_UOM = "EA";
            data.CONVERSION_UNIT_2 = 0;
            data.UNIT3_UOM = string.Empty;
            data.CONVERSION_UNIT_3 = 0;
            data.UNIT4_UOM = string.Empty;
            data.CONVERSION_UNIT_4 = 0;

            data.GTIN_CODE_1 = string.Empty;
            data.GTIN_CODE_2 = string.Empty;
            data.GTIN_CODE_3 = string.Empty;
            data.GTIN_CODE_4 = string.Empty;

            data.UNITWIDTH = 1;
            data.UNITLENGTH = 1;
            data.UNITHEIGHT = 1;
            data.UNITWEIGHT = 1;

            data.CATEGORY_2 = "Cat2ID";
            data.STOCK_UOM = "EA";
            data.BUY_UOM = "EA";
            data.LABEL_UOM = "CA";
            data.BUYER = "BUYER";
            data.VENDORID = "VENDORID";
            data.SCC14 = "SCC14";
            data.CUBIC_PER_EACH = 0;
            data.ABC_ZONE = "A";
            data.SHELF_LIFE = 90;
            data.AUTO_QC_REASON = "QCReason";
            data.RETAIL_PRICE = 2.25;
            data.COUNTRY_OF_ORIGIN = "Country";
            data.SKID_TRACKED = "T";

            data.SERIAL_TRACKED = "F";

            data.TARE_WEIGHT = 1;
            data.BULK_TARE_WEIGHT = 1.1;
            data.BILL_UOM = "EA";

            data.HAZMAT_FLAG = "F";
            data.LOT_FLAG = "T";
            data.LOT_PROD_FLAG = "T";

            data.EXPIRE_DAYS = 15;
            data.EXP_DATE_REQ_FLAG = "T";

            data.RESTOCK_QTY = 0;
            data.LEADTIME = 0;

            data.MINIMUM = 0;
            data.MAXIMUM = 0;

            data.REF_NOTES = "REF_NOTES";


            data.INHOUSE_TIME = 0;

            data.HOST_QTY = 0;

            data.FREIGHT_CLASS_CODE = "";

            data.BUNDLE_SIZE = "";

            data.VMI_CUSTID = "CUSTID";

            data.VMI_RESPID = "CUSTID";

            data.THUMBNAIL_FILENAME = "";
            data.ORGANIC_FLAG = "F";
            data.POST_LOT_TO_HOST_FLAG = "T";
            data.PKG_MATERIAL_FLAG = "F";
            data.MFG_ID = "MFG_ID";
            data.VENDOR1ITEMNUM = "VENDOR1ITEMNUM";

            data.CustomList.Add(new ASCTracInterfaceModel.Model.ModelCustomData("ITEM_CUSTOMDATA", "Custom data 1"));

            AddItemExtData(data);

            var myResult = myRestService.doItemImport(data).Result;

            lblResultCode.Text = myResult.StatusCode.ToString();
            tbContent.Text = await myResult.Content.ReadAsStringAsync();
        }


        private void AddItemExtData(ASCTracInterfaceModel.Model.Item.ItemMasterImport data)
        {
            data.ExtDataList.Add("1", "VALUE");
        }

        private async void doVendorImport()
        {
            var data = new ASCTracInterfaceModel.Model.Vendor.VendorImport();
            data.VENDOR_CODE = "VENDOR_CODE";
            data.VENDOR_DESC = "VENDOR_DESC";

            data.ADDR_LINE1 = "ADDR_LINE1";
            data.ADDR_LINE2 = "ADDR_LINE2";
            data.CITY = "CITY";
            data.STATE = "STATE";
            data.ZIP = "99999";
            data.COUNTRY = "COUNTRY";
            data.CONTACT_NAME = "CONTACT_NAME";
            data.CONTACT_TEL = "1-800-555-5555";
            data.CONTACT_FAX = "1-800-555-5555";

            data.REMIT_TO_ADDR_LINE1 = "REMIT_TO_ADDR_LINE1";
            data.REMIT_TO_ADDR_LINE2 = "REMIT_TO_ADDR_LINE2";
            data.REMIT_TO_CITY = "REMIT_TO_CITY";
            data.REMIT_TO_STATE = "REMIT_TO_STATE";
            data.REMIT_TO_ZIP = "99999";
            data.REMIT_TO_COUNTRY = "REMIT_TO_COUNTRY";
            data.REMIT_TO_CONTACT_NAME = "REMIT_TO_CONTACT_NAME";
            data.REMIT_TO_CONTACT_TEL = "1-800-555-5555";
            data.REMIT_TO_CONTACT_FAX = "1-800-555-5555";

            data.TERMS_ID = "T1";
            data.STATUS = "A";
            data.MASTER_VENDORID = "MASTER_VENDORID";
            data.ORGANIC_FLAG = "F";
            data.ORGANIC_REG_NUM = "ORGANIC_REG_NUM";
            data.AUTOCLOSEPO = "F";
            data.ENABLE_LICENSE_LEVEL_VALIDATION = "F";
            var myResult = myRestService.doVendorImport(data).Result;

            lblResultCode.Text = myResult.StatusCode.ToString();
            tbContent.Text = await myResult.Content.ReadAsStringAsync();
        }
        private async void doPOImport()
        {
            var data = new ASCTracInterfaceModel.Model.PO.POHdrImport();

            data.FACILITY = "HostSiteID";
            data.ORDER_TYPE = "P";
            data.PONUMBER = "PONUMBER";
            data.VENDOR_CODE = "VENDOR_CODE";
            data.TO_FACILITY = "Tohostsiteid";
            data.LEAVES_DATE = DateTime.MinValue;
            data.ARRIVAL_DATE = DateTime.MinValue;
            data.ENTRY_DATE = DateTime.MinValue;
            data.CARRIER = "CARRIER";
            data.DELIVERY_INSTRUCTIONS = "DELIVERY_INSTRUCTIONS";
            data.ADDR_LINE1 = "ADDR_LINE1";
            data.ADDR_LINE2 = "ADDR_LINE2";
            data.ADDR_LINE3 = "ADDR_LINE3";
            data.CITY = "CITY";
            data.STATE = "STATE";
            data.ZIP = "99999";
            data.COUNTRY = "COUNTRY";
            data.CONTACT_NAME = "CONTACT_NAME";
            data.CONTACT_TEL = "1-800-555-5555";
            data.CONTACT_FAX = "1-800-555-5555";
            data.STATUS_FLAG = "O";
            data.CustomList.Add(new ASCTracInterfaceModel.Model.ModelCustomData("CUSTOM_DATA1", "Data 1"));
            data.CustomList.Add(new ASCTracInterfaceModel.Model.ModelCustomData("CUSTOM_DATA2", "Data 2"));
            data.RMA_TYPE = "RMA_TYPE";
            data.LINKED_ORDERNUMBER = "LINKED_ORDERNUMBER";
            data.BUYER_CODE_ID = "BUYER_CODE_ID";
            data.TERMS_ID = "t1";
            data.RELEASENUM = "00";
            data.REQ_NUM = "REQ_NUM";
            data.BILL_ADDR_LINE1 = "BILL_ADDR_LINE1";
            data.BILL_ADDR_LINE2 = "BILL_ADDR_LINE2";
            data.BILL_ADDR_LINE3 = "BILL_ADDR_LINE3";
            //data.BILL_CITY_LINE3 = "BILL_CITY_LINE3";
            data.BILL_CITY = "BILL_CITY";
            data.BILL_STATE = "BILL_STATE";
            data.BILL_ZIP = "99999";
            data.BILL_COUNTRY = "US";
            data.BILL_CONTACT_NAME = "BILL_CONTACT_NAME";
            data.BILL_CONTACT_TEL = "1-800-555-5555";
            data.BILL_CONTACT_FAX = "1-800-555-5555";
            data.DIRECT_SHIP_ORDERNUMBER = "DSORDERNUMBER";
            data.SHIP_TO_NAME = "SHIP_TO_NAME";
            data.BILL_TO_NAME = "BILL_TO_NAME";
            data.SEAL_NUM = "SEAL_NUM";
            data.VMI_CUSTID = "CUSTID";
            data.ASN = "ASN";
            data.PROMO_CODE = "PROMO_CODE";

            AddPODet(data);
            AddPONotes(data);

            var myResult = myRestService.doPOImport(data).Result;

            lblResultCode.Text = myResult.StatusCode.ToString();
            tbContent.Text = await myResult.Content.ReadAsStringAsync();
        }

        private void AddPODet(ASCTracInterfaceModel.Model.PO.POHdrImport data)
        {
            var rec = new ASCTracInterfaceModel.Model.PO.PODetImport();


            rec.LINE_NUMBER = 1;
            rec.VENDOR_ITEM_ID = "VENDOR_ITEM_ID";
            rec.PRODUCT_CODE = "PRODUCT_CODE";
            rec.QUANTITY = 10;

            rec.EXPECTED_RECEIPT_DATE = DateTime.Now.Date;
            rec.COMMENT = "COMMENT";
            rec.COSTEACH = 1.50;
            rec.CW_UOM = "";
            rec.STATUS_FLAG = "A";
            rec.UPC_CODE = "UPC_CODE";
            rec.ITEM_DESCRIPTION = "ITEM_DESCRIPTION";
            rec.LOTID = "LOTID";
            rec.UOM = "UOM";
            rec.DIRECT_SHIP_ORDERNUMBER = "DIRECT_SHIP_ORDERNUMBER";
            rec.LINKED_ORDERNUMBER = "LINKED_ORDERNUMBER";

            rec.CustomList.Add(new ASCTracInterfaceModel.Model.ModelCustomData("CUSTOM_DATA1", "Data 1"));
            rec.CustomList.Add(new ASCTracInterfaceModel.Model.ModelCustomData("CUSTOM_DATA2", "Data 2"));

            rec.SKIDID = "SKIDID";
            rec.SERIAL_NUM = "SERIAL_NUM";
            rec.PROMO_CODE = "PROMO_CODE";
            rec.RELEASENUM = "RELEASENUM";
            rec.EXPECTED_SHIP_DATE = DateTime.MinValue;
            rec.USER_REQUIRED_DATE = DateTime.MinValue;
            rec.ORIG_ORDERNUMBER = "ORIG_ORDERNUMBER";
            rec.CW_QTY = 0; // ascLibrary.ascUtils.ascStrToDouble("CW_QTY"].ToString(), 0);
            rec.VEND_PRODLINE = "VEND_PRODLINE";
            rec.QC_REASON = "QC_REASON";
            rec.ALT_LOTID = "ALT_LOTID";
            rec.HOST_LINENUMBER = 1; // ascLibrary.ascUtils.ascStrToInt("HOST_LINENUMBER"].ToString(), 0);



            data.PODetList.Add(rec);
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
            var data = new ASCTracInterfaceModel.Model.ASN.ASNHdrImport();

            data.FACILITY = "HostSiteID";
            data.CREATE_DATETIME = DateTime.Now;
            data.FACILITY = "HostSiteID";
            data.ASN = "ASN";
            data.PONUMBER = "PONUMBER";
            data.TRUCKNUM = "TRUCKNUM";
            data.FROM_FACILITY = "FROM_FACILITY";
            data.REF_ORDERNUMBER = "REF_ORDERNUMBER";
            data.EXPECTED_RECEIPT_DATE = DateTime.Now.AddDays(14).Date;
            data.VENDORID = "VENDORID";
            data.PACKINGSLIP = "PACKINGSLIP";

            data.ASN_TYPE = "A";
            AddASNDet(data);

            var myResult = myRestService.doASNImport(data).Result;

            lblResultCode.Text = myResult.StatusCode.ToString();
            tbContent.Text = await myResult.Content.ReadAsStringAsync();
        }

        private void AddASNDet(ASCTracInterfaceModel.Model.ASN.ASNHdrImport HDRdata)
        {
            var data = new ASCTracInterfaceModel.Model.ASN.ASNDetImport();

            data.ITEMID = "ITEMID";
            data.LOTID = "LOTID";
            data.SKIDID = "SKIDID";
            data.QUANTITY = 10;
            data.EXPIRE_DATE = DateTime.MinValue;
            data.PONUMBER = "PONUMBER";
            data.RELEASENUM = "RELEASENUM";
            data.LINENUMBER = 1;
            data.VMI_CUSTID = "VMI_CUSTID";
            data.ACTUAL_WEIGHT = 10;
            data.CW_QTY = 0;
            data.VENDORID = "VENDORID";
            data.CONTAINER_ID = "CONTAINER_ID";
            data.PALLET_TYPE = "PALLET_TYPE";
            data.DATETIMEPROD = DateTime.Now;
            data.ALT_SKIDID = "ALT_SKIDID";
            data.ALT_LOTID = "ALT_LOTID";
            data.CustomList.Add(new ASCTracInterfaceModel.Model.ModelCustomData("CUSTOM_DATA1", "Data 1"));
            data.CustomList.Add(new ASCTracInterfaceModel.Model.ModelCustomData("CUSTOM_DATA2", "Data 2"));

            HDRdata.DetailList.Add(data);

        }

        async private void doCOImport()
        {
            var data = new ASCTracInterfaceModel.Model.CustOrder.OrdrHdrImport();

            data.FACILITY = "HostSiteID";
            data.ORDER_TYPE = "S";
            data.ORDERNUMBER = "ORDERNUMBER";
            data.ORDER_CREATE_DATE = DateTime.Now;

            //data.CUST_ID = "CUST_ID";
            data.FROM_FACILITY = "hostSite2";
            data.LEAVES_DATE = DateTime.MinValue;
            data.ENTRY_DATE = DateTime.Now;
            data.CARRIER = "CARRIER";
            data.PAYMENT_TYPE = "PAYMENT_TYPE";

            data.SHIP_TO_CUST_ID = "CUSTID";
            data.SHIP_TO_NAME = "SHIP_TO_NAME";
            data.SHIP_TO_ADDR_LINE1 = "SHIP_TO_ADDR_LINE1";
            data.SHIP_TO_ADDR_LINE2 = "SHIP_TO_ADDR_LINE2";
            data.SHIP_TO_ADDR_LINE3 = "SHIP_TO_ADDR_LINE3";
            data.SHIP_TO_CITY = "SHIP_TO_CITY";
            data.SHIP_TO_STATE = "ZZ";
            data.SHIP_TO_ZIP = "99999";
            data.SHIP_TO_COUNTRY = "US";
            data.SHIP_TO_CONTACT_NAME = "SHIP_TO_CONTACT_NAME";
            data.SHIP_TO_CONTACT_TEL = "1-800-555-5555";
            data.SHIP_TO_CONTACT_FAX = "1-800-555-5555";

            data.BILL_TO_CUST_ID = "CUSTID";
            data.BILL_TO_NAME = "BILL_TO_NAME";
            data.BILL_TO_ADDR_LINE1 = "BILL_TO_ADDR_LINE1";
            data.BILL_TO_ADDR_LINE2 = "BILL_TO_ADDR_LINE2";
            data.BILL_TO_ADDR_LINE3 = "BILL_TO_ADDR_LINE3";
            data.BILL_TO_CITY = "BILL_TO_CITY";
            data.BILL_TO_STATE = "ZZ";
            data.BILL_TO_ZIP = "99999";
            data.BILL_TO_COUNTRY = "US";
            data.BILL_TO_CONTACT_NAME = "BILL_TO_CONTACT_NAME";
            data.BILL_TO_CONTACT_TEL = "1-800-555-5555";
            data.BILL_TO_CONTACT_FAX = "1-800-555-5555";

            data.CUST_PO_NUM = "CUST_PO_NUM";
            data.CUST_BILLTO_PO_NUM = "CUST_BILLTO_PO_NUM";
            data.CUST_SHIPTO_PO_NUM = "CUST_SHIPTO_PO_NUM";
            data.STATUS_FLAG = "A";
            data.LOAD_PLAN_NUM = "";
            data.LOAD_STOP_SEQ = 0;
            data.PRIORITY_ID = 0;
            data.RECIPIENT_EMAIL = "RECIPIENT_EMAIL";
            data.BOL_NUMBER = "BOL_NUMBER";
            data.FREIGHT_ACCOUNT_NUMBER = "ACCT123";
            data.REFERENCE_NUMBER = "REFERENCE_NUMBER";
            data.PREPAY_COLLECT = "02";
            data.CANCEL_DATE = DateTime.MinValue;
            data.CARRIER_SERVICE_CODE = "CSCODE";
            data.DELIVERY_INSTRUCTIONS = "DELIVERY_INSTRUCTIONS";

            data.COD_AMT = 0;
            data.MUST_ARRIVE_BY_DATE = DateTime.MinValue;
            data.SALESPERSON = "SALESPERSON";
            data.TERMS_ID = "";
            data.LINKED_PONUMBER = "LINKED_PONUMBER";
            data.CREDIT_HOLD_STATUS = "";
            data.CLIENTDEPT = "CLIENTDEPT";
            data.CLIENTDIVISION = "CLIENTDIVISION";
            data.CLIENTGLACCT = "CLIENTGLACCT";
            data.CLIENTPROFIT = "CLIENTPROFIT";
            data.ALLOW_SHORT_SHIP = "S";
            data.RESIDENTIAL_FLAG = "F";
            data.SHIP_VIA = "SHIP_VIA";
            data.AREA = "AREA";
            data.ALLOW_OVER_SHIP = "S";
            data.SALESORDERNUMBER = "SALESORDERNUMBER";

            data.THIRDPARTYCUSTID = "THIRDPARTYCUSTID";
            data.THIRDPARTYNAME = "THIRDPARTYNAME";
            data.THIRDPARTYADDRESS1 = "THIRDPARTYADDRESS1";
            data.THIRDPARTYADDRESS2 = "THIRDPARTYADDRESS2";
            data.THIRDPARTYADDRESS3 = "THIRDPARTYADDRESS3";
            data.THIRDPARTYCITY = "THIRDPARTYCITY";
            data.THIRDPARTYSTATE = "ZZ";
            data.THIRDPARTYZIPCODE = "99999";
            data.THIRDPARTYCOUNTRY = "US";

            data.STORE_NUM = "123";
            data.DEPT = "DEPT";
            data.PACKLIST_REQ = "";
            data.DROP_SHIP = "";
            data.BATCH_NUM = "";
            data.ROUTEID = "R123";
            data.PROMO_CODE = "PROMO_CODE";
            data.CUSTORDERCAT = "CCAT";
            data.FOB = "FOB";
            data.COMPLIANCE_LABEL = "COMPLIANCE_LABEL";
            data.VMI_GROUPID = "Cust";
            data.ORDER_SOURCE_SYSTEM = "API";

            data.FREIGHTBILLTONAME = "FREIGHTBILLTONAME";
            data.FREIGHTBILLTOCONTACT = "FREIGHTBILLTOCONTACT";
            data.FREIGHTBILLTOADDRESS1 = "FREIGHTBILLTOADDRESS1";
            data.FREIGHTBILLTOADDRESS2 = "FREIGHTBILLTOADDRESS2";
            data.FREIGHTBILLTOADDRESS3 = "FREIGHTBILLTOADDRESS3";
            data.FREIGHTBILLTOADDRESS4 = "FREIGHTBILLTOADDRESS4";
            data.FREIGHTBILLTOCITY = "FREIGHTBILLTOCITY";
            data.FREIGHTBILLTOSTATE = "ZZ";
            data.FREIGHTBILLTOZIPCODE = "99999";
            data.FREIGHTBILLTOCOUNTRY = "US";
            data.FREIGHTBILLTOTELEPHONE = "1-800-555-5555";
            data.FREIGHTBILLTOALTTEL = "1-800-555-5555";
            data.FREIGHTBILLTOFAX = "1-800-555-5555";

            AddCODet(data);
            AddCONotes(data);

            var myResult = myRestService.doCOImport(data).Result;

            lblResultCode.Text = myResult.StatusCode.ToString();
            tbContent.Text = await myResult.Content.ReadAsStringAsync();
        }

        private void AddCODet(ASCTracInterfaceModel.Model.CustOrder.OrdrHdrImport data)
        {
            var rec = new ASCTracInterfaceModel.Model.CustOrder.OrdrDetImport();


            rec.LINE_NUMBER = 1;
            rec.CUST_ITEMID = "CUST_ITEMID";
            rec.PRODUCT_CODE = "ItemID";
            rec.QUANTITY = 10;

            rec.CREATE_DATETIME = DateTime.Now;

            rec.COMMENT = "COMMENT";
            rec.COSTEACH = 1.75;
            rec.CW_NOT_BASE_UOM = 0;
            rec.CW_UOM = "CW_UOM";
            rec.STATUS_FLAG = "A";
            rec.LIST_PRICE = 0;
            rec.ORDER_STATUS = "O";
            rec.HOST_UOM = "";
            rec.REQUESTED_LOT = "REQUESTED_LOT";
            rec.CLIENTDEPT = "CLIENTDEPT";
            rec.CLIENTDIVISION = "CLIENTDIVISION";
            rec.CLIENTGLACCT = "CLIENTGLACCT";
            rec.CLIENTPROFIT = "CLIENTPROFIT";
            rec.NOTES = "NOTES";
            rec.SHIPDESC = "SHIPDESC";

            rec.SOLD_PRICE = 0;
            rec.QTYBACKORDERED = 0;
            rec.COUNTRY_OF_DESTINATION = "COUNTRY";

            data.DetailList.Add(rec);
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

        async private void doWCSGetPicks()
        {
            var myResult = myRestService.doWCSGetPicks(string.Empty).Result;

            lblResultCode.Text = myResult.StatusCode.ToString();
            tbContent.Text = await myResult.Content.ReadAsStringAsync();


        }

        async private void doWCSExport(string aOrderType)
        {
            ASCTracInterfaceModel.Model.WCS.WCSPick data = new ASCTracInterfaceModel.Model.WCS.WCSPick();
            data.ORDERTYPE = aOrderType;
            data.ORDERNUMBER = "ORDERNUMBER";
            data.TYPE_OF_PICK = "V"; // Voice, Carousel, Light..  if LOCATIONID set, this is ignored
            data.SITE_ID = "1";
            data.PICK_SEQUENCE_NO = 1;
            data.QTY_PICKED = 4;

            data.ITEMID = "ITEMID";
            data.LOTID = "LOTID";
            data.LOCATIONID = "LOCATIONID";
            data.SKIDID = "SKIDID";
            data.CONTAINER_ID = "CONTAINER_ID";
            data.SER_NUM = "SER_NUM";
            data.DATETIME_PICKED = DateTime.Now;
            data.USERID = "USERID";
            data.SER_NUM = "SER_NUM";

            var myResult = myRestService.CallWCSPostPick(data).Result;

            string errmsg = myResult.Content.ReadAsStringAsync().Result;
            lblResultCode.Text = myResult.StatusCode.ToString();
            tbContent.Text = errmsg;
        }
    }
}