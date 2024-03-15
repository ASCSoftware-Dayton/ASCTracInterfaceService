using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Imports
{
    public class ImportCustomerMaster
    {
        //private string funcType = "IM_ITEM";
        private string siteid = string.Empty;
        private Class1 myClass;
        private Dictionary<string, List<string>> GWTranslation = new Dictionary<string, List<string>>();
        public static HttpStatusCode doImportCustomer(Class1 myClass, ASCTracInterfaceModel.Model.Customer.CustomerMasterImport aData, ref string errmsg)
        {
            //myClass = Class1.InitParse(funcType, ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            string CustID = aData.CUST_ID;
            string updstr = string.Empty;
            try
            {
                if (!myClass.FunctionAuthorized(myClass.myLogRecord.FunctionID))
                    retval = HttpStatusCode.NonAuthoritativeInformation;
                else
                {
                    var siteid = myClass.GetSiteIdFromHostId(aData.FACILITY);
                    if (String.IsNullOrEmpty(siteid))
                    {
                        myClass.myLogRecord.LogType = "E";
                        errmsg = "No Facility or Site defined for record.";
                        retval = HttpStatusCode.BadRequest;
                    }

                    if (string.IsNullOrEmpty(CustID))
                    {
                        myClass.myLogRecord.LogType = "E";
                        errmsg = "Customer ID(CUST_ID) value is required.";
                        retval = HttpStatusCode.BadRequest;
                    }
                    else
                    {
                        var myImport = new ImportCustomerMaster(myClass, siteid);
                        retval = myImport.doImportCustomerMaster(aData, ref errmsg);
                    }
                }
            }
            catch (Exception ex)
            {
                myClass.LogException(ex);
                retval = HttpStatusCode.BadRequest;
                errmsg = ex.Message;
            }
            return (retval);
        }

        public ImportCustomerMaster(Class1 aClass, string aSiteID)
        {
            myClass = aClass;
            siteid = aSiteID;
            Configs.ConfigUtils.ReadTransationFields(GWTranslation, "CUST", myClass.myParse.Globals);
        }

        private HttpStatusCode doImportCustomerMaster(ASCTracInterfaceModel.Model.Customer.CustomerMasterImport aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            string custId = aData.CUST_ID.Trim();
            string masterCustId = string.Empty;
            if( !String.IsNullOrEmpty( aData.MASTER_CUSTID))
                masterCustId = aData.MASTER_CUSTID.Trim();

            string sql = "SELECT NULL FROM CUST (NOLOCK) WHERE CUSTID='" + custId + "'";
            string tmpStr = string.Empty;
            var recExists = myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sql, "", ref tmpStr);

            string updstr = string.Empty;
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "SHIPTONAME", aData.CUST_NAME);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "TERMS_ID", aData.TERMS);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "NOTES", aData.HOST_COMMENT);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "INACTIVEFLAG", aData.INACTIVE_FLAG);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "SHIPTOADDRESS1", aData.SHIP_ADDR_LINE1);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "SHIPTOADDRESS2", aData.SHIP_ADDR_LINE2);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "SHIPTOCITY", aData.SHIP_CITY);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "SHIPTOSTATE", aData.SHIP_STATE);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "SHIPTOZIPCODE", aData.SHIP_ZIP);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "SHIPTOCOUNTRY", aData.SHIP_COUNTRY);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "SHIPTOCONTACT", aData.SHIP_CONTACT_NAME);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "SHIPTOTITLE", aData.SHIP_TO_TITLE);
            //Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "SHIP_EMAIL_TO", aData.SHIP_TO_EMAIL);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "SHIPTOTELEPHONE", aData.SHIP_CONTACT_TEL);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "SHIPTOFAX", aData.SHIP_CONTACT_FAX);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "BILLTONAME", aData.CUST_NAME);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "BILLTOADDRESS1", aData.BILL_ADDR_LINE1);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "BILLTOADDRESS2", aData.BILL_ADDR_LINE2);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "BILLTOCITY", aData.BILL_CITY);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "BILLTOSTATE", aData.BILL_STATE);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "BILLTOZIPCODE", aData.BILL_ZIP);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "BILLTOCOUNTRY", aData.BILL_COUNTRY);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "BILLTOCONTACT", aData.BILL_CONTACT_NAME);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "BILLTOTELEPHONE", aData.BILL_CONTACT_TEL);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "BILLTOFAX", aData.BILL_CONTACT_FAX);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "ROUTE_AREAID", aData.ROUTE_AREAID);
            if (!string.IsNullOrEmpty(aData.STATUS_FLAG.Trim()) || !recExists)
            {
                string creditHold = (aData.STATUS_FLAG.Trim() == "H") ? "T" : "F";
                Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "CREDIT_HOLD", creditHold);
            }
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "CUSTCATEGORY", aData.CUST_CATEGORY);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "CREDIT_RISK_RATING", aData.CREDIT_RISK_RATING);
            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "CREDIT_LIMIT", aData.CREDIT_LIMIT.ToString());
            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "PAST_DUE_PERIOD1", aData.PAST_DUE_PERIOD1.ToString());
            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "PAST_DUE_PERIOD2", aData.PAST_DUE_PERIOD2.ToString());
            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "PAST_DUE_PERIOD3", aData.PAST_DUE_PERIOD3.ToString());
            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "PAST_DUE_PERIOD4", aData.PAST_DUE_PERIOD4.ToString());
            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "OPENAMOUNT", aData.OPENAMOUNT.ToString());
            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "PROMOTION_ALLOWANCE", aData.PROMOTION_ALLOWANCE.ToString());
            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "PROMOTION_ALLOWANCE_CONSUMED", aData.PROMOTION_ALLOWANCE_CONSUMED.ToString());

            if (!String.IsNullOrEmpty(masterCustId))
            {
                if (custId.Equals(masterCustId, StringComparison.OrdinalIgnoreCase))
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "MASTER_CUST_FLAG", "T");
                Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "CLIENT_ID_ASSOCIATION", masterCustId);
                Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "CLIENT_LEVEL_ADMIN", "T");
            }
                Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "CNTR_GROUP_ID", aData.CNTR_GROUP_ID);
                Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "PREVENT_ORGANIC_FLAG", aData.PREVENT_ORGANIC_FLAG);

            if (aData.CustomList != null)
                SaveCustomFields(ref updstr, aData.CustomList, GWTranslation);

            if (!recExists)
            {
                Utils.ASCUtils.CheckAndAppend(ref updstr, "CUST",  "CUSTID", custId);
                ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "CREATE_DATETIME", "GETDATE()");
                myClass.myParse.Globals.myDBUtils.InsertRecord("CUST", updstr);
            }
            else
            {
                myClass.myParse.Globals.myDBUtils.UpdateFields("CUST", updstr, "CUSTID='" + custId + "'");
            }

            if (aData.NotesList != null)
                ImportNotes(aData.NotesList, custId);


            return (retval);
        }

        private void ImportNotes(List< ASCTracInterfaceModel.Model.NotesImport> NotesList, string custId)
        {
            if (NotesList.Count > 0)
            {
                myClass.myParse.Globals.myDBUtils.RunSqlCommand("DELETE CUSTNOTES WHERE CUSTID='" + custId + "'");
                int seqnum = 1;
                foreach (var rec in NotesList)
                {
                    string sql = "INSERT INTO CUSTNOTES" +
                        " (CUSTID, SEQNUM, TYPE, NOTE, LAST_UPDATE, LAST_UPDATE_USERID)" +
                        " VALUES('" + custId + "', " + seqnum.ToString() + ", '" + rec.TYPE + "', '" + rec.NOTE + "', GetDate(), '" + myClass.myParse.Globals.curUserID + "')";
                    seqnum += 1;
                    myClass.myParse.Globals.myDBUtils.RunSqlCommand(sql);
                }
            }
        }

        private void SaveCustomFields(ref string updstr, List<ASCTracInterfaceModel.Model.ModelCustomData> CustomList, Dictionary<string, List<string>> TranslationList)
        {
            foreach (var rec in CustomList)
            {
                if (TranslationList.ContainsKey(rec.FieldName.ToUpper()))
                {
                    var asclist = TranslationList[rec.FieldName.ToUpper()];
                    foreach (var ascfield in asclist)
                    {
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, ascfield, rec.Value);
                    }
                }
            }
        }

    }
}