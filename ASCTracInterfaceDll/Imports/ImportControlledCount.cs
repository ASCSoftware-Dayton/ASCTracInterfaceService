using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Imports
{
    public class ImportControlledCount
    {
        //private string funcType = "IM_COUNT";
        private string siteid = string.Empty;
        private Class1 myClass;
        //private Dictionary<string, List<string>> GWTranslation = new Dictionary<string, List<string>>();
        public static HttpStatusCode doImportControlledCount(Class1 myClass, ASCTracInterfaceModel.Model.Count.ModelCountHeader aData, ref string errmsg)
        {
            //myClass = Class1.InitParse(funcType, ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            try
            {
                if (!myClass.FunctionAuthorized(myClass.myLogRecord.FunctionID))
                    retval = HttpStatusCode.NonAuthoritativeInformation;
                else
                {
                    var siteid = myClass.GetSiteIdFromHostId(aData.FACILITY);
                    //Configs.ConfigUtils.ReadTransationFields(GWTranslation, "ASN_DET", myClass.myParse.Globals);
                    if (String.IsNullOrEmpty(siteid))
                    {
                        myClass.myLogRecord.LogType = "E";
                        myClass.myLogRecord.OutData = "No Facility or Site defined for record.";
                        retval = HttpStatusCode.BadRequest;
                    }
                    else
                    {
                        myClass.myParse.Globals.initsite(siteid);
                        var myimport = new ImportControlledCount(myClass, siteid);
                        retval = myimport.ImportControlledCountRecord(aData, ref errmsg);

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

        public ImportControlledCount(Class1 aClass, string aSiteID)
        {
            myClass = aClass;
            siteid = aSiteID;
        }

        private HttpStatusCode ImportControlledCountRecord(ASCTracInterfaceModel.Model.Count.ModelCountHeader aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            myClass.myParse.Globals.mydmupdate.InitUpdate();

            int periodNum = GetPeriodNum(aData.SCHED_START_DATE);
            if (periodNum < 0)
                throw new Exception(String.Format("No period exists for scheduled start date  {0}", aData.SCHED_START_DATE));

            long countId = aData.COUNTID;
            if (countId <= 0)
                countId = myClass.myParse.Globals.dmMiscFunc.GetEnterpriseFromStoredProc( "COUNT_NUM"); //.getnextinorder("COUNT_HDR", "COUNT_NUM IS NOT NULL", "COUNT_NUM");

            myClass.myParse.Globals.mydmupdate.InitUpdate();
            if (!myClass.myParse.Globals.myDBUtils.ifRecExists("SELECT STATUS FROM COUNT_HDR WHERE COUNT_NUM='" + countId.ToString() + "'"))
            {
                string updstr = string.Empty;

                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "COUNT_NUM", countId.ToString());
                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "PERIOD_NUM", periodNum.ToString());
                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "STATUS", "S");
                ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "CREATE_DATE", "GETDATE()");
                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CREATE_USERID", "IMPORT");
                ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "LAST_UPDATE", "GETDATE()");
                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "LAST_UPDATE_USERID", "IMPORT");
                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SITE_ID", siteid);
                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SCHED_START_DATE", aData.SCHED_START_DATE.ToString());
                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SCHED_END_DATE", aData.SCHED_END_DATE.ToString());
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "COUNT_TYPE", aData.COUNT_TYPE);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "DESCRIPTION", aData.DESCRIPTION);
                if (aData.USERLEVELNUMBER > 0)
                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "USERLEVELNUMBER", aData.USERLEVELNUMBER.ToString());
                myClass.myParse.Globals.mydmupdate.InsertRecord("COUNT_HDR", updstr);
            }
            InportControlledCountDetails(aData, countId);

            myClass.myParse.Globals.mydmupdate.ProcessUpdates();
            string aAddedLocsCount = string.Empty;
            ascLibrary.TDBReturnType ret = myClass.myParse.Globals.dmCount.AddToCount(countId.ToString(), "", ref aAddedLocsCount);
            if (ret == ascLibrary.TDBReturnType.dbrtOK)
            {
                myClass.myParse.Globals.mydmupdate.ProcessUpdates();
            }
            else
            {
                string tmperrmsg ;
                if (ret == ascLibrary.TDBReturnType.dbrtCLOSED)
                    tmperrmsg = ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PMSG_PROMPT_COUNT) + countId.ToString() + " " + ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PERR_GEN_COMPLETED);
                else
                    tmperrmsg = ParseNet.dmascmessages.GetErrorMsg(ret);

                errmsg = "Error updating inventory for count " + countId + ": " + tmperrmsg;
                retval = HttpStatusCode.BadRequest;
            }

            if (retval == HttpStatusCode.OK)
                myClass.myParse.Globals.mydmupdate.ProcessUpdates();

            return (retval);
        }

        /*
        private long GetNextCountNum()
        {
            int retries = 0;
            while 

            string sql = "SELECT NEXT_COUNT_NUM FROM CONFIG";
            string tmp = string.Empty;
            myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sql, "", ref tmp);

            long retval = ascLibrary.ascUtils.ascStrToInt(tmp, 0) + 1;

            sql = "UPDATE CONFIG SET NEXT_COUNT_NUM = " + retval;
            if (String.IsNullOrEmpty(tmp))
                sql += " WHERE NEXT_COUNT_NUM IS NULL";
            else
                sql += " WHERE NEXT_COUNT_NUM = " + tmp;
            if ( myClass.myParse.Globals.myDBUtils.RunSqlCommand( sql) <= 0)



            string sql = " DECLARE @TMP_COUNT_NUM VARCHAR(30)" +
                " declare @NEW_VALUE varchar(30)" +
                " declare @IntErrorCode varchar(30)" +
                " DECLARE @NumRowsChanged INT" +
                " SELECT @NumRowsChanged = 0" +
                " SELECT @IntErrorCode = 0" +
                " RESTART:" +
                "             BEGIN TRAN" +
                "       SET NOCOUNT OFF" +
                "       SELECT @NEW_VALUE = NEXT_COUNT_NUM FROM CONFIG" +
                "       SELECT @intErrorCode = @@ERROR" +
                "       IF(@intErrorCode <> 0) GOTO PROBLEM" +
                "       SET @TMP_COUNT_NUM = STR(@NEW_VALUE)" +
                "       UPDATE CONFIG set NEXT_COUNT_NUM = STR(@NEW_VALUE) + 1" +
                "       WHERE NEXT_COUNT_NUM = @NEW_VALUE" +
                "       SELECT @NumRowsChanged = @@ROWCOUNT, @intErrorCode = @@ERROR" +
                "       IF @NumRowsChanged <> 1 begin" +
                "         ROLLBACK" +
                "         GOTO RESTART" +
                "       END" +
                "       IF(@intErrorCode <> 0) GOTO PROBLEM" +
                " COMMIT TRAN" +
                " PROBLEM:" +
                "             IF(@intErrorCode <> 0) BEGIN" +
                "                ROLLBACK TRAN" +
                "            END" +
                " SELECT @NEW_VALUE, @intErrorCode";

            string tmp = string.Empty;
            myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sql, "", ref tmp);

            return (ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 1));
        }
        */
        private void InportControlledCountDetails(ASCTracInterfaceModel.Model.Count.ModelCountHeader aData, long countId)
        {
            long seqNum = myClass.myParse.Globals.dmMiscFunc.getnextinorder("COUNT_DET", "COUNT_NUM = " + countId.ToString(), "SEQ_NUM");
            foreach (var rec in aData.DetailList)
            {
                string fieldName = rec.FIELDNAME;

                if (IsValidFieldName(fieldName))
                {
                    string updstr = string.Empty;
                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "COUNT_NUM", countId.ToString());
                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SEQ_NUM", seqNum.ToString());
                    ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "CREATE_DATE", "GETDATE()");
                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CREATE_USERID", "IMPORT");
                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "FILTER_TYPE", TranslateFilterType(rec.FILTER_TYPE));
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FUNCTIONID", rec.FUNCTIONID);
                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "FIELDNAME", fieldName.ToUpper());
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "START_VALUE", rec.START_VALUE);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "END_VALUE", rec.END_VALUE);
                    if (rec.GROUP_SEQ > 0)
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "GROUP_SEQ", rec.GROUP_SEQ.ToString());
                    myClass.myParse.Globals.mydmupdate.InsertRecord("COUNT_DET", updstr);

                    seqNum += 1;
                }
            }
        }

        private string TranslateFilterType(string interfaceFilterType)
        {
            switch (interfaceFilterType)
            {
                case "E":   // Equal to
                    return "2";
                case "N":   // Not equal to
                    return "3";
                case "R":   // Range
                    return "0";
                case "O":   // Outside range
                    return "1";
                case "C":   // Contains
                    return "5";
                case "B":   // Blank
                    return "4";
                case "S":   // Starts with
                    return "6";
                case "I":   // In Set of
                    return "8";
                default:    // Default to blank
                    return "4";
            }
        }

        private bool IsValidFieldName(string fieldName)
        {
            if (fieldName.Equals("ITEMID", StringComparison.OrdinalIgnoreCase) ||
                fieldName.Equals("ZONEID", StringComparison.OrdinalIgnoreCase) ||
                fieldName.Equals("ABCZONE", StringComparison.OrdinalIgnoreCase) ||
                fieldName.Equals("VMI_CUSTID", StringComparison.OrdinalIgnoreCase) ||
                fieldName.Equals("VMI_RESPID", StringComparison.OrdinalIgnoreCase) ||
                fieldName.Equals("STANDARDCOST", StringComparison.OrdinalIgnoreCase) ||
                fieldName.Equals("LOCATIONID", StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }
        private int GetPeriodNum(DateTime schedStartDate)
        {
            string sqlStr = "SELECT PERIOD_NUM FROM COUNT_PERIOD (NOLOCK) " +
                "WHERE START_DATE <= @date AND END_DATE >= @date";
            using (SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString))
            using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
            {
                conn.Open();
                cmd.Parameters.Add("@date", SqlDbType.DateTime).Value = schedStartDate;
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return Int32.Parse(dr["PERIOD_NUM"].ToString());
                }
            }
            return -1;
        }

    }
}