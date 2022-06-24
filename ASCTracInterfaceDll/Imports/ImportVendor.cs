using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Imports
{
    public class ImportVendor
    {
        private static string funcType = "IM_VENDOR";
        public static HttpStatusCode doImportVendor(ASCTracInterfaceModel.Model.Vendor.VendorImport aData, ref string errmsg)
        {
            var myClass = Class1.InitParse(funcType, ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            string OrderNum = aData.VENDOR_CODE;
            string updstr = string.Empty;
            try
            {
                if (myClass != null )
                {
                    if (!myClass.FunctionAuthorized(funcType))
                        retval = HttpStatusCode.NonAuthoritativeInformation;
                    else
                    {

                        string vendId = aData.VENDOR_CODE;
                        string masterVendId = aData.MASTER_VENDORID;
                        var importAction = aData.STATUS;

                        if (importAction == "D")
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
                                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "MASTER_VENDOR_FLAG", "T");
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CLIENT_ID_ASSOCIATION", masterVendId);
                            }

                            if (!string.IsNullOrEmpty(importAction))
                            {
                                string inactiveFlag = importAction == "I" ? "T" : "F";
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "INACTIVEFLAG", inactiveFlag);
                            }
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "VENDORNAME", aData.VENDOR_DESC);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "ADDRESS1", aData.ADDR_LINE1);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "ADDRESS2", aData.ADDR_LINE2);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CITY", aData.CITY);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "STATE", aData.STATE);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "ZIPCODE", aData.ZIP);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "COUNTRY", aData.COUNTRY);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CONTACTPERSON", aData.CONTACT_NAME);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "TELNUMBER", aData.CONTACT_TEL);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "FAXNUMBER", aData.CONTACT_FAX);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "TERMS_ID", aData.TERMS_ID);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "REMIT_ADDRESS1", aData.REMIT_TO_ADDR_LINE1);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "REMIT_ADDRESS2", aData.REMIT_TO_ADDR_LINE2);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "REMIT_CITY", aData.REMIT_TO_CITY);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "REMIT_STATE", aData.REMIT_TO_STATE);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "REMIT_ZIPCODE", aData.REMIT_TO_ZIP);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "REMIT_COUNTRY", aData.REMIT_TO_COUNTRY);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "REMIT_CONTACT", aData.REMIT_TO_CONTACT_NAME);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "REMIT_TELEPHONE", aData.REMIT_TO_CONTACT_TEL);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "REMIT_FAX", aData.REMIT_TO_CONTACT_FAX);

                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "ORGANIC_FLAG", aData.ORGANIC_FLAG);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "ORGANIC_REG_NUM", aData.ORGANIC_REG_NUM);

                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "ALLOW_GTWY_AUTO_CLOSE_PO", aData.AUTOCLOSEPO);  //added 09-27-16 (JXG) for Driscoll's
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "LPN_LEVEL_VALIDATION", aData.ENABLE_LICENSE_LEVEL_VALIDATION);  //added 09-27-16 (JXG) for Driscoll's

                            if (!recExists)
                            {
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "VENDORID", vendId);
                                ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "CREATE_DATETIME", "GETDATE()");
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CREATE_USERID", "GW");
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
                Class1.WriteException(funcType, Newtonsoft.Json.JsonConvert.SerializeObject(aData), OrderNum, ex.ToString(), updstr);
                retval = HttpStatusCode.BadRequest;
                errmsg = ex.Message;
            }
            return (retval);
        }
    }
}
