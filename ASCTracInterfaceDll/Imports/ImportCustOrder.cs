using ASCTracFunctions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Imports
{
    public class ImportCustOrder
    {
        private static string funcType = "IM_ORDER";
        private static string siteid = string.Empty;
        private static Class1 myClass;
        private static Model.CustOrder.COImportConfig currCOImportConfig;
        public static HttpStatusCode doImportCustOrder(ASCTracInterfaceModel.Model.CustOrder.OrdrHdrImport aData, ref string errmsg)
        {
            myClass = Class1.InitParse(funcType, ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            string OrderNum = aData.ORDERNUMBER;
            string updstr = string.Empty;
            try
            {
                if (myClass != null)
                {
                    if (!myClass.FunctionAuthorized(funcType))
                        retval = HttpStatusCode.NonAuthoritativeInformation;
                    else
                    {
                        siteid = myClass.GetSiteIdFromHostId(aData.FACILITY);
                        currCOImportConfig = Configs.CustOrderConfig.getCOImportSite(siteid, myClass.myParse.Globals);
                        if (String.IsNullOrEmpty(siteid))
                        {
                            errmsg = "No Facility or Site defined for record.";
                            retval = HttpStatusCode.BadRequest;
                        }
                        retval = ImportCORecord(aData, ref errmsg);
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

        private static HttpStatusCode ImportCORecord(ASCTracInterfaceModel.Model.CustOrder.OrdrHdrImport aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            string pickstatus = string.Empty;
            bool fExist = false;
            string orderNum = aData.ORDERNUMBER;
            string batchNum = string.Empty;
            if (myClass.myParse.Globals.myGetInfo.GetOrderInfo(orderNum, "BATCH_NUM, PICKSTATUS", ref pickstatus))
            {
                batchNum = ascLibrary.ascStrUtils.GetNextWord(ref pickstatus);
                fExist = true;
                if (pickstatus.Equals(ascLibrary.dbConst.ssCONF_SHIP))
                    errmsg = "Order " + orderNum + " already Shipped.";
                else if (pickstatus.Equals(ascLibrary.dbConst.ssCANCELLED))
                    errmsg = "Order " + orderNum + " already Cancelled.";
                else if (myClass.myParse.Globals.myDBUtils.ifRecExists("SELECT top 1 ORDERNUMBER FROM PICK_ASSIGNMENTS WHERE ORDERNUMBER='" + orderNum + "'"))
                {
                    errmsg = "Order " + orderNum + " has external Picks, and cannot be changed";
                }
            }
            if (String.IsNullOrEmpty(errmsg))
            {
                string importAction = aData.STATUS_FLAG;
                //if( currCOImportConfig.GWPurgeCODetOnImport)
                //    PurgePODet
                myClass.myParse.Globals.mydmupdate.InitUpdate();

                if (importAction == "D" || importAction == "V")
                {
                    DeleteOrder(orderNum, ref errmsg);
                }
                else if (importAction == "C" || importAction == "X")
                {
                    RemoveOrderFromGroups(orderNum);

                    CancelOrder(orderNum, true);
                }
                else
                {
                    bool fOK = true;
                    if (fExist)
                        fOK = PurgeOrderDet(orderNum, ref errmsg);  //moved PurgeOrderDet to after ImportOrderHdr 08-02-16 (JXG)
                    if (fOK)
                    {
                        if (ImportOrderHdr(orderNum, pickstatus, fExist, aData, ref errmsg))
                        {


                            bool fPostit = false;
                            if (ImportOrderDet(orderNum, pickstatus, aData.ORDER_TYPE, aData, ref errmsg))
                            {
                                myClass.myParse.Globals.mydmupdate.ProcessUpdates();
                                fPostit = true;
                                if (currCOImportConfig.GWEngageBatchPickingOnImport)
                                {
                                    if (!String.IsNullOrEmpty(batchNum))
                                        SetupOrderBatch(orderNum, batchNum);
                                }

                                if (currCOImportConfig.GWCreateTransferPOFromCO)
                                    CreateTransferPOFromCO(orderNum, aData);
                            }
                            else
                            {
                                if (currCOImportConfig.GWLineErrorHandling == "H")
                                {
                                    string sql = "UPDATE ORDRHDR SET PICKSTATUS='K' " +
                                        "WHERE ORDERNUMBER='" + orderNum + "'";
                                    myClass.myParse.Globals.myDBUtils.RunSqlCommand(sql);
                                }
                            }

                            if (fPostit)
                            {
                                //moved from inside of if (ImportOrderDet) 08-09-16 (JXG) for Driscoll's
                                //MiscFuncs.SetPCE(orderNum);
                                myClass.myParse.Globals.dmMiscOrder.setpcefororder(orderNum);
                                myClass.myParse.Globals.dmMiscOrder.SetOrderSubUOMQty(orderNum, string.Empty);
                                SetOrderTotalValues(orderNum);
                                ////////////////////////////////////////////////////////////////////////

                                //ImportOrderLotAlloc(orderNum);
                                SetPickAssignments(orderNum);
                                if (!String.IsNullOrEmpty(orderNum) && myClass.myParse.Globals.myGetInfo.GetOrderInfo(orderNum, "ORDERNUMBER", ref orderNum))
                                    AfterOrderImport(orderNum, !fExist);
                            }
                        }
                        else
                        {
                            if (currCOImportConfig.GWLineErrorHandling == "H")
                            {
                                var sql = "UPDATE ORDRHDR SET PICKSTATUS='K' " +
                                    "WHERE ORDERNUMBER='" + orderNum + "'";
                                myClass.myParse.Globals.myDBUtils.RunSqlCommand(sql);
                            }
                        }

                    }
                }

                if (string.IsNullOrEmpty(errmsg))
                {
                    myClass.myParse.Globals.mydmupdate.ProcessUpdates();
                }

            }
            return (retval);
        }

        private static bool isOrderStatusScheduled(string pickStatus)
        {
            return (pickStatus == "S" || pickStatus == "B" || pickStatus == "D" ||
                    pickStatus == "E" || pickStatus == "P" || pickStatus == "O" ||
                    pickStatus == "L" || pickStatus == "G");
        }

        private static bool isOrderStatusRequired(string pickStatus)
        {
            return (pickStatus == "N" || pickStatus == "I" || pickStatus == "W" ||
                    pickStatus == "H" || pickStatus == "U");
        }

        private static void RemoveOrderFromGroups(string orderNum)
        {
            string batchnum = string.Empty;
            if (myClass.myParse.Globals.myGetInfo.GetOrderInfo(orderNum, "WAVE_NUM, BATCH_NUM", ref batchnum))
            {
                var wavenum = ascLibrary.ascStrUtils.GetNextWord(ref batchnum);
                if (!String.IsNullOrEmpty(wavenum))
                {
                    myClass.myParse.Globals.mydmupdate.AddToUpdate("UPDATE ASN_LABELS SET LOAD_GROUP_NUM=NULL WHERE ORDERNUM = '" + orderNum + "'");
                }
                if (!String.IsNullOrEmpty(batchnum))
                {
                    string sql = "SELECT B.ASCITEMID, B.QTY_TO_PICK, B.QTY_PICKED, SUM( D.QTYORDERED) AS DETAIL_QTY" +
                        " FROM BATPKDET B" +
                        " JOIN ORDRDET D ON D.ASCITEMID = B.ASCITEMID AND D.ORDERNUMBER = '" + orderNum + "'" +
                        " WHERE B.BATCH_NUM = '" + batchnum + "'" +
                        " GROUP BY B.ASCITEMID, B.QTY_TO_PICK, B.QTY_PICKED";
                    using (SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        conn.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                double qtyToPick = ascLibrary.ascUtils.ascStrToDouble(dr["QTY_TO_PICK"].ToString(), 0);
                                double qtyPicked = ascLibrary.ascUtils.ascStrToDouble(dr["QTY_PICKED"].ToString(), 0);
                                double qtyOnOrder = ascLibrary.ascUtils.ascStrToDouble(dr["DETAIL_QTY"].ToString(), 0);
                                if (qtyPicked <= 0) // if anything picked, then dont do anything
                                {
                                    if (qtyOnOrder >= qtyToPick)
                                        myClass.myParse.Globals.mydmupdate.AddToUpdate("DELETE BATPKDET WHERE BATCH_NUM='" + batchnum + "' AND ASCITEMID='" + dr["ASCITEMID"].ToString() + "'");
                                    else
                                        myClass.myParse.Globals.mydmupdate.AddToUpdate("update BATPKDET SET QTY_TO_PICK=QTY_TO_PICK-" + qtyOnOrder.ToString() + " WHERE BATCH_NUM='" + batchnum + "' AND ASCITEMID='" + dr["ASCITEMID"].ToString() + "'");
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool CancelOrder(string orderNum, bool aUpdateHdr)
        {
            bool retval = false;
            string pickStatus = string.Empty;

            if (myClass.myParse.Globals.myGetInfo.GetOrderInfo(orderNum, "ORDERTYPE, PICKSTATUS", ref pickStatus))
            {
                var ordertype = ascLibrary.ascStrUtils.GetNextWord(ref pickStatus);
                if (pickStatus != ascLibrary.dbConst.ssCONF_SHIP)
                {
                    retval = CheckIfPicked(orderNum);
                    if (retval)
                    {
                        myClass.myParse.Globals.dmMiscFunc.BackoutPreallocation("", orderNum);

                        // Decrement item quantities
                        // Decrement item quantities
                        if (!ascLibrary.dbConst.otNOT_FOR_SCHEDING.Contains(ordertype))
                        {
                            if (isOrderStatusScheduled(pickStatus))
                                myClass.myParse.Globals.mydmupdate.SetItemMasterQtyScheduledForCustOrder(orderNum, true);
                            else if (isOrderStatusRequired(pickStatus))
                                myClass.myParse.Globals.mydmupdate.SetItemMasterQtyRequiredForCustOrder(orderNum, true);
                        }

                        if (aUpdateHdr)
                        {
                            string sql = "UPDATE ORDRHDR SET PICKSTATUS='X', ORDERFILLED='C', " +
                                "WAVE_NUM=NULL, BATCH_NUM=NULL," +
                                "CUSTOM_DATA10='01'," +
                                "EXPORT='F' " +  //added 04-17-14 (JXG) for prairie farms
                                "WHERE ORDERNUMBER='" + orderNum + "'";
                            myClass.myParse.Globals.mydmupdate.AddToUpdate(sql);
                        }

                        //added 09-24-15 for Didion
                        //if create work order from cust order detail,
                        //cancel any work orders that are linked to this cust order
                        if (myClass.myParse.Globals.myConfig.vmProduction.boolValue && currCOImportConfig.isActiveWOFROMCO)
                        {
                            string sql = "SELECT H.PROD_ASCITEMID, SUM( H.QTY_TO_MAKE - H.CUR_QTY) AS QTYTOMAKE" +
                                " FROM WO_HDR H " +
                                " WHERE H.LINKED_CO_NUM='" + orderNum + "' " +
                                " AND H.STATUS IN ('N', 'S', 'P', 'A', 'D')" +
                                " GROUP BY H.PROD_ASCITEMID";

                            using (SqlConnection conn2 = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
                            using (SqlCommand cmd2 = new SqlCommand(sql, conn2))
                            {
                                conn2.Open();
                                using (SqlDataReader reader2 = cmd2.ExecuteReader())
                                {
                                    while (reader2.Read())
                                    {
                                        string prodASCItemID = reader2["PROD_ASCITEMID"].ToString();
                                        double qtytomake = ascLibrary.ascUtils.ascStrToDouble(reader2["QTYTOMAKE"].ToString(), 0);
                                        if (qtytomake > 0)
                                            myClass.myParse.Globals.mydmupdate.updateqty("ITEMQTY", "QTY_TO_PRODUCE", "ASCITEMID='" + prodASCItemID + "'", -qtytomake);
                                    }
                                }
                            }


                            sql = "SELECT H.STATUS,D.COMP_ASCITEMID, SUM( D.QTY - D.QTY_PICKED) AS QTYTOPICK" +
                                " FROM WO_HDR H " +
                                " JOIN WO_DET D ON D.WORKORDER_ID=H.WORKORDER-ID" +
                                " WHERE H.LINKED_CO_NUM='" + orderNum + "' " +
                                " AND H.STATUS NOT IN ('X', 'F')" +
                                " GROUP BY H.STATUS, D.COMP_ASCITEMID";

                            using (SqlConnection conn2 = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
                            using (SqlCommand cmd2 = new SqlCommand(sql, conn2))
                            {
                                conn2.Open();
                                using (SqlDataReader reader2 = cmd2.ExecuteReader())
                                {
                                    while (reader2.Read())
                                    {
                                        string wostatus = reader2["STATUS"].ToString();
                                        string compASCItemID = reader2["COMP_ASCITEMID"].ToString();
                                        double qtytopick = ascLibrary.ascUtils.ascStrToDouble(reader2["QTYTOPICK"].ToString(), 0);
                                        if (qtytopick > 0)
                                        {
                                            if (wostatus.Equals(ascLibrary.dbConst.plACTIVE) || wostatus.Equals(ascLibrary.dbConst.plPREPARING) || wostatus.Equals(ascLibrary.dbConst.plPENDING))
                                                myClass.myParse.Globals.mydmupdate.updateqty("ITEMQTY", "QTYSCHEDULED", "ASCITEMID='" + compASCItemID + "'", -qtytopick);
                                            else
                                                myClass.myParse.Globals.mydmupdate.updateqty("ITEMQTY", "QTYREQUIRED", "ASCITEMID='" + compASCItemID + "'", -qtytopick);

                                        }
                                    }
                                }
                            }

                            sql = "UPDATE H " +
                                    "SET H.STATUS='X' " +
                                    "FROM WO_HDR H " +
                                    "WHERE H.LINKED_CO_NUM='" + orderNum + "' " +
                                    "AND H.STATUS<>'X' AND H.CUR_QTY=0 " +
                                    "AND (SELECT COUNT(WORKORDER_ID) FROM WO_DET (NOLOCK) " +
                                    "WHERE WORKORDER_ID=H.WORKORDER_ID " +
                                    "AND (QTY_PICKED<>0 OR QTY_ALLOC<>0 OR QTY_USED<>0)) = 0";
                            myClass.myParse.Globals.mydmupdate.AddToUpdate(sql);
                        }

                        //added 10-05-15 for Driscoll's
                        //if create transfer po from transfer cust order,
                        //cancel any transfer pos that are linked to this cust order
                        if (currCOImportConfig.GWCreateTransferPOFromCO)
                        {
                            string sql = "SELECT H.PONUMBER, H.RELEASENUM, H.RECEIVED " +
                                "FROM POHDR H " +
                                "WHERE H.LINKED_ORDERNUMBER='" + orderNum + "' " +
                                "AND H.TRANSFER_CO_ORDERNUMBER='" + orderNum + "' " +
                                "AND H.RECEIVED<>'X' AND H.ORDERTYPE='T' " +
                                "AND (SELECT COUNT(PONUMBER) FROM PODET (NOLOCK) " +
                                "WHERE PONUMBER=H.PONUMBER AND RELEASENUM=H.RELEASENUM " +
                                "AND (QTYRECEIVED<>0)) = 0";
                            using (SqlConnection conn2 = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
                            using (SqlCommand cmd2 = new SqlCommand(sql, conn2))
                            {
                                conn2.Open();
                                using (SqlDataReader reader2 = cmd2.ExecuteReader())
                                {
                                    while (reader2.Read())
                                    {
                                        string poNum = reader2["PONUMBER"].ToString();
                                        string relNum = reader2["RELEASENUM"].ToString();
                                        string recvStatus = reader2["RECEIVED"].ToString();
                                        if (recvStatus != "C")
                                        {
                                            // Decrement the item qty from expected
                                            myClass.myParse.Globals.mydmupdate.SetItemMasterQtyExpected(poNum, relNum, true);
                                        }
                                        sql = "UPDATE H " +
                                            "SET H.RECEIVED='X' " +
                                            "FROM POHDR H " +
                                            "WHERE H.PONUMBER='" + poNum + "' " +
                                            "AND H.RELEASENUM='" + relNum + "' ";
                                        myClass.myParse.Globals.mydmupdate.AddToUpdate(sql);
                                    }
                                }
                            }
                            myClass.myParse.Globals.mydmupdate.ProcessUpdates();

                        }
                    }
                }
            }
            return (retval);
        }

        private static bool PurgeOrderDet(string orderNum, ref string errmsg)
        {
            string tmpStr = string.Empty;

            if (currCOImportConfig.GWPurgeCODetOnImport)
            {
                string ordertype = string.Empty;
                if (myClass.myParse.Globals.myGetInfo.GetOrderInfo(orderNum, "ORDERTYPE", ref ordertype))
                {
                    bool okToPurge = false;
                    string sql = "SELECT PICKORDERNUM FROM LOCITEMS (NOLOCK) " +
                        "WHERE PICKORDERNUM='" + orderNum + "'";
                    if (!myClass.myParse.Globals.myDBUtils.ifRecExists(sql))
                    {
                        sql = "SELECT PICKORDERNUM FROM OLDLCITM (NOLOCK) " +
                            "WHERE PICKORDERNUM='" + orderNum + "'";
                        if (!myClass.myParse.Globals.myDBUtils.ifRecExists(sql))
                            okToPurge = true;
                    }

                    if (okToPurge)
                    {
                        if (!ascLibrary.dbConst.otNOT_FOR_SCHEDING.Contains(ordertype))
                        {
                            bool statusScheduled = isOrderStatusScheduled(orderNum);
                            bool statusRequired = isOrderStatusRequired(orderNum);

                            // Decrement item quantities
                            if (statusScheduled)
                                myClass.myParse.Globals.mydmupdate.SetItemMasterQtyScheduledForCustOrder(orderNum, true);
                            else if (statusRequired)
                                myClass.myParse.Globals.mydmupdate.SetItemMasterQtyRequiredForCustOrder(orderNum, true);
                        }

                        // It is OK to purge the detail records
                        sql = "DELETE FROM ORDRDET WHERE ORDERNUMBER='" + orderNum + "'";
                        myClass.myParse.Globals.mydmupdate.AddToUpdate(sql);

                        sql = "DELETE FROM PCEPICKING WHERE RECID='" + orderNum + "' ";
                        myClass.myParse.Globals.mydmupdate.AddToUpdate(sql); myClass.myParse.Globals.mydmupdate.ProcessUpdates();
                    }

                    else
                    {
                        errmsg = "Error Importing Order# " + orderNum + ": Cannot purge order lines " +
                            "because order has inventory picked to it.";
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool CheckIfPicked( string orderNum)
        {
            bool okToDelete = false;
            if (currCOImportConfig.GWAllowCancelOfPickedOrder)
                okToDelete = true;
            else
            {
                string tmpStr = string.Empty;
                string sqlStr = "SELECT SUM(QTYPICKED), SUM(QTY_SUBSTITUTED), SUM(QTYSHIPPED) " +
                    "FROM ORDRDET (NOLOCK) WHERE ORDERNUMBER='" + orderNum + "'";
                if (myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref tmpStr))
                {
                    double qtyPicked = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0);
                    double qtySub = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0);
                    double qtyShipped = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0);
                    if (qtyPicked == 0 && qtySub == 0 && qtyShipped == 0)
                        okToDelete = true;
                }
                else
                    okToDelete = true;
            }
            return (okToDelete);
        }

        private static bool DeleteOrder(string orderNum, ref string errmsg)
        {

            try
            {
                if (CancelOrder(orderNum, false))
                {
                    bool okToDelete = CheckIfPicked(orderNum);

                    if (okToDelete)
                    {
                        CancelOrder(orderNum, false);
                        string sqlStr = "DELETE FROM ORDRDET WHERE ORDERNUMBER='" + orderNum + "'";
                        myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
                        sqlStr = "DELETE FROM ORDRHDR WHERE ORDERNUMBER='" + orderNum + "'";
                        myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
                    }
                    else
                    {
                        errmsg = "Error deleting order# " + orderNum + ": Cannot delete order that is already picked.";
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                errmsg = "Error deleting order# " + orderNum + ": " + e.Message;
                return false;
            }
            return true;
        }

        private static void DeleteWorkOrder(string woNum)
        {
            string woType = string.Empty;

            myClass.myParse.Globals.myGetInfo.GetWOHdrInfo(woNum, "STATUS, TYPE", ref woType);
            var status = ascLibrary.ascStrUtils.GetNextWord(ref woType);

            if (status == "P" || status == "A" || status == "D")
                myClass.myParse.Globals.mydmupdate.SetItemMasterQtyScheduledForWorkOrder(woNum, true);
            else if (status != "F")
                myClass.myParse.Globals.mydmupdate.SetItemMasterQtyRequiredForWorkOrder(woNum, true);
            // Update qty to produce
            string sqlStr = "UPDATE ITEMQTY SET QTY_TO_PRODUCE=QTY_TO_PRODUCE-(H.QTY_TO_MAKE - ISNULL(H.CUR_QTY, 0)) " +
                "FROM WO_HDR H WHERE H.PROD_ASCITEMID=ITEMQTY.ASCITEMID " +
                "AND H.WORKORDER_ID='" + woNum + "'";
            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);

            sqlStr = "DELETE FROM WOTRKDET WHERE WORKORDER_ID='" + woNum + "'";
            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);

            sqlStr = "DELETE FROM WOTRKHDR WHERE WORKORDER_ID='" + woNum + "'";
            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);

            sqlStr = "DELETE FROM PROD_REQ WHERE WORKORDER_ID='" + woNum + "'";
            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);

            sqlStr = "DELETE FROM WO_DET WHERE WORKORDER_ID='" + woNum + "'";
            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);

            sqlStr = "DELETE FROM WOPICKS WHERE WO_REPL='" + woNum + "'";
            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);

            sqlStr = "UPDATE LOCITEMS SET PREALLOC_WORKORDER_ID=NULL, QTY_TO_REWORK=0, QAHOLD='F', " +
                "REASONFORHOLD='', QTYONHOLD=0 WHERE PREALLOC_WORKORDER_ID='" + woNum + "'";
            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);

            myClass.myParse.Globals.dmMiscFunc.BackoutPreallocation(woNum, string.Empty);

            if (woType == "C")
            {
                sqlStr = "UPDATE WO_HDR SET CONSOLIDATED_WORKORDER_ID=NULL " +
                    "WHERE CONSOLIDATED_WORKORDER_ID='" + woNum + "'";
                myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
            }
        }




        private static void SetOrderTotalValues(string orderNum)
        {
            string sql = "SELECT COUNT(D.LINENUMBER), SUM( D.QTYORDERED) ";
            sql += ", (CASE WHEN MAX(I.UNITWIDTH) > MAX(I.UNITLENGTH) THEN";
            sql += " (CASE WHEN MAX(I.UNITWIDTH) > MAX(I.UNITHEIGHT) THEN MAX(I.UNITWIDTH) ELSE MAX(I.UNITHEIGHT) END)";
            sql += " ELSE";
            sql += " (CASE WHEN MAX(I.UNITLENGTH) > MAX(I.UNITHEIGHT) THEN MAX(I.UNITLENGTH) ELSE MAX(I.UNITHEIGHT) END)END)";
            //sql += "AS NEW_LARGEST_DIM";
            sql += "FROM ORDRDET D ";
            sql += "JOIN ITEMMSTR I ON I.ASCITEMID = D.ASCITEMID";
            sql += " WHERE ORDERNUMBER = '" + orderNum + "'";

            var tmp = string.Empty;
            myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sql, "", ref tmp);
            long numLines = ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0);// AscUtils.ConvToInt(AscDbUtils.ReadFieldFromDb(sql, myClass.myParse.Globals.myDBUtils.myConnString));
            double NumPieces = ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0);
            double LargestDim = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0);
            sql = "UPDATE ORDRHDR SET NUM_LINES=" + numLines.ToString();
            sql += ", NUMPIECESORDERED=" + NumPieces.ToString();
            sql += ", LARGEST_DIM = " + LargestDim.ToString();
            double totalVol = GetOrderCubic(orderNum);
            if (totalVol > 0)
                sql += ", TOTAL_SHIP_VOLUME=" + totalVol;

            sql += " WHERE ORDERNUMBER='" + orderNum + "'";
            myClass.myParse.Globals.mydmupdate.AddToUpdate(sql);
        }

        public static double GetOrderCubic(string orderNum)
        {
            double result = 0;

            string sql = "SELECT ISNULL(SUM(D.QTYORDERED*I.ITEMCUBIC),0) AS TOTCUBIC " +
                "FROM ORDRDET D, ITEMMSTR I, ORDRHDR H WHERE H.ORDERNUMBER=D.ORDERNUMBER " +
                "AND D.ASCITEMID=I.ASCITEMID AND H.ORDERNUMBER='" + orderNum + "' " +
                "GROUP BY H.ORDERNUMBER";
            using (SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            return Double.Parse(dr["TOTCUBIC"].ToString());
                    }
                }
            }
            return result;
        }


        public static double CalcEstShipWeight(string orderNum)
        {
            string sql = "UPDATE ORDRHDR " +
                "SET EST_SHIPWEIGHT=ISNULL((SELECT SUM(D.QTYORDERED*ISNULL(I.BOL_UNITWEIGHT,I.UNITWEIGHT)) " +
                "FROM ORDRDET D, ITEMMSTR I WHERE D.ORDERNUMBER='" + orderNum + "' AND D.ASCITEMID=I.ASCITEMID AND I.PURORMFG <> 'K'), 0) " +
                "WHERE ORDERNUMBER='" + orderNum + "'";
            myClass.myParse.Globals.myDBUtils.RunSqlCommand(sql);

            string tmp = string.Empty;
            myClass.myParse.Globals.myGetInfo.GetOrderInfo(orderNum, "EST_SHIPWEIGHT", ref tmp);
            return ascLibrary.ascUtils.ascStrToDouble(tmp, 0);
        }

        private static void SaveCustomFields(ref string updStr,List<ASCTracInterfaceModel.Model.ModelCustomData> CustomList, Dictionary<string, List<string>> TranslationList)
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


        private static bool ImportOrderHdr(string orderNum, string pickStatus, bool fExist, ASCTracInterfaceModel.Model.CustOrder.OrdrHdrImport aData, ref string errmsg)
        {
            bool retval = true;
            if (fExist && currCOImportConfig.GWUpdateInProgressOrders)
            {
                bool orderInProgress = false;
                /*
                if (fileXferId.Equals("IM_ORDER_CUSTOM1"))
                {
                    if (pickStatus.Equals(ascLibrary.dbConst.ssCONF_SHIP) || pickStatus.Equals(ascLibrary.dbConst.ssCANCELLED))
                        orderInProgress = true;
                    else
                        orderInProgress = false;
                    if (!pickStatus.Equals(ascLibrary.dbConst.ssNOTSCHED) && !pickStatus.Equals(ascLibrary.dbConst.ssUNLOCKED) &&
                        !pickStatus.Equals(ascLibrary.dbConst.ssCREDIT_HOLD))
                        UpdateAltofieldsOnly = true;
                }
                else 
                */
                if (pickStatus == "" || pickStatus == "N" || pickStatus == "U" || pickStatus == "H")
                    orderInProgress = false;
                else
                    orderInProgress = true;
                if (orderInProgress)
                {
                    errmsg = "Error importing Order# " + orderNum + ": Cannot modify order because " +
                                                "its pick status has changed to '" + pickStatus + "'";
                    return false;
                }
            }

            string sqlStr;
            string sCarrier = aData.CARRIER;
            string sServiceCode = aData.CARRIER_SERVICE_CODE;
            string tmpStr = sServiceCode;
            if (string.IsNullOrEmpty(tmpStr))  //added 10-14-21 (JXG)
                tmpStr = sCarrier;
            if (!string.IsNullOrEmpty(tmpStr))  //added 10-14-21 (JXG)
            {
                sqlStr = "SELECT CARRIER, SERVICE_CODE FROM PARCSERV (NOLOCK) " +
                "WHERE HOST_SERVICE_CODE='" + tmpStr + "'";
                if (myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref tmpStr))
                {
                    sCarrier = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                    sServiceCode = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                }
            }
            //////////////////////

            string stCustId = aData.SHIP_TO_CUST_ID;
            string btCustId = aData.BILL_TO_CUST_ID;
            //if (String.IsNullOrEmpty(btCustId))
            //    btCustId = currCOImportConfig.EDIMasterCustId;

            string stName = aData.SHIP_TO_NAME;
            string custName = aData.BILL_TO_NAME;
            string promoCode = aData.PROMO_CODE;

            DateTime createDate = aData.ORDER_CREATE_DATE;

            if (createDate == DateTime.MinValue)
                createDate = DateTime.Now;
            string updstr = string.Empty;
            if (!currCOImportConfig.useB2BLogic)
            {
                // SHIP TO
                if (!String.IsNullOrEmpty(stCustId) && !myClass.myParse.Globals.myGetInfo.GetCustInfo(stCustId, "CUSTID", ref tmpStr))
                {
                    updstr = string.Empty;
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CUSTID", stCustId);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOCONTACT", aData.SHIP_TO_CONTACT_NAME);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOADDRESS1", aData.SHIP_TO_ADDR_LINE1);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOADDRESS2", aData.SHIP_TO_ADDR_LINE2);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOADDRESS3", aData.SHIP_TO_ADDR_LINE3);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOCITY", aData.SHIP_TO_CITY);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOSTATE", aData.SHIP_TO_STATE);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOZIPCODE", aData.SHIP_TO_ZIP);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOCOUNTRY", aData.SHIP_TO_COUNTRY);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOTELEPHONE", aData.SHIP_TO_CONTACT_TEL);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOFAX", aData.SHIP_TO_CONTACT_FAX);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOCONTACT", aData.BILL_TO_CONTACT_NAME);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOADDRESS1", aData.BILL_TO_ADDR_LINE1);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOADDRESS2", aData.BILL_TO_ADDR_LINE2);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOADDRESS3", aData.BILL_TO_ADDR_LINE3);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOCITY", aData.BILL_TO_CITY);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOSTATE", aData.BILL_TO_STATE);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOZIPCODE", aData.BILL_TO_ZIP);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOCOUNTRY", aData.BILL_TO_COUNTRY);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOTELEPHONE", aData.BILL_TO_CONTACT_TEL);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOFAX", aData.BILL_TO_CONTACT_FAX);

                    if (String.IsNullOrEmpty(stName))
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTONAME", custName);
                    else
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTONAME", stName);

                    if (String.IsNullOrEmpty(custName))
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTONAME", stName);
                    else
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTONAME", custName);

                    myClass.myParse.Globals.mydmupdate.InsertRecord("CUST", updstr);

                }
                // BILL TO
                if (!String.IsNullOrEmpty(btCustId) && !stCustId.Equals( btCustId, StringComparison.OrdinalIgnoreCase) && !myClass.myParse.Globals.myGetInfo.GetCustInfo(btCustId, "CUSTID", ref tmpStr))
                {
                    updstr = string.Empty;
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CUSTID", btCustId);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOCONTACT", aData.BILL_TO_CONTACT_NAME);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOADDRESS1", aData.BILL_TO_ADDR_LINE1);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOADDRESS2", aData.BILL_TO_ADDR_LINE2);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOADDRESS3", aData.BILL_TO_ADDR_LINE3);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOCITY", aData.BILL_TO_CITY);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOSTATE", aData.BILL_TO_STATE);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOZIPCODE", aData.BILL_TO_ZIP);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOCOUNTRY", aData.BILL_TO_COUNTRY);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOTELEPHONE", aData.BILL_TO_CONTACT_TEL);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOFAX", aData.BILL_TO_CONTACT_FAX);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "VMI_GROUPID", aData.VMI_GROUPID);

                    if (String.IsNullOrEmpty(stName))
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTONAME", custName);
                    else
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTONAME", stName);

                    if (String.IsNullOrEmpty(custName))
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTONAME", stName);
                    else
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTONAME", custName);
                    myClass.myParse.Globals.mydmupdate.InsertRecord("CUST", updstr);
                }
            }

            // update ORDRHDR fields
            updstr = string.Empty;

            bool UpdateAltofieldsOnly = false; // this was for IM_ORDER_CUSTOM1 gw function
            if (!UpdateAltofieldsOnly)
            {
                bool useAddrFromCustTable = currCOImportConfig.GWUseAddrFromCustTable;
                bool custExist = myClass.myParse.Globals.myGetInfo.GetCustInfo(stCustId, "SHIPTONAME, SHIPTOADDRESS1, SHIPTOADDRESS2, SHIPTOCITY, " +
                    "SHIPTOSTATE, SHIPTOZIPCODE, SHIPTOCOUNTRY, SHIPTOCONTACT, SHIPTOTELEPHONE, SHIPTOFAX", ref tmpStr);
                if (useAddrFromCustTable && custExist)
                {
                    string tmpName = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                    if (String.IsNullOrEmpty(stName))
                        stName = tmpName;

                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOADDRESS1", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOADDRESS2", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOCITY", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOSTATE", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOZIPCODE", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOCOUNTRY", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOCONTACT", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOTELEPHONE", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOFAX", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                }
                else
                {
                    /////////////////////////////////////
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOADDRESS1", aData.SHIP_TO_ADDR_LINE1);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOADDRESS2", aData.SHIP_TO_ADDR_LINE2);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOCITY", aData.SHIP_TO_CITY);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOSTATE", aData.SHIP_TO_STATE);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOZIPCODE", aData.SHIP_TO_ZIP);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOCOUNTRY", aData.SHIP_TO_COUNTRY);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOCONTACT", aData.SHIP_TO_CONTACT_NAME);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOTELEPHONE", aData.SHIP_TO_CONTACT_TEL);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOFAX", aData.SHIP_TO_CONTACT_FAX);
                }

                //added 05-10-16 (JXG) for Alto Systems
                bool useFreightBillToAddrFromCustTable = currCOImportConfig.GWUseFreightBillToAddrFromCustTable;
                custExist = myClass.myParse.Globals.myGetInfo.GetCustInfo(stCustId, "FREIGHTBILLTONAME, FREIGHTBILLTOCONTACT, FREIGHTBILLTOADDRESS1, FREIGHTBILLTOADDRESS2, FREIGHTBILLTOADDRESS3, " +
                    "FREIGHTBILLTOCITY, FREIGHTBILLTOSTATE, FREIGHTBILLTOZIPCODE, FREIGHTBILLTOCOUNTRY, " +
                    "FREIGHTBILLTOTELEPHONE, FREIGHTBILLTOALTTEL, FREIGHTBILLTOFAX", ref tmpStr);
                if (useFreightBillToAddrFromCustTable && custExist)
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTONAME", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOCONTACT", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOADDRESS1", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOADDRESS2", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOADDRESS3", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOCITY", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOSTATE", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOZIPCODE", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOCOUNTRY", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOTELEPHONE", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOALTTEL", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOFAX", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                }
                else
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTONAME", aData.FREIGHTBILLTONAME);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOCONTACT", aData.FREIGHTBILLTOCONTACT);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOADDRESS1", aData.FREIGHTBILLTOADDRESS1);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOADDRESS2", aData.FREIGHTBILLTOADDRESS2);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOADDRESS3", aData.FREIGHTBILLTOADDRESS3);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOADDRESS4", aData.FREIGHTBILLTOADDRESS4);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOCITY", aData.FREIGHTBILLTOCITY);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOSTATE", aData.FREIGHTBILLTOSTATE);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOZIPCODE", aData.FREIGHTBILLTOZIPCODE);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOCOUNTRY", aData.FREIGHTBILLTOCOUNTRY);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOTELEPHONE", aData.FREIGHTBILLTOTELEPHONE);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOALTTEL", aData.FREIGHTBILLTOALTTEL);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTBILLTOFAX", aData.FREIGHTBILLTOFAX);
                }

                custExist = myClass.myParse.Globals.myGetInfo.GetCustInfo(btCustId, "BILLTOADDRESS1, BILLTOADDRESS2, BILLTOCITY, BILLTOSTATE, " +
                    "BILLTOZIPCODE, BILLTOCOUNTRY, BILLTOCONTACT, BILLTOTELEPHONE, BILLTOFAX", ref tmpStr);
                if (useAddrFromCustTable && custExist)
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOADDRESS1", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOADDRESS2", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOCITY", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOSTATE", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOZIPCODE", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOCOUNTRY", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOCONTACT", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOTELEPHONE", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOFAX", ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
                }
                else
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOADDRESS1", aData.BILL_TO_ADDR_LINE1);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOADDRESS2", aData.BILL_TO_ADDR_LINE2);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOADDRESS3", aData.BILL_TO_ADDR_LINE3);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOCITY", aData.BILL_TO_CITY);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOSTATE", aData.BILL_TO_STATE);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOZIPCODE", aData.BILL_TO_ZIP);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOCOUNTRY", aData.BILL_TO_COUNTRY);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOCONTACT", aData.BILL_TO_CONTACT_NAME);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOTELEPHONE", aData.BILL_TO_CONTACT_TEL);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTOFAX", aData.BILL_TO_CONTACT_FAX);
                }

                myClass.myParse.Globals.myGetInfo.GetCustInfo(btCustId, "BILLTONAME, SHIPTONAME", ref tmpStr);
                string tmpCustName = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                if (currCOImportConfig.useCustBillToNameIfBlank && !string.IsNullOrEmpty(tmpCustName))
                    custName = tmpCustName;

                if (String.IsNullOrEmpty(stName) && currCOImportConfig.useBillToAsShipToNameIfBlank)
                    stName = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);

                if (String.IsNullOrEmpty(custName) && currCOImportConfig.useCustBillToNameIfBlank)
                    custName = stName;

                if (String.IsNullOrEmpty(stName) && currCOImportConfig.useBillToAsShipToNameIfBlank)
                    stName = custName;
            } // end if UpdateAltofieldsOnly 
            if (!fExist)  //added 11-10-15 (JXG)
            {
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ORDERNUMBER", orderNum);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ORDERFILLED", "O");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CREATEDATE", createDate.ToString());
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CREATE_USERID", currCOImportConfig.GatewayUserID); // "GATEWAY";
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ORDER_SOURCE", "I");  //added 06-24-16 (JXG)
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ORDER_SOURCE_SYSTEM", aData.ORDER_SOURCE_SYSTEM);
            }
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "EXPORT", "F");
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LAST_UPDATE", DateTime.Now.ToString()); // AscDbUtils.GetSqlDate();  //DateTime.Now.ToString();  //added 01-27-16 (JXG) for Didion
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LAST_UPDATE_USERID", currCOImportConfig.GatewayUserID); // "GATEWAY";  //added 01-27-16 (JXG) for Didion

            if (UpdateAltofieldsOnly)
            {
                /******
                Customer Order header fields allowed to be modified with this import:
                Ship Info Tab:
                 Carrier Service Code
                 Freight Bill Account #
                 Pre-Assigned BOL#
                 BOL# - Not found in standard import
                 Summary BOL# - Not found in standard import
                 Pro # - Not found in standard import
                 Carrier BOL# - Not found in standard import
                 *******/
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CARRIER_SERVICE_CODE", sServiceCode);

                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHT_BILL_ACCT_NUM", aData.FREIGHT_ACCOUNT_NUMBER);


                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "PREASSIGN_BOLNUM", aData.BOL_NUMBER);
                /********************
                Order Info Tab:
                 Do Not Ship Before
                 Must Arrive By Date
                 Must Arrive By Time
                 Cancel Date                        
                 **********************/

                //if (!String.IsNullOrEmpty(aData.CANCEL_DATE))
                if (aData.CANCEL_DATE != DateTime.MinValue)
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "DONOTSHIPDATE", aData.CANCEL_DATE.ToLongDateString());
                }
                if (aData.MUST_ARRIVE_BY_DATE != DateTime.MinValue)
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "MUST_ARRIVE_BY_TIME", aData.MUST_ARRIVE_BY_DATE.ToString());
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "MUST_ARRIVE_BY_DATE", aData.MUST_ARRIVE_BY_DATE.ToShortDateString());
                }


            }
            else
            {

                string ordertype = aData.ORDER_TYPE;
                if (!string.IsNullOrEmpty(ordertype))
                {
                    ordertype = ordertype.Substring(0, 1);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ORDERTYPE", ordertype);
                }
                else if (!fExist)  //added 11-10-15 (JXG)
                {
                    ordertype = "S";
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ORDERTYPE", ordertype);
                }
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SITE_ID", siteid);
                //added 12-22-15 (JXG) for Didion
                if (aData.LEAVES_DATE != DateTime.MinValue)
                {
                    var theTime = aData.LEAVES_DATE.ToString("HH:mm:ss");
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "REQUIREDSHIPTIME", theTime);
                    if (theTime != "00:00:00")
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "REQUIREDSHIPDATE", aData.LEAVES_DATE.ToShortDateString());
                    else
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "REQUIREDSHIPDATE", aData.LEAVES_DATE.ToString());

                }
                /////////////////////////////////
                if (aData.ENTRY_DATE != DateTime.MinValue)
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CUSTOMERORDERDATE", aData.ENTRY_DATE.ToString());
                if (!fExist)  //added 11-10-15 (JXG)
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "OURORDERDATE", DateTime.Now.ToString());
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CARRIER", sCarrier);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CARRIER_SERVICE_CODE", sServiceCode);

                //added 03-31-14 (JXG) for cabot
                var custOrderCat = aData.CUSTORDERCAT;
                if (String.IsNullOrEmpty(custOrderCat))
                    myClass.myParse.Globals.myGetInfo.GetCustInfo(btCustId, "CUSTCATEGORY", ref custOrderCat);
                ////////////////////////////////
                //CheckFieldChanged("H", false, orderNum, fExist, "MUST_ARRIVE_BY_DATE", ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty( ref updstr, "MUST_ARRIVE_BY_DATE, aData.MUST_ARRIVE_BY_DATE);  //added 09-17-15 (JXG) for Driscoll's
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CUSTORDERCAT", custOrderCat);  // aData.CUSTORDERCAT");
                                                                                                             //added 12-22-15 (JXG) for Didion
                if (aData.MUST_ARRIVE_BY_DATE != DateTime.MinValue)
                {
                    var theTime = aData.MUST_ARRIVE_BY_DATE.ToString("HH:mm:ss");
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "MUST_ARRIVE_BY_TIME", "1900-01-01 " + theTime);  //added "1900-01-01" 01-27-16 (JXG) for Didion
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "MUST_ARRIVE_BY_DATE", aData.MUST_ARRIVE_BY_DATE.ToShortDateString());
                }
                /////////////////////////////////
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOADDRESS3", aData.SHIP_TO_ADDR_LINE3);
                // Added for Numrich
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SALESORDERNUMBER", aData.SALESORDERNUMBER);

                string creditHold = aData.CREDIT_HOLD_STATUS;
                if (creditHold == "H")
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "PICKSTATUS", "H");
                }
                else if (creditHold == "R") // Reset
                {
                    if (pickStatus == "L")
                    {
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "PICKSTATUS", "B");   // Update to being picked
                    }
                    else
                    {
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "PICKSTATUS", "N");
                    }
                }

                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTOCUSTID", stCustId);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPTONAME", stName);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SOLDTOCUSTID", btCustId);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILLTONAME", custName);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CUSTPONUM", aData.CUST_PO_NUM);


                myClass.myParse.Globals.myGetInfo.GetOrderInfo(orderNum, "LINK_NUM, LINK_SEQ_NUM", ref tmpStr);
                if (String.IsNullOrEmpty(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr)))  //added 07-31-13 (JXG) suppress update if already assigned to load
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LINK_NUM", aData.LOAD_PLAN_NUM);
                }
                if (String.IsNullOrEmpty(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr)))  //added 07-31-13 (JXG) suppress update if already assigned to load
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LINK_SEQ_NUM", aData.LOAD_STOP_SEQ);
                }
                //added 09-17-15 (JXG) for Driscoll's
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "COD_AMT", aData.COD_AMT.ToString());
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SALESPERSON", aData.SALESPERSON);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "TERMS_ID", aData.TERMS_ID);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LINKED_PO_NUM", aData.LINKED_PONUMBER);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYCUSTID", aData.THIRDPARTYCUSTID);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYNAME", aData.THIRDPARTYNAME);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYADDRESS1", aData.THIRDPARTYADDRESS1);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYADDRESS2", aData.THIRDPARTYADDRESS2);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYADDRESS3", aData.THIRDPARTYADDRESS3);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYCITY", aData.THIRDPARTYCITY);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYSTATE", aData.THIRDPARTYSTATE);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYZIPCODE", aData.THIRDPARTYZIPCODE);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYCOUNTRY", aData.THIRDPARTYCOUNTRY);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "PREASSIGN_BOLNUM", aData.BOL_NUMBER);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "STORE_NUM", aData.STORE_NUM);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "DEPT", aData.DEPT);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "PACKLIST_REQ", aData.PACKLIST_REQ);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "DROP_SHIP", aData.DROP_SHIP);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BATCH_NUM", aData.BATCH_NUM);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ROUTEID", aData.ROUTEID);

                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CUSTSHIPPONUM", aData.CUST_SHIPTO_PO_NUM);  //changed from CUST_SHIP_TO_PO_NUM 12-06-16 (JXG)
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CUSTBILLPONUM", aData.CUST_BILLTO_PO_NUM);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "COMPLIANCELABEL", aData.COMPLIANCE_LABEL);

                string carrierId = sCarrier;
                if (currCOImportConfig.GWWillCallCarrierFlag && currCOImportConfig.GWWillCallCarrier.Equals(carrierId, StringComparison.OrdinalIgnoreCase))
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "PRIORITYID", "1");
                }
                else
                {
                    if (aData.PRIORITY_ID > 0)
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "PRIORITYID", aData.PRIORITY_ID.ToString());
                }

                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CUSTOMER_EMAIL_TO", aData.RECIPIENT_EMAIL);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHTCODE", aData.PREPAY_COLLECT);

                if (aData.CANCEL_DATE != DateTime.MinValue)
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "DONOTSHIPDATE", aData.CANCEL_DATE.ToShortDateString());
                }
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHT_BILL_ACCT_NUM", aData.FREIGHT_ACCOUNT_NUMBER);


                string ignoreInvAvail = "";
                if ((aData.ALLOW_SHORT_SHIP == "T") || (aData.ALLOW_SHORT_SHIP == "Y"))
                    ignoreInvAvail = "T";
                else if ((aData.ALLOW_SHORT_SHIP == "F") || (aData.ALLOW_SHORT_SHIP == "N"))
                    ignoreInvAvail = "F";
                else if (!fExist)
                    ignoreInvAvail = "T";
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "IGNOREINVAVAIL", ignoreInvAvail);

                string fillToCapacity = "";
                if ((aData.ALLOW_OVER_SHIP == "T") || (aData.ALLOW_OVER_SHIP == "Y"))
                    fillToCapacity = "T";
                else if ((aData.ALLOW_OVER_SHIP == "F") || (aData.ALLOW_OVER_SHIP == "N"))
                    fillToCapacity = "F";
                else if (!fExist)
                    fillToCapacity = "F";
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FILL_TO_CAPACITY", fillToCapacity);


                string residentialFlag = "";
                if ((aData.RESIDENTIAL_FLAG == "T") || (aData.RESIDENTIAL_FLAG == "Y"))
                    residentialFlag = "T";
                else if ((aData.RESIDENTIAL_FLAG == "F") || (aData.RESIDENTIAL_FLAG == "N"))
                    residentialFlag = "F";
                else if (!fExist)
                    residentialFlag = "F";
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "RESIDENTIAL_FLAG", residentialFlag);


                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CLIENTDEPT", aData.CLIENTDEPT);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CLIENTDIVISION", aData.CLIENTDIVISION);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CLIENTGLACCT", aData.CLIENTGLACCT);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CLIENTPROFIT", aData.CLIENTPROFIT);
                string shipvia = aData.SHIP_VIA;
                if (String.IsNullOrEmpty(shipvia))
                    myClass.myParse.Globals.myGetInfo.GetCarrierInfo(sCarrier, "SHIPVIA_ID", ref shipvia);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPVIA", shipvia);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "AREA", aData.AREA);

                //added 08-13-15 (JXG) for Allen Dist
                string pickToContr = "";

                myClass.myParse.Globals.myGetInfo.GetCustInfo(stCustId, "CPTOCONTAINER", ref pickToContr);
                if (String.IsNullOrEmpty(pickToContr))
                    myClass.myParse.Globals.myGetInfo.GetCustInfo(btCustId, "CPTOCONTAINER", ref pickToContr);
                if (String.IsNullOrEmpty(pickToContr) || (pickToContr == "U"))
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "PICKTOCONTAINER", pickToContr);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FOB", aData.FOB);
            } // end else if (UpdateAltofieldsOnly)

            SaveCustomFields(ref updstr, aData.CustomList, currCOImportConfig.GWCOHdrTranslation);

            if (!fExist)
            {
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LOCKED", "F"); // DB defaults do not propagate to the record using datasets
                myClass.myParse.Globals.mydmupdate.InsertRecord("ORDRHDR", updstr);
            }
            else
                myClass.myParse.Globals.mydmupdate.updateordrhdr(orderNum, "", updstr);

            // Remove any pre-existing notes, as they will be recreated
            //if (!updateOnlyPopulatedFields)  //added 01-28-16 (JXG) for Didion
            if (fExist)
            {
                string sql = "DELETE FROM NOTES WHERE ORDERNUM='" + orderNum + "' " +
                    "AND (TYPE='B' OR TYPE='G' OR TYPE='P' OR TYPE='T' OR TYPE='L') ";  //added 04-12-16 (JXG) for Driscoll's
                myClass.myParse.Globals.mydmupdate.AddToUpdate(sql);
            }
            int seq = 1;
            if (!String.IsNullOrEmpty(aData.DELIVERY_INSTRUCTIONS))
            {
                ImportNotes.SaveNotes("G", orderNum, aData.DELIVERY_INSTRUCTIONS, false, 0, 1, myClass.myParse.Globals);
                seq += 1;
            }
            foreach (var noterec in aData.NotesList)
            {
                ImportNotes.SaveNotes("G", orderNum, noterec.NOTE, false, 0, seq, myClass.myParse.Globals);
                seq += 1;
            }

            //added 10-30-09 for Nealanders
            //if create work order from cust order detail,
            //delete any work orders that are linked to this cust order
            //before importing cust order detail lines
            if (myClass.myParse.Globals.myConfig.vmProduction.boolValue && currCOImportConfig.GWCreateWOFromCO)
            {
                string sql = "SELECT WORKORDER_ID, PROD_ASCITEMID FROM WO_HDR (NOLOCK) " +
                    "WHERE LINKED_CO_NUM='" + orderNum + "' " +
                    "AND STATUS NOT IN ('A','F','X') AND CUR_QTY=0 " +
                    "AND (SELECT COUNT(WORKORDER_ID) FROM WO_DET (NOLOCK) " +
                    "WHERE WORKORDER_ID=WO_HDR.WORKORDER_ID " +
                    "AND (QTY_PICKED<>0 OR QTY_ALLOC<>0 OR QTY_USED<>0)) = 0";
                using (SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string woNum = reader["WORKORDER_ID"].ToString();

                            DeleteWorkOrder(woNum);

                            myClass.myParse.Globals.mydmupdate.DeleteRecord("WO_HDR", "WORKORDER_ID='" + woNum + "'");
                        }
                    }
                }
            }
            //added 10-05-15 for Driscoll's
            //if create transfer po from transfer cust order,
            //delete any transfer pos that are linked to this cust order
            if (currCOImportConfig.GWCreateTransferPOFromCO)
            {
                string sql = "SELECT H.PONUMBER, H.RELEASENUM, H.RECEIVED " +
                    "FROM POHDR H " +
                    "WHERE H.LINKED_ORDERNUMBER='" + orderNum + "' " +
                    "AND H.TRANSFER_CO_ORDERNUMBER='" + orderNum + "' " +
                    "AND H.RECEIVED NOT IN ('R','C','X') AND H.ORDERTYPE='T' " +
                    "AND (SELECT COUNT(PONUMBER) FROM PODET (NOLOCK) " +
                    "WHERE PONUMBER=H.PONUMBER AND RELEASENUM=H.RELEASENUM " +
                    "AND (QTYRECEIVED<>0)) = 0";
                using (SqlConnection conn2 = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
                using (SqlCommand cmd2 = new SqlCommand(sql, conn2))
                {
                    conn2.Open();
                    using (SqlDataReader reader2 = cmd2.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            string poNum = reader2["PONUMBER"].ToString();
                            string relNum = reader2["RELEASENUM"].ToString();
                            // Decrement the item qty from expected
                            myClass.myParse.Globals.mydmupdate.SetItemMasterQtyExpected(poNum, relNum, true);
                            sql = "DELETE FROM PODET WHERE PONUMBER='" + poNum + "' AND RELEASENUM='" + relNum + "' ";
                            myClass.myParse.Globals.mydmupdate.AddToUpdate(sql);
                            sql = "DELETE FROM POHDR WHERE PONUMBER='" + poNum + "' AND RELEASENUM='" + relNum + "' ";
                            myClass.myParse.Globals.mydmupdate.AddToUpdate(sql);
                        }
                    }
                }
            }
            return (retval);
        }


        private static void GetConvQty(string aASCItemID, string aHostUOM, bool aRaiseErr, ref bool fPickSub, ref double aOrderQty, ref double aConvFact)
        {
            string tmp = "", stockUOM;
            List<string> uom, cf;
            double aDbl;
            bool fFound = false;
            aConvFact = 1;


            if (!String.IsNullOrEmpty(aHostUOM) && myClass.myParse.Globals.myGetInfo.GetASCItemInfo(aASCItemID, "STOCK_UOM,SUB_UOM,UNIT_MEAS1,UNIT_MEAS2,UNIT_MEAS3,UNIT_MEAS4,CONV_FACT_12,CONV_FACT_23,CONV_FACT_34", ref tmp))
            {
                stockUOM = ascLibrary.ascStrUtils.GetNextWord(ref tmp).ToUpper();
                if (stockUOM == aHostUOM)
                    fFound = true;
                else
                {
                    if (ascLibrary.ascStrUtils.GetNextWord(ref tmp).ToUpper() == aHostUOM)
                    {
                        if (aRaiseErr)
                            throw new Exception("Import UOM: " + aHostUOM + ", cannot be below Stock UOM");
                    }
                    else
                    {
                        uom = new List<string>();
                        cf = new List<string>();

                        uom.Add(ascLibrary.ascStrUtils.GetNextWord(ref tmp).ToUpper());
                        uom.Add(ascLibrary.ascStrUtils.GetNextWord(ref tmp).ToUpper());
                        uom.Add(ascLibrary.ascStrUtils.GetNextWord(ref tmp).ToUpper());
                        uom.Add(ascLibrary.ascStrUtils.GetNextWord(ref tmp).ToUpper());

                        cf.Add(ascLibrary.ascStrUtils.GetNextWord(ref tmp).ToUpper());
                        cf.Add(ascLibrary.ascStrUtils.GetNextWord(ref tmp).ToUpper());
                        cf.Add(ascLibrary.ascStrUtils.GetNextWord(ref tmp).ToUpper());

                        int i = 0;
                        while (i < 3 && stockUOM != uom[i])
                        {
                            if (uom[i] == aHostUOM)
                            {
                                fFound = true;
                                aConvFact *= ascLibrary.ascUtils.ascStrToDouble(cf[i], 1);
                            }
                            i++;
                        }

                        while (i < 4 && !fFound)
                        {
                            if (uom[i] == aHostUOM)
                                fFound = true;
                            if (i < 3)
                            {
                                aDbl = ascLibrary.ascUtils.ascStrToDouble(cf[i], 1);
                                if (aDbl <= 0)
                                    aDbl = 1;
                                aConvFact /= aDbl;
                            }
                            i++;
                        }

                        if (fFound)
                            aOrderQty *= aConvFact;
                        else
                            aConvFact = 1;
                    }
                }
            }

            if (!fFound)
            {
                if (aRaiseErr)
                    throw new Exception("Import UOM: " + aHostUOM + " is not defined for item.");
            }
        }

        private static DateTime GetScheduleDate(DateTime dtReqShipDate, long nLeadTime)
        {
            long i = 0;
            DateTime dtShipDate = dtReqShipDate;
            while (i < nLeadTime)
            {
                dtShipDate = dtShipDate.AddDays(-1);

                if (myClass.myParse.Globals.myDBUtils.ifRecExists("SELECT HOLIDAY_DATE FROM HOLIDAYS WHERE HOLIDAY_DATE='" + dtShipDate.Date.ToString() + "'"))
                    continue;

                if (dtShipDate.DayOfWeek == DayOfWeek.Sunday && myClass.myParse.Globals.myConfig.iniGNWorkDaysSunday.boolValue)
                    i++;
                else if (dtShipDate.DayOfWeek == DayOfWeek.Monday && myClass.myParse.Globals.myConfig.iniGNWorkDaysMonday.boolValue)
                    i++;
                else if (dtShipDate.DayOfWeek == DayOfWeek.Tuesday && myClass.myParse.Globals.myConfig.iniGNWorkDaysTuesday.boolValue)
                    i++;
                else if (dtShipDate.DayOfWeek == DayOfWeek.Wednesday && myClass.myParse.Globals.myConfig.iniGNWorkDaysWednesday.boolValue)
                    i++;
                else if (dtShipDate.DayOfWeek == DayOfWeek.Thursday && myClass.myParse.Globals.myConfig.iniGNWorkDaysThursday.boolValue)
                    i++;
                else if (dtShipDate.DayOfWeek == DayOfWeek.Friday && myClass.myParse.Globals.myConfig.iniGNWorkDaysFriday.boolValue)
                    i++;
                else if (dtShipDate.DayOfWeek == DayOfWeek.Saturday && myClass.myParse.Globals.myConfig.iniGNWorkDaysSaturday.boolValue)
                    i++;
            }
            return dtShipDate;
        }


        private static bool ImportOrderDet(string orderNum, string pickStatus, string ordertype, ASCTracInterfaceModel.Model.CustOrder.OrdrHdrImport aData, ref string errmsg)
        {
            string sqlStr, tmpStr = "";
            string itemId, ascItemId, oldItemId = "", reqLot, hostUom;
            string itemType = "", importAction;
            long lineNum;
            double qty, qtyOrdered, qtyPicked, qtySub, qtyShipped, convFact;
            bool recExists, subUom = false;

            if (pickStatus == "C")
                return true;

            bool statusScheduled = isOrderStatusScheduled(orderNum);
            bool statusRequired = isOrderStatusRequired(orderNum);

            #region Delete Order Lines
            if (currCOImportConfig.GWDeleteCOLinesNotInInterface)
            {
                string wherestr = "";
                foreach (var rec in aData.DetailList)
                {
                    if (string.IsNullOrEmpty(wherestr))
                        wherestr += ",";
                    wherestr += rec.LINE_NUMBER.ToString();
                }

                sqlStr = "SELECT LINENUMBER, ASCITEMID, QTYORDERED, QTYPICKED, QTY_SUBSTITUTED, QTYSHIPPED " +
                    "FROM ORDRDET (NOLOCK) WHERE ORDERNUMBER='" + orderNum + "' AND LINENUMBER < 1000";  //changed from >= to < 08-04-16 (JXG) for Driscoll's
                sqlStr += " AND NOT LINENUMBER IN ( " + wherestr + ")";
                using (SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
                using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long.TryParse(reader["LINENUMBER"].ToString(), out lineNum);

                            // Line does not exist in interface
                            Utils.AllocUtil.BackoutPreAllocationForOrderDet(orderNum, lineNum, myClass.myParse.Globals);

                            double.TryParse(reader["QTYORDERED"].ToString(), out qtyOrdered);
                            double.TryParse(reader["QTYPICKED"].ToString(), out qtyPicked);
                            double.TryParse(reader["QTY_SUBSTITUTED"].ToString(), out qtySub);
                            double.TryParse(reader["QTYSHIPPED"].ToString(), out qtyShipped);
                            double qtyNotPicked = (double)(qtyOrdered - qtyPicked);

                            if (qtyPicked == 0 && qtySub == 0 && qtyShipped == 0)
                            {
                                if (!ascLibrary.dbConst.otNOT_FOR_SCHEDING.Contains(ordertype))
                                {
                                    // Decrement item quantities
                                    if (statusScheduled)
                                        myClass.myParse.Globals.mydmupdate.SetItemMasterQty("QTYSCHEDULED", reader["ASCITEMID"].ToString(), qtyNotPicked, true);
                                    else if (statusRequired)
                                        myClass.myParse.Globals.mydmupdate.SetItemMasterQty("QTYREQUIRED", reader["ASCITEMID"].ToString(), qtyNotPicked, true);
                                }
                                myClass.myParse.Globals.mydmupdate.DeleteRecord("ORDRDET", "ORDERNUMBER='" + orderNum + "' " +
                                    "AND LINENUMBER=" + lineNum);
                                myClass.myParse.Globals.mydmupdate.DeleteRecord("PCEPICKING", "RECTYPE='C' AND PCEPICKING='" + orderNum + "' " +
                                    "AND SEQNUM=" + lineNum);
                            }
                            else
                            {
                                if (!ascLibrary.dbConst.otNOT_FOR_SCHEDING.Contains(ordertype))
                                {
                                    // Decrement item quantities
                                    if (statusScheduled)
                                        myClass.myParse.Globals.mydmupdate.SetItemMasterQty("QTYSCHEDULED", reader["ASCITEMID"].ToString(), qtyNotPicked, true);
                                    else if (statusRequired)
                                        myClass.myParse.Globals.mydmupdate.SetItemMasterQty("QTYREQUIRED", reader["ASCITEMID"].ToString(), qtyNotPicked, true);
                                }
                                myClass.myParse.Globals.mydmupdate.UpdateFields("ORDRDET", "ORDERFILLED='D'", "ORDERNUMBER='" + orderNum + "' AND LINENUMBER=" + lineNum);
                                myClass.myParse.Globals.mydmupdate.UpdateFields("PCEPICKING", "ORDERFILLED='D'", "RECTYPE='C' AND PCEPICKING='" + orderNum + "' " +
                                                                        "AND SEQNUM=" + lineNum);
                            }
                        }
                    }
                    myClass.myParse.Globals.mydmupdate.ProcessUpdates();

                }
            }

            #endregion
            long holdLineNum = 0;
            foreach (var rec in aData.DetailList)
            {
                qtyPicked = 0;
                qtyShipped = 0;
                qtyOrdered = 0;
                qtySub = 0;
                convFact = 1;

                importAction = rec.STATUS_FLAG;
                itemId = rec.PRODUCT_CODE;
                lineNum = rec.LINE_NUMBER;
                qty = rec.QUANTITY;

                reqLot = rec.REQUESTED_LOT;

                string custItemID = rec.CUST_ITEMID;
                double custConvFact = 0;

                ascItemId = string.Empty;
                if (currCOImportConfig.GWCOUseCustItem)
                {
                    if (myClass.myParse.Globals.myDBUtils.ReadFieldFromDB("SELECT CUSTUNITPERSTOCK, ITEMID, ASCITEMID FROM CUSTITEM" +
                        " WHERE CUSTOMERID='" + aData.BILL_TO_CUST_ID + "' AND CUSTITEMID='" + itemId + "' AND SITE_ID='" + siteid + "'", "", ref ascItemId))
                    {
                        custItemID = itemId;
                        custConvFact = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref ascItemId), 1);
                        qty = qty * custConvFact;
                        itemId = ascLibrary.ascStrUtils.GetNextWord(ref ascItemId);
                    }
                }

                if (string.IsNullOrEmpty(ascItemId))
                {
                    if (myClass.myParse.Globals.myConfig.iniGNVMI.boolValue)
                    {
                        ascItemId = myClass.myParse.Globals.dmMiscItem.GetASCItem(siteid, itemId, aData.BILL_TO_CUST_ID);
                        string status = string.Empty;
                        if (!myClass.myParse.Globals.myGetInfo.GetASCItemInfo(ascItemId, "ITEM_STATUS", ref status))
                            ascItemId = string.Empty;
                        else if (currCOImportConfig.GWImportVMIItemIfActive && !status.Equals(ascLibrary.dbConst.isACTIVE))
                        {
                            ascItemId = string.Empty;
                        }
                    }

                    if (string.IsNullOrEmpty(ascItemId))
                    {
                        ascItemId = myClass.myParse.Globals.dmMiscItem.GetASCItem(siteid, itemId, string.Empty);
                    }
                }
                if (!myClass.myParse.Globals.myGetInfo.GetASCItemInfo(ascItemId, "PURORMFG", ref itemType))
                {
                    errmsg = "Item " + itemId + " not found in Item Master";
                    //ascItemId = string.Empty;
                    return (false);
                }

                hostUom = rec.HOST_UOM;
                if (!String.IsNullOrEmpty(hostUom))
                    GetConvQty(ascItemId, hostUom, true, ref subUom, ref qty, ref convFact);

                string orderfilled = string.Empty;
                string linkedWONum = string.Empty;
                recExists = myClass.myParse.Globals.myGetInfo.GetOrderDetInfo(orderNum, "LINKED_WO_NUM,ORDERFILLED,ITEMID, QTYPICKED, QTY_SUBSTITUTED, QTYSHIPPED, QTYORDERED", lineNum.ToString(), ref tmpStr);
                if (recExists)
                {
                    linkedWONum = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                    orderfilled = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                    oldItemId = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                    qtyPicked = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0);
                    qtySub = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0);
                    qtyPicked += qtySub;
                    qtyShipped = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0);
                    qtyOrdered = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0);
                }

                if (importAction == "D" || importAction == "V")
                {
                    if (recExists)
                    {
                        if (qtyPicked == 0 && qtyShipped == 0)
                        {
                            // Decrement item quantities
                            if (!ascLibrary.dbConst.otNOT_FOR_SCHEDING.Contains(ordertype) && orderfilled.Equals(ascLibrary.dbConst.osOPEN))
                            {
                                if (statusScheduled)
                                    myClass.myParse.Globals.mydmupdate.SetItemMasterQty("QTYSCHEDULED", ascItemId, (double)qtyOrdered, true);
                                else if (statusRequired)
                                    myClass.myParse.Globals.mydmupdate.SetItemMasterQty("QTYREQUIRED", ascItemId, (double)qtyOrdered, true);
                                myClass.myParse.Globals.mydmupdate.ProcessUpdates();
                            }
                            Utils.AllocUtil.BackoutPreAllocationForOrderDet(orderNum, lineNum, myClass.myParse.Globals);
                            sqlStr = "DELETE FROM ORDRDET WHERE ORDERNUMBER='" + orderNum + "' AND LINENUMBER=" + lineNum;

                            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
                            myClass.myParse.Globals.mydmupdate.AddToUpdate("DELETE PCEPICKING WHERE RECTYPE='C' AND PCEPICKING='" + orderNum + "' " + "AND SEQNUM=" + lineNum);

                        }
                        else
                        {
                            errmsg = "Error importing order details, Order# " + orderNum + ", Line " + lineNum + ": " +
                                "Cannot delete order line item that has been picked.";
                            return (false);
                        }
                    }
                }
                else
                {
                    if (recExists)
                    {
                        // Decrement item quantities because this is an edit. Inventory will be 
                        // incremented once the edits are complete.
                        double tmpQty = (double)(qtyOrdered - qtyPicked);
                        if (!ascLibrary.dbConst.otNOT_FOR_SCHEDING.Contains(ordertype) && orderfilled.Equals(ascLibrary.dbConst.osOPEN))
                        {
                            if (statusScheduled)
                                myClass.myParse.Globals.mydmupdate.SetItemMasterQty("QTYSCHEDULED", ascItemId, tmpQty, true);
                            else if (statusRequired)
                            {
                                myClass.myParse.Globals.mydmupdate.SetItemMasterQty("QTYREQUIRED", ascItemId, tmpQty, true);
                                myClass.myParse.Globals.mydmupdate.DeleteRecord("PCEPICKING", "RECTYPE='C' AND PCEPICKING='" + orderNum + "' " +
                                        "AND SEQNUM=" + lineNum);
                            }
                        }
                    }

                    string updstr = string.Empty;
                    if (!recExists)
                    {

                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ORDERNUMBER", orderNum);
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LINENUMBER", lineNum.ToString()); ;
                        //if (itemType == "K")
                        //    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty( ref updstr, "ORDERFILLED", "T";
                        //else 
                        if (importAction.Equals("H") || importAction.Equals("S") || importAction.Equals("T") || importAction.Equals("X"))
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ORDERFILLED", importAction);
                        else
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ORDERFILLED", "O");
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "PARTIALSKID", "F");
                        //ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "QTYBACKORDERED", "0");
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "QTYPICKED", "0");
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "QTYLOADED", "0");
                    }
                    else
                    {
                        if (importAction.Equals("H") || importAction.Equals("S") || importAction.Equals("T") || importAction.Equals("X"))
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ORDERFILLED", importAction);
                        else
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ORDERFILLED", "O");
                    }

                    if (!recExists || (qtyPicked == 0 && qtyShipped == 0))
                    {
                        /////////////////////////////////////
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ASCITEMID", ascItemId);
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ITEMID", itemId);
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "PARENT_ITEMID", itemId);
                    }
                    else if (itemId != oldItemId)
                    {
                        errmsg = "Error importing order details, Order# " + orderNum + ", Line " + lineNum + ": " +
                            "Cannot change item id on order line that's already picked.";
                        return (false);
                    }

                    if (lineNum == holdLineNum && !String.IsNullOrEmpty(reqLot))
                    {
                        if (qtyPicked <= (qtyOrdered + qty))
                        {
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "QTYORDERED", qty.ToString());
                        }
                        else
                        {
                            errmsg = "Error importing order details, Order# " + orderNum + ", Line " + lineNum + ": " +
                                "Cannot change qty to less than amount already picked.";
                            return (false);
                        }
                    }
                    else
                    {
                        if (qtyPicked <= qty)
                        {
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "QTYORDERED", qty.ToString());
                        }
                        else
                        {
                            errmsg = "Error importing order details, Order# " + orderNum + ", Line " + lineNum + ": " +
                                "Cannot change qty to less than amount already picked.";
                            return (false);
                        }
                    }
                    holdLineNum = lineNum;

                    if (custConvFact > 0)
                    {
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "HOST_CONV_FACT", convFact.ToString());
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "HOST_QTYORDERED", rec.QUANTITY.ToString());
                    }
                    else if (!String.IsNullOrEmpty(hostUom))
                    {
                        /////////////////////////////////////
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "HOST_CONV_FACT", convFact.ToString());
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "HOST_QTYORDERED", rec.QUANTITY.ToString());
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "HOST_UOM", hostUom);
                    }


                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "QTYBACKORDERED", rec.QTYBACKORDERED.ToString());
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CUST_ITEMID", custItemID); // rec.CUST_ITEMID
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "COSTEACH", rec.COSTEACH.ToString());
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "RETAIL_PRICE", rec.SOLD_PRICE.ToString());
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "COUNTRY_OF_DESTINATION", rec.COUNTRY_OF_DESTINATION);


                    //ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty( ref updstr, "HOST_LINENUMBER", rec.HOST_LINENUMBER);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPDESC", rec.SHIPDESC);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CLIENTDEPT", rec.CLIENTDEPT);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CLIENTDIVISION", rec.CLIENTDIVISION);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CLIENTGLACCT", rec.CLIENTGLACCT);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CLIENTPROFIT", rec.CLIENTPROFIT);

                    SaveCustomFields(ref updstr, rec.CustomList, currCOImportConfig.GWCODetTranslation);
                    if (!recExists)
                    {
                        myClass.myParse.Globals.mydmupdate.InsertRecord("ORDRDET", updstr);
                    }

                    string promoCode = aData.PROMO_CODE;
                    if (!String.IsNullOrEmpty(promoCode) && myClass.myParse.Globals.myConfig.iniCPAllowPromoAlloc.boolValue)
                    {
                        sqlStr = "SELECT MASTER_CLIENT FROM PROMOS (NOLOCK) WHERE PROMO_CODE='" + promoCode + "'";
                        if (myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref tmpStr))
                        {
                            if (tmpStr != aData.BILL_TO_CUST_ID)
                                throw new Exception("Error: Sold to Cust ID on order [" + orderNum + "] does not match Master Client on promo [" + promoCode + "].");
                        }
                        else
                        {
                            Utils.PromoUtil.AddPromo(promoCode, siteid, aData.BILL_TO_CUST_ID, myClass.myParse.Globals);
                        }

                        Utils.PromoUtil.AddPromoItem(promoCode, siteid, ascItemId, myClass.myParse.Globals);

                        Utils.PromoUtil.UpdatePromoOrder(promoCode, siteid, ascItemId, orderNum, lineNum, qty, "C", myClass.myParse.Globals);
                    }

                    int seq = 1;
                    if (!String.IsNullOrEmpty(rec.COMMENT))
                    {
                        ImportNotes.SaveNotes("G", orderNum, rec.COMMENT, false, Convert.ToInt32(rec.LINE_NUMBER), 1, myClass.myParse.Globals);
                        seq = 2;
                    }
                    foreach (var noterec in rec.NotesList)
                    {
                        ImportNotes.SaveNotes("G", orderNum, noterec.NOTE, false, Convert.ToInt32(rec.LINE_NUMBER), seq, myClass.myParse.Globals);
                        seq += 1;
                    }
                    if (!String.IsNullOrEmpty(reqLot))
                    {
                        Utils.AllocUtil.AddLotAllocForOrderDet(orderNum, lineNum, ascItemId, reqLot, qty, myClass.myParse.Globals);

                    }

                    if (myClass.myParse.Globals.myConfig.vmProduction.boolValue && currCOImportConfig.GWCreateWOFromCO)
                    {
                        if (!recExists)
                        {
                            myClass.myParse.Globals.myGetInfo.GetASCItemInfo(ascItemId, "PURORMFG, MRP_ITEM_HAS_BOM, DEFPRODLINE, " +
                                    "DEFAULT_WO_STATUS, QTYTOPROMISE, QTY_EXPECTED, QTY_TO_PRODUCE, " +
                                    "MRP_LEAD_TIME, LEADTIME, INHOUSE_TIME", ref tmpStr);

                            itemType = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                            string itemHasBom = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                            string defProdLine = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                            if (String.IsNullOrEmpty(defProdLine))
                                defProdLine = "1";
                            string defWoStatus = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                            if (String.IsNullOrEmpty(defWoStatus))
                                defWoStatus = "N";
                            double qtyOverShort = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0);
                            qtyOverShort += ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0);
                            double qtyToProduce = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0);
                            double qtyMaking = qtyOverShort + qtyToProduce;

                            string mrpLeadTime = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                            string leadTime = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                            string inHouseTime = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);

                            long nLeadTime = 0;
                            if (String.IsNullOrEmpty(mrpLeadTime))
                                nLeadTime = ascLibrary.ascUtils.ascStrToInt(leadTime, 0);
                            else
                                nLeadTime = ascLibrary.ascUtils.ascStrToInt(mrpLeadTime, 0);
                            nLeadTime += ascLibrary.ascUtils.ascStrToInt(inHouseTime, 0);

                            if (itemType != "K" && itemHasBom == "T" && qty > qtyMaking)
                            {
                                string reqShipDate = string.Empty;
                                DateTime dtReqShipDate;
                                DateTime dtSchedDate = DateTime.Now;
                                myClass.myParse.Globals.myGetInfo.GetOrderInfo(orderNum, "REQUIREDSHIPDATE", ref reqShipDate);
                                if (!String.IsNullOrEmpty(reqShipDate) && nLeadTime > 0)
                                {
                                    if (DateTime.TryParse(reqShipDate, out dtReqShipDate))
                                        dtSchedDate = GetScheduleDate(dtReqShipDate, nLeadTime);
                                    else
                                        reqShipDate = string.Empty;
                                }

                                double woQty = qty - qtyMaking;
                                if (woQty > qty)
                                    woQty = qty;

                                string woNum = myClass.myParse.Globals.dmmiscprod.GetUniqueWONum(ascLibrary.dbConst.wtPRODUCTION); // .myConfig.woMask.GetOrderNum();

                                updstr = string.Empty;
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "WORKORDER_ID", woNum);
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SITE_ID", siteid);
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CREATE_DATE", DateTime.Now.ToString());
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CREATE_USERID", currCOImportConfig.GatewayUserID);
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "PROD_ITEMID", itemId);
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "PROD_ASCITEMID", ascItemId);
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "ORDER_SOURCE", "I");
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "STATUS", defWoStatus);
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "TYPE", "P");
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "PRODLINE", defProdLine);
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "WORKCELL", "1");
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "QTY_TO_MAKE", woQty.ToString());
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CUR_QTY", "0");
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "LINKED_CO_NUM", orderNum);
                                //ascLibrary.ascStrUtils.ascAppendSetStr( ref updstr, "CUSTOM_DATA1", rec.CUSTOM_DATA1"].ToString());
                                if (!String.IsNullOrEmpty(reqShipDate))
                                {
                                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "EXPECTED_RELEASE_DATE", reqShipDate);
                                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SCHED_DATETIME", dtSchedDate.ToString());
                                }
                                myClass.myParse.Globals.mydmupdate.InsertRecord("WO_HDR", updstr);

                                myClass.myParse.Globals.mydmupdate.SetItemMstrQtyToProduce(ascItemId);

                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LINKED_WO_NUM", woNum);
                                myClass.myParse.Globals.myDBUtils.RunSqlCommand("UPDATE ORDRHDR SET LINKED_WO_COMPLETE='F' WHERE ORDERNUMBER='" + orderNum + "'");

                                var myFunction = new ASCTracFunctionMain();
                                myFunction.InitMain();

                                var tmperr = myFunction.DoFunction(FuncConst.funcPROD_BUILDWO_DET
                                    + ascLibrary.dbConst.HHDELIM + woNum
                                    + ascLibrary.dbConst.HHDELIM + ""
                                    + ascLibrary.dbConst.HHDELIM + ""
                                    + ascLibrary.dbConst.HHDELIM + "F", "GWUSER", siteid);
                                //ascLibrary.ascUtils.ascWriteLog(fileXferId, "Result " + tmperr, true);
                                if (!String.IsNullOrEmpty(tmperr) && !tmperr.StartsWith("OK"))
                                {
                                    var fLastErrMsg = "Error building WO Det from BOM " + tmperr;

                                    Class1.WriteException("ImportOrdrDet", "Item " + itemId, orderNum, fLastErrMsg, "");

                                }

                                ImportNotes.SaveNotes("W", woNum, rec.COMMENT, false, 0, 1, myClass.myParse.Globals);
                            }
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(linkedWONum))
                            {
                                sqlStr = "UPDATE WO_HDR SET LINKED_CO_NUM='Unlinked' " +
                                    "WHERE WORKORDER_ID='" + linkedWONum + "'";
                                myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
                            }
                        }
                    }
                }
            }

            // Increment item quantities
            if (!ascLibrary.dbConst.otNOT_FOR_SCHEDING.Contains(ordertype))
            {
                if (statusScheduled)
                    myClass.myParse.Globals.mydmupdate.SetItemMasterQtyScheduledForCustOrder(orderNum, false);
                else if (statusRequired)
                    myClass.myParse.Globals.mydmupdate.SetItemMasterQtyRequiredForCustOrder(orderNum, false);
                myClass.myParse.Globals.mydmupdate.ProcessUpdates();
            }
            CalcEstShipWeight(orderNum);

            return true;
        }

        private static void SetupOrderBatch(string orderNum, string batchNum)
        {
            string updStr = string.Empty;

            // Create new batch header if it doesn't already exist
            string sqlStr = "SELECT NULL FROM BATPKHDR WHERE BATCH_NUM='" + batchNum + "'";
            if (!myClass.myParse.Globals.myDBUtils.ifRecExists(sqlStr))
            {
                updStr = string.Empty;
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "BATCH_NUM", batchNum);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "SITE_ID", siteid);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "CREATE_DATETIME", "GETDATE()");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "CREATE_BY", "IMPORT");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "BATCH_STATUS", "N");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "PCE_TYPE", "X");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "PICK_TYPE", "S");
                myClass.myParse.Globals.mydmupdate.InsertRecord("BATPKHDR", updStr);
            }


            // Get all unique items with total quantities from all orders on this batch. 
            // Update the pick qty in the batch detail record if already exists;
            // otherwise, create new batch detail record.
            sqlStr = "SELECT D.ASCITEMID, D.ITEMID, SUM(D.QTYORDERED) AS TOTALQTY " +
                "FROM ORDRHDR H (NOLOCK) INNER JOIN ORDRDET D (NOLOCK) ON H.ORDERNUMBER=D.ORDERNUMBER " +
                "WHERE H.BATCH_NUM=@batchNum " +
                "GROUP BY D.ASCITEMID, D.ITEMID";
            using (SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
            using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
            {
                conn.Open();
                cmd.Parameters.Add("@batchNum", SqlDbType.VarChar, 100).Value = batchNum;
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string ascItemId = dr["ASCITEMID"].ToString();
                        string itemId = dr["ITEMID"].ToString();
                        string totQty = dr["TOTALQTY"].ToString();

                        sqlStr = "SELECT NULL FROM BATPKDET WHERE BATCH_NUM='" + batchNum + "' AND ASCITEMID='" + ascItemId + "'";
                        if (myClass.myParse.Globals.myDBUtils.ifRecExists(sqlStr))
                        {
                            sqlStr = "UPDATE BATPKDET SET QTY_TO_PICK=" + totQty.ToString() + " WHERE BATCH_NUM='" + batchNum + "' AND ASCITEMID='" + ascItemId + "'";
                            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
                        }
                        else
                        {
                            updStr = String.Empty;
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "BATCH_NUM", batchNum);
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ITEMID", itemId);
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ASCITEMID", ascItemId);
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "PICKED_FLAG", "F");
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "QTY_TO_PICK", totQty);
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "QTY_ALLOC", "0");
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "QTY_PICKED", "0");
                            myClass.myParse.Globals.mydmupdate.InsertRecord("BATPKDET", updStr);
                        }
                    }
                }
            }
            myClass.myParse.Globals.mydmupdate.ProcessUpdates();
        }

        private static void CreateTransferPOFromCO(string orderNum, ASCTracInterfaceModel.Model.CustOrder.OrdrHdrImport aData)
        {
            string itemId, coASCItemId, ascItemId, uom = "";
            string vmiCustId;
            string siteVendorId;
            int lineNum;
            double qtyOrd;
            string reqShipDate;
            DateTime dtReqShipDate;

            string sqlStr = "SELECT PONUMBER, RELEASENUM FROM POHDR WHERE TRANSFER_CO_ORDERNUMBER='" + orderNum + "' ";
            if (!myClass.myParse.Globals.myDBUtils.ifRecExists(sqlStr))
            {
                //sqlStr = "SELECT ORDERTYPE, SITE_ID, SHIPTOCUSTID, REQUIREDSHIPDATE FROM ORDRHDR WHERE ORDERNUMBER='" + orderNum + "' ";
                string orderType = aData.ORDER_TYPE;
                if (orderType == "T")
                {
                    string fromSiteId = siteid;
                    string custId = aData.SHIP_TO_CUST_ID;
                    dtReqShipDate = aData.LEAVES_DATE;
                    reqShipDate = aData.LEAVES_DATE.ToShortDateString();
                    if (!string.IsNullOrEmpty(fromSiteId) && (!string.IsNullOrEmpty(custId)))
                    {
                        string toSiteId = String.Empty;
                        sqlStr = "SELECT SITE_ID FROM SITES WHERE CUSTID='" + custId + "'";
                        if (myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref toSiteId))
                        {

                            if (!string.IsNullOrEmpty(toSiteId))
                            {
                                //added 05-25-16 (JXG)
                                siteVendorId = "";
                                sqlStr = "SELECT VENDORID FROM SITES WHERE SITE_ID='" + fromSiteId + "' ";
                                myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref siteVendorId);

                                string poNum = orderNum;  // myClass.myParse.Globals.myConfig.poOrderMask.GetOrderNum();  //changed 07-11-16 (JXG)

                                String updStr = string.Empty;
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "PONUMBER", poNum);
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "RELEASENUM", "00");
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "SITE_ID", toSiteId);
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "TRANSFER_SITE_ID", fromSiteId);
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "VENDORID", siteVendorId);  //custId
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "TRANSFER_CO_ORDERNUMBER", orderNum);
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "LINKED_ORDERNUMBER", orderNum);
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "RECEIVED", "O");
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ORDERTYPE", "T");
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ORDER_SOURCE", "I");
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ORDERDATE", DateTime.Now.Date.ToString());
                                if (!string.IsNullOrEmpty(reqShipDate))
                                {
                                    DateTime.TryParse(reqShipDate, out dtReqShipDate);
                                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "EXPECTEDRECEIPTDATE", dtReqShipDate.ToString());
                                }
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "CREATE_DATE", "GETDATE()");
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "CREATE_USERID", myClass.myParse.Globals.curUserID);
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "LAST_UPDATE", "GETDATE()");
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "LAST_UPDATE_USERID", myClass.myParse.Globals.curUserID);
                                myClass.myParse.Globals.mydmupdate.InsertRecord("POHDR", updStr);

                                sqlStr = "UPDATE ORDRHDR " +
                                    "SET LINKED_PO_NUM='" + poNum + "', TRANSFER_SITE_ID='" + toSiteId + "' " +
                                    "WHERE ORDERNUMBER='" + orderNum + "' ";
                                myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);

                                sqlStr = "SELECT LINENUMBER, ITEMID, ASCITEMID, QTYORDERED " +
                                    "FROM ORDRDET " +
                                    "WHERE ORDERNUMBER='" + orderNum + "' " +
                                    "ORDER BY LINENUMBER ";
                                using (SqlConnection conn2 = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
                                using (SqlCommand cmd2 = new SqlCommand(sqlStr, conn2))
                                {
                                    conn2.Open();
                                    using (SqlDataReader reader2 = cmd2.ExecuteReader())
                                    {
                                        while (reader2.Read())
                                        {
                                            Int32.TryParse(reader2["LINENUMBER"].ToString(), out lineNum);
                                            itemId = reader2["ITEMID"].ToString();
                                            coASCItemId = reader2["ASCITEMID"].ToString();
                                            double.TryParse(reader2["QTYORDERED"].ToString(), out qtyOrd);

                                            ascLibrary.ascStrUtils.ascGetNextWord(ref coASCItemId, "&");
                                            ascLibrary.ascStrUtils.ascGetNextWord(ref coASCItemId, "&");
                                            vmiCustId = ascLibrary.ascStrUtils.ascGetNextWord(ref coASCItemId, "&");

                                            ascItemId = toSiteId + "&" + itemId + "&" + vmiCustId;
                                            if (!myClass.myParse.Globals.myGetInfo.GetASCItemInfo(ascItemId, "STOCK_UOM", ref uom))
                                            {
                                                Class1.WriteException("ImportOrder", "", orderNum,
                                                    "Item [" + itemId + "] does not exist in Site [" +
                                                        toSiteId + "] for VMI Cust ID [" + vmiCustId + "]. Item will not be added to Transfer PO " + poNum + ".", "");
                                                continue;
                                            }

                                            updStr = string.Empty;
                                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "PONUMBER", poNum);
                                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "RELEASENUM", "00");
                                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "LINENUMBER", lineNum.ToString());
                                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "RECEIVED", "O");
                                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ITEMID", itemId);
                                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ASCITEMID", ascItemId);
                                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "QTY", qtyOrd.ToString());
                                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "UNITMEAS", uom);
                                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "REJECTED", "N");
                                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "QTYOUTOFTOL", "0");
                                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "QTYRECEIVED", "0");
                                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "QTYLASTRECV", "0");
                                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "LINETOTAL", "0");
                                            myClass.myParse.Globals.mydmupdate.InsertRecord("PODET", updStr);
                                            myClass.myParse.Globals.mydmupdate.ProcessUpdates();

                                            // Now increment the item qty expected to use the new line qty
                                            myClass.myParse.Globals.mydmupdate.SetItemMasterQtyExpected(poNum, "00", lineNum, false);
                                            myClass.myParse.Globals.mydmupdate.ProcessUpdates();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }


        //private static void ImportOrderLotAlloc(string orderNum)
        //{
        //}
        private static void SetPickAssignments(string orderNum)
        {
            // I think this was old Celebrate express logic.  the logic in DeterminePickLoc is most up to date, and should be used instead
            /*
            string sqlStr, statFlag, pickStatus = "";

            string lightFlag = myClass.myParse.Globals.myConfig.iniPlanLightCreateLogic.Value;
            if (lightFlag == "D" || lightFlag == "P")
            {
                sqlStr = "SELECT TOP 1 ORDERNUMBER FROM PICK_ASSIGNMENTS (NOLOCK) " +
                    "WHERE ORDERNUMBER='" + orderNum + "' AND STATUS_FLAG NOT IN ('H','R')";
                if (!myClass.myParse.Globals.myDBUtils.ifRecExists(sqlStr))
                {
                    myClass.myParse.Globals.myGetInfo.GetOrderInfo(orderNum, "PICKSTATUS", ref pickStatus);
                    if (lightFlag == "D" || pickStatus == "G" || pickStatus == "L")
                        statFlag = "R";
                    else
                        statFlag = "H";

                    // Initially set pick type to 'S' as default for details
                    sqlStr = "UPDATE ORDRDET SET PICK_TYPE='S' WHERE ORDERNUMBER='" + orderNum + "'";
                    myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);

                    // Set pick type to 'L' for the detail lines that have 
                    // items with Loc.Pick_Assignment_Flag of 'A' or 'L'
                    sqlStr = "UPDATE ORDRDET SET PICK_TYPE='L' FROM LOC (NOLOCK) " +
                            "WHERE LOC.ASCITEMID=ORDRDET.ASCITEMID " +
                            "AND LOC.PICK_ASSIGNMENT_FLAG IN ('A', 'L') " +
                            "AND ORDRDET.ORDERNUMBER='" + orderNum + "'";
                    myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);

                    // Delete old pick assignment recs for this order
                    sqlStr = "DELETE FROM PICK_ASSIGNMENTS WHERE ORDERNUMBER='" + orderNum + "'";
                    myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);

                    // Insert new pick assignments
                    sqlStr = "INSERT INTO PICK_ASSIGNMENTS " +
                            "(DATE_TIME_CREATED, TYPE_OF_PICK, STATUS_FLAG, ASSIGNMENT_NUMBER, " +
                            "PICK_SEQUENCE_NO, QUANTITY_TO_PICK, ITEMID, ORDER_TYPE, " +
                            "TRIGGER_A_REPLEN, SLOT, AISLE, PALLETIZE, NINE_MONTH_INTERNATIONAL,FULL_CASE, " +
                            "GOAL_TIME, DELIVERY_DATE,BASE_ITEM_OVERRIDE, " +
                            "ORDERNUMBER, ZONEID, LOCATIONID, SITE_ID) " +
                            "SELECT GetDate(), 'L', '" + statFlag + "', '0', " +
                            "D.LINENUMBER, D.QTYORDERED-D.QTYPICKED, D.ITEMID, 'C', " +
                            "'F', '-', '-', 'F', 'F', 'F', 0, GetDate(), 'F', " +
                            "D.ORDERNUMBER, MIN(L.ZONEID), MIN(L.LOCATIONID), MIN(L.SITE_ID) " +
                            "FROM ORDRDET D (NOLOCK), LOC L (NOLOCK), ITEMMSTR I (NOLOCK) " +
                            "WHERE L.ASCITEMID = D.ASCITEMID " +
                            "AND I.ASCITEMID = D.ASCITEMID " +
                            "AND I.PURORMFG <> 'K' " +
                            "AND L.PICK_ASSIGNMENT_FLAG IN ('A', 'L') " +
                            "AND D.ORDERNUMBER = '" + orderNum + "' " +
                            "GROUP BY D.LINENUMBER, D.QTYORDERED, D.QTYPICKED, D.ITEMID, D.ORDERNUMBER";
                    myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);

                    // Set the ordrdet.pick_location for the high velocity (L type) items
                    sqlStr = "UPDATE ORDRDET SET PICK_LOCATION = PA.LOCATIONID " +
                            "FROM PICK_ASSIGNMENTS PA " +
                            "WHERE PA.ORDERNUMBER=ORDRDET.ORDERNUMBER " +
                            "AND PA.PICK_SEQUENCE_NO=ORDRDET.LINENUMBER " +
                            "AND ORDRDET.ORDERNUMBER='" + orderNum + "'";
                    myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);

                    // Above, the pick assignments table was filled for any items matching the
                    // pick type L. Further above, all rows were set to a pick type of S by default, 
                    // so at this point we have to set the pick location for S types, filtering out kits
                    sqlStr = "UPDATE ORDRDET SET PICK_LOCATION = LOC.LOCATIONID " +
                            "FROM LOC (NOLOCK), ITEMMSTR I (NOLOCK) " +
                            "WHERE LOC.ASCITEMID=ORDRDET.ASCITEMID " +
                            "AND ORDRDET.ASCITEMID = I.ASCITEMID AND I.PURORMFG <> 'K' " +
                            "AND ORDRDET.PICK_TYPE='S' AND ORDRDET.ORDERNUMBER='" + orderNum + "'";
                    myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
                    myClass.myParse.Globals.mydmupdate.ProcessUpdates();

                    // Set ordrhdr.pick_type
                    //  If all ordrdet 'L', ordrhdr 'L'
                    //  If all ordrdet 'S', ordrhdr 'S'
                    //  If ordrdet mixed, ordrhdr is null (default in the table)
                    // Filter out kits
                    sqlStr = "SELECT D.ORDERNUMBER FROM ORDRDET D (NOLOCK), ITEMMSTR I (NOLOCK) " +
                            "WHERE D.ASCITEMID = I.ASCITEMID AND I.PURORMFG <> 'K'" +
                            "AND D.ORDERNUMBER='" + orderNum + "' " +
                            "AND (D.PICK_TYPE IS NULL OR D.PICK_TYPE<>'L')";
                    if (!myClass.myParse.Globals.myDBUtils.ifRecExists(sqlStr))
                    {
                        // All non-kit items have pick type 'L', so set header pick type to 'L'
                        sqlStr = "UPDATE ORDRHDR SET PICK_TYPE='L' WHERE ORDERNUMBER='" + orderNum + "'";
                        myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
                    }

                    // Set header pick type to S for orders with no lightning pick lines
                    sqlStr = "UPDATE H SET PICK_TYPE='S' " +
                        "FROM ORDRHDR H LEFT JOIN ORDRDET D " +
                        "ON H.ORDERNUMBER=D.ORDERNUMBER AND D.PICK_TYPE='L' " +
                        "WHERE D.ORDERNUMBER IS NULL " +
                        "AND (H.PICK_TYPE IS NULL OR H.PICK_TYPE='L') " +
                        "AND H.ORDERNUMBER='" + orderNum + "'";
                    myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);

                    // Set ordrhdr.pick_first_zoneid and pick_first_locationid
                    // Join in loc table and sort by zone id
                    sqlStr = "UPDATE ORDRHDR SET PICK_FIRST_ZONEID = " +
                            "(SELECT TOP 1 L.ZONEID FROM ORDRDET D, LOC L " +
                            "WHERE D.ORDERNUMBER='" + orderNum + "' AND D.PICK_LOCATION NOT LIKE 'Z%' " +
                            "AND D.PICK_LOCATION IS NOT NULL AND L.LOCATIONID=D.PICK_LOCATION " +
                            "AND L.SITE_ID='" + siteid + "' ORDER BY L.ZONEID), " +
                            "PICK_FIRST_LOCATIONID = (SELECT TOP 1 D.PICK_LOCATION FROM ORDRDET D, LOC L " +
                            "WHERE D.ORDERNUMBER='" + orderNum + "' AND D.PICK_LOCATION NOT LIKE 'Z%' " +
                            "AND D.PICK_LOCATION IS NOT NULL AND L.LOCATIONID=D.PICK_LOCATION " +
                            "ORDER BY L.ZONEID) " +
                            "WHERE ORDERNUMBER='" + orderNum + "'";
                    myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
                }
            }
            */
        }

        private static void AfterOrderImport(string orderNum, bool isNewOrder)
        {
            string sql, tmpStr = "";
            string printerId = "";
            bool autoprintPickList, autoprintPackList;

            bool fOK = true;
            myClass.myParse.Globals.myGetInfo.GetOrderInfo(orderNum, "PICKSTATUS", ref tmpStr);

            // Check the order reports table to see if this order should have a 
            // pick list and/or pack list printed.
            string autoFlag = myClass.myParse.Globals.myGetInfo.getorderreportdata("K", "O", orderNum, "AUTOPRINT_FLAG");
            autoprintPickList = autoFlag != "N" && !String.IsNullOrEmpty(autoFlag);
            autoFlag = myClass.myParse.Globals.myGetInfo.getorderreportdata("P", "O", orderNum, "AUTOPRINT_FLAG");
            autoprintPackList = autoFlag != "N" && !String.IsNullOrEmpty(autoFlag);

            if (currCOImportConfig.GWCOPurgeHeaderWithNoLines)
            {
                if (tmpStr.Equals("N") && !myClass.myParse.Globals.myDBUtils.ifRecExists("SELECT TOP 1 LINENUMBER FROM ORDRDET WHERE ORDERNUMBER='" + orderNum + "'"))
                {
                    myClass.myParse.Globals.mydmupdate.AddToUpdate("DELETE FROM ORDRHDR WHERE ORDERNUMBER='" + orderNum + "'");
                    fOK = false;
                }
            }
            if (fOK)
            {
                if (currCOImportConfig.GWLogChangedOrderTranfile)
                {
                    string errmsg = string.Empty;
                    myClass.myParse.Globals.LogTrans.LogTranToOrder(DateTime.Now, orderNum, ascLibrary.dbConst.ltCHANGEORDER, "", tmpStr, "", ref errmsg);
                }

                // Now check the priority table to determine if the print pick and pack
                // lists apply based on order priority id.
                try
                {
                    myClass.myParse.Globals.dmCustOrder.RunAutoOrderFunctions(orderNum, currCOImportConfig.CPSetORDRDETPickLocOnImport, currCOImportConfig.GWUseStandardKitExplosion, isNewOrder, ref autoprintPickList, ref printerId, ref autoprintPackList);
                }
                catch (Exception e1)
                {
                    Class1.WriteException("ImportOrder", "AfterOrderImport", orderNum, e1.ToString(), string.Empty);
                }

                sql = "UPDATE ORDRDET SET ORDERFILLED='T' WHERE ORDERNUMBER='" + orderNum + "' AND ASCITEMID IN ( SELECT ASCITEMID FROM ITEMMSTR WHERE PURORMFG='K')";
                myClass.myParse.Globals.mydmupdate.AddToUpdate(sql);

                //if (CORateShopOnImport)  //taken out 09-17-20 (JXG) can't get caall to Manifest.dll to work
                //    ASCTracFunctions.iManifest.DoRateShop(orderNum);

                if (autoprintPickList)
                {
                    if (printerId == "")
                        printerId = myClass.myParse.Globals.myGetInfo.getorderreportdata("K", "O", orderNum, "AUTOPRINTERID");

                    // Create record so print server will print the pick list
                    sql = "INSERT INTO DUP_SKID (REQUEST_TIME, LOTID, MISC2, PRINTERID, SITE_ID, " +
                        "USERID, PROCESS, PRIORITY, ORIG_SKIDID, TRANTYPE) VALUES (GETDATE(), '" + orderNum + "', " +
                        "'K', '" + printerId + "', '" + siteid + "', 'ADMIN', 'T', '10', '" + orderNum + "', 'BP')";
                    myClass.myParse.Globals.mydmupdate.AddToUpdate(sql);
                }

                if (autoprintPackList)
                {
                    printerId = myClass.myParse.Globals.myGetInfo.getorderreportdata("P", "O", orderNum, "AUTOPRINTERID");

                    // Create record so print server will print the pack list
                    sql = "INSERT INTO DUP_SKID (REQUEST_TIME, LOTID, MISC2, PRINTERID, SITE_ID, " +
                        "USERID, PROCESS, PRIORITY, ORIG_SKIDID, TRANTYPE) VALUES (GETDATE(), '" + orderNum + "', " +
                        "'P', '" + printerId + "', '" + siteid + "', 'ADMIN', 'T', '10', '" + orderNum + "', 'BP')";
                    myClass.myParse.Globals.mydmupdate.AddToUpdate(sql);
                }
            }

        }

        public static HttpStatusCode doImportCustOrderConfirmShip( string aOrderNum, ref string aErrMsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            try
            {
                string siteId = string.Empty;
                myClass.myParse.Globals.myGetInfo.GetOrderInfo(aOrderNum, "SITE_ID", ref siteId);
                //Service1.Parse.Globals.curSiteID = siteId;
                myClass.myParse.Globals.initsite(siteId);
                myClass.myParse.Globals.mydmupdate.InitUpdate();
                ascLibrary.TDBReturnType ret = myClass.myParse.Globals.dmConfShip.confirmshiporder(aOrderNum, "", "T", false, false, true, DateTime.Now);
                if (ret == ascLibrary.TDBReturnType.dbrtOK)
                {
                    myClass.myParse.Globals.mydmupdate.ProcessUpdates();
                }
                else
                {
                    retval = HttpStatusCode.BadRequest;
                    aErrMsg= "Error confirm shipping order " + aOrderNum + ": " + ParseNet.dmascmessages.GetErrorMsg(ret);
                }
            }
            catch (Exception ex)
            {
                Class1.WriteException(funcType, aOrderNum, aOrderNum, ex.ToString(), "");
                retval = HttpStatusCode.BadRequest;
                aErrMsg= ex.Message;
            }

            return (retval);
        }
    }
}