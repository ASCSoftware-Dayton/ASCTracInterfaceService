using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Exports
{
    public class ExportTranfile
    {
        private static string funcType = "EX_TRAN";
        private static Class1 myClass;
        private static Model.Tranfile.TranfileExportConfig currExportConfig;

        public static HttpStatusCode doExportTranfile(ASCTracInterfaceModel.Model.TranFile.TranFileExportFilter aExportfilter, ref List<ASCTracInterfaceModel.Model.TranFile.TranfileExport> aData, ref string errmsg)
        {
            myClass = Class1.InitParse(funcType);
            HttpStatusCode retval = HttpStatusCode.OK;
            aData = new List<ASCTracInterfaceModel.Model.TranFile.TranfileExport>();
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
                        currExportConfig = Configs.TranfileConfig.getExportSite("1", myClass.myParse.Globals);
                        sqlstr = BuildExportSQL(aExportfilter, ref errmsg);
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

        private static string BuildExportSQL(ASCTracInterfaceModel.Model.TranFile.TranFileExportFilter aExportFilter, ref string errmsg)
        {
            string postedFlagField = currExportConfig.postedFlagField;
            string sqlStr = "SELECT SITES.HOST_SITE_ID, ITEMMSTR.VMI_CUSTID, ITEMMSTR.STOCK_UOM, TRANFILE.* " +
                "FROM TRANFILE (NOLOCK) " +
                "LEFT JOIN SITES (NOLOCK) ON SITES.SITE_ID=TRANFILE.SITE_ID " +
                "LEFT JOIN ITEMMSTR (NOLOCK) ON ITEMMSTR.ASCITEMID=TRANFILE.ASCITEMID " +
                "WHERE ((TRANFILE.TRANTYPE IN ('SC','DI','IP','R2','HS','RP','RW','RK','AD','AH','RY','TZ')) ";  //added TZ 11-07-13 (JXG)

            if (currExportConfig.exportUnreceivesAsInvAdj)
                sqlStr += "OR (TRANFILE.TRANTYPE IN ('RX','RF','RA') AND TRANFILE.QTY < 0) ";

            sqlStr += "OR (TRANFILE.TRANTYPE='RA' AND TRANFILE.SUBTRANTYPE='I') " +
                "OR (TRANFILE.TRANTYPE='DR' AND (SELECT RMA_NUM FROM RMAHDR RH " +
                "WHERE RH.RMA_NUM=TRANFILE.ORDERNUM AND RH.SITE_ID=TRANFILE.SITE_ID) IS NULL)) " +
                "AND SITES.HOST_SITE_ID<>'' AND ISNULL(TRANFILE." + postedFlagField + ",'F')='F' ";

            if (!String.IsNullOrEmpty(aExportFilter.CustID))
                sqlStr += "AND ITEMMSTR.VMI_CUSTID='" + aExportFilter.CustID + "' ";
            if (!String.IsNullOrEmpty(aExportFilter.ExcludeTranType))
                sqlStr += " and NOT TRANFILE.TRANTYPE IN ( '" + aExportFilter.ExcludeTranType.Replace(",", "','") + "')";

            Utils.FilterUtils.AppendToExportFilter(ref sqlStr, aExportFilter.ExportFilterList, "TRANFILE", "SITES|ITEMMSTR");

            sqlStr += "ORDER BY SITES.HOST_SITE_ID, TRANFILE.ID";
            return (sqlStr);
        }

        private static string BuildWhereStr(ASCTracInterfaceModel.Model.ModelExportFilter rec)
        {
            string retval = string.Empty;
            if (myClass.myParse.Globals.myDBUtils.IfFieldExists("TRANFILE", rec.Fieldname))
                retval = ascLibrary.ascStrUtils.buildwherestr(rec.Fieldname, rec.FilterType.ToString(), rec.Startvalue, rec.Endvalue);
            return (retval);
        }

        private static HttpStatusCode BuildExportList(string sqlstr, ref List<ASCTracInterfaceModel.Model.TranFile.TranfileExport> aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.NotFound;
            SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString);
            SqlCommand cmd = new SqlCommand(sqlstr, conn);
            conn.Open();
            SqlDataReader drTrans = cmd.ExecuteReader();

            myClass.myParse.Globals.mydmupdate.InitUpdate();
            try
            {
                while (drTrans.Read())
                {
                    retval = HttpStatusCode.OK;

                    long recId = ascLibrary.ascUtils.ascStrToInt(drTrans["ID"].ToString(), 0);
                    string tranType = drTrans["TRANTYPE"].ToString();
                    string ascItemId = drTrans["ASCITEMID"].ToString();
                    double qty = ascLibrary.ascUtils.ascStrToDouble(drTrans["QTY"].ToString(), 0);
                    double qtyDualUnit = ascLibrary.ascUtils.ascStrToDouble(drTrans["QTY_DUAL_UNIT"].ToString(), 0);
                    string lotId = drTrans["LOTID"].ToString();
                    string skidId = drTrans["SKIDID"].ToString();
                    string lineNum = drTrans["LINENUM"].ToString();
                    string containerId = drTrans["INV_CONTAINER_ID"].ToString();  //added 07-12-13 (JXG)
                    string prodLine = drTrans["PRODUCTIONLINE"].ToString();  //added 07-17-15 (JXG) for Dricoll's
                    string comments = drTrans["COMMENTS"].ToString();
                    string stockUom = drTrans["STOCK_UOM"].ToString();  //added 06-28-16 (JXG)
                    string userId = drTrans["USERID"].ToString();  //added 06-28-16 (JXG)
                    string siteId = drTrans["SITE_ID"].ToString();  //added 03-27-17 (JXG)

                    string sqlStr;
                    //                        containerId = string.Empty;  //taken out 07-12-13 (JXG)
                    if (String.IsNullOrEmpty(containerId) && !String.IsNullOrEmpty(skidId))
                    {
                        sqlStr = "SELECT INV_CONTAINER_ID FROM LOCITEMS WHERE SKIDID='" + skidId + "'";
                        if (!myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref containerId))  //added 05-24-17 (JXG)
                        {
                            sqlStr = "SELECT INV_CONTAINER_ID FROM OLDLCITM WHERE (SKIDID ='" + skidId + "' " +
                                "OR SKIDID LIKE '" + skidId + "-%') ORDER BY ARCHIVE_DATE DESC";
                            myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref containerId);
                        }
                    }


                    if (String.IsNullOrEmpty(lotId) && !String.IsNullOrEmpty(skidId) && !skidId.StartsWith("-"))
                    {
                        sqlStr = "SELECT LOTID FROM LOCITEMS WHERE SKIDID='" + skidId + "'";
                        if (!myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref lotId))
                        {
                            sqlStr = "SELECT LOTID FROM OLDLCITM WHERE (SKIDID ='" + skidId + "' " +
                                "OR SKIDID LIKE '" + skidId + "-%') ORDER BY ARCHIVE_DATE DESC";
                            myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref lotId);
                        }
                    }

                    string recvDatetime = "";
                    string prodDatetime = "";
                    string vendorId = "";
                    string tranRecvrId = "";
                    string altLotId = "";
                    string altSkidId = "";
                    if (!String.IsNullOrEmpty(skidId) && !skidId.StartsWith("-"))  //added 08-04-15 (JXG) for Driscoll's
                    {
                        sqlStr = "SELECT RECVDATETIME, DATETIMEPROD, RECVVENDORID, RECEIVER_ID, ALT_LOTID, ALT_SKIDID FROM LOCITEMS WHERE SKIDID='" + skidId + "'";
                        if (!myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref altSkidId))
                        {
                            sqlStr = "SELECT RECVDATETIME, DATETIMEPROD, RECVVENDORID, RECEIVER_ID, ALT_LOTID, ALT_SKIDID FROM OLDLCITM WHERE (SKIDID ='" + skidId + "' " +
                                "OR SKIDID LIKE '" + skidId + "-%') ORDER BY ARCHIVE_DATE DESC";
                            myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref altSkidId);
                        }
                        recvDatetime = ascLibrary.ascStrUtils.GetNextWord(ref altSkidId);
                        prodDatetime = ascLibrary.ascStrUtils.GetNextWord(ref altSkidId);
                        vendorId = ascLibrary.ascStrUtils.GetNextWord(ref altSkidId);
                        tranRecvrId = ascLibrary.ascStrUtils.GetNextWord(ref altSkidId);
                        altLotId = ascLibrary.ascStrUtils.GetNextWord(ref altSkidId);
                    }

                    if (tranType == "SC" || tranType == "DI")
                    {
                        qty = -qty;
                        qtyDualUnit = -qtyDualUnit;
                    }
                    else if (qty < 0 && qtyDualUnit > 0)
                        qtyDualUnit = -qtyDualUnit;

                    string tmpStr = string.Empty;
                    myClass.myParse.Globals.myGetInfo.GetASCItemInfo(ascItemId, "DUAL_UNIT_ITEM, BILL_UOM, CATID, CAT2ID, MFG_ID, STANDARDCOST", ref tmpStr);
                    bool dualUnitItem = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr).Equals("T");
                    string billUom = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                    string itemCatId = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);  //added 10-17-16 (JXG) for Driscoll's
                    string itemCat2Id = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);  //added 10-17-16 (JXG) for Driscoll's
                    string mfgId = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);  //added 03-24-17 (JXG)
                    string stdCost = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);  //added 03-24-17 (JXG)

                    string whseID = "";
                    if (drTrans["OLDLOCATION"].ToString() == "")
                        myClass.myParse.Globals.myGetInfo.GetLocInfo(drTrans["NEWLOCATION"].ToString(), "WHSE_ID", ref whseID);
                    else
                        myClass.myParse.Globals.myGetInfo.GetLocInfo(drTrans["OLDLOCATION"].ToString(), "WHSE_ID", ref whseID);

                    string fromLoc = "";
                    string toLoc = drTrans["NEWLOCATION"].ToString();  //added 05-11-17 (JXG)
                    if (tranType == "TZ")  //added 11-07-13 (JXG) for transfers between zones
                    {
                        fromLoc = drTrans["OLDLOCATION"].ToString();
                        toLoc = drTrans["NEWLOCATION"].ToString();
                    }
                    else  //added 08-04-16 (JXG) for Merisant
                    {
                        if (!string.IsNullOrEmpty(drTrans["OLDLOCATION"].ToString()))
                        {
                            myClass.myParse.Globals.myGetInfo.GetLocInfo(drTrans["OLDLOCATION"].ToString(), "MFLOCATION", ref fromLoc);
                        }
                        if (string.IsNullOrEmpty(fromLoc))
                            fromLoc = drTrans["OLDLOCATION"].ToString();
                    }

                    ASCTracInterfaceModel.Model.TranFile.TranfileExport rec = new ASCTracInterfaceModel.Model.TranFile.TranfileExport();
                    rec.CREATE_DATETIME = ascLibrary.ascUtils.ascStrToDate(drTrans["TRANDATE"].ToString(), DateTime.Now);
                    rec.FACILITY = drTrans["HOST_SITE_ID"].ToString();
                    rec.PRODUCT_CODE = drTrans["ITEMID"].ToString();
                    rec.QUANTITY = qty;
                    rec.DATE_TIME = ascLibrary.ascUtils.ascStrToDate(drTrans["BILLING_DATE"].ToString(), rec.CREATE_DATETIME);
                    rec.TRANS_TYPE = tranType;
                    string reasoncode = drTrans["REASON"].ToString();
                    if (myClass.myParse.Globals.myGetInfo.GetReasonCodeInfo(reasoncode, "HOST_REASON", ref tmpStr))
                    {
                        if (!String.IsNullOrEmpty(tmpStr))
                            reasoncode = tmpStr;
                    }
                    rec.REASON_CODE = reasoncode;
                    rec.SKIDID = skidId;
                    rec.LOTID = lotId;
                    rec.REF_NUMBER = drTrans["ACCTNUM"].ToString();
                    rec.EXPDATE = ascLibrary.ascUtils.ascStrToDate(drTrans["EXPDATE"].ToString(), DateTime.MinValue);
                    rec.VMI_CUSTID = drTrans["VMI_CUSTID"].ToString();
                    rec.CONTAINER_ID = containerId.ToString();
                    rec.WHSE_ID = whseID;
                    rec.FROM_LOCATION = fromLoc;  //added 11-07-13 (JXG)
                    rec.TO_LOCATION = toLoc;  //added 11-07-13 (JXG)
                    rec.PROD_LINE = prodLine;  //added 07-17-15 (JXG) for Driscoll's
                    rec.COMMENTS = comments;
                    //added 08-04-15 (JXG) for Driscoll's
                    if (!String.IsNullOrEmpty(recvDatetime))
                        rec.RECVDATETIME = ascLibrary.ascUtils.ascStrToDate(recvDatetime, DateTime.MinValue);
                    if (!String.IsNullOrEmpty(prodDatetime))
                        rec.DATETIMEPROD = ascLibrary.ascUtils.ascStrToDate(prodDatetime, DateTime.MinValue);
                    if (!String.IsNullOrEmpty(vendorId))
                        rec.VENDORID = vendorId;

                    /////////////////////////////////////
                    if (!String.IsNullOrEmpty(tranRecvrId))
                        rec.RECEIVER_NO = tranRecvrId;
                    if (!String.IsNullOrEmpty(stockUom))
                        rec.STOCK_UOM = stockUom;
                    if (!String.IsNullOrEmpty(userId))
                        rec.USERID = userId;
                    if (!String.IsNullOrEmpty(altLotId))
                        rec.ALT_LOTID = altLotId;
                    if (!String.IsNullOrEmpty(altSkidId))
                        rec.ALT_SKIDID = altSkidId;
                    if (!String.IsNullOrEmpty(itemCatId))
                        rec.CATEGORY = itemCatId;
                    if (!String.IsNullOrEmpty(itemCat2Id))
                        rec.CATEGORY_2 = itemCat2Id;
                    if (!String.IsNullOrEmpty(mfgId))
                        rec.MFG_ID = mfgId;

                    if (!String.IsNullOrEmpty(stdCost))
                        rec.COST_EACH = ascLibrary.ascUtils.ascStrToDouble(stdCost, 0);


                    if (!String.IsNullOrEmpty(billUom) || dualUnitItem)
                    {
                        rec.CW_UOM = billUom;
                        rec.CW_QTY = qtyDualUnit;
                    }

                    string orderNum = drTrans["ORDERNUM"].ToString();
                    orderNum = (orderNum.StartsWith("ASN-")) ? orderNum.Substring(4) : orderNum;
                    rec.ORDERNUMBER = orderNum;

                    if (!String.IsNullOrEmpty(lineNum))
                        rec.LINE_NUMBER = ascLibrary.ascUtils.ascStrToDouble( lineNum, 0);

                    if ((tranType == ascLibrary.dbConst.ltPRODUCTION) || (tranType == ascLibrary.dbConst.ltISSUE))
                    {
                        myClass.myParse.Globals.myGetInfo.GetWOHdrInfo(orderNum, "ORDER_SOURCE", ref tmpStr);
                        if (tmpStr == "S")
                            rec.SPLIT_ORDER_FLAG = "Split";
                        else
                        {
                            myClass.myParse.Globals.myDBUtils.ReadFieldFromDB("select count( workorder_id) from wo_hdr where workorder_id like '" + orderNum + "%' and ORDER_SOURCE='S'", "", ref tmpStr);
                            if (ascLibrary.ascUtils.ascStrToInt(tmpStr, 0) > 0)
                                rec.SPLIT_ORDER_FLAG = "Original";
                            else
                                rec.SPLIT_ORDER_FLAG = "Not Split";
                        }
                    }

                    if (myClass.myParse.Globals.myGetInfo.GetLotInfo(ascItemId, lotId, "CA_FILENAME", ref tmpStr))
                    {
                        if (!String.IsNullOrEmpty(tmpStr))
                            rec.CA_FILENAME = tmpStr;
                    }

                    if (!String.IsNullOrEmpty(skidId)
                        && !skidId.StartsWith("-"))  //added 05-24-17 (JXG)
                    {
                        tmpStr = "";  //added 05-24-17 (JXG)
                        sqlStr = "SELECT CUSTOM_DATA1, CUSTOM_DATA2, CUSTOM_DATA3, CUSTOM_DATA4, " +
                            "CUSTOM_DATA5, CUSTOM_DATA6, CUSTOM_NUM1, CUSTOM_DATE " +
                            "FROM LOCITEMS (NOLOCK) " +
                            "WHERE SKIDID='" + skidId + "'";
                        if (!myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref tmpStr))  //added 05-24-17 (JXG)
                        {
                            sqlStr = "SELECT CUSTOM_DATA1, CUSTOM_DATA2, CUSTOM_DATA3, CUSTOM_DATA4, " +
                                "CUSTOM_DATA5, CUSTOM_DATA6, CUSTOM_NUM1, CUSTOM_DATE " +
                                "FROM OLDLCITM WHERE (SKIDID ='" + skidId + "' " +
                                "OR SKIDID LIKE '" + skidId + "-%') ORDER BY ARCHIVE_DATE DESC";
                            myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref tmpStr);
                        }
                        rec.CUSTOM_DATA1 = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                        rec.CUSTOM_DATA2 = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                        rec.CUSTOM_DATA3 = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                        rec.CUSTOM_DATA4 = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                        rec.CUSTOM_DATA5 = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                        rec.CUSTOM_DATA6 = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                        rec.CUSTOM_NUM1 = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0);

                        rec.CUSTOM_DATE = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), DateTime.MinValue);
                    }

                    if (!String.IsNullOrEmpty(drTrans["COST_CENTER_ID"].ToString()))
                        rec.COST_CENTER_ID = drTrans["COST_CENTER_ID"].ToString();
                    if (!String.IsNullOrEmpty(drTrans["RESP_SITE_ID"].ToString()))
                        rec.RESP_SITE_ID = drTrans["RESP_SITE_ID"].ToString();

                    rec.TRANFILE_REC_ID = recId;

                    SetPosted(recId.ToString(), "", "S");

                    aData.Add(rec);

                }
                if (retval == HttpStatusCode.OK)
                    myClass.myParse.Globals.mydmupdate.ProcessUpdates();

            }
            finally
            {
                drTrans.Close();
                cmd.Dispose();
                conn.Close();
                conn.Dispose();
            }
            return (retval);
        }

        public static HttpStatusCode UpdateExport(List<ASCTracInterfaceModel.Model.TranFile.TranfileExport> aData, ref string errmsg)
        {
            myClass = Class1.InitParse("Update" + funcType);
            HttpStatusCode retval = HttpStatusCode.OK;
            string OrderNum = string.Empty;
            string sqlstr = string.Empty;
            try
            {
                if (myClass != null)
                {
                    myClass.myParse.Globals.mydmupdate.InitUpdate();
                    currExportConfig = Configs.TranfileConfig.getExportSite("1", myClass.myParse.Globals);
                    retval = DoUpdateExport(aData, ref errmsg);
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

        public static HttpStatusCode DoUpdateExport(List<ASCTracInterfaceModel.Model.TranFile.TranfileExport> aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            foreach (var rec in aData)
            {
                string posted = "T";
                if (!rec.SUCCESSFUL)
                    posted = "E";
                SetPosted(rec.TRANFILE_REC_ID.ToString(), rec.ERROR_MESSAGE, posted);
            }
            return (retval);
        }


        private static void SetPosted(string recId, string aERROR_MESSAGE, string aPostedflag)
        {
            int msgLen = Convert.ToInt32(myClass.myParse.Globals.myDBUtils.getfieldsize("TRANFILE", "ERR_MESSAGE"));
            string sqlStr = "UPDATE TRANFILE";
            if (!aPostedflag.Equals("E"))
                sqlStr += " SET " + currExportConfig.postedFlagField + "='" + aPostedflag + " ', " + currExportConfig.posteddateField + "=GETDATE() ";
            else
                sqlStr = " SET " + currExportConfig.postedFlagField + "='E', " + currExportConfig.posteddateField + "=GETDATE(), " +
                    "ERR_MESSAGE='" + aERROR_MESSAGE.Substring(0, msgLen).Replace("'", "''") + "', " +
                    "LONG_MESSAGE='" + aERROR_MESSAGE.Replace("'", "''") + "' ";
            sqlStr += " WHERE ID = " + recId;
            if (aPostedflag.Equals("S"))
                sqlStr += " AND ISNULL(" + currExportConfig.postedFlagField + ",'F') = 'F'";
            else
                sqlStr += " AND ISNULL(" + currExportConfig.postedFlagField + ",'F') = 'S'";
            //+" AND ISNULL(" + currExportConfig.postedFlagField + "','F') NOT IN ( 'T', 'X', 'D', 'E', 'P', '" + aPostedflag + "')";

            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
        }

    }
}