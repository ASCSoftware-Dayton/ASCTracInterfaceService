using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Imports
{
    public class ImportASN
    {
        private static string funcType = "IM_ASN";
        private static string siteid = string.Empty;
        private static Class1 myClass;
        private static Dictionary<string, List<string>> GWTranslation = new Dictionary<string, List<string>>();
        public static HttpStatusCode doImportASN(ASCTracInterfaceModel.Model.ASN.ASNHdrImport aData, ref string errmsg)
        {
            myClass = Class1.InitParse(funcType, ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            string OrderNum = aData.ASN;
            string updstr = string.Empty;
            try
            {
                if (myClass != null)
                {
                    if (!myClass.FunctionAuthorized(funcType))
                        retval = HttpStatusCode.NonAuthoritativeInformation;
                    else
                    {
                        myClass.myParse.Globals.mydmupdate.InitUpdate();
                        siteid = myClass.GetSiteIdFromHostId(aData.FACILITY);
                        if (String.IsNullOrEmpty(siteid))
                        {
                            errmsg = "No Facility or Site defined for record.";
                            retval = HttpStatusCode.BadRequest;
                        }
                        else
                        {
                            Configs.ConfigUtils.ReadTransationFields(GWTranslation, "ASN_DET", myClass.myParse.Globals);
                            if (string.IsNullOrEmpty(OrderNum))
                            {
                                errmsg = "ASN value is required.";
                                retval = HttpStatusCode.BadRequest;
                            }
                            else
                                retval = ImportASNRecord(aData, ref errmsg);
                            if (retval == HttpStatusCode.OK)
                                myClass.myParse.Globals.mydmupdate.ProcessUpdates();
                        }
                    }
                }
                else
                    retval = HttpStatusCode.InternalServerError;
            }
            catch (Exception ex)
            {
                Class1.WriteException(funcType, Newtonsoft.Json.JsonConvert.SerializeObject(aData), OrderNum, ex.ToString(), updstr);
                retval = HttpStatusCode.BadRequest;
                errmsg = ex.Message;

            }
            return (retval);
        }

        private static HttpStatusCode ImportASNRecord(ASCTracInterfaceModel.Model.ASN.ASNHdrImport aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;

            string asn = aData.ASN;

            if (CanImportAsn(asn))
            {
                DeleteAsnIfExists(asn);
                if (ImportAsnHdr(aData, ref errmsg) && ImportAsnDet(aData))
                {
                    string sqlStr = "UPDATE ASN_HDR SET STATUS='N' WHERE ASN='" + asn + "'";
                    myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
                    //////////////////////
                }
                else
                    retval = HttpStatusCode.BadRequest;
            }
            else
            {
                retval = HttpStatusCode.BadRequest;
                errmsg = "ASN status has changed, cannot update.";
                Class1.WriteException(funcType, Newtonsoft.Json.JsonConvert.SerializeObject(aData), asn, errmsg, "");
            }
            return (retval);
        }

        private static bool CanImportAsn(string asn)
        {
            string tmpStr = string.Empty;
            string sqlStr = "SELECT STATUS FROM ASN_HDR (NOLOCK) WHERE ASN='" + asn + "' AND ISNULL(STATUS,'N')<>'N'";
            if (myClass.myParse.Globals.myDBUtils.ifRecExists(sqlStr))
                return false;
            return true;
        }

        private static void DeleteAsnIfExists(string asn)
        {
            string tmpStr = string.Empty;
            string sqlStr = "SELECT NULL FROM ASN_HDR (NOLOCK) WHERE ASN='" + asn + "'";
            if (myClass.myParse.Globals.myDBUtils.ifRecExists(sqlStr))
            {
                UpdateQtyAsnInTransit(asn, true);

                sqlStr = "DELETE FROM ASN_DET WHERE ASN='" + asn + "'";
                myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
                sqlStr = "DELETE FROM ASN_HDR WHERE ASN='" + asn + "'";
                myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
                myClass.myParse.Globals.mydmupdate.ProcessUpdates();
            }
        }

        private static void UpdateQtyAsnInTransit(string asnNum, bool decrement)
        {
            string qtySign = decrement ? "-" : "+";
            string sqlStr = "SELECT ASCITEMID, QTY FROM ASN_DET (NOLOCK) WHERE ASN=@asn";
            using (SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
            using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
            {
                conn.Open();
                cmd.Parameters.Add("@asn", SqlDbType.VarChar, 1000).Value = asnNum;
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        sqlStr = "UPDATE ITEMQTY SET QTY_ASN_IN_TRANSIT = QTY_ASN_IN_TRANSIT" + qtySign + dr["QTY"].ToString() +
                            " WHERE ASCITEMID='" + dr["ASCITEMID"].ToString() + "'";
                        myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
                    }
                }
            }
        }

        private static bool ImportAsnHdr(ASCTracInterfaceModel.Model.ASN.ASNHdrImport aData, ref string errmsg)
        {

            string tmpStr = string.Empty;
            string asnType, fromSiteId = "";

            asnType = aData.ASN_TYPE;
            if (String.IsNullOrEmpty(asnType))
                asnType = "A";

            if (!String.IsNullOrEmpty(aData.FROM_FACILITY))
            {
                fromSiteId = myClass.GetSiteIdFromHostId(aData.FROM_FACILITY);
                if (String.IsNullOrEmpty(fromSiteId))
                {
                    errmsg = "No site exists in ASCTrac with Host Site ID '" + aData.FROM_FACILITY + "'.";
                    Class1.WriteException(funcType, Newtonsoft.Json.JsonConvert.SerializeObject(aData), aData.ASN, errmsg, "");
                    return false;
                }
            }

            string updstr = string.Empty;
            Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_HDR", "ASN", aData.ASN);
            Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_HDR", "ASN_TYPE", asnType);
            Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_HDR", "STATUS", "I");  //"N"  //changed 11-21-13 (JXG)
            Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_HDR", "SITE_ID", siteid);
            Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_HDR", "TRANSFER_SITE_ID", fromSiteId);
            if (aData.CREATE_DATETIME == DateTime.MinValue)
                ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "CREATE_DATE", "GetDate()");
            else
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_HDR", "CREATE_DATE", aData.CREATE_DATETIME.ToString());
            Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_HDR", "VENDORID", aData.VENDORID);
            Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_HDR", "PONUMBER", aData.PONUMBER);
            Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_HDR", "TRUCKNUM", aData.TRUCKNUM);
            Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_HDR", "REF_ORDERNUMBER", aData.REF_ORDERNUMBER);
            Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_HDR", "PACKINGSLIP", aData.PACKINGSLIP);
            if (aData.EXPECTED_RECEIPT_DATE != DateTime.MinValue)
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_HDR", "EXPECTEDRECEIPTDATE", aData.EXPECTED_RECEIPT_DATE.ToString());
            myClass.myParse.Globals.mydmupdate.InsertRecord("ASN_HDR", updstr);

            return true;
        }

        private static void SaveCustomFields(ref string updStr, List<ASCTracInterfaceModel.Model.ModelCustomData> CustomList, Dictionary<string, List<string>> TranslationList)
        {
            foreach (var rec in CustomList)
            {
                if (TranslationList.ContainsKey(rec.FieldName))
                {
                    var asclist = TranslationList[rec.FieldName];
                    foreach (var ascfield in asclist)
                    {

                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, ascfield, rec.Value);
                    }
                }
            }
        }

        private static bool ImportAsnDet(ASCTracInterfaceModel.Model.ASN.ASNHdrImport aData)
        {
            string tmpStr = string.Empty;
            string vmiCustId, itemId, ascItemId = string.Empty, skidId;

            foreach (var rec in aData.DetailList)
            {
                itemId = rec.ITEMID;
                vmiCustId = string.Empty;
                if (!string.IsNullOrEmpty(rec.VMI_CUSTID))
                    vmiCustId = rec.VMI_CUSTID;

                if( String.IsNullOrEmpty( itemId))
                {
                    string errMsg = "ItemID does not have a value";
                    Class1.WriteException(funcType, Newtonsoft.Json.JsonConvert.SerializeObject(aData), aData.ASN, errMsg, "");
                    return false;

                }
                if (myClass.myParse.Globals.myConfig.iniGNVMI.boolValue)
                {
                    ascItemId = myClass.myParse.Globals.dmMiscItem.GetASCItem(siteid, itemId, vmiCustId);
                    if (!myClass.myParse.Globals.myGetInfo.GetASCItemInfo(ascItemId, "ITEM_STATUS", ref tmpStr))
                        ascItemId = string.Empty;
                }
                if (string.IsNullOrEmpty(ascItemId))
                {
                    ascItemId = myClass.myParse.Globals.dmMiscItem.GetASCItem(siteid, itemId, "");
                    if (!myClass.myParse.Globals.myGetInfo.GetASCItemInfo(ascItemId, "ITEM_STATUS", ref tmpStr))
                        ascItemId = string.Empty;
                }
                if (string.IsNullOrEmpty(ascItemId))
                {
                    string errMsg = "Item " + itemId + " not found in Item Master.";
                    Class1.WriteException(funcType, Newtonsoft.Json.JsonConvert.SerializeObject(aData), aData.ASN, errMsg, "");
                    return false;
                }

                skidId = rec.SKIDID;

                string sqlStr = "SELECT NULL FROM LOCITEMS (NOLOCK) WHERE SKIDID='" + skidId + "'";
                if (myClass.myParse.Globals.myDBUtils.ifRecExists(sqlStr))
                    throw new Exception(String.Format("License {0} already exists.", skidId));

                string updstr = string.Empty;
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "ASN", aData.ASN);
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "STATUS", "N");
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "ITEMID", itemId);
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "ASCITEMID", ascItemId);
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "VENDORID", aData.VENDORID);
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "QTY", rec.QUANTITY.ToString());
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "SKIDID", skidId);
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "CNTRTYPE_ID", rec.PALLET_TYPE);
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "INV_CONTAINER_ID", rec.CONTAINER_ID);
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "LOTID", rec.LOTID);
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "QTY_DUAL_UNIT", rec.CW_QTY.ToString());
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "PONUMBER", rec.PONUMBER);
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "RELEASENUM", rec.RELEASENUM);
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "LINENUMBER", rec.LINENUMBER.ToString());
                if (rec.EXPIRE_DATE != DateTime.MinValue)
                    Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "EXPDATE", rec.EXPIRE_DATE.ToString());
                if (rec.DATETIMEPROD != DateTime.MinValue)
                    Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "DATETIMEPROD", rec.DATETIMEPROD.ToString());
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "ACTUAL_WEIGHT", rec.ACTUAL_WEIGHT.ToString());

                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "ALT_SKIDID", rec.ALT_SKIDID);
                Utils.ASCUtils.CheckAndAppend( ref updstr, "ASN_DET",  "ALT_LOTID", rec.ALT_LOTID);
                SaveCustomFields(ref updstr, rec.CustomList, GWTranslation);

                myClass.myParse.Globals.mydmupdate.InsertRecord("ASN_DET", updstr);

                sqlStr = "UPDATE ITEMQTY SET QTY_ASN_IN_TRANSIT = ISNULL( QTY_ASN_IN_TRANSIT, 0) + " + rec.QUANTITY.ToString() +
                        " WHERE ASCITEMID='" + ascItemId + "'";
                myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);

            }
            return (true);
        }
    }
}
