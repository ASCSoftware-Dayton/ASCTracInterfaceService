using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Imports
{
    public class ImportVendor
    {
        public static HttpStatusCode doImportVendor(Class1 myClass, ASCTracInterfaceModel.Model.Vendor.VendorImport aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            string OrderNum = aData.VENDOR_CODE;
            string updstr = string.Empty;
            try
            {
                if (myClass != null )
                {
                    if (!myClass.FunctionAuthorized(myClass.myLogRecord.FunctionID))
                        retval = HttpStatusCode.NonAuthoritativeInformation;
                    else
                    {
                        string vendId = aData.VENDOR_CODE;
                        string masterVendId = aData.MASTER_VENDORID;
                        var importAction = aData.STATUS;

                        if (string.IsNullOrEmpty(importAction))
                        {
                            errmsg = "Invalid Status";
                            retval = HttpStatusCode.BadRequest;
                        }
                        else if (importAction == "D")
                        {
                            string sql = "DELETE FROM VENDOR WHERE VENDORID='" + vendId + "'";
                            myClass.myParse.Globals.myDBUtils.RunSqlCommand(sql);
                        }
                        else
                        {
                            string sql = "SELECT NULL FROM VENDOR (NOLOCK) WHERE VENDORID='" + vendId + "'";
                            bool recExists = myClass.myParse.Globals.myDBUtils.ifRecExists(sql);

                            if (!String.IsNullOrEmpty(masterVendId))
                            {
                                if (vendId.Equals(masterVendId, StringComparison.OrdinalIgnoreCase))
                                    Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "MASTER_VENDOR_FLAG", "T");
                                Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "CLIENT_ID_ASSOCIATION", masterVendId);
                            }

                            if (!string.IsNullOrEmpty(importAction))
                            {
                                string inactiveFlag = importAction == "I" ? "T" : "F";
                                Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "INACTIVEFLAG", inactiveFlag);
                            }
                            Utils.ASCUtils.CheckAndAppend(ref updstr, "VENDOR", "VENDORNAME", aData.VENDOR_DESC);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "ADDRESS1", aData.ADDR_LINE1);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "ADDRESS2", aData.ADDR_LINE2);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "CITY", aData.CITY);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "STATE", aData.STATE);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "ZIPCODE", aData.ZIP);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "COUNTRY", aData.COUNTRY);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "CONTACTPERSON", aData.CONTACT_NAME);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "TELNUMBER", aData.CONTACT_TEL);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "FAXNUMBER", aData.CONTACT_FAX);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "TERMS_ID", aData.TERMS_ID);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "REMIT_ADDRESS1", aData.REMIT_TO_ADDR_LINE1);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "REMIT_ADDRESS2", aData.REMIT_TO_ADDR_LINE2);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "REMIT_CITY", aData.REMIT_TO_CITY);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "REMIT_STATE", aData.REMIT_TO_STATE);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "REMIT_ZIPCODE", aData.REMIT_TO_ZIP);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "REMIT_COUNTRY", aData.REMIT_TO_COUNTRY);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "REMIT_CONTACT", aData.REMIT_TO_CONTACT_NAME);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "REMIT_TELEPHONE", aData.REMIT_TO_CONTACT_TEL);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "REMIT_FAX", aData.REMIT_TO_CONTACT_FAX);

                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "ORGANIC_FLAG", aData.ORGANIC_FLAG);
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "ORGANIC_REG_NUM", aData.ORGANIC_REG_NUM);

                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "ALLOW_GTWY_AUTO_CLOSE_PO", aData.AUTOCLOSEPO);  //added 09-27-16 (JXG) for Driscoll's
                            Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "LPN_LEVEL_VALIDATION", aData.ENABLE_LICENSE_LEVEL_VALIDATION);  //added 09-27-16 (JXG) for Driscoll's

                            if (!recExists)
                            {
                                Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "VENDORID", vendId);
                                ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "CREATE_DATETIME", "GETDATE()");
                                Utils.ASCUtils.CheckAndAppend( ref updstr, "VENDOR", "CREATE_USERID", "GW");
                                myClass.myParse.Globals.myDBUtils.InsertRecord("VENDOR", updstr);
                            }
                            else
                            {
                                myClass.myParse.Globals.myDBUtils.UpdateFields("VENDOR", updstr, "VENDORID='" + vendId + "'");
                            }
                        }
                    }
                }
                else
                    retval = HttpStatusCode.InternalServerError;
            }
            catch( Exception ex)
            {
                myClass.LogException(ex);
                retval = HttpStatusCode.BadRequest;
                errmsg = ex.Message;
            }
            return (retval);
        }
    }
}
