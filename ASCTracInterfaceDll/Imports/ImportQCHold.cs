using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Imports
{
    public class ImportQCHold
    {
        public static HttpStatusCode doImportQCHold(Class1 myClass, ASCTracInterfaceModel.Model.QC.QCHoldImport aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            string OrderNum = aData.License_ID;
            string updstr = string.Empty;
            try
            {
                if (!myClass.FunctionAuthorized(myClass.myLogRecord.FunctionID))
                    retval = HttpStatusCode.NonAuthoritativeInformation;
                else
                {
                    if (!string.IsNullOrEmpty(aData.User_ID))
                        myClass.myParse.Globals.curUserID = aData.User_ID;
                    else
                        myClass.myParse.Globals.curUserID = "Interface";
                    if ((aData.datetime != null) && (aData.datetime != DateTime.MinValue))
                        myClass.myParse.Globals.curTranDateTime = aData.datetime;

                    var siteid = myClass.GetSiteIdFromHostId(aData.Facility);
                    if (String.IsNullOrEmpty(siteid))
                    {
                        myClass.myLogRecord.LogType = "E";
                        errmsg = "No Facility or Site defined for record.";
                        retval = HttpStatusCode.BadRequest;
                    }
                    else
                    {
                       // myClass.myParse.Globals.initsite(siteid);
                        string refnum = string.Empty;
                        if ( aData.Hold_Ref_Num > 0)
                            aData.Hold_Ref_Num.ToString();
                        string sql = BuildSQL(aData, siteid, myClass.myParse.Globals.myConfig.iniGNVMI.boolValue);
                        //currPOImportConfig = Configs.POConfig.getPOImportSite(siteid, myClass.myParse.Globals);
                        if (String.IsNullOrEmpty(sql))
                        {
                            retval = HttpStatusCode.BadRequest;
                            errmsg = "Invalid Filter Options.";
                        }
                        else if (aData.Transaction == "R")
                            retval = DoQCRelease(myClass, aData, sql, ref errmsg);
                        else if (aData.Transaction == "C")
                            retval = DoQCChangeReason(myClass, aData, sql, ref errmsg);
                        else
                        {
                            retval = DoQCHold(myClass, aData, sql, ref errmsg, ref refnum);
                        }
                        if (retval == HttpStatusCode.OK)
                        {
                            if (!String.IsNullOrEmpty(aData.Exception_Message))
                            {
                                myClass.myParse.Globals.WriteAppLog(refnum, ascLibrary.dbConst.cmdCHG_QAHOLD_SKID, "", Newtonsoft.Json.JsonConvert.SerializeObject(aData), aData.Exception_Message, "", "", aData.Host_String_ID);
                            }
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

        private static string BuildSQL( ASCTracInterfaceModel.Model.QC.QCHoldImport aData, string siteid, bool includeVMI)
        {
            string retval = string.Empty;
            if (!String.IsNullOrEmpty(aData.License_ID))
                retval = "LI.SKIDID='" + aData.License_ID + "'";
            else
            {
                retval = "LI.SITE_ID ='" + siteid + "' ";
                if (!String.IsNullOrEmpty(aData.Product_Code))
                    retval += " AND LI.ITEMID='" + aData.Product_Code + "'";
                if (includeVMI)
                {
                    if (!String.IsNullOrEmpty(aData.VMI_Cust_ID))
                        retval += " AND LI.VMI_CUSTID='" + aData.VMI_Cust_ID + "'";
                    else
                        retval += " AND ISNULL( LI.VMI_CUSTID, '') =''";
                }
                if (!String.IsNullOrEmpty(aData.Lot_ID))
                    retval += " AND LI.LOTID='" + aData.Lot_ID + "'";
                if (!String.IsNullOrEmpty(aData.Alt_Lot_ID))
                    retval += " AND LI.altLOTID='" + aData.Alt_Lot_ID + "'";
                if (!String.IsNullOrEmpty(aData.Workorder_ID))
                    retval += " AND LI.WORKORDER_ID='" + aData.Workorder_ID + "'";
                if (!String.IsNullOrEmpty(aData.Recv_PO_Num))
                    retval += " AND LI.RECVPONUM='" + aData.Recv_PO_Num + "'";
                if (!String.IsNullOrEmpty(aData.Receiver_ID))
                    retval += " AND LI.RECEIVER_ID='" + aData.Receiver_ID + "'";
                if ((aData.RecvDateTime != null) && ( aData.RecvDateTime != DateTime.MinValue))
                    retval += " AND LI.RECVDATETIME='" + aData.RecvDateTime.ToString() + "'";
                if (!String.IsNullOrEmpty(aData.Host_String_ID))
                    retval += " AND LI.SKIDID IN ( SELECT SKIDID FROM TRANFILE WHERE HOST_ID_STR='" + aData.Host_String_ID+ "')";
            }
            return (retval);
        }

        private static HttpStatusCode DoQCHold(Class1 myClass, ASCTracInterfaceModel.Model.QC.QCHoldImport aData, string asql, ref string errmsg, ref string refnum)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            string sql = "SELECT LI.SITE_ID, LI.SKIDID FROM LOCITEMS LI" +
                " LEFT JOIN LOCITEMS_QC QC ON QC.SKIDID=LI.SKIDID AND QC.QAHOLD='T' AND QC.REASONFORHOLD='" + aData.Add_Reason_Code + "'";
            sql += " WHERE QC.REASONFORHOLD IS NULL AND " + asql;

            SqlConnection myConnection = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sql, myConnection);
            string StatusID = "Opening DataSEt";
            myConnection.Open();
            SqlDataReader myReader = myCommand.ExecuteReader();
            try
            {
                if (!myReader.HasRows)
                {
                    retval = HttpStatusCode.BadRequest;
                    errmsg = "No records found to put on hold";
                }
                else
                {
                    string mafnum = string.Empty;
                    if (aData.MafNum > 0)
                        mafnum = aData.MafNum.ToString();
                    String comments = string.Empty;
                    if (!String.IsNullOrEmpty(aData.Hold_Comments))
                        comments = aData.Hold_Comments;
                    //if (String.IsNullOrEmpty(comments) && !String.IsNullOrEmpty(aData.Exception_Message))
                    //    comments = aData.Exception_Message;

                    while (myReader.Read() && (retval == HttpStatusCode.OK))
                    {
                        myClass.myParse.Globals.initsite(myReader["SITE_ID"].ToString());
                        myClass.myParse.Globals.mydmupdate.InitUpdate();

                        var tmpret = myClass.myParse.Globals.dmQC.toggleqaholdonskid(myReader["SKIDID"].ToString(), aData.Add_Reason_Code, refnum, mafnum, comments, false, true, false);
                        if (tmpret != ascLibrary.TDBReturnType.dbrtOK)
                        {
                            retval = HttpStatusCode.BadRequest;
                            errmsg = ParseNet.dmascmessages.GetErrorMsg(tmpret);
                        }
                        else
                        {
                            string updstr = string.Empty;
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CUSTOM_DATA1", aData.Inventory_Custom_Data1);
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CUSTOM_DATA2", aData.Inventory_Custom_Data2);
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CUSTOM_DATA3", aData.Inventory_Custom_Data3);
                            myClass.myParse.Globals.mydmupdate.updateskid(myReader["SKIDID"].ToString(), "", updstr);

                            myClass.myParse.Globals.mydmupdate.ProcessUpdates();
                            myClass.myParse.Globals.myDBUtils.ReadFieldFromDB("SELECT REF_NUM, MAF_NUM FROM LOCITEMS_QC WHERE SKIDID='" + myReader["SKIDID"].ToString() + "' AND REASONFORHOLD='JBA' AND QAHOLD='T'", "", ref mafnum);
                            refnum = ascLibrary.ascStrUtils.GetNextWord(ref mafnum);
                        }
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }
            return (retval);
        }
        private static HttpStatusCode DoQCRelease(Class1 myClass, ASCTracInterfaceModel.Model.QC.QCHoldImport aData, string asql, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            string sql = "SELECT LI.SITE_ID, LI.SKIDID FROM LOCITEMS LI" +
                " LEFT JOIN LOCITEMS_QC QC ON QC.SKIDID=LI.SKIDID AND QC.QAHOLD='T' AND QC.REASONFORHOLD='" + aData.Remove_Reason_Code + "'";
            sql += " WHERE QC.REASONFORHOLD IS NOT NULL AND " + asql;

            SqlConnection myConnection = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sql, myConnection);
            string StatusID = "Opening DataSEt";
            myConnection.Open();
            SqlDataReader myReader = myCommand.ExecuteReader();
            try
            {
                if (!myReader.HasRows)
                {
                    retval = HttpStatusCode.BadRequest;
                    errmsg = "No records found on hold";
                }
                string refnum = string.Empty;
                if (aData.Hold_Ref_Num > 0)
                    aData.Hold_Ref_Num.ToString();
                string mafnum = string.Empty;
                if (aData.MafNum > 0)
                    mafnum = aData.MafNum.ToString();


                while (myReader.Read() && (retval == HttpStatusCode.OK))
                {
                    myClass.myParse.Globals.initsite(myReader["SITE_ID"].ToString());
                    myClass.myParse.Globals.mydmupdate.InitUpdate();
                    var tmpret = myClass.myParse.Globals.dmQC.toggleqaholdonskid(myReader["SKIDID"].ToString(), aData.Remove_Reason_Code, refnum, mafnum, aData.Hold_Comments, false, false, false);
                    if (tmpret != ascLibrary.TDBReturnType.dbrtOK)
                    {
                        retval = HttpStatusCode.BadRequest;
                        errmsg = ParseNet.dmascmessages.GetErrorMsg(tmpret);
                    }
                    else
                        myClass.myParse.Globals.mydmupdate.ProcessUpdates();
                }
            }
            finally
            {
                myConnection.Close();
            }
            return (retval);
        }
        private static  HttpStatusCode DoQCChangeReason(Class1 myClass, ASCTracInterfaceModel.Model.QC.QCHoldImport aData, string asql, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;

            string sql = "SELECT LI.SITE_ID, LI.SKIDID FROM LOCITEMS LI" +
                " LEFT JOIN LOCITEMS_QC QC ON QC.SKIDID=LI.SKIDID AND QC.QAHOLD='T' AND QC.REASONFORHOLD='" + aData.Remove_Reason_Code + "'";
            sql += " WHERE QC.REASONFORHOLD IS not NULL AND " + asql;

            SqlConnection myConnection = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sql, myConnection);
            string StatusID = "Opening DataSEt";
            myConnection.Open();
            SqlDataReader myReader = myCommand.ExecuteReader();
            try
            {
                if (!myReader.HasRows)
                {
                    retval = HttpStatusCode.BadRequest;
                    errmsg = "No records found on hold";
                }
                string refnum = string.Empty;
                if (aData.Hold_Ref_Num > 0)
                    aData.Hold_Ref_Num.ToString();
                string mafnum = string.Empty;
                if (aData.MafNum > 0)
                    mafnum = aData.MafNum.ToString();

                while (myReader.Read() && (retval == HttpStatusCode.OK))
                {
                    myClass.myParse.Globals.initsite(myReader["SITE_ID"].ToString());
                    myClass.myParse.Globals.mydmupdate.InitUpdate();
                    var tmpret = myClass.myParse.Globals.dmQC.toggleqaholdonskid(myReader["SKIDID"].ToString(), aData.Add_Reason_Code, refnum, mafnum, aData.Hold_Comments, false, true, true);
                    if (tmpret != ascLibrary.TDBReturnType.dbrtOK)
                    {
                        retval = HttpStatusCode.BadRequest;
                        errmsg = ParseNet.dmascmessages.GetErrorMsg(tmpret);
                    }
                    else
                        myClass.myParse.Globals.mydmupdate.ProcessUpdates();
                }
            }
            finally
            {
                myConnection.Close();
            }

            return (retval);
        }
    }
}
