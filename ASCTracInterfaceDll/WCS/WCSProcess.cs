using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.WCS
{
    public class WCSProcess
    {
        public static HttpStatusCode doWCSPickImport( Class1 myClass, string aImportType, ASCTracInterfaceModel.Model.WCS.WCSPick aData, ref string errMsg)
        {
            string funcType = "WCS";
            HttpStatusCode retval = HttpStatusCode.OK;
            //var myClass = Class1.InitParse(funcType, ref errMsg);
            string OrderNum = aData.ORDERNUMBER;
            try
            {
                if (!myClass.FunctionAuthorized(funcType))
                {
                    retval = HttpStatusCode.NonAuthoritativeInformation;
                    errMsg = "WCS is not an authorized interface for this ASCTrac.";
                }
                else
                {
                    string siteID = aData.SITE_ID;
                    if (!aImportType.Equals("E") && !aImportType.Equals("U") && !aImportType.Equals("I"))
                        myClass.myParse.Globals.myGetInfo.GetOrderInfo(OrderNum, "SITE_ID", ref siteID);

                    myClass.myParse.Globals.initsite(siteID);
                    string pickSeqNum = string.Empty;
                    if (aData.PICK_SEQUENCE_NO > 0)
                        pickSeqNum = aData.PICK_SEQUENCE_NO.ToString();
                    double qtyLefttoPick = aData.QTY_PICKED;

                    string itemid = Utils.ASCUtils.GetTrimString(aData.ITEMID, string.Empty);
                    string lotid = Utils.ASCUtils.GetTrimString(aData.LOTID, string.Empty);
                    string locid = Utils.ASCUtils.GetTrimString(aData.LOCATIONID, string.Empty);
                    string containerid = Utils.ASCUtils.GetTrimString(aData.CONTAINER_ID, string.Empty);
                    string sernum = Utils.ASCUtils.GetTrimString(aData.SER_NUM, string.Empty);
                    string userid = Utils.ASCUtils.GetTrimString(aData.USERID, string.Empty);

                    DateTime dtPicked = myClass.myParse.Globals.GetSiteCurrDateTime();
                    if ((aData.DATETIME_PICKED != null) && (aData.DATETIME_PICKED != DateTime.MinValue))
                        dtPicked = aData.DATETIME_PICKED;
                    if (dtPicked == DateTime.MinValue)
                        dtPicked = DateTime.Now;
                    string aInfoMsg = string.Empty;
                    switch (aImportType)
                    {
                        case "C":
                            // Ticket 2056-11.00, remove Type of pick check
                            errMsg = myClass.myWCSPickImport.ProcessPick(aImportType, aData.TYPE_OF_PICK, aData.ORDERNUMBER, pickSeqNum,
                                        itemid, lotid, locid, aData.SKIDID,
                                        containerid, sernum, dtPicked.ToString(), userid, aData.TYPE_OF_PICK,
                                        ref qtyLefttoPick, ref aInfoMsg);
                            break;
                        case "E":
                            errMsg = myClass.myWCSPickImport.ProcessReplen(aData.TYPE_OF_PICK, siteID,
                                aData.DELIVERY_LOCATION,
                                itemid, locid, aData.SKIDID,
                                 dtPicked.ToString(), userid, aData.QTY_PICKED);
                            break;
                        case "I": // Issue
                            errMsg = "Issue function not available.";
                            /*
                            myClass.myWCSPickImport.ProcessIssue(aData.ORDERNUMBER, pickSeqNum,
                                        aData.ITEMID, aData.LOTID, aData.LOCATIONID, aData.SKIDID,
                                        aData.CONTAINER_ID, aData.SER_NUM,
                                        aData.DATETIME_PICKED, aData.USERID, aData.QTY_PICKED);
                            */
                            break;
                        case "N": // Unpick
                            errMsg = myClass.myWCSPickImport.ProcessUnpick(aData.ORDERNUMBER, pickSeqNum,
                                itemid, lotid, locid, aData.SKIDID,
                                containerid, sernum, dtPicked.ToString(), userid,
                                ref qtyLefttoPick);
                            break;
                        case "R": // repick
                            errMsg = myClass.myWCSPickImport.ProcessScrap(aData.ORDERNUMBER, pickSeqNum, aData.SKIDID, ref qtyLefttoPick);
                            if (String.IsNullOrEmpty(errMsg))
                            {
                                errMsg = myClass.myWCSPickImport.ProcessPick(aImportType, aData.TYPE_OF_PICK, aData.ORDERNUMBER, pickSeqNum,
                                            itemid, lotid, locid, aData.SKIDID,
                                            containerid, sernum,
                                            dtPicked.ToString(), userid, aData.TYPE_OF_PICK,
                                            ref qtyLefttoPick, ref aInfoMsg);
                            }
                            break;
                        case "S":
                            errMsg = myClass.myWCSPickImport.ProcessScrap(aData.ORDERNUMBER, pickSeqNum,
                                aData.SKIDID, ref qtyLefttoPick);
                            break;
                        case "U":
                            errMsg = myClass.myWCSPickImport.ProcessPutaway(aData.TYPE_OF_PICK, itemid, locid, aData.SKIDID,
                                dtPicked.ToString(), userid);
                            break;
                    }
                    if (!String.IsNullOrEmpty(errMsg))
                        retval = HttpStatusCode.BadRequest;
                    else
                        myClass.myLogRecord.infoMsg = aInfoMsg;
                }
            }
            catch (Exception ex)
            {
                myClass.LogException(ex);
                //Class1.WriteException(funcType, Newtonsoft.Json.JsonConvert.SerializeObject(aData), OrderNum, ex.Message, ex.StackTrace);
                retval = HttpStatusCode.BadRequest;
                errMsg = "(DoWCSPickImport) " + ex.Message;
            }
            return (retval);
        }

        public static HttpStatusCode doWCSPickExport(Class1 myClass, string aOrderType, ref List<ASCTracInterfaceModel.Model.WCS.WCSPick> aData, ref string errmsg)
        {
            string funcType = "WCS";
            HttpStatusCode retval = HttpStatusCode.OK;
            string OrderNum = string.Empty;
            try
            {
                if (!myClass.FunctionAuthorized(funcType))
                    retval = HttpStatusCode.NonAuthoritativeInformation;
                else
                {
                    //currPOExportConfig = Configs.POConfig.getPOExportSite("1", myClass.myParse.Globals);
                    string sqlstr = BuildWCSExportSQL(aOrderType, ref errmsg);
                    if (!String.IsNullOrEmpty(sqlstr))
                    {
                        retval = BuildExportList(myClass, sqlstr, ref aData, ref errmsg);
                    }
                    else
                        retval = HttpStatusCode.BadRequest;
                }
            }
            catch (Exception ex)
            {
                myClass.LogException(ex);
                //Class1.WriteException(funcType, Newtonsoft.Json.JsonConvert.SerializeObject(aData), OrderNum, ex.Message, ex.StackTrace);
                retval = HttpStatusCode.BadRequest;
                errmsg = ex.Message;
            }
            return (retval);
        }

        private static string BuildWCSExportSQL(string aOrderType, ref string errmsg)
        {
            string retval = string.Empty;

            retval = "SELECT PA.* FROM PICK_ASSIGNMENTS PA" +
                " WHERE PA.STATUS_FLAG='R' ";
            if (!String.IsNullOrEmpty(aOrderType))
                retval += " AND PA.ORDER_TYPE='" + aOrderType + "'";
            retval += " ORDER BY PA.DATE_TIME_CREATED, PA.ID";


            return (retval);
        }

        private static HttpStatusCode BuildExportList(Class1 myClass, string sqlstr, ref List<ASCTracInterfaceModel.Model.WCS.WCSPick> aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;

            SqlConnection myConnection = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sqlstr, myConnection);
            string StatusID = "Opening DataSEt";
            myConnection.Open();
            SqlDataReader myReader = myCommand.ExecuteReader();
            try
            {
                retval = HttpStatusCode.NoContent;
                string lastOrder = string.Empty;
                while (myReader.Read())
                {
                    string ordernum = myReader["ORDERNUMBER"].ToString();
                    myClass.myParse.Globals.initsite(myReader["SITE_ID"].ToString());
                    string whereStr = "ID=" + myReader["ID"].ToString();
                    StatusID = "Processing Pick Assignment Record " + myReader["ID"].ToString();

                    bool fok = false;
                    try
                    {
                        ASCTracInterfaceModel.Model.WCS.WCSPick rec = new ASCTracInterfaceModel.Model.WCS.WCSPick();
                        bool fNewOrder = !lastOrder.Equals(myReader["ORDERNUMBER"].ToString(), StringComparison.CurrentCultureIgnoreCase);
                        lastOrder = myReader["ORDERNUMBER"].ToString();
                        if (myReader["ORDER_TYPE"].ToString() == "C")
                        {
                            fok = true;
                            ExportStandard(myClass, myReader, fNewOrder, ref rec);
                        }
                        if (myReader["ORDER_TYPE"].ToString() == "E") // replen
                        {
                            fok = true;
                            ExportStandard(myClass, myReader, fNewOrder, ref rec);
                        }
                        if (myReader["ORDER_TYPE"].ToString() == "K") // kanban
                        {
                            fok = true;
                            ExportStandard(myClass, myReader, fNewOrder, ref rec);
                        }
                        if (myReader["ORDER_TYPE"].ToString() == "V") // MOVE
                        {
                            fok = true;
                            ExportStandard(myClass, myReader, fNewOrder, ref rec);
                        }
                        if (myReader["ORDER_TYPE"].ToString() == "P") // PROD PICK
                        {
                            fok = true;
                            ExportStandard(myClass, myReader, fNewOrder, ref rec);
                        }
                        /*
                        if (myReader["ORDER_TYPE"].ToString() == "U") // putaway
                        {
                            ExportPutaway(myReader);
                        }
                        if (String.IsNullOrEmpty(myReader["ORDER_TYPE"].ToString())) // putaway
                        {
                            if (myReader["TYPE_OF_PICK"].ToString() == "U")
                                ExportPutaway(myReader);
                        }
                        */
                        if (fok)
                        {
                            aData.Add(rec);
                            retval = HttpStatusCode.OK;
                        }
                    }
                    catch (Exception ex)
                    {
                        errmsg = ex.Message;
                        myClass.LogException(ex);
                        //Class1.WriteException("WCS_Import", "", ordernum, errmsg.ToString(), "");
                        //WriteFilelog(StatusID + ":" + ex.Message, ex.StackTrace);
                    }

                    if (fok)
                    {

                        string updstr = "";
                        if (String.IsNullOrEmpty(errmsg))
                        {
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "STATUS_FLAG", "S");
                            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "DATE_TIME_TO_SYSTEM", "GetDate()");
                        }
                        else
                        {
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "STATUS_FLAG", "E");
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "ERR_MESSAGE", ascLibrary.ascStrUtils.ascSubString(errmsg.Replace("'", ""), 0, 80));
                        }
                        myClass.myParse.Globals.myDBUtils.RunSqlCommand("UPDATE PICK_ASSIGNMENTS SET " + updstr + " WHERE " + whereStr);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return (retval);
        }

        private static void ExportStandard(Class1 myClass, SqlDataReader myReader, bool aNewOrder, ref ASCTracInterfaceModel.Model.WCS.WCSPick rec)
        {
            string shipcustid = string.Empty;
            string updstr = string.Empty;
            string ascItemID = string.Empty;
            string itemDesc = string.Empty;
            if (String.IsNullOrEmpty(myReader["ORDERNUMBER"].ToString()))
            {
                myClass.myParse.Globals.myGetInfo.GetItemInfo(myReader["ITEMID"].ToString(), "ITEMMSTR.DESCRIPTION,ITEMMSTR.ASCITEMID", ref ascItemID);
                itemDesc = ascLibrary.ascStrUtils.GetNextWord(ref ascItemID);
            }
            else
            {
                if (!String.IsNullOrEmpty(myReader["PICK_SEQUENCE_NO"].ToString()))
                    myClass.myParse.Globals.myGetInfo.GetOrderDetInfo(myReader["ORDERNUMBER"].ToString(), "ASCITEMID", myReader["PICK_SEQUENCE_NO"].ToString(), ref ascItemID);
                if (string.IsNullOrEmpty(ascItemID))
                    myClass.myParse.Globals.myGetInfo.GetOrderDetItemInfo(myReader["ORDERNUMBER"].ToString(), myReader["ITEMID"].ToString(), "ASCITEMID", ref ascItemID);
                if (string.IsNullOrEmpty(ascItemID))
                {
                    myClass.myParse.Globals.myGetInfo.GetItemInfo(myReader["ITEMID"].ToString(), "ITEMMSTR.DESCRIPTION, ITEMMSTR.ASCITEMID", ref ascItemID);
                    itemDesc = ascLibrary.ascStrUtils.GetNextWord(ref ascItemID);
                }
                else
                    itemDesc = myClass.myParse.Globals.myGetInfo.GetASCItemData(ascItemID, "DESCRIPTION");

                /*
                string sqlStr = "SELECT * FROM ORDRHDR WHERE ORDERNUMBER='" + myReader["ORDERNUMBER"].ToString() + "'";

                SqlConnection myConnection = new SqlConnection(Globals.myDBUtils.myConnString);
                SqlCommand myCommand = new SqlCommand(sqlStr, myConnection);

                myConnection.Open();
                SqlDataReader myReaderHdr = myCommand.ExecuteReader();
                try
                {
                    if (myReaderHdr.Read())
                    {
                        shipcustid = myReaderHdr["SHIPTOCUSTID"].ToString();

                        if (aNewOrder && !myInterfaceDBUtils.ifRecExists("SELECT ORDERNUMBER FROM tbl_toasc_cust_ordr_header WHERE ORDERNUMBER='" + myReader["ORDERNUMBER"].ToString() + "'"))
                        {
                            string schedDate = myReaderHdr["SCHEDDATE"].ToString();
                            string schedTTime = myReaderHdr["SCHEDTTIME"].ToString();
                            if (String.IsNullOrEmpty(schedDate) && !String.IsNullOrEmpty(myReaderHdr["LINK_NUM"].ToString()))
                            {
                                Globals.myGetInfo.GetLoadPlanInfo(myReaderHdr["LINK_NUM"].ToString(), "SCHEDULED_DATE, SCHEDTTIME", ref schedTTime);
                                schedDate = ascLibrary.ascStrUtils.GetNextWord(ref schedTTime);
                            }

                            updstr = "PROCESS_FLAG='D'";
                            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "CREATE_DATETIME", "GetDate()");
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "status_flag", "A");

                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ORDER_CREATE_DATE", myReaderHdr["OURORDERDATE"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FACILITY", myReaderHdr["SITE_ID"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ORDER_TYPE", myReaderHdr["ORDERTYPE"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ordernumber", myReaderHdr["ordernumber"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CUST_ID", myReaderHdr["SOLDTOCUSTID"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "from_facility", myReaderHdr["TRANSFER_SITE_ID"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "leaves_date", myReaderHdr["REQUIREDSHIPDATE"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "entry_date", myReaderHdr["CUSTOMERORDERDATE"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "carrier", myReaderHdr["carrier"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "payment_type", ""); //myReaderHdr["carrier"].ToString());

                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SCHEDULEDATE", schedDate);
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SCHEDULETIME", schedTTime);

                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ship_to_name", myReaderHdr["SHIPTONAME"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ship_to_addr_line1", myReaderHdr["SHIPTOADDRESS1"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ship_to_addr_line2", myReaderHdr["SHIPTOADDRESS2"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ship_to_addr_line3", myReaderHdr["SHIPTOADDRESS3"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ship_to_city", myReaderHdr["SHIPTOCITY"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ship_to_state", ascLibrary.ascStrUtils.ascSubString(myReaderHdr["SHIPTOSTATE"].ToString(), 1, 20));
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ship_to_zip", myReaderHdr["SHIPTOZIPCODE"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ship_to_country", myReaderHdr["SHIPTOCOUNTRY"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ship_to_contact_name", myReaderHdr["SHIPTOCONTACT"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ship_to_contact_tel", myReaderHdr["SHIPTOTELEPHONE"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ship_to_contact_fax", myReaderHdr["SHIPTOFAX"].ToString());

                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "bill_to_name", myReaderHdr["billTONAME"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "bill_to_addr_line1", myReaderHdr["billTOADDRESS1"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "bill_to_addr_line2", myReaderHdr["billTOADDRESS2"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "bill_to_addr_line3", myReaderHdr["billTOADDRESS3"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "bill_to_city", myReaderHdr["billTOCITY"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "bill_to_state", ascLibrary.ascStrUtils.ascSubString(myReaderHdr["billTOSTATE"].ToString(), 1, 20));
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "bill_to_zip", myReaderHdr["billTOZIPCODE"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "bill_to_country", myReaderHdr["billTOCOUNTRY"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "bill_to_contact_name", myReaderHdr["billTOCONTACT"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "bill_to_contact_tel", myReaderHdr["billTOTELEPHONE"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "bill_to_contact_fax", myReaderHdr["billTOFAX"].ToString());

                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "cust_po_num", myReaderHdr["CUSTPONUM"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "load_plan_num", myReaderHdr["LINK_NUM"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "load_stop_seq", myReaderHdr["LINK_SEQ_NUM"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "priority_id", myReaderHdr["PRIORITYID"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "recipient_email", myReaderHdr["CUSTOMER_EMAIL_TO"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "bol_number", myReaderHdr["PREASSIGN_BOLNUM"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "freight_account_number", myReaderHdr["FREIGHT_BILL_ACCT_NUM"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "reference_number", ""); //myReaderHdr["CUSTOMER_EMAIL_TO"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ship_to_cust_id", myReaderHdr["SHIPTOCUSTID"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "bill_to_cust_id", myReaderHdr["SOLDTOCUSTID"].ToString());

                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data1", myReaderHdr["custom_data1"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data2", myReaderHdr["custom_data2"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data3", myReaderHdr["custom_data3"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data4", myReaderHdr["custom_data4"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data5", myReaderHdr["custom_data5"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data6", myReaderHdr["custom_data6"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data7", myReaderHdr["custom_data7"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data8", myReaderHdr["custom_data8"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data9", myReaderHdr["custom_data9"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data10", myReaderHdr["custom_data10"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data11", myReaderHdr["custom_data11"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data12", myReaderHdr["custom_data12"].ToString());
                            //ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_date", myReaderHdr["custom_date"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_num1", myReaderHdr["custom_num1"].ToString());

                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "prepay_collect", myReaderHdr["FREIGHTCODE"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "carrier_service_code", myReaderHdr["carrier_service_code"].ToString());
                            // comes from notes
                            //ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "delivery_instructions", myReaderHdr["custom_date"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "cust_shipto_po_num", myReaderHdr["CUSTSHIPPONUM"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "cod_amt", myReaderHdr["cod_amt"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "must_arrive_by_date", myReaderHdr["MUST_ARRIVE_BY_DATE"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "salesperson", myReaderHdr["salesperson"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "terms_id", myReaderHdr["terms_id"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "linked_ponumber", myReaderHdr["LINKED_PO_NUM"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "credit_hold_status", ""); //myReaderHdr["LINKED_PO_NUM"].ToString());

                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "clientdept", myReaderHdr["clientdept"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "clientdivision", myReaderHdr["clientdivision"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "clientglacct", myReaderHdr["clientglacct"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "clientprofit", myReaderHdr["clientprofit"].ToString());

                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "allow_short_ship", myReaderHdr["IGNOREINVAVAIL"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "residential_flag", myReaderHdr["residential_flag"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ship_via", myReaderHdr["SHIPVIA"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "area", myReaderHdr["area"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "allow_over_ship", myReaderHdr["FILL_TO_CAPACITY"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SALESORDERNUMBER", myReaderHdr["SALESORDERNUMBER"].ToString());

                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYCUSTID", myReaderHdr["THIRDPARTYCUSTID"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYname", myReaderHdr["THIRDPARTYNAME"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYADDRESS1", myReaderHdr["THIRDPARTYADDRESS1"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYADDRESS2", myReaderHdr["THIRDPARTYADDRESS2"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYADDRESS3", myReaderHdr["THIRDPARTYADDRESS3"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYcity", myReaderHdr["THIRDPARTYCITY"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYstate", ascLibrary.ascStrUtils.ascSubString(myReaderHdr["THIRDPARTYSTATE"].ToString(), 1, 20));
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYzip", myReaderHdr["THIRDPARTYZIPCODE"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "THIRDPARTYcountry", myReaderHdr["THIRDPARTYCOUNTRY"].ToString());

                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "STORE_NUM", myReaderHdr["STORE_NUM"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "DEPT", myReaderHdr["DEPT"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "PACKLIST_REQ", myReaderHdr["PACKLIST_REQ"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "DROP_SHIP", myReaderHdr["DROP_SHIP"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BATCH_NUM", myReaderHdr["BATCH_NUM"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ROUTEID", myReaderHdr["ROUTEID"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "PROMO_CODE", ""); //myReaderHdr["SALESORDERNUMBER"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CUSTORDERCAT", myReaderHdr["CUSTORDERCAT"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FOB", myReaderHdr["FOB"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CUST_BILLTO_PO_NUM", myReaderHdr["CUSTBILLPONUM"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "COMPLIANCE_LABEL", myReaderHdr["COMPLIANCELABEL"].ToString());

                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LOADPLAN", myReaderHdr["LINK_NUM"].ToString());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LOADPLAN_SEQ_NUM", myReaderHdr["LINK_SEQ_NUM"].ToString());

                            string tmp = string.Empty;
                            Globals.myGetInfo.GetDockSchedInfo("C", myReader["ORDERNUMBER"].ToString(), "LOADINGBAY", true, ref tmp);
                            if (String.IsNullOrEmpty(tmp) && !String.IsNullOrEmpty(myReaderHdr["LINK_NUM"].ToString()))
                            {
                                Globals.myGetInfo.GetDockSchedInfo("L", myReader["LINK_NUM"].ToString(), "LOADINGBAY", true, ref tmp);
                            }

                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LOADINGBAY", tmp);

                            myInterfaceDBUtils.InsertRecord("tbl_toasc_cust_ordr_header", updstr);

                            sqlStr = "SELECT * FROM ORDRDET WHERE ORDERNUMBER ='" + myReader["ORDERNUMBER"].ToString() + "'";
                            {
                                SqlConnection myConnectionDet = new SqlConnection(Globals.myDBUtils.myConnString);
                                SqlCommand myCommandDet = new SqlCommand(sqlStr, myConnectionDet);

                                myConnectionDet.Open();
                                SqlDataReader myReaderDet = myCommandDet.ExecuteReader();
                                try
                                {
                                    while (myReaderDet.Read())
                                    {
                                        updstr = "PROCESS_FLAG='R'";
                                        ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "CREATE_DATETIME", "GetDate()");
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "status_flag", "A");
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ordernumber", myReaderDet["ordernumber"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FACILITY", myReaderHdr["SITE_ID"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ORDER_TYPE", myReaderHdr["ORDERTYPE"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "line_number", myReaderDet["LINENUMBER"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "product_code", myReaderDet["ITEMID"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "quantity", myReaderDet["QTYORDERED"].ToString());


                                        // from notes ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "comment", myReaderDet["QTYORDERED"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "COSTEACH", myReaderDet["COSTEACH"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "cw_not_base_uom", ""); // myReaderDet["QTYORDERED"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "cw_uom", ""); //myReaderDet["QTYORDERED"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "list_price", ""); // myReaderDet["QTYORDERED"].ToString());

                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data1", myReaderDet["custom_data1"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data2", myReaderDet["custom_data2"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data3", myReaderDet["custom_data3"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data4", myReaderDet["custom_data4"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data5", myReaderDet["custom_data5"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_data6", myReaderDet["custom_data6"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "custom_num1", myReaderDet["custom_num1"].ToString());

                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "clientdept", myReaderDet["clientdept"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "clientdivision", myReaderDet["clientdivision"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "clientglacct", myReaderDet["clientglacct"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "clientprofit", myReaderDet["clientprofit"].ToString());

                                        //ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "order_status", myReaderDet["custom_num1"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "host_uom", myReaderDet["host_uom"].ToString());
                                        //ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "requested_lot", myReaderDet["custom_num1"].ToString());
                                        //ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "notes", myReaderDet["custom_num1"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHIPDESC", myReaderDet["SHIPDESC"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CUST_ITEMID", myReaderDet["CUST_ITEMID"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SOLD_PRICE", myReaderDet["RETAIL_PRICE"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "QTYBACKORDERED", myReaderDet["QTYBACKORDERED"].ToString());
                                        //ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "HDR_REC_ID", myReaderDet["custom_num1"].ToString());
                                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "COUNTRY_OF_DESTINATION", myReaderDet["COUNTRY_OF_DESTINATION"].ToString());


                                        myInterfaceDBUtils.InsertRecord("tbl_toasc_cust_ordr_detail", updstr);
                                    }
                                }
                                finally
                                {
                                    myConnectionDet.Close();
                                }

                            } // end ordrdet
                        }
                    }
                }
                finally
                {
                    myConnection.Close();

                } // end ordrhdr
                myInterfaceDBUtils.RunSqlCommand("update tbl_toasc_cust_ordr_header set process_flag='R' where ordernumber='" + myReader["ORDERNUMBER"].ToString() + "'");
                */
            }  // end if new order

            // write PA to tbl_asc_WCS_Pick
            //updstr = "PROCESS_FLAG='R', process_recipient='E'";
            //ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "CREATE_DATETIME", "GetDate()");
            rec.SITE_ID = myReader["SITE_ID"].ToString();
            rec.ASSIGNMENT_NUMBER = ascLibrary.ascUtils.ascStrToDouble(myReader["ASSIGNMENT_NUMBER"].ToString(), 0);
            //rec.PICK_SYS_ID = "ASC"); //myReader["PICK_SYS_ID"].ToString());
            if (String.IsNullOrEmpty(myReader["TYPE_OF_PICK"].ToString()))
                rec.TYPE_OF_PICK = "Z"; // unknown
            else
                rec.TYPE_OF_PICK = myReader["TYPE_OF_PICK"].ToString();
            rec.ORDERTYPE = myReader["ORDER_TYPE"].ToString();
            rec.ORDERNUMBER = myReader["ORDERNUMBER"].ToString();
            rec.PRIORITY = ascLibrary.ascUtils.ascStrToDouble(myReader["PRIORITY"].ToString(), 0);
            rec.DELIVERY_LOCATION = myReader["DELIVERY_LOCATION"].ToString();
            if (ascLibrary.ascUtils.ascStrToDate(myReader["DELIVERY_DATE"].ToString(), DateTime.MinValue) != DateTime.MinValue)
                rec.DELIVERY_DATE = ascLibrary.ascUtils.ascStrToDate(myReader["DELIVERY_DATE"].ToString(), DateTime.MinValue);
            rec.ROUTE = myReader["ROUTE"].ToString();
            rec.GOAL_TIME = ascLibrary.ascUtils.ascStrToDouble(myReader["GOAL_TIME"].ToString(), 0);
            rec.PICK_SEQUENCE_NO = ascLibrary.ascUtils.ascStrToDouble(myReader["PICK_SEQUENCE_NO"].ToString(), 0);
            rec.ITEMID = myReader["ITEMID"].ToString();
            rec.ITEM_DESCRIPTION = itemDesc;
            rec.ZONEID = myReader["ZONEID"].ToString();
            rec.AISLE = myReader["AISLE"].ToString();
            rec.SLOT = myReader["SLOT"].ToString();
            rec.LOCATIONID = myReader["LOCATIONID"].ToString();
            rec.LOCATION_IDENTIFIER = myReader["LOCATION_IDENTIFIER"].ToString();
            rec.LOTID = myReader["LOTID"].ToString();
            rec.DIRECTED_CONTAINER_ID = myReader["CONTAINER_ID"].ToString();
            rec.SKIDID = myReader["SKIDID"].ToString();
            rec.FULL_CASE = myReader["FULL_CASE"].ToString();
            rec.PCE_TYPE = myReader["PCE_TYPE"].ToString();
            double convFact = myClass.myParse.Globals.dmMiscItem.GetEachesPerCase(ascItemID);
            if (convFact > 0)
                rec.NUM_EACHES_CASE = convFact;
            string itemInfo = myClass.myParse.Globals.myGetInfo.GetASCItemData(ascItemID, "UNITWIDTH, UNITLENGTH, UNITHEIGHT, UNITWEIGHT,STOCK_UOM, UPCCODE");
            rec.UNIT_WIDTH = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref itemInfo), 0);
            rec.UNIT_LENGTH = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref itemInfo), 0);
            rec.UNIT_HEIGHT = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref itemInfo), 0);
            string sWgt = ascLibrary.ascStrUtils.GetNextWord(ref itemInfo);
            rec.UNIT_WEIGHT = ascLibrary.ascUtils.ascStrToDouble(sWgt, 0);
            rec.PICK_UNIT = ascLibrary.ascStrUtils.GetNextWord(ref itemInfo);
            rec.ITEM_UPC = ascLibrary.ascStrUtils.GetNextWord(ref itemInfo);

            if (myClass.myParse.Globals.myDBUtils.ReadFieldFromDB("SELECT INTERFACE_LEN, INTERFACE_WIDTH, INTERFACE_HEIGHT FROM ITEM_KNAPP WHERE ASCITEMID='" + ascItemID + "'", "", ref itemInfo))
            {
                rec.WCS_WIDTH = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref itemInfo), 0);
                rec.WCS_LENGTH = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref itemInfo), 0);
                rec.WCS_HEIGHT = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref itemInfo), 0);
                rec.WCS_WEIGHT = ascLibrary.ascUtils.ascStrToDouble(sWgt, 0); //ascLibrary.ascStrUtils.GetNextWord(ref itemInfo));
            }
            rec.QTY_TO_PICK = ascLibrary.ascUtils.ascStrToDouble(myReader["QUANTITY_TO_PICK"].ToString(), 0);
            rec.TRIGGER_A_REPLEN = myReader["TRIGGER_A_REPLEN"].ToString();
            string locInfo = string.Empty;
            if (myClass.myParse.Globals.myDBUtils.ReadFieldFromDB("SELECT INTERFACE_RACK,INTERFACE_LEVEL, INTERFACE_POSITION, COORD_X,COORD_Y FROM LOC WHERE LOCATIONID='" + myReader["LOCATIONID"].ToString() + "' AND SITE_ID='" + myReader["SITE_ID"].ToString() + "'", "", ref locInfo))
            {
                rec.INTERFACE_RACK = ascLibrary.ascStrUtils.GetNextWord(ref locInfo);
                rec.INTERFACE_LEVEL = ascLibrary.ascStrUtils.GetNextWord(ref locInfo);
                rec.INTERFACE_POSITION = ascLibrary.ascStrUtils.GetNextWord(ref locInfo);
                rec.COORD_X = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref locInfo), 0);
                rec.COORD_Y = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref locInfo), 0);

                if (myClass.myParse.Globals.myDBUtils.ReadFieldFromDB("SELECT MIN( EXPDATE) FROM LOCITEMS WHERE LOCATIONID='" + myReader["LOCATIONID"].ToString() + "' AND ASCITEMID='" + ascItemID + "' ", "", ref locInfo))
                    rec.EXPDATE_FIRST = ascLibrary.ascUtils.ascStrToDate(locInfo, DateTime.MinValue);
            }

            var expiredays = myClass.myParse.Globals.dmMiscItem.GetExpireDays(ascItemID, shipcustid);
            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "EXPIRE_DAYS", expiredays.ToString());
        }

    }
}

