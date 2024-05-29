using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using ASCTracWCSProcess.Exports;

namespace ASCTracInterfaceDll
{
    /* possible functions to add
     * IM_CAT, IM_CAT2, IM_BOM, IM_CSTMR, IM_ASN, IM_INV, IM_LOTALLOC, IM_WO
     * EX_LOCKED
     * Complete
     * IM_VENDOR, IM_RECV, IM_ORDER, IM_ITEM
     * EX_RECV, EX_ORDER, EX_TRAN, EX_PARC 
     */
    public class Class1
    {
        public static string fLogging = "X"; // Exceptions, Error, All (Default exceptions)
        public static bool fLogged = false;
        public static string fDefaultConnectionStr = string.Empty;
        internal ASCTracWCSProcess.Imports.dmPickImport myWCSPickImport ;
        //private static Dictionary<string, Class1> parseList = new Dictionary<string, Class1>();
        public ParseNet.ParseNetMain myParse;
        //private static ParseNet.ParseNetMain myStaticParse;
        private bool fPostAPILog = true;

        public readonly object LockObject = new object();
        public Model.ModelLog myLogRecord;

        //public static ascLibrary.ascDBUtils myInterface
        //public static bool fInitParse = false;

        public static void InitParse( ref Class1 retval, string aURL, string aFuncType, ref string errmsg)
        {
            //Class1 retval;
            //if (parseList.ContainsKey(aFuncType))
            //    retval = parseList[aFuncType];
            //else
            //{
            //    retval = new Class1();
            if (!retval.Init(aURL, aFuncType, true, ref errmsg))
                retval = null;
            //else
            //    parseList.Add(aFuncType, retval);
            //}
            //return (retval);
        }

        public static void InitParse(Class1 retval, string aURL, string aFuncType, bool aPostAPILog, ref string errmsg)
        {
            if (!retval.Init(aURL, aFuncType, aPostAPILog, ref errmsg))
                retval = null;
        }

        public static Class1 InitParse2(string aConnectionString, string aURL, string aFuncType, ref string errmsg)
        {
            Class1 retval;
            fDefaultConnectionStr = aConnectionString;
            retval = new Class1();
            if (!retval.Init(aURL, aFuncType, true, ref errmsg))
                retval = null;
            return (retval);
        }

