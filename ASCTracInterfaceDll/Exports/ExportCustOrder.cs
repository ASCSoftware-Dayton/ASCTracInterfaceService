using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Exports
{
    public class ExportCustOrder
    {
        private static string funcType = "EX_ORDER";
        private static Class1 myClass;
        private static Model.CustOrder.COExportConfig currExportConfig;

        public static HttpStatusCode doExportCustOrders(ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter aCOExportfilter, ref List<ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport> aData, ref string errmsg)
        {
            myClass = Class1.InitParse(funcType, ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            aData = new List<ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport>();
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
                        currExportConfig = Configs.CustOrderConfig.getCOExportSite("1", myClass.myParse.Globals);
                        sqlstr = BuildCustOrderExportSQL(aCOExportfilter, ref errmsg);
                        if (!String.IsNullOrEmpty(sqlstr))
                        {
                            retval = BuildExportList(sqlstr, aCOExportfilter.MaxRecords, ref aData, ref errmsg);
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

        private static string BuildCustOrderExportSQL(ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter aExportFilter, ref string errmsg)
        {
            string postedFlagField = currExportConfig.postedFlagField;
            string sql = "SELECT SITES.HOST_SITE_ID, TRANFILE.ORDERNUM, TRANFILE.SHIPMENT_ID, TRANFILE.ID, ORDRHDR.CARRIER, ORDRHDR.CARRIER_SERVICE_CODE" +
                ", TRANFILE.USERID, TRANFILE.TRANDATE, ORDRHDR.ORDERTYPE, ORDRHDR.SOLDTOCUSTID, ORDRHDR.ORDER_SOURCE, ORDRHDR.ORDER_SOURCE_SYSTEM, ORDRHDR.SHIPVIA, ORDRHDR.CUSTOM_DATA1 " +
                ", ORDRHDR.SALESORDERNUMBER, ORDRHDR.CUSTPONUM, ORDRHDR.HOST_ORDER_TYPE" +
                ", ORDRHDR.CUSTOM_DATA1, ORDRHDR.CUSTOM_DATA2, ORDRHDR.CUSTOM_DATA3, ORDRHDR.CUSTOM_DATA4, ORDRHDR.CUSTOM_DATA5, ORDRHDR.CUSTOM_DATA6, ORDRHDR.CUSTOM_DATA7" +
                ", ORDRHDR.CUSTOM_DATA8, ORDRHDR.CUSTOM_DATA9, ORDRHDR.CUSTOM_DATA10, ORDRHDR.CUSTOM_DATA11, ORDRHDR.CUSTOM_DATA12" +
                " FROM TRANFILE (NOLOCK) " +
                " JOIN SITES (NOLOCK) ON SITES.SITE_ID=TRANFILE.SITE_ID " +
                " JOIN ORDRHDR (NOLOCK) ON ORDRHDR.ORDERNUMBER=TRANFILE.ORDERNUM " +
                " JOIN CUST (NOLOCK) ON CUST.CUSTID=ORDRHDR.SOLDTOCUSTID " +
                " WHERE ISNULL(TRANFILE." + postedFlagField + ",'F')='F' AND SITES.HOST_SITE_ID<>'' ";
            if (aExportFilter.ExportShipmentType == "P")
            {
                sql += " AND TRANFILE.TRANTYPE = 'LO' AND TRANFILE.REASON IN('L','D','T') ";
            }
            else
            {
                sql += " AND TRANFILE.TRANTYPE='CS' ";
            }

            if (!String.IsNullOrEmpty(aExportFilter.CustID))
                sql += " AND ORDRHDR.SOLDTOCUSTID='" + aExportFilter.CustID + "' ";
            else if (!String.IsNullOrEmpty(aExportFilter.EDIMasterCustId))
                sql += " AND (OH.SOLDTOCUSTID='" + aExportFilter.EDIMasterCustId + "' OR CUST.CLIENT_ID_ASSOCIATION='" + aExportFilter.EDIMasterCustId + "') ";

            Utils.FilterUtils.AppendToExportFilter(ref sql, aExportFilter.ExportFilterList, "TRANFILE", "SITES|ORDRHDR|CUST");
            sql += "ORDER BY TRANFILE.SHIPMENT_ID, TRANFILE.ORDERNUM, TRANFILE.ID";
            return (sql);
        }


        private static string GetAltLotId(string orderNum, string lineNum, string ascItemId, string lotId)
        {
            string altLotId = "", tmpStr = "";

            string sqlStr2 = "";
            //taken out 08-09-16 (JXG) no need to query LOCITEMS
            //sqlStr2 = "SELECT MAX(ALT_LOTID) AS ALT_LOTID FROM LOCITEMS " +
            //    "WHERE PICKORDERNUM='" + orderNum + "' AND PICKLINENUM=" + lineNum + " ";
            //if (!string.IsNullOrEmpty(lotId))
            //    sqlStr2 += " AND LOTID='" + lotId + "' ";
            ////else
            ////    sqlStr2 += " AND (LOTID='' OR LOTID IS NULL)";
            //if (!AscDbUtils.ReadFieldsFromAscDb(sqlStr2, ref tmpStr))
            //{
            sqlStr2 = "SELECT MAX(ALT_LOTID) AS ALT_LOTID FROM OLDLCITM " +
                "WHERE PICKORDERNUM='" + orderNum + "' ";
            if (!string.IsNullOrEmpty(lineNum))  //added 01-31-20 (JXG)
                sqlStr2 += " AND PICKLINENUM=" + lineNum + " ";
            else
                sqlStr2 += " AND ASCITEMID='" + ascItemId + "' ";  //added 01-31-20 (JXG)
            if (!string.IsNullOrEmpty(lotId))
                sqlStr2 += " AND LOTID='" + lotId + "' ";
            sqlStr2 += "AND ALT_LOTID<>'' AND ALT_LOTID IS NOT NULL ";
            if (myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr2, "", ref tmpStr))
                altLotId = tmpStr;
            return altLotId;
        }

        private static HttpStatusCode BuildExportList(string sqlstr, long aMaxRecords, ref List<ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport> aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.NoContent;
            SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString);
            SqlCommand cmd = new SqlCommand(sqlstr, conn);
            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            myClass.myParse.Globals.mydmupdate.InitUpdate();
            try
            {
                long count = 1;
                while (dr.Read())
                {
                    if ((aMaxRecords > 0) && (count > aMaxRecords))
                        break;
                    count += 1;

                    retval = HttpStatusCode.OK;
                    string shipmentId = string.Empty;
                    var orderNum = dr["ORDERNUM"].ToString();
                    if (!myClass.myParse.Globals.myConfig.iniCPOneTrailerPerOrder.boolValue)
                        shipmentId = dr["SHIPMENT_ID"].ToString();
                    string hostSiteId = dr["HOST_SITE_ID"].ToString();
                    //holdRecId = dr["ID"].ToString();
                    string bolNum = string.Empty;
                    string proNum = String.Empty;
                    string shipcarrier = dr["CARRIER"].ToString();  //added 08-07-13 (JXG)
                    string shipUserId = dr["USERID"].ToString();  //added 08-02-16 (JXG) for Driscoll's
                    double shipCost = 0;

                    string sql = "SELECT BOL_NUM, PRO_NUM, CARRIER, SHIPCOST FROM SHIPMENT (NOLOCK) " +
                        "WHERE ORDERNUM='" + orderNum + "'";
                    if (!String.IsNullOrEmpty(shipmentId))
                        sql += " AND SHIPMENT_ID=" + shipmentId;
                    string tmpStr = string.Empty;
                    if (myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sql, "", ref tmpStr))
                    {
                        bolNum = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                        proNum = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                        shipcarrier = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);  //added 08-07-13 (JXG)
                        shipCost = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0);
                    }

                    string customOrderType = dr["ORDERTYPE"].ToString() + dr["ORDER_SOURCE"].ToString().Trim() + dr["CUSTOM_DATA1"].ToString().Trim();  //added 10-26-16 (JXG) for Driscoll's
                    var hdrRec = new ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport();

                    hdrRec.CREATE_DATETIME = ascLibrary.ascUtils.ascStrToDate(dr["TRANDATE"].ToString(), DateTime.Now);
                    hdrRec.FACILITY = hostSiteId;
                    hdrRec.SHIPMENT_NUMBER = bolNum;
                    hdrRec.ORDER_TYPE = dr["ORDERTYPE"].ToString();
                    hdrRec.ORDERNUMBER = orderNum;
                    hdrRec.CUST_ID = dr["SOLDTOCUSTID"].ToString();
                    hdrRec.ORDER_SOURCE = dr["ORDER_SOURCE"].ToString();  //added 02-20-16 (JXG) for Driscoll's
                    hdrRec.ORDER_SOURCE_SYSTEM = dr["ORDER_SOURCE_SYSTEM"].ToString();
                    hdrRec.SHIP_VIA_CODE = dr["SHIPVIA"].ToString();  //added 06-24-16 (JXG)
                    hdrRec.USERID = shipUserId;  //added 08-02-16 (JXG) for Driscoll's
                    hdrRec.CUSTOM_ORDER_TYPE = customOrderType;  //added 10-26-16 (JXG) for Driscoll's
                    hdrRec.SALESORDERNUMBER = dr["SALESORDERNUMBER"].ToString();
                    hdrRec.CUST_PO_NUM = dr["CUSTPONUM"].ToString();
                    hdrRec.PRO_NUMBER = proNum;
                    hdrRec.LOCKED_FLAG = "L";
                    hdrRec.DOC_TYPE = dr["HOST_ORDER_TYPE"].ToString();
                    string sCarrier = dr["CARRIER"].ToString();
                    string sSerevCode = dr["CARRIER_SERVICE_CODE"].ToString();
                    string sqlStr = "SELECT HOST_SERVICE_CODE FROM PARCSERV (NOLOCK) " +
                        "WHERE CARRIER='" + sCarrier + "' AND SERVICE_CODE='" + sSerevCode + "'";
                    if (myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref tmpStr))
                        sSerevCode = tmpStr;
                    hdrRec.CARRIER = sCarrier;
                    hdrRec.CARRIER_SERVICE_CODE = sSerevCode;
                    hdrRec.FREIGHT_COST = shipCost;

                    hdrRec.CUSTOM_DATA1 = dr["CUSTOM_DATA1"].ToString();
                    hdrRec.CUSTOM_DATA2 = dr["CUSTOM_DATA2"].ToString();
                    hdrRec.CUSTOM_DATA3 = dr["CUSTOM_DATA3"].ToString();
                    hdrRec.CUSTOM_DATA4 = dr["CUSTOM_DATA4"].ToString();
                    hdrRec.CUSTOM_DATA5 = dr["CUSTOM_DATA5"].ToString();
                    hdrRec.CUSTOM_DATA6 = dr["CUSTOM_DATA6"].ToString();
                    hdrRec.CUSTOM_DATA7 = dr["CUSTOM_DATA7"].ToString();
                    hdrRec.CUSTOM_DATA8 = dr["CUSTOM_DATA8"].ToString();
                    hdrRec.CUSTOM_DATA9 = dr["CUSTOM_DATA9"].ToString();
                    hdrRec.CUSTOM_DATA10 = dr["CUSTOM_DATA10"].ToString();
                    hdrRec.CUSTOM_DATA11 = dr["CUSTOM_DATA11"].ToString();
                    hdrRec.CUSTOM_DATA12 = dr["CUSTOM_DATA12"].ToString();

                    WritePicks(hdrRec, shipmentId);
                    WriteUnpickedLines(hdrRec);
                    WriteContainers(hdrRec, shipmentId);
                    WriteSerNums(hdrRec);
                    //if (exportNotes)  //added 04-11-16 (JXG) for Driscoll's
                    //    WriteNotes(hdrRec);

                    string where = "ID=" + dr["id"].ToString();
                    SetPosted(where, string.Empty, "S");

                    aData.Add(hdrRec);
                    /*

                                        if (!String.IsNullOrEmpty(ordrHdrUserDefField1))
                                            hdrRec.CUSTOM_DATA1"] = drHeader[ordrHdrUserDefField1];
                                        if (!String.IsNullOrEmpty(ordrHdrUserDefField2))
                                            hdrRec.CUSTOM_DATA2"] = drHeader[ordrHdrUserDefField2];
                                        if (!String.IsNullOrEmpty(ordrHdrUserDefField3))
                                            hdrRec.CUSTOM_DATA3"] = drHeader[ordrHdrUserDefField3];
                                        if (!String.IsNullOrEmpty(ordrHdrUserDefField4))
                                            hdrRec.CUSTOM_DATA4"] = drHeader[ordrHdrUserDefField4];
                                        if (!String.IsNullOrEmpty(ordrHdrUserDefField5))
                                            hdrRec.CUSTOM_DATA5"] = drHeader[ordrHdrUserDefField5];
                                        if (!String.IsNullOrEmpty(ordrHdrUserDefField6))
                                            hdrRec.CUSTOM_DATA6"] = drHeader[ordrHdrUserDefField6];
                                        if (!String.IsNullOrEmpty(ordrHdrUserDefField7))
                                            hdrRec.CUSTOM_DATA7"] = drHeader[ordrHdrUserDefField7];
                                        if (!String.IsNullOrEmpty(ordrHdrUserDefField8))
                                            hdrRec.CUSTOM_DATA8"] = drHeader[ordrHdrUserDefField8];
                                        if (!String.IsNullOrEmpty(ordrHdrUserDefField9))
                                            hdrRec.CUSTOM_DATA9"] = drHeader[ordrHdrUserDefField9];
                                        if (!String.IsNullOrEmpty(ordrHdrUserDefField10))
                                            hdrRec.CUSTOM_DATA10"] = drHeader[ordrHdrUserDefField10];
                                        if (!String.IsNullOrEmpty(ordrHdrUserDefField11))
                                            hdrRec.CUSTOM_DATA11"] = drHeader[ordrHdrUserDefField11];
                                        if (!String.IsNullOrEmpty(ordrHdrUserDefField12))
                                            hdrRec.CUSTOM_DATA12"] = drHeader[ordrHdrUserDefField12];
                    */

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

        private static void WritePicks(ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport aHdrRec, string shipmentId)
        {
            string sql = "SELECT S.HOST_SITE_ID, T.LINENUM, T.ASCITEMID, T.ITEMID, T.LOTID, T.HOST_ITEM_NUMBER, max( T.TRANDATE) AS TRANDATE," +
                "SUM(T.QTY) AS ITEMQTY, SUM(QTY_DUAL_UNIT) AS QTYDUALUNIT " +
                "FROM TRANFILE T (NOLOCK) " +
                "LEFT JOIN SITES S (NOLOCK) ON S.SITE_ID=T.SITE_ID " +
                "WHERE T.ORDERNUM=@orderNum AND T.TRANTYPE='PK' " +
                "AND ISNULL(T." + currExportConfig.postedFlagField + ",'F')='F' AND S.HOST_SITE_ID<>'' ";
            if (!String.IsNullOrEmpty(shipmentId))
                sql += "AND T.SHIPMENT_ID=@shipId ";
            sql += "GROUP BY S.HOST_SITE_ID, T.ASCITEMID, T.ITEMID, T.LINENUM, T.LOTID, T.HOST_ITEM_NUMBER " +
                "ORDER BY S.HOST_SITE_ID, T.ASCITEMID";
            using (SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                cmd.Parameters.Add("@orderNum", SqlDbType.VarChar, 100).Value = aHdrRec.ORDERNUMBER;
                if (!String.IsNullOrEmpty(shipmentId))
                    cmd.Parameters.Add("@shipId", SqlDbType.VarChar, 100).Value = shipmentId;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var itemId = dr["ITEMID"].ToString().ToUpper();
                        //var hostItemId = dr["HOST_ITEM_NUMBER"].ToString();
                        var lotId = dr["LOTID"].ToString();
                        var ascItemId = dr["ASCITEMID"].ToString();
                        var lineNum = dr["LINENUM"].ToString();
                        var qtyShip = ascLibrary.ascUtils.ascStrToDouble(dr["ITEMQTY"].ToString(), 0);
                        var qtyDualUnit = ascLibrary.ascUtils.ascStrToDouble(dr["QTYDUALUNIT"].ToString(), 0);

                        if (qtyShip < 0 && qtyDualUnit > 0)
                            qtyDualUnit = -qtyDualUnit;

                        string billUom = string.Empty;
                        myClass.myParse.Globals.myGetInfo.GetASCItemInfo(ascItemId, "DUAL_UNIT_ITEM, BILL_UOM", ref billUom);
                        var dualUnitItem = ascLibrary.ascStrUtils.GetNextWord(ref billUom);
                        string tmpStr = string.Empty;
                        myClass.myParse.Globals.myGetInfo.GetOrderDetInfo(aHdrRec.ORDERNUMBER, "PICKOPERID,HOST_CONV_FACT, CUST_ITEMID, HOST_QTYORDERED, QTYORDERED, EXT_PRICE, CUSTOM_DATA1, " +
                            "CUSTOM_DATA2, CUSTOM_DATA3, CUSTOM_DATA4", lineNum, ref tmpStr);
                        var pickOperId = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                        double convFact = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0);
                        string custItem = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                        double hostQtyOrd = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0);
                        double qtyOrd = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0);
                        var extPrice = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0);
                        var custom1 = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                        var custom2 = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                        var custom3 = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                        var custom4 = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                        var custom5 = "";  //added 09-22-15 (JXG) for Gold Star

                        //added 10-26-16 (JXG) for Driscoll's
                        if (pickOperId == "GWUSER")
                            myClass.myParse.Globals.myGetInfo.GetOrderInfo(aHdrRec.ORDERNUMBER, "CREATE_USERID", ref pickOperId);
                        /////////////////////////////////////

                        //changed 09-22-15 (JXG) for Gold Star
                        //custom1 = GetUserDefInfo(orderNum, lineNum.ToString(), lotId, custom1, ordrDetUserDefField1, cust1InIntfc);
                        //custom2 = GetUserDefInfo(orderNum, lineNum.ToString(), lotId, custom2, ordrDetUserDefField2, cust2InIntfc);
                        //custom3 = GetUserDefInfo(orderNum, lineNum.ToString(), lotId, custom3, ordrDetUserDefField3, cust3InIntfc);
                        //custom4 = GetUserDefInfo(orderNum, lineNum.ToString(), lotId, custom4, ordrDetUserDefField4, cust4InIntfc);
                        //custom5 = GetUserDefInfo(orderNum, lineNum.ToString(), lotId, custom5, ordrDetUserDefField5, cust5InIntfc);
                        //if (!String.IsNullOrEmpty(ordrDetUserDefField1))
                        //    AscDbUtils.GetOrderDetInfo(orderNum, lineNum, ordrDetUserDefField1, ref custom1);
                        //if (!String.IsNullOrEmpty(ordrDetUserDefField2))
                        //    AscDbUtils.GetOrderDetInfo(orderNum, lineNum, ordrDetUserDefField2, ref custom2);
                        //if (!String.IsNullOrEmpty(ordrDetUserDefField3))
                        //    AscDbUtils.GetOrderDetInfo(orderNum, lineNum, ordrDetUserDefField3, ref custom3);
                        //if (!String.IsNullOrEmpty(ordrDetUserDefField4))
                        //    AscDbUtils.GetOrderDetInfo(orderNum, lineNum, ordrDetUserDefField4, ref custom4);
                        //////////////////////////////////////

                        var altLotId = GetAltLotId(aHdrRec.ORDERNUMBER, lineNum, ascItemId, lotId);  //added 08-03-16 (JXG) for Driscoll's

                        ASCTracInterfaceModel.Model.CustOrder.CustOrderPicksExport rec = new ASCTracInterfaceModel.Model.CustOrder.CustOrderPicksExport();
                        rec.CREATE_DATETIME = ascLibrary.ascUtils.ascStrToDate(dr["TRANDATE"].ToString(), DateTime.Now);
                        rec.SHIPMENT_NUMBER = aHdrRec.SHIPMENT_NUMBER;
                        rec.LINE_NUMBER = ascLibrary.ascUtils.ascStrToInt(lineNum, 0);
                        rec.PRODUCT_CODE = itemId;
                        if (currExportConfig.GWCOUseCustItem && !String.IsNullOrEmpty(custItem))
                            rec.ITEM_NUMBER = custItem;
                        else
                            rec.ITEM_NUMBER = itemId;
                        rec.EXT_PRICE = extPrice;
                        if (currExportConfig.GWCOUseCustItem && (hostQtyOrd > 0))
                        {
                            rec.QTY_ORDERED = hostQtyOrd;
                            rec.QTY_SHIPPED = qtyShip / convFact;
                        }
                        else
                        {
                            rec.QTY_ORDERED = qtyOrd;
                            rec.QTY_SHIPPED = qtyShip;
                        }

                        if (!String.IsNullOrEmpty(billUom) || dualUnitItem == "T")
                        {
                            rec.CW_QTY = qtyDualUnit;
                            rec.CW_UOM = billUom;
                            rec.HOST_UOM = billUom;
                        }

                        rec.CUSTOM_DATA1 = custom1;
                        rec.CUSTOM_DATA2 = custom2;
                        rec.CUSTOM_DATA3 = custom3;
                        rec.CUSTOM_DATA4 = custom4;
                        rec.CUSTOM_DATA5 = custom5;

                        rec.PICK_OPR = pickOperId;  //added 10-26-16 (JXG) for Driscoll's

                        if (!String.IsNullOrEmpty(lotId))
                            rec.LOTID = lotId;
                        rec.ALT_LOTID = altLotId;

                        string where = "ORDERNUM='" + aHdrRec.ORDERNUMBER + "' AND LINENUM=" + rec.LINE_NUMBER.ToString() + " AND ISNULL( LOTID, '')='" + rec.LOTID + "' AND TRANTYPE='PK'";
                        //where += " AND ISNULL(T." + currExportConfig.postedFlagField + ", 'F') = 'F' ";
                        if (!String.IsNullOrEmpty(shipmentId))
                            where += " AND SHIPMENT_ID=" + shipmentId;
                        SetPosted(where, string.Empty, "S");
                        // Insert the Shipment Line
                        aHdrRec.PicksList.Add(rec);

                    }
                }
            }
        }

        private static void WriteUnpickedLines(ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport aHdrRec)
        {

            string sql = "SELECT D.ORDERNUMBER, D.ITEMID, D.ASCITEMID, D.LINENUMBER, D.QTYORDERED, D.QTYPICKED, D.QTY_SUBSTITUTED, D.HOST_LINENUMBER, " +
                " D.CUSTOM_DATA1, D.CUSTOM_DATA2, D.CUSTOM_DATA3, D.CUSTOM_DATA4, " +
                " I.BILL_UOM" +
                " FROM ORDRDET D (NOLOCK) " +
                " JOIN ITEMMSTR I ON I.ASCITEMID=D.ASCITEMID" +
                " WHERE D.ORDERNUMBER='" + aHdrRec.ORDERNUMBER + "' " +
                " AND ((D.QTYPICKED=0) OR (D.QTYPICKED<>0 AND D.QTYPICKED=D.QTY_SUBSTITUTED)) " +
                " ORDER BY D.ORDERNUMBER, D.ITEMID";
            using (SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var itemId = dr["ITEMID"].ToString().ToUpper();
                        var ascItemId = dr["ASCITEMID"].ToString();
                        var lineNum = dr["LINENUMBER"].ToString();
                        double qtyOrd = ascLibrary.ascUtils.ascStrToDouble(dr["QTYORDERED"].ToString(), 0);

                        if (!String.IsNullOrEmpty(ascItemId))
                        {
                            var billUom = dr["BILL_UOM"].ToString();
                            var custom1 = dr["CUSTOM_DATA1"].ToString();
                            var custom2 = dr["CUSTOM_DATA2"].ToString();
                            var custom3 = dr["CUSTOM_DATA3"].ToString();
                            var custom4 = dr["CUSTOM_DATA4"].ToString();

                            ASCTracInterfaceModel.Model.CustOrder.CustOrderPicksExport rec = new ASCTracInterfaceModel.Model.CustOrder.CustOrderPicksExport();
                            rec.CREATE_DATETIME = aHdrRec.CREATE_DATETIME;
                            rec.SHIPMENT_NUMBER = aHdrRec.SHIPMENT_NUMBER;
                            rec.LINE_NUMBER = ascLibrary.ascUtils.ascStrToInt(lineNum, 0);
                            rec.PRODUCT_CODE = itemId;
                            rec.ITEM_NUMBER = itemId;
                            rec.EXT_PRICE = 0;
                            rec.SOLD_PRICE = 0;
                            rec.QTY_ORDERED = qtyOrd;
                            rec.QTY_SHIPPED = 0;

                            rec.CUSTOM_DATA1 = custom1;
                            rec.CUSTOM_DATA2 = custom2;
                            rec.CUSTOM_DATA3 = custom3;
                            rec.CUSTOM_DATA4 = custom4;

                            rec.HOST_UOM = billUom;

                            aHdrRec.PicksList.Add(rec);
                        }
                    }
                }
            }


        }
        private static void WriteContainers(ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport aHdrRec, string shipmentId)
        {
            string sqlStr = "SELECT CONTAINER_ID, CNTRTYPE_ID, " +
                    "SUM (QTY * ISNULL(I.BOL_UNITWEIGHT,I.UNITWEIGHT)) AS CONTR_WEIGHT " +  //added 08-07-13 (JXG)
                    "FROM CONTAINR (NOLOCK) " +
                    "LEFT JOIN ITEMMSTR I ON I.ASCITEMID=CONTAINR.ASCITEMID " +  //added 08-07-13 (JXG)
                    "WHERE ORDERNUM=@orderNum AND ISNULL(CNTRTYPE_ID,'')<>'' ";
            if (!String.IsNullOrEmpty(shipmentId))
                sqlStr += "AND SHIPMENT_ID=@shipId ";
            sqlStr += "GROUP BY CONTAINER_ID, CNTRTYPE_ID";
            using (SqlDataAdapter daHostPallet = new SqlDataAdapter())
            using (SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
            using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
            {

                conn.Open();
                cmd.Parameters.Add("@orderNum", SqlDbType.VarChar, 100).Value = aHdrRec.ORDERNUMBER;
                if (!String.IsNullOrEmpty(shipmentId))
                    cmd.Parameters.Add("@shipId", SqlDbType.VarChar, 100).Value = shipmentId;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        double ContrWeight = ascLibrary.ascUtils.ascStrToDouble(dr["CONTR_WEIGHT"].ToString(), 0);  //added 08-07-13 (JXG)
                        ASCTracInterfaceModel.Model.CustOrder.CustOrderContainersExport rec = new ASCTracInterfaceModel.Model.CustOrder.CustOrderContainersExport();
                        rec.SHIPMENT_NUMBER = aHdrRec.SHIPMENT_NUMBER;
                        rec.CONTAINER_ID = dr["CONTAINER_ID"].ToString();
                        rec.PALLET_TYPE = dr["CNTRTYPE_ID"].ToString();
                        rec.CREATE_DATETIME = aHdrRec.CREATE_DATETIME;
                        rec.TOTAL_WEIGHT = ContrWeight;  //added 08-07-13 (JXG)
                        aHdrRec.ContainersList.Add(rec);
                    }
                }

            }
        }

        private static void WriteSerNums(ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport aHdrRec)
        {
            string sqlStr = "SELECT * FROM SER_NUM (NOLOCK) WHERE CUST_ORDER_NUM=@orderNum " + 
                    "ORDER BY ORDLINENUM, SER_NUM";
            using (SqlDataAdapter daHostPallet = new SqlDataAdapter())
            using (SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
            using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
            {

                conn.Open();
                cmd.Parameters.Add("@orderNum", SqlDbType.VarChar, 100).Value = aHdrRec.ORDERNUMBER;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        ASCTracInterfaceModel.Model.CustOrder.CustOrderSerNumExport rec = new ASCTracInterfaceModel.Model.CustOrder.CustOrderSerNumExport();
                        rec.CREATE_DATETIME = aHdrRec.CREATE_DATETIME;
                        rec.ITEMID = dr["ITEMID"].ToString();
                        rec.LOTID = dr["LOT_NUM"].ToString();
                        rec.ORDER_LINENUM = ascLibrary.ascUtils.ascStrToInt(dr["ORDLINENUM"].ToString(), 0);
                        rec.QTY = ascLibrary.ascUtils.ascStrToDouble(dr["QTY"].ToString(), 0);
                        rec.SER_NUM = dr["SER_NUM"].ToString();
                        aHdrRec.SerialsList.Add(rec);
                    }
                }

            }
        }

        //private static void WriteNotes(ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport aHdrRec)
        //{
        //
        //}


        private static void SetPosted(string wherestr, string aERROR_MESSAGE, string aPostedflag)
        {
            int msgLen = Convert.ToInt32(myClass.myParse.Globals.myDBUtils.getfieldsize("TRANFILE", "ERR_MESSAGE"));
            string shortErrorMessage = aERROR_MESSAGE;
            if (shortErrorMessage.Length > msgLen)
                shortErrorMessage = aERROR_MESSAGE.Substring(0, msgLen);

            string sqlStr = "UPDATE TRANFILE";
            if (!aPostedflag.Equals("E"))
                sqlStr += " SET " + currExportConfig.postedFlagField + "='" + aPostedflag + "', " + currExportConfig.posteddateField + "=GETDATE() ";
            else
                sqlStr += " SET " + currExportConfig.postedFlagField + "='E', " + currExportConfig.posteddateField + "=GETDATE(), " +
                    "ERR_MESSAGE='" + shortErrorMessage.Replace("'", "''") + "', " +
                    "LONG_MESSAGE='" + aERROR_MESSAGE.Replace("'", "''") + "' ";
            sqlStr += " WHERE " + wherestr;
            if (aPostedflag.Equals("S"))
                sqlStr += " AND ISNULL(" + currExportConfig.postedFlagField + ",'F') = 'F'";
            else
                sqlStr += " AND ISNULL(" + currExportConfig.postedFlagField + ",'F') = 'S'";
            //+" AND ISNULL(" + currExportConfig.postedFlagField + "','F') NOT IN ( 'T', 'X', 'D', 'E', 'P', '" + aPostedflag + "')";

            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
        }

        public static HttpStatusCode DoUpdateExportCustOrder(List<ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport> aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            foreach( var rec in aData)
            {
                string posted = "T";
                if (!rec.SUCCESSFUL)
                    posted = "E";
                         string where = "ORDERNUM='" + rec.ORDERNUMBER + "' AND TRANTYPE IN ( 'PK', 'CS', 'LO')";
                SetPosted(where, rec.ERROR_MESSAGE, posted);
            }
            return (retval);
        }

        public static HttpStatusCode updateExportCustOrder(List<ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport> aData, ref string errmsg)
        {
            myClass = Class1.InitParse("UpdateExportCustOrder", ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            string OrderNum = string.Empty;
            string sqlstr = string.Empty;
            try
            {
                if (myClass != null)
                {
                    myClass.myParse.Globals.mydmupdate.InitUpdate();
                    currExportConfig = Configs.CustOrderConfig.getCOExportSite("1", myClass.myParse.Globals);
                    retval = DoUpdateExportCustOrder(aData, ref errmsg);
                    if (retval == HttpStatusCode.OK)
                        myClass.myParse.Globals.mydmupdate.ProcessUpdates();
                }
                else
                    retval = HttpStatusCode.InternalServerError;
            }
            catch (Exception ex)
            {
                Class1.WriteException("updateExportCustOrder", Newtonsoft.Json.JsonConvert.SerializeObject(aData), OrderNum, ex.ToString(), sqlstr);
                retval = HttpStatusCode.BadRequest;
                errmsg = ex.Message;
            }
            return (retval);

        }
    }
}