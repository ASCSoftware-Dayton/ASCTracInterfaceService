using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Imports
{
    public class ImportLot
    {
        //private string funcType = "IM_LOT";
        private string siteid = string.Empty;
        private Class1 myClass;
        //private Model.Item.ItemImportConfig currImportConfig;
        public static HttpStatusCode doImportLot(Class1 myClass, ASCTracInterfaceModel.Model.Lot.LotImport aData, ref string errmsg)
        {
            //myClass = Class1.InitParse(funcType, ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            string ItemID = aData.PRODUCT_CODE;
            string lotId = aData.LOTID;
            string updstr = string.Empty;
            try
            {
                if (!myClass.FunctionAuthorized(myClass.myLogRecord.FunctionID))
                    retval = HttpStatusCode.NonAuthoritativeInformation;
                else
                {
                    var siteid = string.Empty;
                    var hostSiteid = aData.FACILITY;
                    if (!string.IsNullOrEmpty(hostSiteid))
                    {
                        siteid = myClass.GetSiteIdFromHostId(hostSiteid);
                        if (String.IsNullOrEmpty(siteid))
                        {
                            myClass.myLogRecord.LogType = "E";
                            errmsg = "Site ID not found for Host Site ID: " + hostSiteid + ", Lot: " + lotId + ", Item: " + ItemID;
                            retval = HttpStatusCode.BadRequest;
                        }
                    }

                    if (string.IsNullOrEmpty(errmsg))
                    {
                        if (string.IsNullOrEmpty(ItemID))
                        {
                            myClass.myLogRecord.LogType = "E";
                            errmsg = "Itemid (PRODUCT_CODE) value is required for Lot: " + lotId;
                            retval = HttpStatusCode.BadRequest;
                        }
                        else if (string.IsNullOrEmpty(lotId))
                        {
                            myClass.myLogRecord.LogType = "E";
                            errmsg = "LotID (LOTID) value is required for Item: " + ItemID;
                            retval = HttpStatusCode.BadRequest;
                        }
                        else
                        {
                            var myImport = new ImportLot(myClass, siteid);
                            retval = myImport.ImportLotRecord(aData, ref errmsg);
                        }
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

        public ImportLot(Class1 aClass, string aSiteID)
        {
            myClass = aClass;
            siteid = aSiteID;
            //currImportConfig = Configs.ItemConfig.getImportSite(siteid, myClass.myParse.Globals);
        }

        private HttpStatusCode ImportLotRecord(ASCTracInterfaceModel.Model.Lot.LotImport aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            string sqlStr, info = "", whereStr = "";
            string lotId, itemId, ascItemId, vmiCustId;
            bool recExists = false;

            myClass.myParse.Globals.initsite(siteid);
            myClass.myParse.Globals.mydmupdate.InitUpdate();

            itemId = aData.PRODUCT_CODE.ToUpper().Trim();
            lotId = aData.LOTID.ToUpper().Trim();

            //vmiCustId = Utils.ASCUtils.GetTrimString(aData.VMI_CUSTID, string.Empty).ToUpper();
            sqlStr = "SELECT EDI_MASTER_CUSTID FROM FILEXFER (NOLOCK) WHERE ID='IM_LOT'";
            myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref info);
            vmiCustId = ascLibrary.ascStrUtils.GetNextWord(ref info);

            bool fUseVMI = myClass.myParse.Globals.myConfig.iniGNVMI.boolValue;
            if (fUseVMI)
                ascItemId = siteid + "&" + itemId + "&" + vmiCustId;
            else
                ascItemId = siteid + "&" + itemId + "&";

            string updstr = string.Empty;
            Utils.ASCUtils.CheckAndAppend(ref updstr, "LOT", "STANDARDCOST", aData.STD_COST.ToString());
            if (aData.MFG_DATE != DateTime.MinValue)
                Utils.ASCUtils.CheckAndAppend(ref updstr, "LOT", "MFG_DATE", aData.MFG_DATE.ToString());
            Utils.ASCUtils.CheckAndAppend(ref updstr, "LOT", "QAHOLD", Utils.ASCUtils.GetTrimString(aData.QAHOLD, string.Empty), ref errmsg);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "LOT", "QAREASON", Utils.ASCUtils.GetTrimString(aData.QAREASON, string.Empty), ref errmsg);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "LOT", "CA_FILENAME", Utils.ASCUtils.GetTrimString(aData.CA_FILENAME, string.Empty), ref errmsg);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "LOT", "WORKORDER_ID", Utils.ASCUtils.GetTrimString(aData.WORKORDER_ID, string.Empty), ref errmsg);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "LOT", "LANDED_COST", aData.LANDED_COST.ToString());

            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "LAST_UPDATE", "GETDATE()");
            Utils.ASCUtils.CheckAndAppend(ref updstr, "LOT", "LAST_UPDATE_USERID", "GATEWAY");

            recExists = false;
            if (!string.IsNullOrEmpty(siteid))
            {
                whereStr = "ASCITEMID='" + ascItemId + "' AND LOTID='" + lotId + "'";
                sqlStr = "SELECT LOTID FROM LOT (NOLOCK)" +
                    " WHERE " + whereStr;
                if (myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref info))
                    recExists = true;
            }
            else
                whereStr = "ITEMID='" + itemId + "' AND LOTID='" + lotId + "'";

            if (!string.IsNullOrEmpty(errmsg))
                retval = HttpStatusCode.BadRequest;
            else
            {
                if (recExists || string.IsNullOrEmpty(siteid))
                {
                    myClass.myParse.Globals.mydmupdate.UpdateFields("LOT", updstr, whereStr);
                }
                else if (!string.IsNullOrEmpty(siteid))
                {
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "LOT", "SITE_ID", siteid, ref errmsg);
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "LOT", "LOTID", lotId, ref errmsg);
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "LOT", "ITEMID", itemId, ref errmsg);
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "LOT", "ASCITEMID", ascItemId, ref errmsg);
                    myClass.myParse.Globals.mydmupdate.InsertRecord("LOT", updstr);
                }

                if (retval == HttpStatusCode.OK)
                    myClass.myParse.Globals.mydmupdate.ProcessUpdates();
            }
            return (retval);
        }

    }
}