        public bool Init(string URL, string aFuncType, bool aPostAPILog, ref string errmsg)
        {
            bool retval = true;
            lock (LockObject)
            {

                myLogRecord = new Model.ModelLog(URL, aFuncType);
                string tmp = string.Empty;

                fLogged = false;
                string Status = "Status: 004";
                string asc004ErrMsg = string.Empty;
                try
                {
                    bool fOK = false;
                    int count = 1;
                    string myConnStr = string.Empty;
                    while (!fOK && (count <= 3))
                    {
                        myParse = new ParseNet.ParseNetMain();
                        try
                        {
                            myParse.InitParse("AliasASCTrac");
                            myLogRecord.StartDateTime = myParse.Globals.myDBUtils.GetSQLDateTime();
                            fOK = true;
                            myConnStr = myParse.Globals.myDBUtils.myConnString;

                            fPostAPILog = aPostAPILog;
                            if (fPostAPILog)
                                fPostAPILog = myParse.Globals.myDBUtils.ifRecExists("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'API_LOG'");
                        }
                        catch (Exception ex)
                        {
                            asc004ErrMsg = ex.ToString() + "\r\nRetry Count " + count.ToString();
                            myParse = new ParseNet.ParseNetMain();
                            fOK = false;
                            //if (asc004ErrMsg.Contains("primary key") || asc004ErrMsg.Contains("ConnectionString") || asc004ErrMsg.Contains("Reading of configuration failed"))
                                count += 1;
                            //else
                            //    count = 4;
                            ascLibrary.ascUtils.ascPause(500);
                        }
                    }
                    if (!fOK)
                    {
                        try
                        {
                            myConnStr = ""; // ConfigurationManager.ConnectionStrings["ASCTracConnectionString"].ConnectionString;
                        }
                        catch
                        { }
                        if (String.IsNullOrEmpty(myConnStr))
                            myConnStr = fDefaultConnectionStr; // "";
                        Status = "Status: Web.Config";
                        myParse.InitParse(myConnStr, ref errmsg);

                        myLogRecord.StartDateTime = myParse.Globals.myDBUtils.GetSQLDateTime();
                    }
                    fOK = string.IsNullOrEmpty(errmsg);

                    if (fOK)
                    {
                        myParse.Globals.initASCLog("INTERFACE", "ASCTracInterface", "1", "ASCTrac Interface API");
                        if (aFuncType.StartsWith("WCS") || aFuncType.StartsWith("Retry"))
                        {
                            Status = "WCS";
                            string wcsConnStr = string.Empty;
                            try
                            {
                                ascLibrary.ascDBUtils tmpDBUtils = new ascLibrary.ascDBUtils();
                                tmpDBUtils.BuildConnectString("AliasWCS");
                                wcsConnStr = tmpDBUtils.myConnString;
                            }
                            catch { }
                            if (String.IsNullOrEmpty(wcsConnStr))
                                wcsConnStr = "packet size=4096;user id=app_user;Password='WeH73w';data source=asc-cin-app01;persist security info=False;initial catalog=ascWCSPicking";

                            ASCTracWCSProcess.wcsGlobals.InitWCSGlobalsForInterface("ASCTracInterface", "ASCTracInterface", "ASCWEB", true, myConnStr, wcsConnStr);
                            myWCSPickImport = new ASCTracWCSProcess.Imports.dmPickImport(aFuncType, myParse.Globals);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errmsg = "Initialize Connection Exception: \r\nFirst Exception: " + asc004ErrMsg + "\r\nSecondary Exception: " + ex.ToString();
                    //WriteException(aFuncType, "InitDatabase", "", errmsg, ex.StackTrace);
                    //ascLibrary.ascUtils.ascWriteLog("INTERFACE_ERR", ex.ToString(), false);
                    //throw ex;
                }

                if (!String.IsNullOrEmpty(errmsg))
                {
                    retval = false;
                    errmsg += "(" + Status + ")";
                }
                //else
                //    myStaticParse = myParse;
            }
            return (retval);
        }

        public string GetSiteIdFromHostId(string aHostSiteId)
        {
            return (GetSiteIdFromHostId( aHostSiteId, true));
        }
        public string GetSiteIdFromHostId(string aHostSiteId, bool aSetGlobalSite)
        {
            string retval = string.Empty;
            if (!String.IsNullOrEmpty(aHostSiteId))
            {
                string sqlStr = "SELECT SITE_ID FROM SITES (NOLOCK)" +
                                " WHERE HOST_SITE_ID=@HOST_SITE_ID"; // '" + aHostSiteId + "'";
                retval = myParse.Globals.myDBUtils.ReadFieldFromDBWithParam(sqlStr, "@HOST_SITE_ID", aHostSiteId); // sql.ReadFieldFromDB(sqlStr, "", ref retval);
            }
            if (!string.IsNullOrEmpty(retval) && aSetGlobalSite)
                myParse.Globals.initsite(retval);

            return (retval);
        }

        public string BuildWhereFilter(List<ASCTracInterfaceModel.Model.ModelExportFilter> aExportFilterList, string atblName, Dictionary<String,String> paramlist)
        {
            string retval = string.Empty;
            int idx = 1;
            foreach (var rec in aExportFilterList)
            {
                string fieldname = rec.Fieldname;
                if (myParse.Globals.myDBUtils.IfFieldExists("TRANFILE", fieldname))
                {
                    string startval = ascLibrary.ascStrUtils.ConvertDateValue(rec.Startvalue);
                    string endval = ascLibrary.ascStrUtils.ConvertDateValue(rec.Endvalue);

                    // filter types
                    //rtRange        = 0;
                    //rtOutsideRange = 1;
                    //rtEqualTo      = 2;
                    //rtNotEqualTo   = 3;
                    //rtBlank        = 4; 
                    //rtContains     = 5;
                    //rtStartsWith   = 6;
                    //rtEndsWith     = 7;
                    //rtInSetOf      = 8;

                    string wherestr = "";
                    string paramname = String.Empty;
                    string filtertype = rec.FilterType.ToString();
                    if (filtertype == "0")
                    {
                        if (startval != "")
                        {
                            paramname = "PARAM" + (idx++).ToString();
                            wherestr = fieldname + ">=@" + paramname;
                            paramlist.Add(paramname, startval);
                        }
                        if (endval != "")
                        {
                            if (!String.IsNullOrEmpty(wherestr))
                                wherestr += " AND ";
                            paramname = "PARAM" + (idx++).ToString();
                            wherestr += fieldname + "<=@" + paramname;
                            paramlist.Add(paramname, endval);
                        }
                    }
                    else if (filtertype == "1")
                    {
                        if (startval != "")
                        {
                            paramname = "PARAM" + (idx++).ToString();
                            wherestr = fieldname + "<@" + paramname;
                            paramlist.Add(paramname, startval);
                        }
                        if (endval != "")
                        {
                            if (!String.IsNullOrEmpty(wherestr))
                                wherestr += " OR ";
                            paramname = "PARAM" + (idx++).ToString();
                            wherestr += fieldname + ">@" + paramname;
                            paramlist.Add(paramname, endval);
                        }

                    }
                    else if (filtertype == "8") // setof
                    {
                        wherestr = "( " + fieldname + " in ( '" + startval.Replace(" ", "").Replace(",", "','") + "'))";
                    }
                    else
                    {
                        paramname = "PARAM" + (idx++).ToString();
                        wherestr = fieldname;
                        if (filtertype == "2")
                            wherestr += "= @" + paramname;
                        else if (filtertype == "3")
                            wherestr += "<> @" + paramname;
                        else if (filtertype == "4")
                            wherestr = "( ISNULL( @" + paramname + ",'') = ''";
                        else if (filtertype == "5")
                        {
                            wherestr += " like @" + paramname;
                            startval = "%" + startval + "%";
                          }
                        else if (filtertype == "6")
                        {
                            wherestr += " like @" + paramname;
                            startval = startval + "%";
                        }
                        //wherestr += " like '" + startval + "%'";
                        else if (filtertype == "7")
                        {
                            wherestr += " like @" + paramname;
                            startval = "%" + startval;
                        }
                        //wherestr += " like '%" + startval + "'";
                        else
                            wherestr = "";
                        if (!String.IsNullOrEmpty(wherestr))
                            paramlist.Add(paramname, startval);
                    }


                    if (!String.IsNullOrEmpty(wherestr))
                    {
                        retval += " AND ( " + wherestr + ")";
                    }
                }
            }

            return (retval);
        }

        internal bool FunctionAuthorized( string aFuncType)
        {
            bool retval = true;
            if( aFuncType.Equals( "WCS"))
            {
                retval = myParse.Globals.myConfig.gwWCSKAR.boolValue || myParse.Globals.myConfig.gwWCSKN.boolValue;
            }

            return (retval);
        }

        public void WriteException( string aFunc, string aData,  string aOrderNum,  string ErrorStr, string aStackTrace)
        {
            fLogged = true;
            //if (parseList.Count > 0)
            //{ 
            //    ParseNet.ParseNetMain myParse = null;
            //    foreach (var rec in parseList)
            //        myParse = rec.Value.myParse;
            if (myParse != null)
            {
                myParse.Globals.WriteAppLog( "", aFunc, "DATA", aData, ErrorStr, aStackTrace, aData, aOrderNum );
                /*
                var con = new SqlConnection(myParse.Globals.myDBUtils.myConnString);
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("ASC_INSERT_APP_LOG", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("AppName", "ASCTracInterfaceService");
                    cmd.Parameters.AddWithValue("TranID", -1); // tranId);
                    cmd.Parameters.AddWithValue("TranType", aFunc);
                    cmd.Parameters.AddWithValue("OrderNum", aOrderNum);
                    cmd.Parameters.AddWithValue("ErrorType", "Data"); // errorType);
                    cmd.Parameters.AddWithValue("ErrorID", "0"); // errorCode);
                    cmd.Parameters.AddWithValue("Version", myParse.Globals.VER_STR);
                    cmd.Parameters.AddWithValue("StackTrace", aData);
                    cmd.Parameters.AddWithValue("SQLData", aSQLData);
                    cmd.Parameters.AddWithValue("ErrorMsg", ErrorStr);

                    cmd.ExecuteNonQuery();
                    //ascLibrary.ascUtils.ascWriteLog("errorlog", "Complete use of Stored Proc ASC_INSERT_APP_LOG", true);
                }
                finally
                {
                    con.Close();
                }
                */
                //}
            }
            else
            {
                ascLibrary.ascUtils.ascWriteLog("INTERFACE_ERR", aFunc + ": " + aData + "\r\n" + ErrorStr, true);
                //EventLog.WriteEntry( aFunc + ": " + aData, ErrorStr, EventLogEntryType.Error);
            }
        }

        /*
        public static void LogTransaction(string FuncID, string aOrdernum, string inData, string outData, bool isError)
        {
            try
            {
                if (!fLogged)
                {
                    bool fDoit = false;
                    if (fLogging.Equals("A"))
                        fDoit = true;
                    else if (fLogging.Equals("E"))
                        fDoit = isError;

                    if (fDoit)
                    {
                        if (inData.Length > 250)
                        {
                            if (myStaticParse != null)
                            {
                                myStaticParse.Globals.WriteAppLog( "", FuncID, "", "", outData, "", inData, aOrdernum);
                            }                       
                        }
                        else
                            WriteException(FuncID, inData, aOrdernum, outData, inData);
                    }
                }
            }
            catch( Exception ex)
            {
                ascLibrary.ascUtils.ascWriteLog("INTERFACE_ERR", "Log Transactin: " + inData+ "\r\n" + ex.ToString(), true);
            }
        }
        */

        internal string GetZone(string siteId)
        {
            string sql = "SELECT CFGDATA FROM CFGSETTINGS WHERE CFGFIELD='GNDefZone' AND SITE_ID='" + siteId + "'";
            string zone = string.Empty;
            myParse.Globals.myDBUtils.ReadFieldFromDB(sql, "", ref zone);
            if (string.IsNullOrEmpty(zone) || (zone == "NIL"))
            {
                sql = "SELECT ZONEID FROM ZONES (NOLOCK) " +
                    "WHERE SITE_ID='" + siteId + "' ORDER BY ZONEID";
                myParse.Globals.myDBUtils.ReadFieldFromDB(sql, "", ref zone);
            }
            return zone;
        }

        internal void ImportCustomData(string aGatewayId, string aAscTable, string aAscWhere, string aKeyValue)
        {
            //added 10-17-17 (JXG)
            //Process the custom fields as defined in ASCGatewaySettings
            string sqlStr, tmpStr = "", ascFieldName = "", customData = "";
            bool anyAdded = false;

            bool GatewayCustomFieldsExists = myParse.Globals.myDBUtils.ifRecExists("SELECT TOP 1 NAME FROM SYSOBJECTS WHERE NAME='GATEWAY_CUSTOM_FIELDS' ");

            if (GatewayCustomFieldsExists)
            {
                sqlStr = "SELECT GATEWAY_ID, FIELD_NAME, FIELD_VALUE" +
                    " FROM GATEWAY_CUSTOM_FIELDS (NOLOCK)" +
                    " WHERE GATEWAY_ID='" + aGatewayId + "'" +
                    " ORDER BY FIELD_NAME";

                try
                {
                    SqlConnection customConnection = new SqlConnection(myParse.Globals.myDBUtils.myConnString);
                    SqlCommand customCommand = new SqlCommand(sqlStr, customConnection);
                    //customCommand.CommandTimeout = DbConst.SQL_TIMEOUT;
                    customConnection.Open();
                    SqlDataReader customReader = customCommand.ExecuteReader();

                    try
                    {
                        string updStr = string.Empty;
                        while (customReader.Read())
                        {
                            try
                            {
                                ascFieldName = customReader["FIELD_NAME"].ToString();
                                customData = customReader["FIELD_VALUE"].ToString();

                                if (!string.IsNullOrEmpty(ascFieldName) && (!string.IsNullOrEmpty(customData)))
                                {
                                    sqlStr = "SELECT C.XTYPE, C.LENGTH" +
                                        " FROM SYSOBJECTS O (NOLOCK), SYSCOLUMNS C (NOLOCK)" +
                                        " WHERE O.ID=C.ID AND O.NAME='" + aAscTable + "' AND C.NAME='" + ascFieldName + "'";
                                    myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref tmpStr);
                                    int ascFieldType = Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0));
                                    int ascFieldLen = Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 0));

                                    if ((ascFieldType > 0) && (ascFieldLen > 0))
                                    {
                                        if (ascFieldType == 35 || ascFieldType == 167 ||
                                            ascFieldType == 175 || ascFieldType == 231)
                                        {
                                            if (customData.Length > ascFieldLen)
                                                customData = customData.Substring(0, ascFieldLen);

                                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, ascFieldName, customData);
                                        }
                                        else if (ascFieldType == 61)
                                        {
                                            DateTime dtTmp;
                                            if (!DateTime.TryParse(customData, out dtTmp))
                                                customData = "";

                                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, ascFieldName, customData);
                                        }
                                        else if (ascFieldType == 56 || ascFieldType == 106 || ascFieldType == 108)
                                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, ascFieldName, customData);
                                        anyAdded = true;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                string errMsg = "Error importing Custom Data [" + customData + "] for Field: " + ascFieldName + ", for New Item: " + aKeyValue + ".  " + e.ToString();
                                WriteException("CustomData", aAscTable + "," + aKeyValue, aGatewayId, e.ToString(), updStr);
                                //AscErrorUtils.LogError(errMsg, aGatewayId, 0, "", ErrorType.CRITICAL, ErrorCode.UNKNOWN);
                            }
                            
                        } // end while

