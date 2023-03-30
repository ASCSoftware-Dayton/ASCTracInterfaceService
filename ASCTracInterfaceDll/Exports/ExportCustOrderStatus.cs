using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Exports
{
    public class ExportCustOrderStatus
    {
        private string funcType = "EX_ORDER_STATUS";
        private Class1 myClass;
        private Model.CustOrder.COExportConfig currExportConfig;

        public static HttpStatusCode doExportCustOrderStatus (Class1 myClass, ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter aCOExportfilter, ref List<ASCTracInterfaceModel.Model.CustOrder.CustOrderStatusExport> aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            aData = new List<ASCTracInterfaceModel.Model.CustOrder.CustOrderStatusExport>();
            string OrderNum = string.Empty;
            string sqlstr = string.Empty;
            try
            {
                if (!myClass.FunctionAuthorized(myClass.myLogRecord.FunctionID))
                    retval = HttpStatusCode.NonAuthoritativeInformation;
                else
                {
                    var myExport = new ExportCustOrderStatus(myClass);
                    sqlstr = myExport.BuildCustOrderExportSQL(aCOExportfilter, ref errmsg);
                    if (!String.IsNullOrEmpty(sqlstr))
                    {
                        myClass.myLogRecord.SQLData = sqlstr;
                        retval = myExport.BuildExportList(sqlstr, aCOExportfilter.MaxRecords, ref aData, ref errmsg);
                    }
                    else
                        retval = HttpStatusCode.BadRequest;
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

        public ExportCustOrderStatus(Class1 aClass)
        {
            myClass = aClass;
            currExportConfig = Configs.CustOrderConfig.getCOExportSite("1", myClass.myParse.Globals);
        }
        private string BuildCustOrderExportSQL(ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter aExportFilter, ref string errmsg)
        {
            string postedFlagField = currExportConfig.StatusPostedFlagField;
            string sql = "SELECT TRANFILE.TRANDATE, TRANFILE.TRANTYPE, TRANFILE.ID, TRANFILE.ORDERNUM, TRANFILE.RELEASENUM, TRANFILE.ACCTNUM, TRANFILE.SERIALNUM, TRANFILE.AREA, ORDRHDR.PICKSTATUS AS ORDER_STATUS, ORDRHDR.SOLDTOCUSTID AS CUST_VEND_ID, SITES.HOST_SITE_ID " +
                "FROM TRANFILE (NOLOCK) " +
                "LEFT JOIN ORDRHDR (NOLOCK) ON ORDRHDR.ORDERNUMBER=TRANFILE.ORDERNUM " +
                "LEFT JOIN SITES (NOLOCK) ON SITES.SITE_ID=TRANFILE.SITE_ID " +
                " WHERE TRANFILE.TRANTYPE IN ( 'LC', 'LO', 'CS', 'CU', 'LU')" +
               // "WHERE (TRANFILE.TRANTYPE='LC' " +  //re-schedule Cust Order
               // "    OR TRANFILE.TRANTYPE='LO') " +  //Cust Order Pick Status change
                " AND isnull(TRANFILE." + postedFlagField + ", 'F') IN (" + currExportConfig.FilterPostedValues + ") " +
                " AND (SITES.HOST_SITE_ID<>'') " +
                " AND ORDRHDR.ORDERTYPE<>'C' ";  //"AND ORDRHDR.ORDERTYPE<>'T' AND ORDRHDR.PICKSTATUS<>'C' AND ORDRHDR.PICKSTATUS<>'X'"
            if (!String.IsNullOrEmpty(aExportFilter.CustID))
                sql += " AND ORDRHDR.SOLDTOCUSTID='" + aExportFilter.CustID + "'";
            Utils.FilterUtils.AppendToExportFilter(ref sql, aExportFilter.ExportFilterList, "TRANFILE", "SITES|ORDRHDR|CUST");
            /*
            sql += " UNION " +
                "SELECT TRANFILE.TRANDATE, TRANFILE.TRANTYPE, TRANFILE.ID, TRANFILE.ORDERNUM, TRANFILE.RELEASENUM, TRANFILE.ACCTNUM, TRANFILE.SERIALNUM, TRANFILE.AREA, ORDRHDR.PICKSTATUS AS ORDER_STATUS, ORDRHDR.SOLDTOCUSTID AS CUST_VEND_ID, SITES.HOST_SITE_ID " +
                "FROM TRANFILE T (NOLOCK) " +
                "LEFT JOIN ORDRHDR OH (NOLOCK) ON ORDRHDR.ORDERNUMBER=TRANFILE.ORDERNUM " +
                "LEFT JOIN SITES S (NOLOCK) ON SITES.SITE_ID=TRANFILE.SITE_ID " +
                "WHERE (TRANFILE.TRANTYPE='CS' " +  //Cust Order Confirm Ship  //added 07-18-16 (JXG)
                "    OR TRANFILE.TRANTYPE='CU' " +  //UnConfirm Ship Order  //added 08-02-16 (JXG)
                "    OR TRANFILE.TRANTYPE='LU') " +  //UnLock Order  //added 08-02-16 (JXG)
                "AND (TRANFILE." + postedFlagField + "='F' OR TRANFILE." + postedFlagField + " IS NULL) " +
                "AND (SITES.HOST_SITE_ID<>'') " +
                "AND ORDRHDR.ORDERTYPE<>'C' ";  //"AND ORDRHDR.ORDERTYPE<>'T' AND ORDRHDR.PICKSTATUS<>'C' AND ORDRHDR.PICKSTATUS<>'X'"
            Utils.FilterUtils.AppendToExportFilter(ref sql, aExportFilter.ExportFilterList, "TRANFILE", "SITES|ORDRHDR|CUST");
            */
            sql += " UNION " +
                "SELECT TRANFILE.TRANDATE, TRANFILE.TRANTYPE, TRANFILE.ID, TRANFILE.ORDERNUM, TRANFILE.RELEASENUM, TRANFILE.ACCTNUM, TRANFILE.SERIALNUM, TRANFILE.AREA, POHDR.RECEIVED AS ORDER_STATUS, POHDR.VENDORID AS CUST_VEND_ID, SITES.HOST_SITE_ID " +
                "FROM TRANFILE (NOLOCK) " +
                "LEFT JOIN POHDR (NOLOCK) ON POHDR.PONUMBER=TRANFILE.ORDERNUM AND POHDR.RELEASENUM=TRANFILE.RELEASENUM " +
                "LEFT JOIN SITES (NOLOCK) ON SITES.SITE_ID=TRANFILE.SITE_ID " +
                "WHERE TRANFILE.TRANTYPE='LR' " +  //re-schedule PO
                " AND isnull(TRANFILE." + postedFlagField + ", 'F') IN (" + currExportConfig.FilterPostedValues + ") " +
                "AND (SITES.HOST_SITE_ID<>'') " +
                "AND (POHDR.ORDERTYPE='S' OR POHDR.ORDERTYPE='T') ";  //"AND ORDRHDR.RECEIVED<>'C' AND ORDRHDR.RECEIVED<>'X'"
            if (!String.IsNullOrEmpty(aExportFilter.CustID))
                sql += " AND POHDR.VENDORID='" + aExportFilter.CustID + "'";
            Utils.FilterUtils.AppendToExportFilter(ref sql, aExportFilter.ExportFilterList, "TRANFILE", "SITES|POHDR|CUST");

            sql += "ORDER BY TRANFILE.ORDERNUM, TRANFILE.ID ";  //TRANFILE.TRANTYPE, TRANFILE.ID ";  //changed 07-18-16 (JXG)
            return (sql);
        }

        private HttpStatusCode BuildExportList(string sqlstr, long aMaxRecords, ref List<ASCTracInterfaceModel.Model.CustOrder.CustOrderStatusExport> aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.NoContent;
            SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString);
            SqlCommand cmd = new SqlCommand(sqlstr, conn);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            myClass.myParse.Globals.mydmupdate.InitUpdate();
            try
            {
                long count = 1;
                while (reader.Read())
                {
                    if ((aMaxRecords > 0) && (count > aMaxRecords))
                        break;
                    count += 1;
                    retval = HttpStatusCode.OK;
                    ASCTracInterfaceModel.Model.CustOrder.CustOrderStatusExport rec = new ASCTracInterfaceModel.Model.CustOrder.CustOrderStatusExport();

                    var tranType = reader["TRANTYPE"].ToString();
                    var orderNum = reader["ORDERNUM"].ToString();
                    var relNum = reader["RELEASENUM"].ToString();
                    var tranDate = ascLibrary.ascUtils.ascStrToDate(reader["TRANDATE"].ToString(), DateTime.MinValue);  //added 08-01-16 (JXG)

                    //rec.PROCESS_FLAG"] = "R";
                    rec.CREATE_DATETIME = DateTime.Now;
                    rec.FACILITY = reader["HOST_SITE_ID"].ToString();
                    rec.ORDERNUMBER = orderNum;
                    rec.TRANSACTION_DATE = tranDate;  //added 08-01-16 (JXG)
                    if ((tranType == "LC") || (tranType == "LR"))
                        rec.RESCHEDULED_FLAG = "T";
                    else
                        rec.RESCHEDULED_FLAG = "F";
                    if (tranType == "LR")
                        rec.ORDER_TYPE = "R";
                    else
                        rec.ORDER_TYPE = "C";
                    if (tranType == "LU")
                        rec.PICKSTATUS = "U";  //added 08-02-16 (JXG)
                    else if (tranType == "CU")
                        rec.PICKSTATUS = "Q";  //added 08-02-16 (JXG)
                    else
                        rec.PICKSTATUS = reader["AREA"].ToString();  //reader["ORDER_STATUS"].ToString();
                    if ((tranType == "LC") || (tranType == "LR")
                        || (tranType == "LO"))  //added 09-13-16 (JXG)
                                                //|| ((tranType == "LO") && (reader["AREA"].ToString() == "S")))  //added 08-02-16 (JXG)
                    {
                        DateTime tdate;
                        if (!String.IsNullOrEmpty(reader["ACCTNUM"].ToString()))
                        {
                            DateTime.TryParse(reader["ACCTNUM"].ToString(), out tdate);
                            rec.NEWDATETIME = tdate;
                        }
                        if (!String.IsNullOrEmpty(reader["SERIALNUM"].ToString()))
                        {
                            DateTime.TryParse(reader["SERIALNUM"].ToString(), out tdate);
                            rec.ORIGINALDATETIME = tdate;
                        }
                    }
                    if (tranType == "LR")
                        rec.VENDORID = reader["CUST_VEND_ID"].ToString();
                    else
                        rec.CUSTOMERID = reader["CUST_VEND_ID"].ToString();

                    string where = "ID=" + reader["id"].ToString();
                    SetPosted(where, string.Empty, "S");

                    aData.Add(rec);

                }
                if (retval == HttpStatusCode.OK)
                    myClass.myParse.Globals.mydmupdate.ProcessUpdates();

            }
            finally
            {
                reader.Close();
                cmd.Dispose();
                conn.Close();
                conn.Dispose();
            }
            return (retval);
        }

        private void SetPosted(string wherestr, string aERROR_MESSAGE, string aPostedflag)
        {
            int msgLen = Convert.ToInt32(myClass.myParse.Globals.myDBUtils.getfieldsize("TRANFILE", "ERR_MESSAGE"));
            string shortErrorMessage = aERROR_MESSAGE;
            if (shortErrorMessage.Length > msgLen)
                shortErrorMessage = aERROR_MESSAGE.Substring(0, msgLen);
            string sqlStr = "UPDATE TRANFILE";
            if (!aPostedflag.Equals("E"))
                sqlStr += " SET " + currExportConfig.StatusPostedFlagField + "='" + aPostedflag + "', " + currExportConfig.StatusPosteddateField + "=GETDATE() ";
            else
                sqlStr += " SET " + currExportConfig.StatusPostedFlagField + "='E', " + currExportConfig.StatusPosteddateField + "=GETDATE(), " +
                    "ERR_MESSAGE='" + shortErrorMessage.Replace("'", "''") + "', " +
                    "LONG_MESSAGE='" + aERROR_MESSAGE.Replace("'", "''") + "' ";
            sqlStr += " WHERE " + wherestr;
            if (aPostedflag.Equals("S"))
                sqlStr += " AND ISNULL(" + currExportConfig.StatusPostedFlagField + ",'F') IN(" + currExportConfig.FilterPostedValues + ") ";
            else
                sqlStr += " AND ISNULL(" + currExportConfig.StatusPostedFlagField + ",'F') = 'S'";
            //+" AND ISNULL(" + currExportConfig.postedFlagField + "','F') NOT IN ( 'T', 'X', 'D', 'E', 'P', '" + aPostedflag + "')";

            myClass.myParse.Globals.mydmupdate.AddToUpdate(sqlStr);
        }



        public static HttpStatusCode updateExportCustOrderStatus(Class1 myClass, List<ASCTracInterfaceModel.Model.CustOrder.CustOrderStatusExport> aData, ref string errmsg)
        {
            //myClass = Class1.InitParse("UpdateExportCustOrder", ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            string OrderNum = string.Empty;
            try
            {
                var myExport = new ExportCustOrderStatus(myClass);
                retval = myExport.DoupdateExportCustOrderStatus(aData, ref errmsg);
            }
            catch (Exception ex)
            {
                myClass.LogException(ex);
                retval = HttpStatusCode.BadRequest;
                errmsg = ex.Message;
            }
            return (retval);

        }
        private HttpStatusCode DoupdateExportCustOrderStatus(List<ASCTracInterfaceModel.Model.CustOrder.CustOrderStatusExport> aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            string OrderNum = string.Empty;
            try
            {
                foreach (var rec in aData)
                {
                    OrderNum = rec.ORDERNUMBER;
                    string posted = "T";
                    if (!rec.SUCCESSFUL)
                        posted = "E";
                    string where = "ORDERNUM='" + rec.ORDERNUMBER + "' AND TRANTYPE IN ( 'LC', 'LO', 'LR', 'LU', 'CS', 'CU')";
                    SetPosted( where, rec.ERROR_MESSAGE, posted);
                }
                myClass.myParse.Globals.mydmupdate.ProcessUpdates();
            }
            catch (Exception ex)
            {
                myClass.LogException(ex);
                retval = HttpStatusCode.BadRequest;
                errmsg = ex.Message;
            }

            return (retval);
        }
    }
}
