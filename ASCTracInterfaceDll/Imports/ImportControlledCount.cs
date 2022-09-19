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
        private static string funcType = "IM_COUNT";
        private static string siteid = string.Empty;
        private static Class1 myClass;
        //private static Dictionary<string, List<string>> GWTranslation = new Dictionary<string, List<string>>();
        public static HttpStatusCode doImportControlledCount(ASCTracInterfaceModel.Model.Count.ModelCountHeader aData, ref string errmsg)
        {
            myClass = Class1.InitParse(funcType, ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            string updstr = string.Empty;
            try
            {
                if (myClass != null)
                {
                    if (!myClass.FunctionAuthorized(funcType))
                        retval = HttpStatusCode.NonAuthoritativeInformation;
                    else
                    {
                        myClass.myParse.Globals.mydmupdate.InitUpdate();
                        siteid = myClass.GetSiteIdFromHostId(aData.FACILITY);
                        //Configs.ConfigUtils.ReadTransationFields(GWTranslation, "ASN_DET", myClass.myParse.Globals);
                        if (String.IsNullOrEmpty(siteid))
                        {
                            errmsg = "No Facility or Site defined for record.";
                            retval = HttpStatusCode.BadRequest;
                        }
                        else
                        {
                            retval = ImportControlledCountRecord(aData, ref errmsg);
                            if (retval == HttpStatusCode.OK)
                                myClass.myParse.Globals.mydmupdate.ProcessUpdates();
                        }
                    }
                }
                else
                    retval = HttpStatusCode.InternalServerError;
            }
            catch (Exception ex)
            {
                Class1.WriteException(funcType, Newtonsoft.Json.JsonConvert.SerializeObject(aData), "", ex.ToString(), updstr);
                retval = HttpStatusCode.BadRequest;
                errmsg = ex.Message;
            }
            return (retval);
        }
        private static HttpStatusCode ImportControlledCountRecord(ASCTracInterfaceModel.Model.Count.ModelCountHeader aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;

            int periodNum = GetPeriodNum(aData.SCHED_START_DATE);
            if (periodNum < 0)
                throw new Exception(String.Format("No period exists for scheduled start date  {0}", aData.SCHED_START_DATE));

            long countId = myClass.myParse.Globals.dmMiscFunc.getnextinorder("COUNT_HDR", "COUNT_NUM IS NOT NULL", "COUNT_NUM");

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
            myClass.myParse.Globals.mydmupdate.InitUpdate();
            myClass.myParse.Globals.mydmupdate.InsertRecord("COUNT_HDR", updstr);

            InportControlledCountDetails(aData, countId);

            myClass.myParse.Globals.mydmupdate.ProcessUpdates();
            ascLibrary.TDBReturnType ret = myClass.myParse.Globals.dmCount.AddToCount(countId.ToString(), "", ref errmsg);
            if (ret == ascLibrary.TDBReturnType.dbrtOK)
            {
                myClass.myParse.Globals.mydmupdate.ProcessUpdates();
            }
            else
            {
                errmsg = "Error updating inventory for count " + countId + ": " + ParseNet.dmascmessages.GetErrorMsg(ret) + "\r\n" + errmsg;
                retval = HttpStatusCode.BadRequest;
            }
            return (retval);
        }

        private static void InportControlledCountDetails(ASCTracInterfaceModel.Model.Count.ModelCountHeader aData, long countId)
        {
            long seqNum = 1;
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

        private static string TranslateFilterType(string interfaceFilterType)
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

        private static bool IsValidFieldName(string fieldName)
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
        private static int GetPeriodNum(DateTime schedStartDate)
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