                        if (anyAdded)
                            myParse.Globals.mydmupdate.UpdateFields(aAscTable, updStr, aAscWhere);
                            //customUpd.UpdateRec(aAscTable, aAscWhere, new SqlConnection(Globals.ascConnStr));
                    }
                    finally
                    {
                        customReader.Close();
                        customCommand.Dispose();
                        customConnection.Close();
                        customConnection.Dispose();
                    }
                }
                catch (InvalidOperationException invEx)
                {
                    throw new InvalidOperationException("Error Importing Custom Fields during " + aGatewayId + "." +
                        Environment.NewLine + "Attempted SQL command: " + Environment.NewLine + sqlStr +
                        Environment.NewLine + Environment.NewLine + invEx.ToString());
                }
            }
        }


        public void GetConvQty(string ascItemId, string hostUom, bool raiseErr, ref double orderQty, ref double convFact)
        {
            string stockUom, tmp = "";
            List<string> uom = new List<string>();
            List<string> cf = new List<string>();
            bool found = false;
            convFact = 1;

            if (!String.IsNullOrEmpty(hostUom) &&
                myParse.Globals.myGetInfo.GetASCItemInfo(ascItemId, "STOCK_UOM,SUB_UOM,UNIT_MEAS1,UNIT_MEAS2,UNIT_MEAS3,UNIT_MEAS4,CONV_FACT_12,CONV_FACT_23,CONV_FACT_34", ref tmp))
            {
                stockUom = ascLibrary.ascStrUtils.GetNextWord (ref tmp).ToUpper();
                if (stockUom == hostUom)
                    found = true;
                else if (ascLibrary.ascStrUtils.GetNextWord (ref tmp).ToUpper() == hostUom)
                {
                    if (raiseErr)
                        throw new Exception("Import UOM [" + hostUom + "] cannot be below Stock UOM.");
                }
                else
                {
                    uom.Add(ascLibrary.ascStrUtils.GetNextWord (ref tmp).ToUpper());
                    uom.Add(ascLibrary.ascStrUtils.GetNextWord (ref tmp).ToUpper());
                    uom.Add(ascLibrary.ascStrUtils.GetNextWord (ref tmp).ToUpper());
                    uom.Add(ascLibrary.ascStrUtils.GetNextWord (ref tmp).ToUpper());

                    cf.Add(ascLibrary.ascStrUtils.GetNextWord (ref tmp).ToUpper());
                    cf.Add(ascLibrary.ascStrUtils.GetNextWord (ref tmp).ToUpper());
                    cf.Add(ascLibrary.ascStrUtils.GetNextWord (ref tmp).ToUpper());

                    int i = 0;
                    while (i < 3 && stockUom != uom[i])
                    {
                        if (uom[i] == hostUom)
                            found = true;
                        if (found)
                            convFact *= ascLibrary.ascUtils.ascStrToDouble(cf[i], 1);
                        i++;
                    }

                    while (i < 4 && !found)
                    {
                        if (uom[i] == hostUom)
                            found = true;
                        if (i < 3)
                        {
                            double tmpVal = ascLibrary.ascUtils.ascStrToDouble(cf[i], 1);
                            if (tmpVal <= 0)
                                tmpVal = 1;
                            convFact /= tmpVal;
                        }
                        i++;
                    }

                    if (found)
                        orderQty *= convFact;
                    else
                        convFact = 1;
                }
            }

            if (!found && raiseErr)
                throw new Exception("Import UOM [" + hostUom + "] is not defined for item.");
        }

        internal double GetItemConv(string aAscItemId, string aToUomType, string aFromUomType)
        {
            double result = 1;
            int tmp, i, fromIndex, toIndex;
            string tmpStr = "", toUom, fromUom;
            string[] uom = new string[4];
            double[] convFact = new double[3];

            if (aAscItemId != "")
            {
                if (myParse.Globals.myGetInfo.GetASCItemInfo(aAscItemId, aToUomType + ", " + aFromUomType + ", CONV_FACT_12, CONV_FACT_23, CONV_FACT_34, UNIT_MEAS1, UNIT_MEAS2, UNIT_MEAS3, UNIT_MEAS4", ref tmpStr))
                {
                    toUom = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);
                    fromUom = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);

                    for (int j = 0; j < 3; j++)
                        convFact[j] = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmpStr), 1);
                    for (int j = 0; j < 4; j++)
                        uom[j] = ascLibrary.ascStrUtils.GetNextWord(ref tmpStr);

                    fromIndex = 0;
                    toIndex = 0;
                    i = 0;
                    while (toIndex == 0 && i < 4)
                    {
                        if (toUom == uom[i])
                            toIndex = i;
                        i++;
                    }
                    i = 0;
                    while (fromIndex == 0 && i < 4)
                    {
                        if (fromUom == uom[i])
                            fromIndex = i;
                        i++;
                    }
                    if (fromIndex == toIndex)
                        result = 1;
                    else
                    {
                        tmp = 0;
                        if (fromIndex >= toIndex)
                        {
                            tmp = fromIndex;
                            fromIndex = toIndex;
                            toIndex = tmp;
                        }
                        for (i = fromIndex; i < toIndex; i++)
                            result = result * convFact[i];
                        if (tmp > 0 && result != 0)
                            result = 1 / result;
                    }
                }
            }
            if (result <= 0)
                result = 1;
            return (result);
        }

        internal void AddPromo(string promoCode, string curSiteId, string masterClient, ParseNet.GlobalClass Globals)
        {
            string updStr = string.Empty;
                ascLibrary.ascStrUtils.ascAppendSetStr( ref updStr, "PROMO_CODE", promoCode);
                ascLibrary.ascStrUtils.ascAppendSetStr( ref updStr, "SITE_ID", curSiteId);
                ascLibrary.ascStrUtils.ascAppendSetStr( ref updStr, "MASTER_CLIENT", masterClient);
                ascLibrary.ascStrUtils.ascAppendSetStr( ref updStr, "DESCRIPTION", "Imported");
                ascLibrary.ascStrUtils.ascAppendSetStr( ref updStr, "PROMO_CODE", promoCode);
                ascLibrary.ascStrUtils.ascAppendSetStr( ref updStr, "CREATE_USERID", "IMPORT");
                ascLibrary.ascStrUtils.ascAppendSetQty( ref updStr, "CREATE_DATE", "GETDATE()");
                ascLibrary.ascStrUtils.ascAppendSetStr( ref updStr, "LAST_UPDATE_USERID", "IMPORT");
                ascLibrary.ascStrUtils.ascAppendSetQty( ref updStr, "LAST_UPDATE_DATE", "GETDATE()");
            Globals.mydmupdate.InsertRecord("PROMO", updStr);
        }

        internal void AddPromoItem(string promoCode,  string ascItemId, ParseNet.GlobalClass Globals)
        {
            String updstr = "INSERT INTO PROMO_ITEMS" +
                " ( PROMO_CODE, SITE_ID, ITEMID, ASCITEMID', COMMENT, FILLED_FLAG, QTY_PROMO, QTYRECEIVED, QTYALLOCATED, QTYFILLED, CREATE_USERID, CREATE_DATE, LAST_UPDATE_USERID, LAST_UPDATE_DATE)" +
                " SELECT '" + promoCode + ", I.SITE_ID, I.ITEMID, I.ASCITEMID, 'Imported', 'O', 0, 0, 0, 0, GetDate(), 'IMPORT', GetDate(), 'IMPORT'" +
                " FROM ITEMMSTR I WHERE ASCITEMID='" + ascItemId + "'";
            Globals.mydmupdate.AddToUpdate(updstr);
        }

        internal void AddPromoOrder(string promoCode,  string ascItemId, string orderNum, string lineNum, double orderQty, string orderType, ParseNet.GlobalClass Globals)
        {
                String updstr = "INSERT INTO PROMO_ORDERS" +
                    " ( PROMO_CODE, SITE_ID, ASCITEMID, ORDERTYPE, ORDERNUMBER, LINENUMBER, QTYORDERED, QTYFILLED, CREATE_USERID, CREATE_DATE, LAST_UPDATE_USERID, LAST_UPDATE_DATE)" +
                    " SELECT '" + promoCode + ", I.SITE_ID, I.ASCITEMID, '" + orderType + "','" + orderNum + "','" + lineNum.ToString() + "'," + orderQty.ToString() + ", 0, GetDate(), 'IMPORT', GetDate(), 'IMPORT'" +
                    " FROM ITEMMSTR I WHERE ASCITEMID='" + ascItemId + "'";
        }

        internal void UpdatePromoOrder(string promoCode, string curSiteId, string ascItemId, string orderNum, string lineNum, double orderQty, string orderType, ParseNet.GlobalClass Globals)
        {
                string whereStr = "PROMO_CODE='" + promoCode + "' AND SITE_ID='" + curSiteId + "' " +
                    "AND ASCITEMID='" + ascItemId + "' AND ORDERTYPE='" + orderType + "' " +
                    "AND ORDERNUMBER='" + orderNum + "' AND LINENUMBER=" + lineNum.ToString();

            string updStr = string.Empty;
                ascLibrary.ascStrUtils.ascAppendSetStr( ref updStr, "QTYORDERED", orderQty.ToString());
                ascLibrary.ascStrUtils.ascAppendSetStr( ref updStr, "LAST_UPDATE_USERID", "IMPORT");
                ascLibrary.ascStrUtils.ascAppendSetQty( ref updStr, "LAST_UPDATE_DATE", "GETDATE()");
            Globals.mydmupdate.UpdateFields( "PROMO_ORDERS", updStr, whereStr);
            
        }

        internal void UpdatePromoItemQty(string promoCode, string siteId, string ascItemId, string orderType, ParseNet.GlobalClass Globals)
        {
            string sqlStr = "SELECT SUM(QTYORDERED) FROM PROMO_ORDERS (NOLOCK) " +
                "WHERE PROMO_CODE='" + promoCode + "' AND SITE_ID='" + siteId + "' " +
                "AND ASCITEMID='" + ascItemId + "' AND ORDERTYPE='" + orderType + "'";
            string tmp = string.Empty;
            Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref tmp);
            double qtyOrdered = ascLibrary.ascUtils.ascStrToDouble(tmp, 0);

            sqlStr = "UPDATE PROMO_ITEMS SET QTY_PROMO=" + qtyOrdered + " " +
                "WHERE PROMO_CODE='" + promoCode + "' AND SITE_ID='" + siteId + "' " +
                "AND ASCITEMID='" + ascItemId + "'";
            Globals.mydmupdate.AddToUpdate(sqlStr);
        }

        public void LogError(string errmsg)
        {
            if ((myLogRecord != null) && !myLogRecord.LogType.Equals( "X"))
            {
                myLogRecord.LogType = "E";
                myLogRecord.OutData = errmsg;
            }
        }
        public void LogException(Exception ex)
        {
            if (myLogRecord != null)
            {
                myLogRecord.LogType = "X";
                myLogRecord.StackTrace = ex.StackTrace;
                myLogRecord.OutData = ex.Message;
            }
        }

        public void PostLog(HttpStatusCode statusCode, string errmsg)
        {
            if (!String.IsNullOrEmpty(errmsg) && myLogRecord.LogType.Equals("I"))
            {
                myLogRecord.LogType = "E";
                if (String.IsNullOrEmpty(myLogRecord.OutData))
                    myLogRecord.OutData = errmsg;
            }
            if ((statusCode != HttpStatusCode.OK) && myLogRecord.LogType.Equals("I"))
            {
                myLogRecord.LogType = "E";
            }
            myLogRecord.ReturnStatus = Convert.ToInt32(statusCode);

            bool fDoit = false;
            if (fLogging.Equals("A") || myLogRecord.LogType.Equals("X"))
                fDoit = true;
            else if (fLogging.Equals("E") && myLogRecord.LogType.Equals("E"))
                fDoit = true;
            if (fDoit)
            {
                string errorType = "INFO";
                if (statusCode == HttpStatusCode.BadRequest)
                    errorType = "CRIT";
                else if (statusCode != HttpStatusCode.OK)
                    errorType = "DATA";
                else if (!String.IsNullOrEmpty(errmsg))
                    errorType = "WARN";

                if( fPostAPILog)
                    PostAPILog(statusCode, errmsg, errorType);
                else
                    PostAPPLog(statusCode, errmsg, errorType);
            }
        }

        private void PostAPPLog(HttpStatusCode statusCode, string errmsg, string errorType)
        {
            string sqldata = myLogRecord.SQLData;
            if (string.IsNullOrEmpty(sqldata) && (myParse.Globals.myASCLog != null))
                sqldata = myParse.Globals.myASCLog.GetSQLData();

            myParse.Globals.WriteAppLog("", myLogRecord.FunctionID, errorType, myLogRecord.InData, errmsg, myLogRecord.StackTrace, myLogRecord.OutData, myLogRecord.OrderNum);
        }

        private void PostAPILog(HttpStatusCode statusCode, string errmsg, string errorType)
        {
            string tmp = string.Empty;
            myLogRecord.StopDateTime  = myParse.Globals.myDBUtils.GetSQLDateTime();

            var con = new SqlConnection(myParse.Globals.myDBUtils.myConnString);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("ASC_INSERT_API_LOG", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("Url", myLogRecord.URL);
                cmd.Parameters.AddWithValue("LogType", myLogRecord.LogType);
                cmd.Parameters.AddWithValue("FunctionID", myLogRecord.FunctionID);
                cmd.Parameters.AddWithValue("HttpFunctionID", myLogRecord.HttpFunctionID);
                cmd.Parameters.AddWithValue("OrderNum", myLogRecord.OrderNum);
                cmd.Parameters.AddWithValue("ItemID", myLogRecord.ItemID);
                cmd.Parameters.AddWithValue("ASC_ERROR_TYPE", errorType);

                cmd.Parameters.AddWithValue("StartDateTime", myLogRecord.StartDateTime);
                cmd.Parameters.AddWithValue("StopDateTime", myLogRecord.StopDateTime);
                cmd.Parameters.AddWithValue("ReturnStatus", myLogRecord.ReturnStatus);

                string sqldata = myLogRecord.SQLData;
                if (string.IsNullOrEmpty(sqldata) && (myParse.Globals.myASCLog != null))
                    sqldata = myParse.Globals.myASCLog.GetSQLData();
                cmd.Parameters.AddWithValue("SQLData", sqldata);
                cmd.Parameters.AddWithValue("OutData", myLogRecord.OutData);
                cmd.Parameters.AddWithValue("InData", myLogRecord.InData);
                cmd.Parameters.AddWithValue("InfoMsg", myLogRecord.infoMsg);
                cmd.Parameters.AddWithValue("StackTrace", myLogRecord.StackTrace);

                cmd.ExecuteNonQuery();
            }
            finally
            {
                con.Close();
            }
        }
        /*
        internal string GetUserDefInfo(string poNum, string lineNum, string skidId, string customDataDefault, string userDefField, bool userDefFieldInIntfc)
        {
            string customData = customDataDefault;


            if ((!String.IsNullOrEmpty(userDefField)) && userDefFieldInIntfc)
            {
                if (MiscFuncs.Copy(userDefField.ToUpper(), 0, 9) == "LOCITEMS.")
                {
                    string locitemsUserDefField = MiscFuncs.Copy(userDefField.ToUpper(), 9, 50);
                    if ((!String.IsNullOrEmpty(skidId)) && (!String.IsNullOrEmpty(locitemsUserDefField)))
                    {
                        string sqlStr2 = "SELECT MAX(" + locitemsUserDefField + ") AS " + locitemsUserDefField + " FROM LOCITEMS WHERE SKIDID='" + skidId + "'";
                        if (!AscDbUtils.ReadFieldsFromAscDb(sqlStr2, ref customData))
                        {
                            sqlStr2 = "SELECT " + locitemsUserDefField + " FROM OLDLCITM WHERE (SKIDID ='" + skidId + "' " +
                                "OR SKIDID LIKE '" + skidId + "-%') " + "AND ISNULL(" + locitemsUserDefField + ",'')<>'' " +
                                "ORDER BY ARCHIVE_DATE DESC";
                            customData = AscDbUtils.ReadFieldFromAscDb(sqlStr2);
                        }
                    }
                }
                else
                    AscDbUtils.GetPODetInfo(poNum, lineNum, userDefField, ref customData);
            }
            if (string.IsNullOrEmpty(customData))  //added 10-22-15 (JXG)
                customData = customDataDefault;
            return customData;
        }
        */
    }
}
