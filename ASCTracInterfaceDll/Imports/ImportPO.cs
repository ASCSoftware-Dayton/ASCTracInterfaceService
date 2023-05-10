using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Imports
{
    public class ImportPO
    {
        //private string funcType = "IM_RECV";
        private string siteid = string.Empty;
        private Class1 myClass;
        private Model.PO.POImportConfig currPOImportConfig;
        public static HttpStatusCode doImportPO(Class1 myClass, ASCTracInterfaceModel.Model.PO.POHdrImport aData, ref string errmsg)
        {
            //myClass = Class1.InitParse funcType, ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            string OrderNum = aData.PONUMBER;
            try
            {
                if (!myClass.FunctionAuthorized(myClass.myLogRecord.FunctionID))
                    retval = HttpStatusCode.NonAuthoritativeInformation;
                else
                {
                    var siteid = myClass.GetSiteIdFromHostId(aData.FACILITY);
                    //currPOImportConfig = Configs.POConfig.getPOImportSite(siteid, myClass.myParse.Globals);
                    if (String.IsNullOrEmpty(siteid))
                    {
                        myClass.myLogRecord.LogType = "E";
                        errmsg = "No Facility or Site defined for record.";
                        retval = HttpStatusCode.BadRequest;
                    }
                    else if (string.IsNullOrEmpty(OrderNum))
                    {
                        myClass.myLogRecord.LogType = "E";
                        errmsg = "PO Number value is required.";
                        retval = HttpStatusCode.BadRequest;
                    }
                    else if (string.IsNullOrEmpty(aData.STATUS_FLAG))
                    {
                        myClass.myLogRecord.LogType = "E";
                        errmsg = "STATUS_FLAG value is required.";
                        retval = HttpStatusCode.BadRequest;
                    }
                    else
                    {
                        var myimport = new ImportPO(myClass, siteid);
                        string WarningMsg = string.Empty;
                        if (String.IsNullOrEmpty(aData.ORDER_TYPE))
                            retval = myimport.ImportPORecord(aData, ref errmsg, ref WarningMsg);
                        else if (aData.ORDER_TYPE.Equals("R"))
                            retval = myimport.ImportRMARecord(aData, ref errmsg, ref WarningMsg);
                        else if (aData.ORDER_TYPE.Equals("A"))
                            retval = myimport.ImportASNRecord(aData, ref errmsg, ref WarningMsg);
                        else
                            retval = myimport.ImportPORecord(aData, ref errmsg, ref WarningMsg);
                        if (!String.IsNullOrEmpty(errmsg))
                            retval = HttpStatusCode.BadRequest;
                        else
                            errmsg = WarningMsg;
                    }

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
        public ImportPO(Class1 aClass, string aSiteID)
        {
            myClass = aClass;
            siteid = aSiteID;
            currPOImportConfig = Configs.POConfig.getPOImportSite(siteid, myClass.myParse.Globals);
        }



        #region RMA_REGION
        private HttpStatusCode ImportRMARecord(ASCTracInterfaceModel.Model.PO.POHdrImport aData, ref string errmsg, ref string aWarningMsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            string tmp = string.Empty;
            bool fExist = false;
            if (myClass.myParse.Globals.myGetInfo.GetRMAHdrInfo(aData.PONUMBER, "STATUS", ref tmp))
            {
                fExist = true;
                if (tmp.Equals("C"))
                {
                    errmsg = "Cannot update RMA# " + aData.PONUMBER + ": RMA is already received.";
                }
            }
            if (String.IsNullOrEmpty(errmsg))
            {
                myClass.myParse.Globals.mydmupdate.InitUpdate();
                if (aData.STATUS_FLAG.Equals("D") || aData.STATUS_FLAG.Equals("V"))
                {
                    if (fExist)
                        errmsg = DeleteRma(aData.PONUMBER);
                }
                else
                {
                    if (fExist)
                        errmsg = PurgeRMADet(aData.PONUMBER);
                    if (String.IsNullOrEmpty(errmsg))
                    {
                        errmsg = ImportRMAHeaderRecord(aData, fExist);
                        if (string.IsNullOrEmpty(errmsg))
                            errmsg = ImportRMADetailRecords(aData, ref aWarningMsg);
                    }
                }
                if (string.IsNullOrEmpty(errmsg))
                    myClass.myParse.Globals.mydmupdate.ProcessUpdates();
            }
            return (retval);
        }

        private string ImportRMAHeaderRecord(ASCTracInterfaceModel.Model.PO.POHdrImport aData, bool recExists)
        {
            string errmsg = string.Empty;

            string custName = string.Empty;
            if( !String.IsNullOrEmpty( aData.VENDOR_CODE))
                myClass.myParse.Globals.myGetInfo.GetCustInfo(aData.VENDOR_CODE, "SHIPTONAME", ref custName);

            string rmaType = aData.RMA_TYPE;
            if (String.IsNullOrEmpty(rmaType))
                rmaType = currPOImportConfig.RMA_TYPE;

            string updStr = string.Empty;
            if (!recExists)
            {
                Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "RMA_NUM", aData.PONUMBER);
                ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "CREATE_DATE", "GETDATE()");
                Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "CREATE_USERID", currPOImportConfig.GatewayUserID);

                if (aData.STATUS_FLAG != "C")
                    Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "STATUS", "O");
                if (aData.ENTRY_DATE == DateTime.MinValue)
                    ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "RMA_DATE", "GETDATE()");
            }

            Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "SITE_ID", siteid);
            if (aData.STATUS_FLAG == "C")
                Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "STATUS", "C");
            if (!String.IsNullOrEmpty(rmaType))
                Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "RMA_TYPE", rmaType);

            Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "CUSTID", aData.VENDOR_CODE);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "SHIP_VIA", aData.CARRIER);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "SHIP_NAME", custName);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "SHIP_ADDRESS1", aData.ADDR_LINE1);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "SHIP_ADDRESS2", aData.ADDR_LINE2);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "SHIP_CITY", aData.CITY);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "SHIP_STATE", aData.STATE);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "SHIP_ZIPCODE", aData.ZIP);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "SHIP_COUNTRY", aData.COUNTRY);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "SHIP_PERSON", aData.CONTACT_NAME);
            if (aData.ENTRY_DATE != DateTime.MinValue)
                Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "RMA_DATE", aData.ENTRY_DATE.ToString());
            if (aData.ARRIVAL_DATE != DateTime.MaxValue)
                Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_HDR", "EXPECTEDDATE", aData.ARRIVAL_DATE.ToString());

            if (recExists)
                myClass.myParse.Globals.mydmupdate.UpdateFields("RMA_HDR", updStr, "RMA_NUM='" + aData.PONUMBER + "' AND SITE_ID='" + siteid + "'");
            else
                myClass.myParse.Globals.mydmupdate.InsertRecord("RMA_HDR", updStr);

            ImportNotes.SaveNotes("R", aData.PONUMBER, aData.DELIVERY_INSTRUCTIONS, false, 0, 1, myClass.myParse.Globals);

            return (errmsg);
        }

        private string ImportRMADetailRecords(ASCTracInterfaceModel.Model.PO.POHdrImport aData, ref string aWarningMsg)
        {
            string errmsg = string.Empty;
            if (currPOImportConfig.GWDeleteRMALinesNotInInterface)
                DeleteMissingRMALines(aData);

            string rmaNum = aData.PONUMBER;
            foreach (var rec in aData.PODetList)
            {
                var lineNum = rec.LINE_NUMBER;
                string itemId = rec.PRODUCT_CODE;
                if (String.IsNullOrEmpty(itemId))
                {
                    errmsg = "Item ID is required.  Line:  " + lineNum;
                    break;
                }

                string uom = string.Empty;
                string ascItemId = myClass.myParse.Globals.dmMiscItem.GetASCItem(siteid, itemId, aData.VMI_CUSTID);
                if (!myClass.myParse.Globals.myGetInfo.GetASCItemInfo(ascItemId, "STOCK_UOM", ref uom))
                {
                    if (currPOImportConfig.GWPOMissingItemAction == 2)
                        AddItem(siteid, itemId, aData.VMI_CUSTID);  //added 01-23-17 (JXG)
                    else if (currPOImportConfig.GWPOMissingItemAction == 0)
                    {
                        return "Item " + itemId + " does not exist)";
                    }
                    else
                    {
                        aWarningMsg += itemId + ascLibrary.dbConst.HHDELIM;
                        continue;
                    }
                }
                if( !String.IsNullOrEmpty( rec.UOM))
                    uom = rec.UOM;
                double qty = rec.QUANTITY;

                string whereStr = "RMA_NUM='" + rmaNum + "' AND LINENUM=" + lineNum.ToString();
                string sqlStr = "SELECT STATUS FROM RMADET (NOLOCK) WHERE " + whereStr;
                string currStatus = string.Empty;
                bool recExists = myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref currStatus);
                if (!currStatus.Equals("O"))
                {

                }
                else
                {
                    string importAction = "O";
                    if (!String.IsNullOrEmpty(rec.STATUS_FLAG))
                        importAction = rec.STATUS_FLAG;
                    if (importAction == "D")
                    {
                        myClass.myParse.Globals.mydmupdate.DeleteRecord("RMADET", whereStr);
                    }
                    else if (importAction == "C")
                    {
                        sqlStr = "UPDATE RMADET SET STATUS='C' WHERE" + whereStr;
                        myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
                    }
                    else
                    {
                        double convFact = 1;
                        //if (String.IsNullOrEmpty(uom))
                        //    myClass.myParse.Globals.myGetInfo.GetASCItemInfo(ascItemId, "STOCK_UOM", ref uom);
                        myClass.GetConvQty(ascItemId, uom, false, ref qty, ref convFact);

                        string updStr = string.Empty;

                        if (!recExists)
                        {
                            Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_det", "RMA_NUM", rmaNum);
                            Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_det", "LINENUM", lineNum.ToString());
                            Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_det", "STATUS", "O");
                            Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_det", "CREATE_USERID", currPOImportConfig.GatewayUserID);
                            ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "CREATE_DATE", "GETDATE()");
                        }

                        Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_det", "ASCITEMID", ascItemId);
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_det", "ITEMID", itemId);
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_det", "QTY", qty.ToString());
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_det", "COSTEACH", rec.COSTEACH.ToString());
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_det", "REPAIR_COST", rec.COSTEACH.ToString());
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_det", "CUST_PRICE", rec.COSTEACH.ToString());
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_det", "PROBLEM1", rec.COMMENT);
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_det", "LOTID", rec.LOTID);
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_det", "ORIG_ORDERNUM", rec.ORIG_ORDERNUMBER);
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_det", "RMA_TYPE", aData.ORDER_TYPE);
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_det", "QTY_DUAL_UNIT", rec.CW_QTY.ToString());
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "RMA_det", "SERIAL_NUM", rec.SERIAL_NUM);

                        if (recExists)
                            myClass.myParse.Globals.mydmupdate.UpdateFields("RMA_DET", updStr, whereStr);
                        else
                            myClass.myParse.Globals.mydmupdate.InsertRecord("RMA_DET", updStr);

                    }
                }
            }

            return (errmsg);
        }

        private string DeleteRma(string rmaNum)
        {
            string errMsg = string.Empty;

            string sqlStr = "DELETE FROM RMADET WHERE RMA_NUM='" + rmaNum + "'";
            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
            sqlStr = "DELETE FROM RMAHDR WHERE RMA_NUM='" + rmaNum + "'";
            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
            return (errMsg);
        }

        private string PurgeRMADet(string rmaNum)
        {
            string errMsg = string.Empty;
            string tmpStr = string.Empty;
            if (currPOImportConfig.GWPurgeRMADetOnImport)
            {
                try
                {
                    string sql = "SELECT RMA_NUM FROM RMADET (NOLOCK) " +
                        "WHERE RMA_NUM='" + rmaNum + "' AND STATUS<>'O' AND STATUS<>'A'";
                    if (myClass.myParse.Globals.myDBUtils.ifRecExists(sql))
                    {
                        sql = "DELETE FROM RMADET WHERE RMA_NUM='" + rmaNum + "'";
                        myClass.myParse.Globals.myDBUtils.RunSqlCommand(sql);
                    }
                    else
                    {
                        errMsg = "Error Importing RMA# " + rmaNum + ": Cannot purge RMA lines " +
                            "because RMA has inventory received against it.";
                    }
                }
                catch (Exception e)
                {
                    errMsg = "Error Importing RMA# " + rmaNum + ": " + e.Message;
                }
            }
            return (errMsg);
        }

        private void DeleteMissingRMALines(ASCTracInterfaceModel.Model.PO.POHdrImport aData)
        {

            string wherestr = string.Empty;
            foreach (var rec in aData.PODetList)
            {
                if (!String.IsNullOrEmpty(wherestr))
                    wherestr += ",";
                wherestr += rec.LINE_NUMBER.ToString();
            }
            if (!String.IsNullOrEmpty(wherestr))
                wherestr = "RMA_NUM='" + aData.PONUMBER + "' AND STATUS='O' AND LINENUM NOT IN ( " + wherestr + ")";
            myClass.myParse.Globals.mydmupdate.DeleteRecord("RMA_DET", wherestr);
        }

        #endregion
        #region ASN_REGION
        private HttpStatusCode ImportASNRecord(ASCTracInterfaceModel.Model.PO.POHdrImport aData, ref string errmsg, ref string aWarningMsg)
        {
            HttpStatusCode retval = HttpStatusCode.BadRequest;
            errmsg = "ASNs no longer supported through the Receipts Import. Please use the ASN Import.";
            return (retval);
        }
        #endregion
        #region PO_REGION
        private HttpStatusCode ImportPORecord(ASCTracInterfaceModel.Model.PO.POHdrImport aData, ref string errmsg, ref string aWarningMsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            string ponum = aData.PONUMBER;
            string relnum = aData.RELEASENUM;
            if (String.IsNullOrEmpty(relnum))
                relnum = "00";
            String poStatus = string.Empty;
            bool fExist = false;
            if (myClass.myParse.Globals.myGetInfo.GetPOHdrInfo(ponum, relnum, "RECEIVED", ref poStatus))
            {
                fExist = true;
                if (poStatus.Equals("C"))
                    errmsg = "Cannot update PO# " + ponum + ": PO is already Completed.";
                else if (poStatus.Equals("X"))
                    errmsg = "Cannot update PO# " + ponum + ": PO is already Cancelled.";
            }
            if (String.IsNullOrEmpty(errmsg))
            {
                myClass.myParse.Globals.mydmupdate.InitUpdate();
                myClass.myParse.Globals.mydmupdate.SetItemMasterQtyExpected(ponum, relnum, true);  //"00"  //changed 08-18-20 (JXG)
                if (String.IsNullOrEmpty(aData.STATUS_FLAG))
                    errmsg = "Invalid Status flag";
                else if (aData.STATUS_FLAG.Equals("D") || aData.STATUS_FLAG.Equals("V"))
                {
                    if (fExist)
                        errmsg = PurgePoDet(ponum, relnum, true);
                }
                else
                {
                    if (poStatus.Equals("R"))
                        errmsg = "Cannot update PO# " + ponum + ": PO is already Received.";
                    else
                    {
                        if (fExist && currPOImportConfig.GWPurgePODetOnImport)
                        {
                            errmsg = PurgePoDet(ponum, relnum, false);
                            if (String.IsNullOrEmpty(errmsg))
                                myClass.myParse.Globals.mydmupdate.ProcessUpdates();

                        }
                        if (String.IsNullOrEmpty(errmsg))
                        {
                            errmsg = ImportPOHeaderRecord(aData, ponum, relnum, fExist);
                            if (string.IsNullOrEmpty(errmsg))
                                errmsg = ImportPODetailRecords(aData, ponum, relnum, ref aWarningMsg);
                            if (string.IsNullOrEmpty(errmsg))
                                ImportOrderNotes(aData.NotesList, ponum, relnum, string.Empty);
                        }
                    }
                }
                if (string.IsNullOrEmpty(errmsg))
                {
                    myClass.myParse.Globals.mydmupdate.ProcessUpdates();
                    myClass.myParse.Globals.mydmupdate.SetItemMasterQtyExpected(ponum, relnum, false);  //"00"  //changed 08-18-20 (JXG)

                    string wherestr = "PONUMBER='" + ponum + "' AND RELEASENUM='" + relnum + "'";
                    //#region update pohdr values  //added 10-02-19 (JXG)
                    string sql = "UPDATE POHDR SET NUM_LINES = ( SELECT COUNT( LINENUMBER) FROM PODET WHERE " + wherestr + ") WHERE " + wherestr;
                    myClass.myParse.Globals.mydmupdate.AddToUpdate(sql);

                    myClass.myParse.Globals.mydmupdate.ProcessUpdates();
                }
            }
            return (retval);
        }

        private string PurgePoDet(string aponum, string arelnum, bool aDelHeader)
        {
            string retval = String.Empty;
            {
                if (myClass.myParse.Globals.dmMiscFunc.GetCount("PODET", "PONUMBER='" + aponum + "'AND RELEASENUM='" + arelnum + "' AND QTYRECEIVED>0") > 0)
                {
                    retval = "Error Importing PO# " + aponum + ": Cannot purge PO lines " +
                        "because PO has inventory received against it.";
                }
                else
                {
                    // Decrement the item qty from expected
                    //myClass.myParse.Globals.mydmupdate.SetItemMasterQtyExpected(aponum, arelnum, true);  //"00"  //changed 08-18-20 (JXG)
                    myClass.myParse.Globals.mydmupdate.DeleteRecord("PODET", "PONUMBER='" + aponum + "' AND RELEASENUM='" + arelnum + "'");
                    if (aDelHeader)
                    {
                        myClass.myParse.Globals.mydmupdate.DeleteRecord("POHDR", "PONUMBER='" + aponum + "' AND RELEASENUM='" + arelnum + "'");
                        myClass.myParse.Globals.mydmupdate.DeleteRecord("NOTES", "TYPE IN ( 'H', 'D') AND ORDERNUM='" + aponum + "'");
                    }
                }
            }
            return (retval);
        }

        private string ImportPOHeaderRecord(ASCTracInterfaceModel.Model.PO.POHdrImport aData, string ponum, string relnum, bool recExists)
        {
            string retval = String.Empty;

            var importAction = aData.STATUS_FLAG;
            var orderType = aData.ORDER_TYPE;
            orderType = orderType == "T" ? orderType : "S";

            var promoCode = aData.PROMO_CODE;

            string updStr = string.Empty;
            if (!recExists)
            {
                Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "PONUMBER", ponum);
                Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "RELEASENUM", relnum);
                Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "ORDERTYPE", orderType);
                ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "CREATE_DATE", "GETDATE()");
                Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "CREATE_USERID", currPOImportConfig.GatewayUserID);
                if (importAction != "C")
                    Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "RECEIVED", "O");
                // ORDER_SOURCE F for Demand Forecasting,  Import, P-PO module, M- MRP, S for plus sign.,A=AutoCreate
                Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "ORDER_SOURCE", "I");
                Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "SITE_ID", siteid);

            }

            if (importAction == "C")
            {
                Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "RECEIVED", "R");
            }

            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "VENDORID", aData.VENDOR_CODE);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "BUYER", aData.BUYER_CODE_ID);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "LINKED_ORDERNUMBER", aData.LINKED_ORDERNUMBER);
            if (aData.ARRIVAL_DATE != DateTime.MinValue)
                Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "EXPECTEDRECEIPTDATE", aData.ARRIVAL_DATE.ToString());
            if (aData.ENTRY_DATE != DateTime.MinValue)
                Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "ORDERDATE", aData.ENTRY_DATE.ToString());
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "CARRIERNAME", aData.CARRIER);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "TERMS_ID", aData.TERMS_ID);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "SHIP_TO_NAME", aData.SHIP_TO_NAME);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "SHIP_ADDRESS1", aData.ADDR_LINE1);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "SHIP_ADDRESS2", aData.ADDR_LINE2);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "SHIP_ADDRESS3", aData.ADDR_LINE3);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "SHIP_CITY", aData.CITY);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "SHIP_STATE", aData.STATE);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "SHIP_ZIP", aData.ZIP);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "SHIP_COUNTRY", aData.COUNTRY);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "SHIP_CONTACT", aData.CONTACT_NAME);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "SHIP_TELEPHONE", aData.CONTACT_TEL);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "BILL_TO_NAME", aData.BILL_TO_NAME);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "BILL_ADDRESS1", aData.BILL_ADDR_LINE1);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "BILL_ADDRESS2", aData.BILL_ADDR_LINE2);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "BILL_ADDRESS3", aData.BILL_ADDR_LINE3);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "BILL_CITY", aData.BILL_CITY);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "BILL_STATE", aData.BILL_STATE);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "BILL_ZIP", aData.BILL_ZIP);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "BILL_COUNTRY", aData.BILL_COUNTRY);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "BILL_CONTACT", aData.BILL_CONTACT_NAME);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "BILL_TELEPHONE", aData.BILL_CONTACT_TEL);
            Utils.ASCUtils.CheckAndAppend(ref updStr, "POHDR", "DIRECT_SHIP_ORDERNUMBER", aData.DIRECT_SHIP_ORDERNUMBER);

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr,"REQ_NUM", aData.REQ_NUM);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr,"SEAL_NUM", aData.SEAL_NUM);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr,"VMI_CUSTID", aData.VMI_CUSTID);

            SaveCustomFields(ref updStr, aData.CustomList, currPOImportConfig.GWPOHdrTranslation);

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr,"COMMENTS", aData.DELIVERY_INSTRUCTIONS);
            /////////////////////////////////////

            // ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr,"HOST_PONUMBER", hostPoNum);  //added 09-20-16 (JXG) for Driscoll's

            if (recExists)
                myClass.myParse.Globals.mydmupdate.UpdateFields("POHDR", updStr, "PONUMBER='" + ponum + "' AND RELEASENUM='" + relnum + "'");
            else
                myClass.myParse.Globals.mydmupdate.InsertRecord("POHDR", updStr);

            ImportNotes.SaveNotes("H", ponum, aData.DELIVERY_INSTRUCTIONS, false, 0, 1, myClass.myParse.Globals);
            return (retval);
        }


        private string ImportPODetailRecords(ASCTracInterfaceModel.Model.PO.POHdrImport aData, string ponum, string relnum, ref string aWarningMsg)
        {
            string retval = String.Empty;

            if( currPOImportConfig.GWDeletePOLinesNotInInterface)
            {
                DeleteMissingPOLines(aData, ponum, relnum);
            }

            long nextLinenum = myClass.myParse.Globals.dmMiscFunc.getnextinorder("PODET", "PONUMBER = '" + ponum + "' AND RELEASENUM = '" + relnum + "'", "LINENUMBER");
            foreach (var rec in aData.PODetList)
            {
                string tmpStr = string.Empty;
                long llineNum = rec.LINE_NUMBER;
                var itemId = rec.PRODUCT_CODE;
                if (llineNum<= 0)
                {
                    if (currPOImportConfig.GWPOUseHostLineToCalcLineNum)
                    {
                        if (myClass.myParse.Globals.myDBUtils.ReadFieldFromDB("SELECT LINENUMBER FROM PODET WHERE PONUMBER = '" + ponum + "' AND RELEASENUM = '" + relnum + "' AND HOST_LINENUMBER = '" + rec.HOST_LINENUMBER.ToString() + "'", "", ref tmpStr))
                            llineNum = ascLibrary.ascUtils.ascStrToInt(tmpStr, 0);
                    }
                    else
                    {
                        if (myClass.myParse.Globals.myDBUtils.ReadFieldFromDB("SELECT LINENUMBER FROM PODET WHERE PONUMBER = '" + ponum + "' AND RELEASENUM = '" + relnum + "' AND ITEMID='" + itemId + "'", "", ref tmpStr))
                            llineNum = ascLibrary.ascUtils.ascStrToInt(tmpStr, 0);
                    }

                    if (llineNum <= 0)
                    {
                        llineNum = nextLinenum;
                        nextLinenum++;
                    }
                }
                var lineNum = llineNum.ToString();
                if (String.IsNullOrEmpty(itemId))
                {
                    retval= "Item ID is required.  Line:  " + lineNum;
                    break;
                }
                var upcCode = rec.UPC_CODE;
                var itemDesc = rec.ITEM_DESCRIPTION;
                var qty = rec.QUANTITY;
                var uom = rec.UOM;

                string ascItemId = myClass.myParse.Globals.dmMiscItem.GetASCItem(siteid, itemId, aData.VMI_CUSTID);

                if (!myClass.myParse.Globals.myGetInfo.GetASCItemInfo(ascItemId, "BUY_UOM, STOCK_UOM, UNIT_MEAS1, UNIT_MEAS2, UNIT_MEAS3, UNIT_MEAS4", ref tmpStr))
                {
                    if (currPOImportConfig.GWPOMissingItemAction == 2)
                    {
                        AddItem(siteid, itemId, aData.VMI_CUSTID);  //added 01-23-17 (JXG)
                        myClass.myParse.Globals.myGetInfo.GetASCItemInfo(ascItemId, "BUY_UOM, STOCK_UOM, UNIT_MEAS1, UNIT_MEAS2, UNIT_MEAS3, UNIT_MEAS4", ref tmpStr);
                    }
                    else if (currPOImportConfig.GWPOMissingItemAction == 0)
                    {
                        retval = "Item " + itemId + " does not exist in ITEMMSTR";
                        break;
                    }
                    else
                    {
                        aWarningMsg += itemId + ascLibrary.dbConst.HHDELIM;
                        continue;
                    }
                }

                string stockUom = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                string buyUom = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                if (string.IsNullOrEmpty(uom))
                    uom = buyUom;
                if (string.IsNullOrEmpty(uom))
                    uom = stockUom;

                string uomField = string.Empty;
                if (ascLibrary.ascStrUtils.GetNextWord(ref tmpStr) == uom)
                    uomField = "UNIT_MEAS1";
                else if (ascLibrary.ascStrUtils.GetNextWord(ref tmpStr) == uom)
                    uomField = "UNIT_MEAS2";
                else if (ascLibrary.ascStrUtils.GetNextWord(ref tmpStr) == uom)
                    uomField = "UNIT_MEAS3";
                else if (ascLibrary.ascStrUtils.GetNextWord(ref tmpStr) == uom)
                    uomField = "UNIT_MEAS4";

                double convFact = 1;
                if (rec.BUY_TO_STOCK_CONV_FACTOR > 0)
                    convFact = rec.BUY_TO_STOCK_CONV_FACTOR;
                else if (!String.IsNullOrEmpty(uomField))
                    convFact = myClass.GetItemConv(ascItemId, "STOCK_UOM", uomField);

                double qtyRcvd = 0;
                string oldAscItemId = string.Empty;
                string currStatus = string.Empty;
                bool recExists = myClass.myParse.Globals.myGetInfo.GetPODetInfo(ponum, relnum, lineNum, "RECEIVED, QTYRECEIVED, ASCITEMID", ref oldAscItemId);
                if (recExists)
                {
                    currStatus = ascLibrary.ascStrUtils.GetNextWord(ref oldAscItemId);
                    qtyRcvd = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref oldAscItemId), 0);
                }

                /*
            string sqlStr = "QTY, ASCITEMID, ITEMID, QTYRECEIVED, " +
                "VENDORITEMID, HOST_LINENUMBER, COSTEACH, LOTID, BUY_TO_STOCK_CONV_FACTOR, EXPECTEDRECEIPTDATE, EXP_SHIP_DATE, USER_REQUIRED_DATE, " +  //added 09-17-15 (JXG) for Driscoll's
                "UNITMEAS, DIRECT_SHIP_ORDERNUMBER, LINKED_ORDERNUMBER, CUSTOM_DATA1, CUSTOM_DATA2, CUSTOM_DATA3";  //added 09-17-15 (JXG) for Driscoll's
                                                                                                                    //added 09-17-15 (JXG) for Driscoll's
            if (!String.IsNullOrEmpty(impDetCustData1AsFieldName) && MiscFuncs.FieldExists(Globals.ascConnStr, "PODET", impDetCustData1AsFieldName))
                sqlStr += ", " + impDetCustData1AsFieldName;
            if (!String.IsNullOrEmpty(impDetCustData2AsFieldName) && MiscFuncs.FieldExists(Globals.ascConnStr, "PODET", impDetCustData2AsFieldName))
                sqlStr += ", " + impDetCustData2AsFieldName;
            if (!String.IsNullOrEmpty(impDetCustData3AsFieldName) && MiscFuncs.FieldExists(Globals.ascConnStr, "PODET", impDetCustData3AsFieldName))
                sqlStr += ", " + impDetCustData3AsFieldName;
            if (!String.IsNullOrEmpty(impDetCustData4AsFieldName) && MiscFuncs.FieldExists(Globals.ascConnStr, "PODET", impDetCustData4AsFieldName))
                sqlStr += ", " + impDetCustData4AsFieldName;
            if (!String.IsNullOrEmpty(impDetCustData5AsFieldName) && MiscFuncs.FieldExists(Globals.ascConnStr, "PODET", impDetCustData5AsFieldName))
                sqlStr += ", " + impDetCustData5AsFieldName;
            if (!String.IsNullOrEmpty(impDetCustData6AsFieldName) && MiscFuncs.FieldExists(Globals.ascConnStr, "PODET", impDetCustData6AsFieldName))
                sqlStr += ", " + impDetCustData6AsFieldName;
            /////////////////////////////////////
            //added 03-16-17 (JXG)
            if (vendProdLineInInterface)
                sqlStr += ", VEND_PRODLINE";
            if (qcReasonInInterface)
                sqlStr += ", QC_REASON";
            if (altLotIdInInterface)
                sqlStr += ", ALT_LOTID";
            //////////////////////
            recExists = AscDbUtils.GetPODetInfo(ponum, releaseNum, lineNum.ToString(), sqlStr, ref tmpStr);
            double oldqtytorecv = AscUtils.ConvToDbl(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
            string oldAscItemId = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            oldItemId = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            qtyRcvd = AscUtils.ConvToDec(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr));
            double chgqtytorecv = oldqtytorecv - Convert.ToDouble(qty);
            //added 09-17-15 (JXG) for Driscoll's
            prevVendorItemId = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            prevCostEach = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            prevHostLineNum = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            prevLotId = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            prevConvFact = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            prevExpRcptDate = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            prevExpShipDate = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            prevUserReqDate = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            prevUOM = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            prevDirectShipOrderNum = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            prevLinkedOrderNum = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            prevCustomData1 = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            prevCustomData2 = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            prevCustomData3 = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            //added 09-17-15 (JXG) for Driscoll's
            if (!String.IsNullOrEmpty(impDetCustData1AsFieldName) && MiscFuncs.FieldExists(Globals.ascConnStr, "PODET", impDetCustData1AsFieldName))
                prevDetCustData1As = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            if (!String.IsNullOrEmpty(impDetCustData2AsFieldName) && MiscFuncs.FieldExists(Globals.ascConnStr, "PODET", impDetCustData2AsFieldName))
                prevDetCustData2As = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            if (!String.IsNullOrEmpty(impDetCustData3AsFieldName) && MiscFuncs.FieldExists(Globals.ascConnStr, "PODET", impDetCustData3AsFieldName))
                prevDetCustData3As = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            if (!String.IsNullOrEmpty(impDetCustData4AsFieldName) && MiscFuncs.FieldExists(Globals.ascConnStr, "PODET", impDetCustData4AsFieldName))
                prevDetCustData4As = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            if (!String.IsNullOrEmpty(impDetCustData5AsFieldName) && MiscFuncs.FieldExists(Globals.ascConnStr, "PODET", impDetCustData5AsFieldName))
                prevDetCustData5As = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            if (!String.IsNullOrEmpty(impDetCustData6AsFieldName) && MiscFuncs.FieldExists(Globals.ascConnStr, "PODET", impDetCustData6AsFieldName))
                prevDetCustData6As = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            /////////////////////////////////////
            //added 03-16-17 (JXG)
            if (vendProdLineInInterface)
                prevVendProdLine = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            if (qcReasonInInterface)
                prevQCReason = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            if (altLotIdInInterface)
                prevAltLotId = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
            //////////////////////
            */
                string sqlStr;
                string whereStr = "PONUMBER='" + ponum + "' AND RELEASENUM='" + relnum + "' AND LINENUMBER=" + lineNum;
                string importAction = "O";
                if (!String.IsNullOrEmpty(rec.STATUS_FLAG))
                    importAction = rec.STATUS_FLAG;
                if (importAction == "D")
                {
                    if (qtyRcvd == 0)
                    {
                        oldAscItemId = "";
                        // Decrement the item qty from expected
                        //Service1.Parse.Globals.mydmupdate.SetItemMasterQtyExpected(ponum, "00", lineNum, true);
                        //Service1.Parse.Globals.mydmupdate.ProcessUpdates();

                        sqlStr = "DELETE FROM PODET WHERE " + whereStr;
                        myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
                    }
                    else
                    {
                        retval = "Error on PO# " + ponum + ", Release " + relnum + ", line " + lineNum + ": " +
                            "Cannot delete PO line item that has been received.";
                        myClass.WriteException("POImport", "PO# " + ponum + ", Release " + relnum + ", line " + lineNum, ponum, retval, "");
                        break;
                    }
                }
                else if (importAction == "C")
                {
                    oldAscItemId = "";
                    // Decrement the item qty from expected
                    //Service1.Parse.Globals.mydmupdate.SetItemMasterQtyExpected(ponum, "00", lineNum, true);
                    //Service1.Parse.Globals.mydmupdate.ProcessUpdates();

                    sqlStr = "UPDATE PODET SET RECEIVED='R' WHERE " + whereStr;
                    myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
                }
                else
                {
                    string updStr = string.Empty;

                    if (!recExists)
                    {
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "PONUMBER", ponum);
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "RELEASENUM", relnum); // "00");
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "LINENUMBER", lineNum);
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "RECEIVED", "O");
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "REJECTED", "N");
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "QTYOUTOFTOL", "0");
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "QTYRECEIVED", "0");
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "QTYLASTRECV", "0");
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "LINETOTAL", "0");
                    }

                    // Only change item id if po not already started to receive
                    if (!recExists || qtyRcvd == 0)
                    {
                        /////////////////////////////////////
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "ASCITEMID", ascItemId);
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "ITEMID", itemId);
                    }
                    else if (ascItemId != oldAscItemId)
                    {
                        retval = "Error on PO# " + ponum + ", Release " + relnum + ", line " + lineNum + ": " +
                       "Cannot change Item ID on PO Line that was already received.";
                        myClass.WriteException("POImport", "PO# " + ponum + ", Release " + relnum + ", line " + lineNum, ponum, retval, "");
                        break;
                    }

                    // If change po qty to less than what's already received, don't update qty
                    if (qtyRcvd <= qty)
                    {
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "QTY", qty.ToString());
                    }
                    else
                    {
                        retval = "Error on PO# " + ponum + ", Release " + relnum + ", line " + lineNum + ": " +
                        "Cannot change qty to less than the quantity already received.";
                        myClass.WriteException("POImport", "PO# " + ponum + ", Release " + relnum + ", line " + lineNum, ponum, retval, "");
                        break;
                    }

                    /////////////////////////////////////
                    Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "VENDORITEMID", rec.VENDOR_ITEM_ID);
                    Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "COSTEACH", rec.COSTEACH.ToString());
                    Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "LOTID", rec.LOTID);
                    Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "BUY_TO_STOCK_CONV_FACTOR", convFact.ToString());
                    if (rec.EXPECTED_RECEIPT_DATE != DateTime.MinValue)
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "EXPECTEDRECEIPTDATE", rec.EXPECTED_RECEIPT_DATE.ToString());
                    if (rec.EXPECTED_SHIP_DATE != DateTime.MinValue)
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "EXP_SHIP_DATE", rec.EXPECTED_SHIP_DATE.ToString());
                    if (rec.USER_REQUIRED_DATE != DateTime.MinValue)
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "USER_REQUIRED_DATE", rec.USER_REQUIRED_DATE.ToString());
                    Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "UNITMEAS", uom);
                    Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "DIRECT_SHIP_ORDERNUMBER", rec.DIRECT_SHIP_ORDERNUMBER);
                    Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "LINKED_ORDERNUMBER", rec.LINKED_ORDERNUMBER);
                    Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "HOST_LINENUMBER", rec.HOST_LINENUMBER.ToString());
                    Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "VEND_PRODLINE", rec.VEND_PRODLINE);

                    if (!string.IsNullOrEmpty(rec.QC_REASON))
                    {
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "QC_REASON", rec.QC_REASON);
                        Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "QC_CHECK", "T");  //QC_RULE  //changed 05-08-17 (JXG)
                    }

                    Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "ALT_LOTID", rec.ALT_LOTID);
                    Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "PROJECT_NUMBER", rec.PROJECT_NUMBER);
                    //////////////////////


                    SaveCustomFields(ref updStr, rec.CustomList, currPOImportConfig.GWPODetTranslation);
                    // Custom data fields
                    //Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "CUSTOM_DATA1", rec.CUSTOM_DATA1);
                    //Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "CUSTOM_DATA2", rec.CUSTOM_DATA2);
                    //Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "CUSTOM_DATA3", rec.CUSTOM_DATA3);

                    //added 04-12-16 (JXG) for Driscoll's
                    //CheckFieldChanged("D", false, recExists, "ITEM_NOTES", prevItemNotes, rec.COMMENT);
                    Utils.ASCUtils.CheckAndAppend(ref updStr, "PODET", "ITEM_NOTES", rec.COMMENT);
                    /////////////////////////////////////

                    if (recExists)
                    {
                        // Decrement the item qty from expected, as this is an edit
                        //myClass.myParse.Globals.mydmupdate.SetItemMasterQtyExpected(ponum, relnum, lineNum, true);
                        myClass.myParse.Globals.mydmupdate.UpdateFields("PODET", updStr, whereStr);

                    }
                    else
                    {
                        myClass.myParse.Globals.mydmupdate.InsertRecord("PODET", updStr);
                    }


                    string detPromoCode = rec.PROMO_CODE;
                    if (string.IsNullOrEmpty(detPromoCode))
                        detPromoCode = aData.PROMO_CODE;

                    if (!String.IsNullOrEmpty(detPromoCode) && myClass.myParse.Globals.myConfig.iniCPAllowPromoAlloc.boolValue)
                    {
                        string promoClientId = string.Empty;
                        sqlStr = "SELECT MASTER_CLIENT FROM PROMOS (NOLOCK) WHERE PROMO_CODE='" + detPromoCode + "'";
                        if (!myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref promoClientId))
                            myClass.AddPromo(detPromoCode, siteid, aData.VENDOR_CODE, myClass.myParse.Globals);

                        sqlStr = "SELECT NULL FROM PROMO_ITEMS (NOLOCK) " +
                            "WHERE PROMO_CODE='" + detPromoCode + "' AND ASCITEMID='" + ascItemId + "'";
                        if (!myClass.myParse.Globals.myDBUtils.ifRecExists(sqlStr))
                            myClass.AddPromoItem(detPromoCode, ascItemId, myClass.myParse.Globals);

                        string poNumForPromo = ponum + "-" + relnum;
                        sqlStr = "SELECT PROMO_CODE FROM PROMO_ORDERS (NOLOCK) " +
                            "WHERE PROMO_CODE='" + detPromoCode + "' AND SITE_ID='" + siteid + "' " +
                            "AND ASCITEMID='" + ascItemId + "' AND ORDERTYPE='P' " +
                            "AND ORDERNUMBER='" + ponum + "' AND LINENUMBER=" + lineNum.ToString();
                        if (!myClass.myParse.Globals.myDBUtils.ifRecExists(sqlStr))
                            myClass.UpdatePromoOrder(detPromoCode, siteid, ascItemId, ponum, lineNum, qty, "P", myClass.myParse.Globals);
                        else
                            myClass.AddPromoOrder(detPromoCode, ascItemId, ponum, lineNum, qty, "P", myClass.myParse.Globals);

                        myClass.UpdatePromoItemQty(detPromoCode, siteid, ascItemId, "P", myClass.myParse.Globals);
                    }

                    ImportNotes.SaveNotes("D", ponum, rec.COMMENT, false, Convert.ToInt32(rec.LINE_NUMBER), 1, myClass.myParse.Globals);
                    ImportOrderNotes(rec.NotesList, ponum, relnum, lineNum);
                }
            }
            return (retval);
        }

        private void ImportOrderNotes(List<ASCTracInterfaceModel.Model.NotesImport> NotesList, string ponum, string relnum, string alinenum)
        {
            foreach( var rec in NotesList)
            {
                int linenum = 0;
                if (!String.IsNullOrEmpty(alinenum))
                    linenum = Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(alinenum, 0));
                    string noteType = "H";
                if( linenum > 0 )
                    noteType = "D";
                ImportNotes.SaveNotes(noteType, ponum, rec.NOTE, false, linenum, Convert.ToInt32(rec.SEQNUM), myClass.myParse.Globals);
            }
        }

        private void DeleteMissingPOLines(ASCTracInterfaceModel.Model.PO.POHdrImport aData, string ponum, string relnum)
        {

            string wherestr = string.Empty;
            foreach (var rec in aData.PODetList)
            {
                if (!String.IsNullOrEmpty(wherestr))
                    wherestr += ",";
                wherestr += rec.LINE_NUMBER.ToString();
            }
            if (!String.IsNullOrEmpty(wherestr))
                wherestr = "PONUMBER='" + ponum + "' AND RELEASENUM='" + relnum + "' AND STATUS='O' AND LINENUM NOT IN ( " + wherestr + ")";
            myClass.myParse.Globals.mydmupdate.DeleteRecord("PODET", wherestr);
        }


        #endregion

        #region MISC_REGION
        private void AddItem(string siteId, string itemId, string VmiCustId)  //added 01-23-17 (JXG)
        {
            //if (currPOImportConfig.createSkeletonItems)
            {
                string ascItemId = siteId + "&" + itemId + "&" + VmiCustId;  //added 08-17-17 (JXG)
                string tmpStr = "";
                if (!myClass.myParse.Globals.myGetInfo.GetASCItemInfo(ascItemId, "SITE_ID", ref tmpStr))
                {
                    string zoneId = myClass.GetZone(siteId);

                    string updStr = string.Empty;
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ITEMID", itemId);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ASCITEMID", ascItemId);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "SITE_ID", siteId);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "DESCRIPTION", "Skeleton");
                    ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "CREATE_DATE", "GETDATE()");
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "CREATE_USERID", "GATEWAY");
                    ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "LAST_UPDATE", "GETDATE()");
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "LAST_UPDATE_USERID", "GATEWAY");
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "PURORMFG", "F"); // defItemType);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ITEM_STATUS", "I");  //Pending
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "FREIGHT_CLASS_CODE", ""); // defFreightClass);
                    ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "MAXSKIDSHIGH", "1");
                    ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "SKIDWIDTH", "1");
                    ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "SKIDLENGTH", "1");
                    ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "SKID_HEIGHT", "1");
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "TRACKBYSKID", "T"); // defTrackBy);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "UNIT_MEAS1", "EA");
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "UNIT_MEAS2", "EA");
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "LABEL_UOM", "CA"); // defLabelUOM);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "BUY_UOM", "EA");
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "STOCK_UOM", "EA");
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "CONV_FACT_12", "1");
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ABCZONE", "A");
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ZONEID", zoneId);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "TAXABLE", "F");
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "VMI_OWNERFLAG", "C");
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "VMI_RESPFLAG", "C");
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "VMI_CUSTID", VmiCustId);
                    //ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty( ref updStr, "ALLOW_BACKORDER", "T");

                    myClass.myParse.Globals.myDBUtils.InsertRecord("ITEMMSTR", updStr);

                    updStr = "IF NOT EXISTS ( SELECT * FROM ITEMQTY WHERE ASCITEMID='" + ascItemId + "')" +
                        " BEGIN" +
                        " INSERT INTO ITEMQTY" +
                        " (ASCITEMID, QTYTOTAL, QTYALLOC, QTYONHOLD, QTYSUBITEMS, QTYREQUIRED, QTYSCHEDULED)" +
                        " VALUES('" + ascItemId + "', 0, 0, 0, 0, 0, 0)" + 
                        " END ";
                    myClass.myParse.Globals.myDBUtils.RunSqlCommand(updStr);

                    myClass.ImportCustomData("ReceiptsNewItem", "ITEMMSTR", "ASCITEMID='" + ascItemId + "'", itemId);  //added 10-17-17 (JXG)
                }
            }
        }

        private void SaveCustomFields(ref string updStr, List<ASCTracInterfaceModel.Model.ModelCustomData> CustomList, Dictionary<string, List<string>> TranslationList)
        {
            foreach (var rec in CustomList)
            {
                if (TranslationList.ContainsKey(rec.FieldName.ToUpper()))
                {
                    var asclist = TranslationList[rec.FieldName.ToUpper()];
                    foreach (var ascfield in asclist)
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, ascfield, rec.Value);
                }
            }
        }
        #endregion
    }
}