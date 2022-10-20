using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Exports
{
    public class ExportParcel
    {
        private static string funcType = "EX_PARC";
        private static Class1 myClass;
        private static Model.CustOrder.ParcelExportConfig currExportConfig;

        public static HttpStatusCode doExportParcel(ASCTracInterfaceModel.Model.CustOrder.ParcelExporFilter aExportfilter, ref List<ASCTracInterfaceModel.Model.CustOrder.ParcelExport> aData, ref string errmsg)
        {
            myClass = Class1.InitParse(funcType, ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            aData = new List<ASCTracInterfaceModel.Model.CustOrder.ParcelExport>();
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
                        currExportConfig = Configs.ParcelConfig.getExportSite("1", myClass.myParse.Globals);
                        sqlstr = BuildExportSQL(aExportfilter, ref errmsg);
                        if (!String.IsNullOrEmpty(sqlstr))
                        {
                            retval = BuildExportList(sqlstr, ref aData, ref errmsg);
                            BuildShipmentList(ref aData, ref errmsg);
                            if (aData.Count == 0)
                                retval = HttpStatusCode.NoContent;
                            else
                                retval = HttpStatusCode.OK;
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

        private static string BuildExportSQL(ASCTracInterfaceModel.Model.CustOrder.ParcelExporFilter aExportFilter, ref string errmsg)
        {
            string postedFlagField = currExportConfig.postedFlagField;
            string sqlStr = "SELECT P.*, OH.SALESORDERNUMBER FROM PARCEL P (NOLOCK) " +
                "LEFT JOIN ORDRHDR OH (NOLOCK) ON OH.ORDERNUMBER=P.ORDERNUMBER " +
                "LEFT JOIN CUST C (NOLOCK) ON C.CUSTID=OH.SOLDTOCUSTID " +
                "WHERE P.VOID<>'Y' AND P.VOID<>'T' AND P.EXPORT_TO_HOST<>'T' ";
            if (!String.IsNullOrEmpty(aExportFilter.CustID))
                sqlStr += "AND (OH.SOLDTOCUSTID='" + aExportFilter.CustID + "' OR C.CLIENT_ID_ASSOCIATION='" + aExportFilter.CustID + "') ";

            sqlStr += "ORDER BY P.TRANS_DATE";
            return (sqlStr);
        }

        private static HttpStatusCode BuildExportList(string sqlstr, ref List<ASCTracInterfaceModel.Model.CustOrder.ParcelExport> aData, ref string errmsg)
        {
            bool fExportByLot = true; // fExportByLot

            HttpStatusCode retval = HttpStatusCode.NoContent;
            SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString);
            SqlCommand cmd = new SqlCommand(sqlstr, conn);
            conn.Open();
            SqlDataReader drParcel = cmd.ExecuteReader();

            myClass.myParse.Globals.mydmupdate.InitUpdate();
            try
            {
                while (drParcel.Read())
                {
                    retval = HttpStatusCode.OK;

                    var orderNum = drParcel["ORDERNUMBER"].ToString();
                    var packageId = drParcel["PACKAGE_ID"].ToString();
                    var containerId = drParcel["CONTAINER_ID"].ToString();  //added 05-10-16 (JXG) for Alto Systems
                    var salesOrderNum = drParcel["SALESORDERNUMBER"].ToString();
                    string hostSiteId = string.Empty;
                    myClass.myParse.Globals.myGetInfo.GetSiteInfo(drParcel["SITE_ID"].ToString(), "HOST_SITE_ID", ref hostSiteId);

                    //added 05-10-16 (JXG) for Alto Systems
                    //post 1 parcel record per orderpkg record
                    string ascItemIds = "";
                    if (currExportConfig.includePackoutItemsInParcelsExport)
                    {
                        if (string.IsNullOrEmpty(ascItemIds))  //added 08-05-21 (JXG)
                        {
                            string sqlStr = "SELECT C.LINENUMBER AS PICK_LINE_NUM, C.ASCITEMID ";
                            if (fExportByLot)  //added 06-08-21 (JXG) for Hikma
                                sqlStr += ", C.LOTID, SUM(C.QTY) AS CONTR_QTY ";
                            sqlStr += "FROM ORDERPKG C ";
                            sqlStr += "WHERE C.ORDERNUM='" + orderNum + "' ";
                            sqlStr += "AND C.CONTAINER_ID='" + containerId + "' ";
                            sqlStr += "GROUP BY C.LINENUMBER, C.ASCITEMID ";
                            if (fExportByLot)  //added 06-08-21 (JXG) for Hikma
                                sqlStr += ", C.LOTID ";
                            sqlStr += "ORDER BY C.LINENUMBER, C.ASCITEMID ";
                            using (SqlConnection conn3 = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
                            using (SqlCommand cmd3 = new SqlCommand(sqlStr, conn3))
                            {
                                conn3.Open();
                                using (SqlDataReader dr3 = cmd3.ExecuteReader())
                                {
                                    while (dr3.Read())
                                    {
                                        ascItemIds += dr3["PICK_LINE_NUM"].ToString() + "|" + dr3["ASCITEMID"].ToString() + "|";
                                        //added 06-08-21 (JXG) for Hikma
                                        if (fExportByLot)  //added 06-08-21 (JXG) for Hikma
                                        {
                                            string lotId = dr3["LOTID"].ToString();
                                            if (string.IsNullOrEmpty(lotId)  //added 10-05-21 (JXG)
                                                && !string.IsNullOrEmpty(dr3["PICK_LINE_NUM"].ToString())  //added 10-13-21 (JXG)
                                                && !string.IsNullOrEmpty(containerId))  //added 10-13-21 (JXG)
                                            {
                                                sqlStr = "SELECT LOTID";
                                                sqlStr += " FROM ORDERPKG_DET";
                                                sqlStr += " WHERE ORDERNUM='" + orderNum + "'";
                                                sqlStr += " AND LINENUMBER=" + dr3["PICK_LINE_NUM"].ToString();
                                                sqlStr += " AND CONTAINER_ID='" + containerId + "'";
                                                myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref lotId);
                                            }
                                            ascItemIds += lotId + "|" + dr3["CONTR_QTY"].ToString() + "|";
                                        }
                                        else
                                            ascItemIds += "" + "|" + "0" + "|";
                                    }
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(ascItemIds))
                        {
                            string sqlStr = "SELECT ISNULL(LI.PICKLINENUM, OL.PICKLINENUM) AS PICK_LINE_NUM, C.ASCITEMID ";
                            if (fExportByLot)  //added 06-08-21 (JXG) for Hikma
                                sqlStr += ", C.LOTID, SUM(C.QTY) AS CONTR_QTY ";
                            sqlStr += "FROM CONTAINR C ";
                            sqlStr += "LEFT JOIN LOCITEMS LI ON LI.PICKORDERNUM=C.ORDERNUM AND LI.SKIDID=C.SKIDID ";
                            sqlStr += "LEFT JOIN OLDLCITM OL ON OL.PICKORDERNUM=C.ORDERNUM AND OL.SKIDID=C.SKIDID ";
                            sqlStr += "WHERE C.ORDERNUM='" + orderNum + "' ";
                            sqlStr += "AND C.CONTAINER_ID='" + containerId + "' ";
                            sqlStr += "GROUP BY LI.PICKLINENUM, OL.PICKLINENUM, C.ASCITEMID ";
                            if (fExportByLot)  //added 06-08-21 (JXG) for Hikma
                                sqlStr += ", C.LOTID ";
                            sqlStr += "ORDER BY LI.PICKLINENUM, OL.PICKLINENUM, C.ASCITEMID ";
                            //sqlStr = "SELECT LINENUMBER, ASCITEMID " +
                            //    "FROM ORDERPKG " +
                            //    "WHERE ORDERNUM='" + orderNum + "' " +
                            //    "AND CONTAINER_ID='" + containerId + "' " +
                            //    "GROUP BY LINENUMBER, ASCITEMID ";
                            using (SqlConnection conn2 = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
                            using (SqlCommand cmd2 = new SqlCommand(sqlStr, conn2))
                            {
                                conn2.Open();
                                using (SqlDataReader dr2 = cmd2.ExecuteReader())
                                {
                                    while (dr2.Read())
                                    {
                                        ascItemIds += dr2["PICK_LINE_NUM"].ToString() + "|" + dr2["ASCITEMID"].ToString() + "|";
                                        //added 06-08-21 (JXG) for Hikma
                                        if (fExportByLot)
                                            ascItemIds += dr2["LOTID"].ToString() + "|" + dr2["CONTR_QTY"].ToString() + "|";
                                        else
                                            ascItemIds += "" + "|" + "0" + "|";
                                        ////////////////////////////////
                                    }
                                }
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(ascItemIds))
                        ascItemIds = "||||";

                    while (!string.IsNullOrEmpty(ascItemIds))
                    {
                        var lineNum = ascLibrary.ascStrUtils.GetNextWord(ref ascItemIds);  //added 05-25-16 (JXG) for Alto Systems
                        var ascItemId = ascLibrary.ascStrUtils.GetNextWord(ref ascItemIds);
                        var lotId = ascLibrary.ascStrUtils.GetNextWord(ref ascItemIds);  //added 06-08-21 (JXG) for Hikma
                        var contrQty = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref ascItemIds), 0);
                        //////////////////////////////////////////

                        ASCTracInterfaceModel.Model.CustOrder.ParcelExport rec = new ASCTracInterfaceModel.Model.CustOrder.ParcelExport();
                        rec.CREATE_DATETIME = ascLibrary.ascUtils.ascStrToDate(drParcel["TRANS_DATE"].ToString(), DateTime.Now);
                        rec.FACILITY = hostSiteId;
                        rec.ORDERNUMBER = orderNum;
                        rec.PARCEL_NUMBER = packageId;
                        rec.CARRIER = drParcel["CARRIER"].ToString();
                        rec.FREIGHT_COST = ascLibrary.ascUtils.ascStrToDouble(drParcel["COST"].ToString(), 0);
                        rec.CUST_FREIGHT_COST = ascLibrary.ascUtils.ascStrToDouble(drParcel["SHIPCUSTCOST"].ToString(), 0);
                        rec.WEIGHT = ascLibrary.ascUtils.ascStrToDouble(drParcel["WEIGHT"].ToString(), 0);
                        rec.SHIPDATE = ascLibrary.ascUtils.ascStrToDate(drParcel["SHIPDATETIME"].ToString(), DateTime.MinValue);
                        rec.TRACKING_NUMBER = drParcel["TRACKING_NUM"].ToString();
                        rec.CARRIER_SERVICE_CODE = drParcel["SHIPPER"].ToString();
                        rec.SALESORDERNUMBER = salesOrderNum;
                        rec.USERID = drParcel["USERID"].ToString();
                        if (!string.IsNullOrEmpty(ascItemId))  //added 05-10-16 (JXG) for Alto Systems
                        {
                            string itemDesc = myClass.myParse.Globals.myGetInfo.GetASCItemData(ascItemId, "ITEMID,DESCRIPTION");
                            var itemId = ascLibrary.ascStrUtils.GetNextWord(ref itemDesc);
                            rec.ITEMID = itemId;
                            rec.ASCITEMID = ascItemId;
                            rec.ITEM_DESC = itemDesc;
                            rec.LINENUMBER = ascLibrary.ascUtils.ascStrToDouble(lineNum, 0);
                        }
                        else if (!string.IsNullOrEmpty(drParcel["ITEMID"].ToString()))  //added 11-18-15 (JXG) for Alto
                        {
                            string itemDesc = myClass.myParse.Globals.myGetInfo.GetASCItemData(drParcel["ASCITEMID"].ToString(), "DESCRIPTION");
                            rec.ITEMID = drParcel["ITEMID"].ToString();
                            rec.ASCITEMID = drParcel["ASCITEMID"].ToString();
                            rec.ITEM_DESC = itemDesc;
                        }
                        if (fExportByLot)  //added 06-08-21 (JXG) for Hikma
                        {
                            rec.LOT_ID = lotId;
                            rec.QTY = contrQty;
                        }
                        aData.Add(rec);
                    }

                    PostParcel(orderNum, packageId, "S", "");

                }
                if (retval == HttpStatusCode.OK)
                    myClass.myParse.Globals.mydmupdate.ProcessUpdates();

            }
            finally
            {
                drParcel.Close();
                cmd.Dispose();
                conn.Close();
                conn.Dispose();
            }
            return (retval);
        }

        private static void BuildShipmentList(ref List<ASCTracInterfaceModel.Model.CustOrder.ParcelExport> aData, ref string errmsg)
        {
            string orderNum, proNum, shipmentId, trailerId, salesOrderNum = "";
            bool useTrailerShipments = (myClass.myParse.Globals.myConfig.iniCPInputTrailer.Value != "F");

            if (useTrailerShipments)
            {
                string sqlStr = "SELECT S.HOST_SITE_ID, SH.PRO_NUM, SH.ORDERNUM, SH.TRAILER_NUM, SH.SHIPMENT_ID, " +
                    "CARRIER, SHIPPER, ISNULL(FREIGHT,0) AS FREIGHT, ISNULL(SHIPCOST,0) AS SHIPCOST, SHIP_DATETIME " +
                    "FROM SHIPMENT SH (NOLOCK), SITES S (NOLOCK) " +
                    "WHERE SH.SITE_ID=S.SITE_ID AND ISNULL(SH.EXPORT, 'F') = 'F' " +
                    "ORDER BY SH.ORDERNUM";
                using (SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
                using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            orderNum = dr["ORDERNUM"].ToString();
                            proNum = dr["PRO_NUM"].ToString();
                            shipmentId = dr["SHIPMENT_ID"].ToString();
                            trailerId = dr["TRAILER_NUM"].ToString();
                            myClass.myParse.Globals.myGetInfo.GetOrderInfo(orderNum, "SALESORDERNUMBER", ref salesOrderNum);

                            if (!string.IsNullOrEmpty(proNum)  //added 05-04-16 (JXG) for Alto Systems  //taken out 09-09-16 (JXG) for ND Mill
                                || currExportConfig.exportProNumAsTrackNumWhenBlank)  //added 08-16-21 (KXG)
                            {
                                ASCTracInterfaceModel.Model.CustOrder.ParcelExport rec = new ASCTracInterfaceModel.Model.CustOrder.ParcelExport();

                                rec.CREATE_DATETIME = DateTime.Now;
                                rec.FACILITY = dr["HOST_SITE_ID"].ToString();
                                rec.ORDERNUMBER = orderNum;
                                rec.PARCEL_NUMBER = proNum;
                                rec.CARRIER = dr["CARRIER"].ToString();
                                rec.CARRIER_SERVICE_CODE = dr["SHIPPER"].ToString();
                                rec.FREIGHT_COST = ascLibrary.ascUtils.ascStrToDouble(dr["FREIGHT"].ToString(), 0);
                                rec.CUST_FREIGHT_COST = ascLibrary.ascUtils.ascStrToDouble(dr["SHIPCOST"].ToString(), 0);
                                rec.WEIGHT = 0;  //added 05-04-16 (JXG) for Alto Systems
                                rec.SHIPDATE = ascLibrary.ascUtils.ascStrToDate(dr["SHIP_DATETIME"].ToString(), DateTime.MinValue);
                                rec.TRACKING_NUMBER = proNum;
                                rec.SALESORDERNUMBER = salesOrderNum;
                                rec.SEAL_NUMBERS = GetSealNums(orderNum, shipmentId);
                                aData.Add(rec);
                            }

                            SetShipmentPosted(orderNum, trailerId, "S", "");
                            sqlStr = "UPDATE SHIPMENT SET EXPORT='T' " +
                                "WHERE ORDERNUM='" + orderNum + "' AND TRAILER_NUM='" + trailerId + "'";
                        }
                    }
                }
            }
            else
            {
                string sqlStr = "SELECT HOST_SITE_ID, PRO_NUM, ORDERNUMBER, CARRIER, SHIPDATE, CARRIER_SERVICE_CODE, " +
                    "ISNULL(SHIPCOST,0) AS SHIPCOST, ISNULL(SHIPCUSTCOST,0) AS SHIPCUSTCOST, SALESORDERNUMBER " +
                    "FROM ORDRHDR H (NOLOCK), SITES S (NOLOCK) " +
                    "WHERE PICKSTATUS='C' AND H.SITE_ID=S.SITE_ID AND ISNULL(H.SHIPMENT_EXPORT, 'F') = 'F' " +
                    "ORDER BY ORDERNUMBER";
                using (SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
                using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            orderNum = dr["ORDERNUMBER"].ToString();
                            proNum = dr["PRO_NUM"].ToString();

                            if (!string.IsNullOrEmpty(proNum)  //added 05-04-16 (JXG) for Alto Systems  //taken out 09-09-16 (JXG) for ND Mill
                                || currExportConfig.exportProNumAsTrackNumWhenBlank)  //added 08-16-21 (KXG)
                            {
                                ASCTracInterfaceModel.Model.CustOrder.ParcelExport rec = new ASCTracInterfaceModel.Model.CustOrder.ParcelExport();

                                rec.CREATE_DATETIME = DateTime.Now;
                                rec.FACILITY = dr["HOST_SITE_ID"].ToString();
                                rec.ORDERNUMBER = orderNum;
                                rec.PARCEL_NUMBER = proNum;
                                rec.CARRIER = dr["CARRIER"].ToString();
                                rec.CARRIER_SERVICE_CODE = dr["CARRIER_SERVICE_CODE"].ToString();
                                rec.FREIGHT_COST = ascLibrary.ascUtils.ascStrToDouble(dr["SHIPCOST"].ToString(), 0);
                                rec.CUST_FREIGHT_COST = ascLibrary.ascUtils.ascStrToDouble(dr["SHIPCUSTCOST"].ToString(), 0);
                                rec.WEIGHT = 0;  //added 05-04-16 (JXG) for Alto Systems
                                rec.SHIPDATE = ascLibrary.ascUtils.ascStrToDate(dr["SHIPDATE"].ToString(), DateTime.MinValue);
                                rec.TRACKING_NUMBER = proNum;
                                rec.SALESORDERNUMBER = dr["SALESORDERNUMBER"].ToString();
                                rec.SEAL_NUMBERS = GetSealNums(orderNum, "");
                                aData.Add(rec);
                            }

                            SetShipmentPosted(orderNum, "", "S", "");
                        }
                    }
                }
            }
        }

        private static string GetSealNums(string orderNum, string shipmentId)
        {
            string sealNums = string.Empty;
            string sqlStr = "SELECT SEAL_NUM FROM ORDR_SEALS (NOLOCK) " +
                "WHERE ORDERNUMBER='" + orderNum + "' ";
            if (!String.IsNullOrEmpty(shipmentId))
                sqlStr += "AND SHIPMENT_ID='" + shipmentId + "'";
            using (SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
            using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
            {
                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        sealNums += dr["SEAL_NUM"].ToString() + "|";
                }

                // Trim if greater than 120
                sealNums = (sealNums.Length > 120) ? sealNums.Substring(0, 120) : sealNums;
            }
            return sealNums;
        }



        private static void PostParcel(string orderNum, string packageId, string newStatus, string ErrorMsg)
        {
            string sql = "UPDATE PARCEL SET EXPORT_TO_HOST='" + newStatus + "', EXPORT_TO_HOST_DATE=GetDate() " +
                "WHERE ORDERNUMBER='" + orderNum + "' AND PACKAGE_ID='" + packageId + "'";
            myClass.myParse.Globals.mydmupdate.AddToUpdate(sql);
        }

        private static void SetShipmentPosted(string orderNum, string trailerId, string newStatus, string ErrorMsg)
        {
            string sqlStr = "UPDATE SHIPMENT SET EXPORT='" + newStatus + "' " +
                "WHERE ORDERNUM='" + orderNum + "' ";
            if (!String.IsNullOrEmpty(trailerId))
                sqlStr += " ADN TRAILER_NUM='" + trailerId + "'";
            if (newStatus == "S")
                sqlStr += " AND ISNULL( EXPORT='F') = 'F'";
            else
                sqlStr += " AND EXPORT='S'";
            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
        }


        public static HttpStatusCode UpdateExport(List<ASCTracInterfaceModel.Model.CustOrder.ParcelExport> aData, ref string errmsg)
        {
            myClass = Class1.InitParse("Update" + funcType, ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            string OrderNum = string.Empty;
            try
            {
                if (myClass != null)
                {
                    myClass.myParse.Globals.mydmupdate.InitUpdate();
                    currExportConfig = Configs.ParcelConfig.getExportSite("1", myClass.myParse.Globals);
                    retval = DoUpdateExport(aData, ref errmsg);
                    if (retval == HttpStatusCode.OK)
                        myClass.myParse.Globals.mydmupdate.ProcessUpdates();
                }
                else
                    retval = HttpStatusCode.InternalServerError;
            }
            catch (Exception ex)
            {
                Class1.WriteException(funcType, Newtonsoft.Json.JsonConvert.SerializeObject(aData), OrderNum, ex.Message, ex.StackTrace);
                retval = HttpStatusCode.BadRequest;
                errmsg = ex.Message;
            }
            return (retval);
        }

        public static HttpStatusCode DoUpdateExport(List<ASCTracInterfaceModel.Model.CustOrder.ParcelExport> aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            foreach (var rec in aData)
            {
                string posted = "T";
                if (!rec.Successful)
                    posted = "E";
                PostParcel(rec.ORDERNUMBER, rec.PARCEL_NUMBER, posted, "");
                SetShipmentPosted(rec.ORDERNUMBER, "", posted, "");
            }
            return (retval);
        }


    }
}