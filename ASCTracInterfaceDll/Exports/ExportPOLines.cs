using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Exports
{
    public class ExportPOLines
    {
        private string funcType = "EX_RECV_LINES";
        private Class1 myClass;
        private Model.PO.POExportConfig currPOExportConfig;

        public static HttpStatusCode doExportPOLines(Class1 myClass, ASCTracInterfaceModel.Model.PO.POExportFilter aPOExportfilter,  ref List<ASCTracInterfaceModel.Model.PO.POExportLines> aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            aData = new List<ASCTracInterfaceModel.Model.PO.POExportLines>();
            string OrderNum = string.Empty;
            string sqlstr = string.Empty;
            try
            {
                if (!myClass.FunctionAuthorized(myClass.myLogRecord.FunctionID))
                    retval = HttpStatusCode.NonAuthoritativeInformation;
                else
                {
                    var myExport = new ExportPOLines(myClass);
                    sqlstr = myExport.BuildPOExportSQL(aPOExportfilter, ref errmsg);
                    if (!String.IsNullOrEmpty(sqlstr))
                    {
                        myClass.myLogRecord.SQLData = sqlstr;
                        retval = myExport.BuildExportList(sqlstr, ref aData, ref errmsg);
                    }
                    else
                        retval = HttpStatusCode.BadRequest;
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

        public ExportPOLines(Class1 aClass)
        {
            myClass = aClass;
            currPOExportConfig = Configs.POConfig.getPOExportSite("1", myClass.myParse.Globals);
        }
        private string BuildWhereStr(ASCTracInterfaceModel.Model.ModelExportFilter rec)
        {
            string retval = string.Empty;
            if (myClass.myParse.Globals.myDBUtils.IfFieldExists("TRANFILE", rec.Fieldname))
                retval = ascLibrary.ascStrUtils.buildwherestr(rec.Fieldname, rec.FilterType.ToString(), rec.Startvalue, rec.Endvalue);
            return (retval);
        }
        private string BuildPOExportSQL(ASCTracInterfaceModel.Model.PO.POExportFilter aPOExportfilter, ref string errmsg)
        {
            string postedFlagField = currPOExportConfig.postedFlagField;
            string retval = "SELECT SITE_ID, ORDERNUM, RELEASENUM, TRANTYPE, LINENUM, ASCITEMID, ITEMID, LOTID, VENDORID, RECEIVER_ID," +
                            "MAX(ID) AS ID, MAX( NEWLOCATION) AS NEWLOCATION, MAX( SKIDID) AS SKIDID, MAX( USERID) AS USERID, MAX( PRODUCTIONLINE) AS PRODUCTIONLINE, " +
                        "SUM(QTY) AS QTY, SUM(QTY_DUAL_UNIT) AS QTYDUALUNIT, " +
                        "MAX(CUSTID) AS CUSTID " +  //added 10-12-16 (JXG) for Driscoll's
                        "FROM TRANFILE (NOLOCK) " +
                        "WHERE TRANTYPE IN ('RX','RF','RA') AND ISNULL(" + postedFlagField + ",'F') IN (" + currPOExportConfig.FilterPostedValues + ") ";
            if( aPOExportfilter.OnlySendCompletedReceipts)
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
            if(!String.IsNullOrEmpty( retval))
            {
                retval += " GROUP BY SITE_ID, ORDERNUM, RELEASENUM, TRANTYPE, LINENUM, ASCITEMID, ITEMID, LOTID, VENDORID, RECEIVER_ID";
                retval += " ORDER BY SITE_ID, ORDERNUM, RELEASENUM, LINENUM";
            }
            return (retval);
        }

        private HttpStatusCode BuildExportList(string sqlstr, ref List<ASCTracInterfaceModel.Model.PO.POExportLines> aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.NoContent;
            SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString);
            SqlCommand cmd= new SqlCommand(sqlstr, conn);
            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            try
            {
                while (dr.Read())
                {
                    retval = HttpStatusCode.OK;
                    var rec = new ASCTracInterfaceModel.Model.PO.POExportLines();

                    string hostSiteId = string.Empty;
                    myClass.myParse.Globals.myGetInfo.GetSiteInfo(dr["SITE_ID"].ToString(), "HOST_SITE_ID", ref hostSiteId);
                    var poNum = dr["ORDERNUM"].ToString();
                    var relNum = dr["RELEASENUM"].ToString();
                    var tranType = dr["TRANTYPE"].ToString();
                    var lineNum = dr["LINENUM"].ToString();
                    var itemId = dr["ITEMID"].ToString();
                    var ascItemId = dr["ASCITEMID"].ToString();
                    var lotId = dr["LOTID"].ToString();
                    var vendId = dr["VENDORID"].ToString();
                    var qty = ascLibrary.ascUtils.ascStrToDouble(dr["QTY"].ToString(), 0);
                    var qtyDualUnit = ascLibrary.ascUtils.ascStrToDouble(dr["QTYDUALUNIT"].ToString(), 0);
                    var tranCustId = dr["CUSTID"].ToString();  //added 10-12-16 (JXG) for Driscoll's
                    string orderType = "A";
                    myClass.myParse.Globals.myGetInfo.GetPOHdrInfo(poNum, relNum, "CUSTOM_DATA1, CUSTOM_DATA2, CUSTOM_DATA3, CUSTOM_DATA4, CUSTOM_DATA5, CUSTOM_DATA6, CUSTOM_DATA7, CUSTOM_DATA8, RECEIVED,ORDERTYPE", ref orderType);
                    var customHdrData1 = ascLibrary.ascStrUtils.GetNextWord(ref orderType);
                    var customHdrData2 = ascLibrary.ascStrUtils.GetNextWord(ref orderType);
                    var customHdrData3 = ascLibrary.ascStrUtils.GetNextWord(ref orderType);
                    var customHdrData4 = ascLibrary.ascStrUtils.GetNextWord(ref orderType);
                    var customHdrData5 = ascLibrary.ascStrUtils.GetNextWord(ref orderType);
                    var customHdrData6 = ascLibrary.ascStrUtils.GetNextWord(ref orderType);
                    var customHdrData7 = ascLibrary.ascStrUtils.GetNextWord(ref orderType);
                    var customHdrData8 = ascLibrary.ascStrUtils.GetNextWord(ref orderType);
                    var rxStatus = ascLibrary.ascStrUtils.GetNextWord(ref orderType);

                    if (tranType == "RA")
                        orderType = "A";
                    else
                    {
                        orderType = (orderType == "T") ? orderType : "E";
                    }

                    // Get locid and skidid - making the assumption that these values 
                    // will be the same for these receive records, or that this will
                    // suffice for the purpose these values are intended.
                    ////////////////////////////////////
                    var locId = dr["NEWLOCATION"].ToString();
                    var skidId = dr["SKIDID"].ToString();
                    var userId = dr["USERID"].ToString();
                    userId = userId.Length > 20 ? userId.Substring(0, 20) : userId;  //changed from 5 to 20 08-08-16 (JXG)
                                                                                     //added 07-17-15 (JXG) for Dricoll's
                    var prodLine = dr["PRODUCTIONLINE"].ToString();
                    //if (String.IsNullOrEmpty(prodLine))  //taken out 08-08-16 (JXG)
                    //    prodLine = "0000";
                    ////////////////////////////////////

                    if (qty < 0 && qtyDualUnit > 0)
                        qtyDualUnit = -qtyDualUnit;

                    string tmpStr = string.Empty;
                    var expDate = "";
                    var altLotId = "";
                    var prodDate = string.Empty;
                    if (!String.IsNullOrEmpty(skidId))
                    {
                        if (!myClass.myParse.Globals.myGetInfo.GetSkidInfo(skidId, "DATETIMEPROD, EXPDATE, ALT_LOTID", ref tmpStr))
                            myClass.myParse.Globals.myGetInfo.GetSkidHistInfo(skidId, "DATETIMEPROD, EXPDATE, ALT_LOTID", ref tmpStr);
                        prodDate = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                        expDate = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                        altLotId = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);  //added 08-03-16 (JXG) for Driscoll's
                    }

                    myClass.myParse.Globals.myGetInfo.GetPODetInfo(poNum, relNum, lineNum, "PACKINGSLIP, CARRIERNAME, RECEIVEDDATE, VENDORITEMID, " +
                        "RECEIVED, CUSTOM_DATA1, CUSTOM_DATA2, CUSTOM_DATA3", ref tmpStr);
                    var packSlip = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                    var carrier = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                    var receiveDate = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), DateTime.MinValue);
                    var vendItemId = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                    var rxLineStatus = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);  //added 07-17-15 (JXG) for Driscoll's
                    var customData1 = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                    var customData2 = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                    var customData3 = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                    var customData4 = "";
                    var customData5 = "";

                    myClass.myParse.Globals.myGetInfo.GetASCItemInfo(ascItemId, "BILL_UOM, VMI_CUSTID, DUAL_UNIT_ITEM, BUY_UOM, STOCK_UOM, CATID, CAT2ID, MFG_ID, STANDARDCOST", ref tmpStr);
                    var billUom = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                    var vmiCustId = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                    var dualUnitItem = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr).StartsWith("T");
                    string buyUom = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);  //added 06-28-16 (JXG)
                    var stockUom = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);  //added 06-28-16 (JXG)
                    var itemCatId = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);  //added 10-17-16 (JXG) for Driscoll's
                    var itemCat2Id = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);  //added 10-17-16 (JXG) for Driscoll's
                    var mfgId = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);  //added 03-24-17 (JXG)
                    var stdCost = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);  //added 03-24-17 (JXG)

                    /*
                    //changed 07-30-15 (JXG) for Driscoll's
                    customData1 = GetUserDefInfo(poNum, lineNum, skidId, customData1, poDetUserDefField1, cust1InIntfc);
                    customData2 = GetUserDefInfo(poNum, lineNum, skidId, customData2, poDetUserDefField2, cust2InIntfc);
                    customData3 = GetUserDefInfo(poNum, lineNum, skidId, customData3, poDetUserDefField3, cust3InIntfc);
                    customData4 = GetUserDefInfo(poNum, lineNum, skidId, customData4, poDetUserDefField4, cust4InIntfc);
                    customData5 = GetUserDefInfo(poNum, lineNum, skidId, customData5, poDetUserDefField5, cust5InIntfc);
                    */


                    //rec.CREATE_DATETIME = DateTime.Now;
                    rec.ID = ascLibrary.ascUtils.ascStrToInt(dr["ID"].ToString(), -1);
                    rec.FACILITY = hostSiteId;
                    rec.RECEIVER_NO = dr["RECEIVER_ID"].ToString();
                    rec.LINE_NUMBER = ascLibrary.ascUtils.ascStrToInt(lineNum, 0);
                    rec.ORDER_TYPE = orderType;
                    rec.PONUMBER = poNum;  //changed 09-20-16 (JXG) for Driscoll's
                    rec.RELEASENUM = relNum;
                    rec.VENDOR_ITEM_ID = vendItemId;
                    rec.QTY_RECEIVED = qty;
                    rec.PRODUCT_CODE = itemId;
                    rec.BIN_LOCATION = locId;
                    rec.STATUS = rxLineStatus;  //string.Empty;  //changed 07-17-15 (JXG) for Driscoll's
                    rec.PACKING_SLIP_NBR = packSlip;
                    rec.CARRIER_NAME = carrier;
                    rec.RECEIVED_DATE = receiveDate;
                    rec.RECV_OPR = userId;
                    //added 07-17-15 (JXG) for Driscoll's
                    rec.PROD_LINE = prodLine;
                    rec.VENDOR_ID = vendId;
                    /////////////////////////////////////

                    if (rxStatus == "C")
                        rec.CLOSED_PO_FLAG = "C";

                    if (!String.IsNullOrEmpty(expDate))
                        rec.EXPIRED_DATE_BY_LOT = ascLibrary.ascUtils.ascStrToDate(expDate, DateTime.MinValue);

                    if (!String.IsNullOrEmpty(prodDate))
                        rec.PROD_DATE = ascLibrary.ascUtils.ascStrToDate(prodDate, DateTime.MinValue);

                    if (!String.IsNullOrEmpty(billUom) || dualUnitItem)
                    {
                        rec.CW_UOM = billUom;
                        rec.CW_QTY = qtyDualUnit;
                    }
                    rec.UOM = buyUom;

                    if (!String.IsNullOrEmpty(lotId))
                        rec.LOTID = lotId;
                    if (!String.IsNullOrEmpty(altLotId))
                        rec.ALT_LOTID = altLotId;

                    // Header custom data
                    rec.CUSTOM_HDRDATA1 = customHdrData1;
                    rec.CUSTOM_HDRDATA2 = customHdrData2;
                    rec.CUSTOM_HDRDATA3 = customHdrData3;
                    rec.CUSTOM_HDRDATA4 = customHdrData4;
                    rec.CUSTOM_HDRDATA5 = customHdrData5;
                    rec.CUSTOM_HDRDATA6 = customHdrData6;
                    rec.CUSTOM_HDRDATA7 = customHdrData7;
                    rec.CUSTOM_HDRDATA8 = customHdrData8;


                    // Detail custom data
                    //if (cust1InIntfc && !String.IsNullOrEmpty(customData1))
                    if (!String.IsNullOrEmpty(customData1))
                        rec.CUSTOM_DATA1 = customData1;
                    //if (cust2InIntfc && !String.IsNullOrEmpty(customData2))
                    if (!String.IsNullOrEmpty(customData2))
                        rec.CUSTOM_DATA2 = customData2;
                    //if (cust3InIntfc && !String.IsNullOrEmpty(customData3))
                    if (!String.IsNullOrEmpty(customData3))
                        rec.CUSTOM_DATA3 = customData3;
                    //if (cust4InIntfc && !String.IsNullOrEmpty(customData4))
                    if (!String.IsNullOrEmpty(customData4))
                        rec.CUSTOM_DATA4 = customData4;
                    //if (cust5InIntfc && !String.IsNullOrEmpty(customData5))
                    if (!String.IsNullOrEmpty(customData5))
                        rec.CUSTOM_DATA5 = customData5;

                    //if (custIdInIntfc)
                    {
                        if (!string.IsNullOrEmpty(tranCustId))  //added 10-12-16 (JXG) for Driscoll's
                            rec.CUST_ID = tranCustId;
                        else
                            rec.CUST_ID = vmiCustId;
                    }

                    SetPosted(rec.PONUMBER, rec.RELEASENUM, rec.LINE_NUMBER, rec.PRODUCT_CODE, rec.LOTID, rec.RECEIVER_NO, string.Empty, "S");
                    aData.Add(rec);
                }
                if (retval == HttpStatusCode.OK)
                    myClass.myParse.Globals.mydmupdate.ProcessUpdates();
            }
            finally
            {
                dr.Close();
                cmd.Dispose();
                conn.Close();
                conn.Dispose();
            }
            return (retval);
        }


        public static HttpStatusCode updateExportPOLines(Class1 myClass, List<ASCTracInterfaceModel.Model.PO.POExportLines> aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            try
            {
                var myExport = new ExportPOLines(myClass);
                retval = myExport.DoUpdateExportPOLines(aData, ref errmsg);
            }
            catch (Exception ex)
            {
                myClass.LogException(ex);
                retval = HttpStatusCode.BadRequest;
                errmsg = ex.Message;
            }
            return (retval);

        }

        private HttpStatusCode DoUpdateExportPOLines(List<ASCTracInterfaceModel.Model.PO.POExportLines> aData, ref string errmsg)
        {
            myClass.myParse.Globals.mydmupdate.InitUpdate();

            HttpStatusCode retval = HttpStatusCode.OK;
            foreach( var rec in aData)
            {
                string posted = "T";
                if (!rec.SUCCESSFUL)
                    posted = "E";
                SetPosted(rec.PONUMBER, rec.RELEASENUM, rec.LINE_NUMBER, rec.PRODUCT_CODE, rec.LOTID, rec.RECEIVER_NO, rec.ERROR_MESSAGE, posted);
            }
            if (retval == HttpStatusCode.OK)
                myClass.myParse.Globals.mydmupdate.ProcessUpdates();

            return (retval);
        }

        private void SetPosted(string aPONUMBER, string aRELEASENUM, long aLINE_NUMBER, string aPRODUCT_CODE, string aLOTID, string aRECEIVER_NO, string aERROR_MESSAGE,  string aPostedflag)
        {
            int msgLen = Convert.ToInt32(myClass.myParse.Globals.myDBUtils.getfieldsize("TRANFILE", "ERR_MESSAGE"));
            string shortErrorMessage = aERROR_MESSAGE;
            if (shortErrorMessage.Length > msgLen)
                shortErrorMessage = aERROR_MESSAGE.Substring(0, msgLen);
            
            string sqlStr = "UPDATE TRANFILE";
            if( !aPostedflag.Equals("E" ))
                sqlStr += " SET " + currPOExportConfig.postedFlagField + "='" + aPostedflag + "', " + currPOExportConfig.posteddateField + "=GETDATE() ";
            else
                sqlStr += " SET " + currPOExportConfig.postedFlagField + "='E', " + currPOExportConfig.posteddateField + "=GETDATE(), " +
                    "ERR_MESSAGE='" + shortErrorMessage.Replace("'", "''") + "', " +
                    "LONG_MESSAGE='" + aERROR_MESSAGE.Replace("'", "''") + "' ";

            sqlStr += " WHERE ORDERNUM='" + aPONUMBER + "' AND TRANTYPE IN ('RX', 'RF', 'RA') ";
            if (aPostedflag.Equals("S"))
                sqlStr += " AND ISNULL(" + currPOExportConfig.postedFlagField + ",'F') IN (" + currPOExportConfig.FilterPostedValues + ") ";
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

            if (!String.IsNullOrEmpty(aRECEIVER_NO))
                sqlStr += "AND RECEIVER_ID='" + aRECEIVER_NO + "' ";

            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
        }
    }
}
