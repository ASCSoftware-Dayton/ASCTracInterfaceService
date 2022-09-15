using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Exports
{
    public class ExportPOLicenses
    {
        private static string funcType = "EX_RECV_SKIDS";
        private static Class1 myClass;
        private static Model.PO.POExportConfig currPOExportConfig;

        public static HttpStatusCode doExportPOLicenses(ASCTracInterfaceModel.Model.PO.POExportFilter aPOExportfilter, ref List<ASCTracInterfaceModel.Model.PO.POExportLicenses> aData, ref string errmsg)
        {
            myClass = Class1.InitParse(funcType, ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            aData = new List<ASCTracInterfaceModel.Model.PO.POExportLicenses>();
            string OrderNum = string.Empty;
            string sqlstr = string.Empty;
            try
            {
                if (myClass != null)
                {
                    if (!myClass.FunctionAuthorized(funcType))
                        retval = HttpStatusCode.NonAuthoritativeInformation;
                    else
                    {

                        currPOExportConfig = Configs.POConfig.getPOExportSite("1", myClass.myParse.Globals);
                        sqlstr = BuildPOExportSQL(aPOExportfilter, ref errmsg);
                        if (!String.IsNullOrEmpty(sqlstr))
                        {
                            retval = BuildExportList(sqlstr, ref aData, ref errmsg);
                        }
                        else
                            retval = HttpStatusCode.BadRequest;
                    }
                }
                else
                    retval = HttpStatusCode.InternalServerError;
            }
            catch (Exception ex)
            {
                Class1.WriteException(funcType, Newtonsoft.Json.JsonConvert.SerializeObject(aData), OrderNum, ex.ToString(), sqlstr);
                retval = HttpStatusCode.BadRequest;
                errmsg = ex.Message;
            }
            return (retval);
        }

        private static string BuildWhereStr(ASCTracInterfaceModel.Model.ModelExportFilter rec)
        {
            string retval = string.Empty;
            if (myClass.myParse.Globals.myDBUtils.IfFieldExists("TRANFILE", rec.Fieldname))
                retval = ascLibrary.ascStrUtils.buildwherestr(rec.Fieldname, rec.FilterType.ToString(), rec.Startvalue, rec.Endvalue);
            return (retval);
        }
        private static string BuildPOExportSQL(ASCTracInterfaceModel.Model.PO.POExportFilter aPOExportfilter, ref string errmsg)
        {
            string postedFlagField = currPOExportConfig.postedFlagField;
            /*
            if (aPOExportfilter.PostedFieldNumber == 2)
                postedFlagField = "POSTED2";
            if (aPOExportfilter.PostedFieldNumber == 3)
                postedFlagField = "POSTED3";
            if (aPOExportfilter.PostedFieldNumber == 4)
                postedFlagField = "POSTED_GL";
            */
            string retval = "SELECT SITE_ID, ORDERNUM, RELEASENUM, TRANTYPE, LINENUM, ASCITEMID, ITEMID, LOTID, VENDORID, RECEIVER_ID," +
                "SKIDID, MAX(INV_CONTAINER_ID) AS INV_CONTAINER_ID, MAX(ID) AS ID, MAX(STANDARDCOST) AS STANDARDCOST, " +
                                    "MAX(REASON) AS REASON, MAX(ACCTNUM) AS ACCTNUM, MAX(COMMENTS) AS COMMENTS, " +
                                    "MAX(COST_CENTER_ID) AS COST_CENTER_ID, MAX(RESP_SITE_ID) AS RESP_SITE_ID, " +
                                    "MAX(OLDLOCATION) AS OLDLOCATION, MAX(NEWLOCATION) AS NEWLOCATION, " +
                                    "SUM(QTY) AS QTY, SUM(QTY_DUAL_UNIT) AS QTYDUALUNIT, " +
                            "MAX( SKIDID) AS SKIDID, MAX( USERID) AS USERID, MAX( PRODUCTIONLINE) AS PRODUCTIONLINE, " +
                        "SUM(QTY) AS QTY, SUM(QTY_DUAL_UNIT) AS QTYDUALUNIT, " +
                        "MAX(CUSTID) AS CUSTID " +  //added 10-12-16 (JXG) for Driscoll's
                        "FROM TRANFILE (NOLOCK) " +
                        "WHERE TRANTYPE IN ('RX','RF','RA') AND ISNULL(" + postedFlagField + ",'F')='F' ";
            if (aPOExportfilter.OnlySendCompletedReceipts)
            {
                retval += " AND (( RECEIVER_ID>'' AND RECEIVER_ID IN ( SELECT RECEIVER_ID FROM TRANFILE WHERE TRANTYPE='RV'))" +
                    " OR ( ISNULL( RECEIVER_ID, '') = '' AND ORDERNUM IN ( SELECT ORDERNUM FROM TRANFILE WHERE TRANTYPE='RC'))) ";
            }

            if (currPOExportConfig.ExportUnreceivesAsInvAdj)
                retval += " AND QTY > 0";

            foreach (var rec in aPOExportfilter.ExportFilterList)
            {
                string wherestr = BuildWhereStr(rec);
                if (!String.IsNullOrEmpty(wherestr))
                {
                    retval += " AND " + wherestr;
                }
            }
            if (!String.IsNullOrEmpty(retval))
            {
                retval += " GROUP BY SITE_ID, ORDERNUM, RELEASENUM, TRANTYPE, LINENUM, ASCITEMID, ITEMID, LOTID, VENDORID, RECEIVER_ID, SKIDID";
                retval += " ORDER BY SITE_ID, ORDERNUM, RELEASENUM, LINENUM";
            }
            return (retval);
        }

        private static HttpStatusCode BuildExportList(string sqlstr, ref List<ASCTracInterfaceModel.Model.PO.POExportLicenses> aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.NotFound;
            SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString);
            SqlCommand cmd = new SqlCommand(sqlstr, conn);
            conn.Open();
            SqlDataReader dr2 = cmd.ExecuteReader();

            try
            {
                while (dr2.Read())
                {
                    retval = HttpStatusCode.OK;
                    var rec = new ASCTracInterfaceModel.Model.PO.POExportLicenses();

                    String vmiCustId = String.Empty;
                    myClass.myParse.Globals.myGetInfo.GetASCItemInfo(dr2["ASCITEMID"].ToString(), "STOCK_UOM,BILL_UOM, BUY_UOM,DUAL_UNIT_ITEM,CATID, CAT2ID, MFG_ID, VMI_CUSTID", ref vmiCustId);


                    string stockUom = ascLibrary.ascStrUtils.GetNextWord(ref vmiCustId);
                    string billUom = ascLibrary.ascStrUtils.GetNextWord(ref vmiCustId);
                    String buyUom = ascLibrary.ascStrUtils.GetNextWord(ref vmiCustId);
                    bool dualUnitItem = ascLibrary.ascStrUtils.GetNextWord(ref vmiCustId).StartsWith( "T");
                    var itemCatId = ascLibrary.ascStrUtils.GetNextWord(ref vmiCustId);
                    var itemCat2Id = ascLibrary.ascStrUtils.GetNextWord(ref vmiCustId);
                    var mfgId = ascLibrary.ascStrUtils.GetNextWord(ref vmiCustId);

                    string hostSiteId = string.Empty;
                    myClass.myParse.Globals.myGetInfo.GetSiteInfo(dr2["SITE_ID"].ToString(), "HOST_SITE_ID", ref hostSiteId);
                    var linenum = dr2["LINENUM"].ToString();
                    var skidId = dr2["SKIDID"].ToString();
                    var containerId = dr2["INV_CONTAINER_ID"].ToString();
                    var reasonCd = string.Empty;
                    myClass.myParse.Globals.myGetInfo.GetReasonCodeInfo( dr2["REASON"].ToString(), "HOST_REASON", ref reasonCd);  //added 10-11-16 (JXG) for Driscoll's
                    if (String.IsNullOrEmpty(reasonCd))
                        reasonCd = dr2["REASON"].ToString();
                    var acctNum = dr2["ACCTNUM"].ToString();
                    var comments = dr2["COMMENTS"].ToString();
                    var costCtrId = dr2["COST_CENTER_ID"].ToString();
                    var respSiteId = dr2["RESP_SITE_ID"].ToString();
                    var oldLoc = dr2["OLDLOCATION"].ToString();
                    var newLoc = dr2["NEWLOCATION"].ToString();
                    double qty = ascLibrary.ascUtils.ascStrToDouble(dr2["QTY"].ToString(), 0);
                    var qtyDualUnit = ascLibrary.ascUtils.ascStrToDouble(dr2["QTYDUALUNIT"].ToString(), 0);
                    var tranRecvrId = dr2["RECEIVER_ID"].ToString();  //added 06-03-16 (JXG) for Driscoll's

                    var userId = dr2["USERID"].ToString();  //added 06-28-16 (JXG)
                    var siteId = dr2["SITE_ID"].ToString();  //added 03-27-17 (JXG)

                    if (qty < 0 && qtyDualUnit > 0)
                        qtyDualUnit = -qtyDualUnit;

                    if (String.IsNullOrEmpty(containerId) && !String.IsNullOrEmpty(skidId))
                    {
                        if (!myClass.myParse.Globals.myGetInfo.GetSkidInfo(skidId, "INV_CONTAINER_ID", ref containerId))
                            myClass.myParse.Globals.myGetInfo.GetSkidHistInfo(skidId, "INV_CONTAINER_ID", ref containerId);
                    }
                    string tmpStr = string.Empty;
                    string lpnCustomData = string.Empty;
                    var expDate = "";
                    var recvDatetime = "";
                    var prodDatetime = "";
                    var altLotId = "";
                    var altSkidId = "";
                    if (!String.IsNullOrEmpty(skidId) && !skidId.StartsWith("-"))
                    {
                        if (!myClass.myParse.Globals.myGetInfo.GetSkidInfo(skidId, "EXPDATE, RECVDATETIME, DATETIMEPROD, ALT_LOTID, ALT_SKIDID, CUSTOM_DATA1, CUSTOM_DATA2, CUSTOM_DATA3, CUSTOM_DATA4, " +
                            "CUSTOM_DATA5, CUSTOM_DATA6, CUSTOM_NUM1, CUSTOM_DATE", ref lpnCustomData))
                            myClass.myParse.Globals.myGetInfo.GetSkidHistInfo(skidId, "EXPDATE, RECVDATETIME, DATETIMEPROD, ALT_LOTID, ALT_SKIDID, CUSTOM_DATA1, CUSTOM_DATA2, CUSTOM_DATA3, CUSTOM_DATA4, " +
                            "CUSTOM_DATA5, CUSTOM_DATA6, CUSTOM_NUM1, CUSTOM_DATE", ref lpnCustomData);
                        expDate =  ascLibrary.ascStrUtils.GetNextWord( ref lpnCustomData);
                        recvDatetime = ascLibrary.ascStrUtils.GetNextWord( ref lpnCustomData);
                        prodDatetime = ascLibrary.ascStrUtils.GetNextWord( ref lpnCustomData);
                        altLotId = ascLibrary.ascStrUtils.GetNextWord( ref lpnCustomData);  //added 08-03-16 (JXG) for Driscoll's
                        altSkidId = ascLibrary.ascStrUtils.GetNextWord( ref lpnCustomData);  //added 05-03-17 (JXG) for Allen Dist
                    }

                    string whseID = "";
                    if (string.IsNullOrEmpty(oldLoc))
                        myClass.myParse.Globals.myGetInfo.GetLocInfo(newLoc, "WHSE_ID", ref whseID);
                    else
                        myClass.myParse.Globals.myGetInfo.GetLocInfo(oldLoc, "WHSE_ID", ref whseID);

                    
                    //rec.PROCESS_FLAG = "R";
                    //rec.CREATE_DATETIME = DateTime.Now.ToString();
                    rec.FACILITY = hostSiteId;
                    rec.PRODUCT_CODE = dr2["ITEMID"].ToString();
                    rec.QUANTITY = qty;
                    rec.DATE_TIME = ascLibrary.ascUtils.ascStrToDate(recvDatetime, DateTime.MinValue);
                    rec.TRANS_TYPE = "RL";  //tranType;
                    rec.REASON_CODE = reasonCd;
                    rec.SKIDID = skidId;
                    rec.LOTID = dr2["LOTID"].ToString();
                    rec.REF_NUMBER = acctNum;
                    if (!String.IsNullOrEmpty(expDate))
                        rec.EXPDATE = ascLibrary.ascUtils.ascStrToDate(expDate, DateTime.MinValue);
                    rec.VMI_CUSTID = vmiCustId;
                    rec.CONTAINER_ID = containerId;
                    rec.WHSE_ID = whseID;
                    //rec.FROM_LOCATION"] = fromLoc;
                    //rec.TO_LOCATION"] = toLoc;
                    if (!String.IsNullOrEmpty(dr2["PRODUCTIONLINE"].ToString()))
                        rec.PROD_LINE = dr2["PRODUCTIONLINE"].ToString();
                        rec.COMMENTS = comments;
                        if (!String.IsNullOrEmpty(recvDatetime))
                            rec.RECVDATETIME = ascLibrary.ascUtils.ascStrToDate(recvDatetime, DateTime.MinValue); ;
                        if (!String.IsNullOrEmpty(prodDatetime))
                            rec.DATETIMEPROD = ascLibrary.ascUtils.ascStrToDate(prodDatetime, DateTime.MinValue);
                    if (!String.IsNullOrEmpty(dr2["VENDORID"].ToString()))
                        rec.VENDORID = dr2["VENDORID"].ToString();
                    
                        if (!String.IsNullOrEmpty(dr2["RECEIVER_ID"].ToString()))
                            rec.RECEIVER_NO= dr2["RECEIVER_ID"].ToString();
            
                        if (!String.IsNullOrEmpty(stockUom))
                            rec.STOCK_UOM = stockUom;
                        if (!String.IsNullOrEmpty(userId))
                            rec.USERID = userId;
                        if (!String.IsNullOrEmpty(altLotId))
                            rec.ALT_LOTID = altLotId;
                    
                        if (!String.IsNullOrEmpty(altSkidId))
                            rec.ALT_SKIDID = altSkidId;
                    
                        if (!String.IsNullOrEmpty(itemCatId))
                            rec.CATID = itemCatId;
                        if (!String.IsNullOrEmpty(itemCat2Id))
                            rec.CAT2ID = itemCat2Id;
                    
                        if (!String.IsNullOrEmpty(mfgId))
                            rec.MFG_ID = mfgId;
                        if( !String.IsNullOrEmpty( dr2["STANDARDCOST"].ToString()))
                    {
                        rec.COST_EACH = ascLibrary.ascUtils.ascStrToDouble(dr2["STANDARDCOST"].ToString(), 0);
                    }

                    if (!String.IsNullOrEmpty(billUom) || dualUnitItem)
                    {
                        rec.CW_UOM = billUom;
                        rec.CW_QTY = qtyDualUnit;
                    }
                    rec.UOM = buyUom;

                    string orderNum = dr2["ORDERNUM"].ToString(); // hostPoNum;  //poNum  //changed 09-20-16 (JXG) for Driscoll's
                    orderNum = (orderNum.StartsWith("ASN-")) ? orderNum.Substring(4) : orderNum;
                    rec.ORDERNUMBER = orderNum;

                    if (!String.IsNullOrEmpty(linenum))
                        rec.LINE_NUMBER = ascLibrary.ascUtils.ascStrToInt(linenum, 0);

                    myClass.myParse.Globals.myGetInfo.GetLotInfo(dr2["ASCITEMID"].ToString(), dr2["LOTID"].ToString(), "CA_FILENAME", ref tmpStr);
                    
                    if (!String.IsNullOrEmpty(tmpStr))
                        rec.CA_FILENAME = tmpStr;

                    if (!String.IsNullOrEmpty(skidId))
                    {

                        rec.CUSTOM_DATA1 = ascLibrary.ascStrUtils.GetNextWord(ref lpnCustomData);
                        rec.CUSTOM_DATA2 = ascLibrary.ascStrUtils.GetNextWord(ref lpnCustomData);
                        rec.CUSTOM_DATA3 = ascLibrary.ascStrUtils.GetNextWord(ref lpnCustomData);
                        rec.CUSTOM_DATA4 = ascLibrary.ascStrUtils.GetNextWord(ref lpnCustomData);
                        rec.CUSTOM_DATA5 = ascLibrary.ascStrUtils.GetNextWord(ref lpnCustomData);
                        rec.CUSTOM_DATA6 = ascLibrary.ascStrUtils.GetNextWord(ref lpnCustomData);
                        tmpStr = ascLibrary.ascStrUtils.GetNextWord(ref lpnCustomData);
                        if (!String.IsNullOrEmpty(tmpStr))
                            rec.CUSTOM_NUM1 = ascLibrary.ascUtils.ascStrToDouble(tmpStr, 0);

                        tmpStr = ascLibrary.ascStrUtils.GetNextWord(ref lpnCustomData);
                        if (!String.IsNullOrEmpty(tmpStr))
                            rec.CUSTOM_DATE = ascLibrary.ascUtils.ascStrToDate(tmpStr, DateTime.MinValue);
                        
                    }

                    if (!String.IsNullOrEmpty(costCtrId))
                        rec.COST_CENTER_ID = costCtrId;
                    if (!String.IsNullOrEmpty(respSiteId))
                        rec.RESP_SITE_ID = respSiteId;

                        rec.RELEASENUM = dr2["RELEASENUM"].ToString();
                    SetPosted(rec.ORDERNUMBER, rec.RELEASENUM, rec.LINE_NUMBER, rec.PRODUCT_CODE, rec.LOTID, rec.SKIDID, rec.RECEIVER_NO, string.Empty, "S");
                    aData.Add(rec);
                }
                if (retval == HttpStatusCode.OK)
                    myClass.myParse.Globals.mydmupdate.ProcessUpdates();
            }
            finally
            {
                dr2.Close();
                cmd.Dispose();
                conn.Close();
                conn.Dispose();
            }
            return (retval);
        }


        public static HttpStatusCode updateExportPOLicenses(List<ASCTracInterfaceModel.Model.PO.POExportLicenses> aData, ref string errmsg)
        {
            myClass = Class1.InitParse("UpdateExportPO", ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            string OrderNum = string.Empty;
            string sqlstr = string.Empty;
            try
            {
                if (myClass != null)
                {
                    myClass.myParse.Globals.mydmupdate.InitUpdate();
                    currPOExportConfig = Configs.POConfig.getPOExportSite("1", myClass.myParse.Globals);
                    retval = DoUpdateExportPOLicenses(aData, ref errmsg);
                    if (retval == HttpStatusCode.OK)
                        myClass.myParse.Globals.mydmupdate.ProcessUpdates();
                }
                else
                    retval = HttpStatusCode.InternalServerError;
            }
            catch (Exception ex)
            {
                Class1.WriteException("POExport", Newtonsoft.Json.JsonConvert.SerializeObject(aData), OrderNum, ex.ToString(), sqlstr);
                retval = HttpStatusCode.BadRequest;
                errmsg = ex.Message;
            }
            return (retval);

        }

        private static HttpStatusCode DoUpdateExportPOLicenses(List<ASCTracInterfaceModel.Model.PO.POExportLicenses> aData, ref string errmsg)
        {
            int msgLen = Convert.ToInt32(myClass.myParse.Globals.myDBUtils.getfieldsize("TRANFILE", "ERR_MESSAGE"));
            HttpStatusCode retval = HttpStatusCode.OK;
            foreach (var rec in aData)
            {
                string posted = "T";
                if (!rec.SUCCESSFUL)
                    posted = "E";
                SetPosted(rec.ORDERNUMBER, rec.RELEASENUM, rec.LINE_NUMBER, rec.PRODUCT_CODE, rec.LOTID, rec.SKIDID, rec.RECEIVER_NO, rec.ERROR_MESSAGE, posted);
/*
                string sqlStr = "UPDATE TRANFILE";
                if (rec.SUCCESSFUL)
                    sqlStr += " SET " + currPOExportConfig.postedFlagField + "='T', " + currPOExportConfig.posteddateField + "=GETDATE() ";
                else
                    sqlStr = " SET " + currPOExportConfig.postedFlagField + "='E', " + currPOExportConfig.posteddateField + "=GETDATE(), " +
                        " ERR_MESSAGE='" + rec.ERROR_MESSAGE.Substring(0, msgLen).Replace("'", "''") + "', " +
                        " LONG_MESSAGE='" + rec.ERROR_MESSAGE.Replace("'", "''") + "' ";

                sqlStr += " WHERE ORDERNUM='" + rec.ORDERNUMBER + "' AND TRANTYPE IN ('RX', 'RF', 'RA') " +
                    " AND (" + currPOExportConfig.postedFlagField + " IN ('F', 'N', '0', '') OR " + currPOExportConfig.postedFlagField + " IS NULL) ";
                if (!String.IsNullOrEmpty(rec.RELEASENUM))
                    sqlStr += " AND RELEASENUM='" + rec.RELEASENUM + "'";
                if (rec.LINE_NUMBER > 0)
                    sqlStr += " AND LINENUM='" + rec.LINE_NUMBER + "'";
                else if (!String.IsNullOrEmpty(rec.PRODUCT_CODE))
                    sqlStr += " AND ITEMID='" + rec.PRODUCT_CODE + "'";
                if (!String.IsNullOrEmpty(rec.LOTID))
                    sqlStr += " AND LOTID='" + rec.LOTID + "'";
                if (!String.IsNullOrEmpty(rec.SKIDID))
                    sqlStr += " AND SKIDID='" + rec.SKIDID + "'";

                if (!String.IsNullOrEmpty(rec.RECEIVER_NO))
                    sqlStr += "AND RECEIVER_ID='" + rec.RECEIVER_NO + "' ";

                myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
*/
            }

            return (retval);
        }
        private static void SetPosted(string aPONUMBER, string aRELEASENUM, double aLINE_NUMBER, string aPRODUCT_CODE, string aLOTID, string aSKIDID, string aRECEIVER_NO, string aERROR_MESSAGE, string aPostedflag)
        {
            int msgLen = Convert.ToInt32(myClass.myParse.Globals.myDBUtils.getfieldsize("TRANFILE", "ERR_MESSAGE"));
            string shortErrorMessage = aERROR_MESSAGE;
            if (shortErrorMessage.Length > msgLen)
                shortErrorMessage = aERROR_MESSAGE.Substring(0, msgLen);

            string sqlStr = "UPDATE TRANFILE";
            if (!aPostedflag.Equals("E"))
                sqlStr += " SET " + currPOExportConfig.postedFlagField + "='" + aPostedflag + "', " + currPOExportConfig.posteddateField + "=GETDATE() ";
            else
                sqlStr += " SET " + currPOExportConfig.postedFlagField + "='E', " + currPOExportConfig.posteddateField + "=GETDATE(), " +
                    "ERR_MESSAGE='" + shortErrorMessage.Replace("'", "''") + "', " +
                    "LONG_MESSAGE='" + aERROR_MESSAGE.Replace("'", "''") + "' ";

            sqlStr += " WHERE ORDERNUM='" + aPONUMBER + "' AND TRANTYPE IN ('RX', 'RF', 'RA') ";
            if (aPostedflag.Equals("S"))
                sqlStr += " AND ISNULL(" + currPOExportConfig.postedFlagField + ",'F') = 'F'";
            else
                sqlStr += " AND ISNULL(" + currPOExportConfig.postedFlagField + ",'F') = 'S'";
            //" AND ISNULL(" + currPOExportConfig.postedFlagField + "','F') NOT IN ( 'T', 'X', 'D', 'E', 'P', '" + aPostedflag + "')";
            //"AND (" + currPOExportConfig.postedFlagField + " IN ('F', 'N', '0', '') OR " + currPOExportConfig.postedFlagField + " IS NULL) ";
            if (!String.IsNullOrEmpty(aRELEASENUM))
                sqlStr += " AND RELEASENUM='" + aRELEASENUM + "'";
            if (aLINE_NUMBER > 0)
                sqlStr += " AND LINENUM='" + aLINE_NUMBER + "'";
            else if (!String.IsNullOrEmpty(aPRODUCT_CODE))
                sqlStr += " AND ITEMID='" + aPRODUCT_CODE + "'";
            if (!String.IsNullOrEmpty(aLOTID))
                sqlStr += " AND LOTID='" + aLOTID + "'";
            if (!String.IsNullOrEmpty(aSKIDID))
                sqlStr += " AND SKIDID='" + aSKIDID + "'";

            if (!String.IsNullOrEmpty(aRECEIVER_NO))
                sqlStr += "AND RECEIVER_ID='" + aRECEIVER_NO + "' ";

            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
        }

    }
}


    