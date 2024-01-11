using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace ASCTracInterfaceService.Filters
{
    public class StandardUserServices : IUserServices
    {
        private ascLibrary.ascDBUtils myDBUtils = new ascLibrary.ascDBUtils();
        private bool fInit = false;
        private bool fUsingAuthentication = false;

        private bool InitAuthenticate()
        {
            if (!fInit)
            {
//                try
//               {
                    var currDT = string.Empty;
//                    try
//                    {
                        myDBUtils.BuildConnectString("AliasASCTrac");
                        string tmp = string.Empty;
                        if (myDBUtils.ReadFieldFromDB("select CFGDATA from CFGSETTINGS WHERE CFGFIELD = 'GWInterfaceUseAuthentication'", "", ref tmp))
                            fUsingAuthentication = tmp.Equals("T");
//                    }
//                    catch (Exception EX1)
//                    {
//                        ascLibrary.ascUtils.ascWriteLog("ASCTracInterface", "Exception at BuildConnectString: " + EX1.ToString(), false);
//                        myDBUtils.myConnString = "packet size=4096;user id=app_user;Password='WeH73w';data source=asc-cin-app01;persist security info=False;initial catalog=ASCTRAC904Dev";
//                    }
                    myDBUtils.ReadFieldFromDB("SELECT GETDATE()", "", ref currDT);
                    fInit = true;
//                }
//                catch (Exception ex)
//                {
//                    ascLibrary.ascUtils.ascWriteLog("ASCTracInterface", "Exception during Init Authenticate" + ex.ToString(), false);
//                }
            }
            return (fInit);
        }

        public int Authenticate(string token, string param)
        {
            int retval = 1;
            string aerrmsg = string.Empty;
            if (InitAuthenticate())
            {
                if (fUsingAuthentication)
                {
                    string tmp = string.Empty;
                    if (myDBUtils.ReadFieldFromDB("SELECT START_DATE, END_DATE, GetDate() FROM ASCREST_AUTH WHERE TOKEN_VALUE='" + token + "'", "", ref tmp))
                    {
                        DateTime startDT = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref tmp), DateTime.MinValue);
                        DateTime endDT = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref tmp), DateTime.MinValue);
                        DateTime currDT = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref tmp), DateTime.MinValue);
                        if ((currDT < startDT) || (currDT > endDT))
                            retval = 0;
                        else
                            retval = 1;
                    }
                    else
                    {
                        retval = 0;
                    }
                }
            }

            return (retval);
        }
        public int Authenticate(string token )
        {
            int retval = 1;
            string aerrmsg = string.Empty;
            try
            {
                if (InitAuthenticate())
                {
                    if (fUsingAuthentication)
                    {
                        string tmp = string.Empty;
                        if( myDBUtils.ReadFieldsFromDBWithParam("SELECT START_DATE, END_DATE, GetDate() FROM ASCREST_AUTH WHERE TOKEN_VALUE=@TOKEN_VALUE", "@TOKEN_VALUE", token, ref tmp))
                        //if (myDBUtils.ReadFieldFromDB("SELECT START_DATE, END_DATE, GetDate() FROM ASCREST_AUTH WHERE TOKEN_VALUE='" + token + "'", "", ref tmp))
                        {
                            DateTime startDT = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref tmp), DateTime.MinValue);
                            DateTime endDT = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref tmp), DateTime.MinValue);
                            DateTime currDT = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref tmp), DateTime.MinValue);
                            if ((currDT < startDT) || (currDT > endDT))
                                retval = 0;
                            else
                                retval = 1;
                        }
                        else
                        {
                            retval = 0;
                        }
                    }
                }
            }
            catch( Exception e)
            {
                if( fInit)
                {
                    var con = new SqlConnection(myDBUtils.myConnString);
                    try
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("ASC_INSERT_APP_LOG", con);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("AppName", "ASCTracInterface");
                        cmd.Parameters.AddWithValue("TranID", "");
                        cmd.Parameters.AddWithValue("TranType", "Authenticate");
                            cmd.Parameters.AddWithValue("Instance", "0");
                        cmd.Parameters.AddWithValue("OrderNum", "");
                        cmd.Parameters.AddWithValue("ErrorType", "CRIT");
                        cmd.Parameters.AddWithValue("ErrorID", "????");
                        cmd.Parameters.AddWithValue("REMOTE_IPADDR", "");
                        cmd.Parameters.AddWithValue("APP_IPADDR", "");
                        cmd.Parameters.AddWithValue("END_DATETIME", DateTime.Now);
                        cmd.Parameters.AddWithValue("Version", "");
                        cmd.Parameters.AddWithValue("USERID", "");
                        cmd.Parameters.AddWithValue("FILENAME", "");

                        cmd.Parameters.AddWithValue("OutputData", "");
                        cmd.Parameters.AddWithValue("InputData", token);
                        cmd.Parameters.AddWithValue("ErrorMsg", e.Message);
                        cmd.Parameters.AddWithValue("StackTrace", e.StackTrace);
                        string sqldata = string.Empty;
                        cmd.Parameters.AddWithValue("SQLData", sqldata);

                        cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        con.Close();
                    }

                }
                else
                {
                    string errMsg = string.Empty;
                    LoggingUtil.LogEventView("Auth", token, e.ToString(), ref errMsg);
                }
            }

            return (retval);
        }
    }
}