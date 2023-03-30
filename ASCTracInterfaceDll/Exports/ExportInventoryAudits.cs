using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Exports
{
    public class ExportInventoryAudits
    {
        private string funcType = "EX_INAUD";
        private Class1 myClass;
        private Model.Item.ItemImportConfig currExportConfig;

        public static HttpStatusCode DoExportInventoryAudits(Class1 myClass, string aVMICustID, string aSiteID, string aItemID, ref List<ASCTracInterfaceModel.Model.Item.InventoryAuditExport> aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            aData = new List<ASCTracInterfaceModel.Model.Item.InventoryAuditExport>();
            string OrderNum = string.Empty;
            string sqlstr = string.Empty;
            try
            {
                if (!myClass.FunctionAuthorized(myClass.myLogRecord.FunctionID))
                    retval = HttpStatusCode.NonAuthoritativeInformation;
                else
                {
                    var myExport = new ExportInventoryAudits(myClass);
                    sqlstr = myExport.BuildExportSQL(aVMICustID, aSiteID, aItemID, ref errmsg);
                    if (!String.IsNullOrEmpty(sqlstr))
                    {
                        myClass.myLogRecord.SQLData = sqlstr;
                        retval = myExport.BuildExportList(sqlstr, ref aData, ref errmsg);
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

        public ExportInventoryAudits( Class1 aClass)
        {
            myClass = aClass;
            currExportConfig = Configs.ItemConfig.getImportSite("1", myClass.myParse.Globals);
        }

        private string BuildExportSQL(string aVMICustID, string aSiteID, string aItemID, ref string errmsg)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append("SELECT S.SITE_ID, S.HOST_SITE_ID, LI.ASCITEMID, LI.ITEMID, SUM(LI.QTYTOTAL) AS TOTQTY, ");
            sql.Append("SUM(LI.QTYALLOC) AS ALLOCQTY, SUM(LI.QTYONHOLD) AS HOLDQTY, SUM(QTY_DUAL_UNIT) AS QTYDUALUNIT, ");
            sql.Append("I.STOCK_UOM, I.VMI_CUSTID, LI.EXPDATE, ");
            if (currExportConfig.InvAuditExportLotId)
            {
                sql.Append("ISNULL(LI.LOTID, '') AS LOTID, ");
                sql.Append("ISNULL(MAX(LI.ALT_LOTID), '') AS ALT_LOTID ");  //added 08-02-16 (JXG) for Driscoll's
            }
            else
            {
                sql.Append("'' AS LOTID, ");
                sql.Append("'' AS ALT_LOTID ");  //added 08-02-16 (JXG) for Driscoll's
            }
            sql.Append("FROM LOCITEMS LI (NOLOCK) ");
            if (currExportConfig.InvAuditExportExcludeNonPickable )
                sql.Append("INNER JOIN LOC L (NOLOCK) ON L.LOCATIONID=LI.LOCATIONID AND L.SITE_ID=LI.SITE_ID ");
            sql.Append("LEFT JOIN SITES S (NOLOCK) ON S.SITE_ID=LI.SITE_ID ");
            sql.Append("LEFT JOIN ITEMMSTR I (NOLOCK) ON I.ASCITEMID=LI.ASCITEMID ");
            sql.Append("WHERE S.HOST_SITE_ID<>'' ");
            if (currExportConfig.InvAuditExportExcludeNonPickable)
                sql.Append("AND ISNULL(L.PICKABLE_FLAG,'')<>'F' ");
            if (!String.IsNullOrEmpty(aVMICustID))
                sql.Append("AND LI.VMI_CUSTID='" + aVMICustID + "' ");
            if (!String.IsNullOrEmpty(aSiteID))
                sql.Append("AND LI.SITE_ID='" + aSiteID + "' ");
            if (!String.IsNullOrEmpty(aItemID))
                sql.Append("AND LI.ITEMID='" + aItemID + "' ");
            sql.Append("GROUP BY S.SITE_ID, S.HOST_SITE_ID, LI.ASCITEMID, LI.ITEMID, I.STOCK_UOM, I.VMI_CUSTID, LI.EXPDATE ");
            if (currExportConfig.InvAuditExportLotId)
                sql.Append(", ISNULL(LI.LOTID, '') ");
            sql.Append("UNION ");
            sql.Append("SELECT S.SITE_ID, S.HOST_SITE_ID, LI.ASCITEMID, LI.ITEMID, 0 AS TOTQTY, 0 AS ALLOCQTY, 0 AS HOLDQTY, ");
            sql.Append("0 AS QTYDUALUNIT, LI.STOCK_UOM, LI.VMI_CUSTID, '' AS EXPDATE, '' AS LOTID, '' AS ALT_LOTID ");
            sql.Append("FROM ITEMMSTR LI (NOLOCK) ");
            sql.Append("LEFT JOIN SITES S (NOLOCK) ON S.SITE_ID=LI.SITE_ID ");
            sql.Append("LEFT JOIN ITEMQTY L2 (NOLOCK) ON LI.ASCITEMID=L2.ASCITEMID ");
            sql.Append("WHERE S.HOST_SITE_ID<>'' ");
            if (!String.IsNullOrEmpty(aVMICustID))
                sql.Append("AND LI.VMI_CUSTID='" + aVMICustID + "' ");
            if (!String.IsNullOrEmpty(aSiteID))
                sql.Append("AND LI.SITE_ID='" + aSiteID+ "' ");
            if (!String.IsNullOrEmpty(aItemID))
                sql.Append("AND LI.ITEMID='" + aItemID + "' ");
            sql.Append("AND (L2.QTYTOTAL=0 OR L2.QTYTOTAL IS NULL) ");
            sql.Append("GROUP BY S.SITE_ID, S.HOST_SITE_ID, LI.ASCITEMID, LI.ITEMID, LI.STOCK_UOM, LI.VMI_CUSTID ");
            sql.Append("ORDER BY S.SITE_ID, S.HOST_SITE_ID, LI.ASCITEMID, LI.ITEMID");
            return (sql.ToString());
        }

        private string BuildWhereStr(ASCTracInterfaceModel.Model.ModelExportFilter rec)
        {
            string retval = string.Empty;
            if (myClass.myParse.Globals.myDBUtils.IfFieldExists("TRANFILE", rec.Fieldname))
                retval = ascLibrary.ascStrUtils.buildwherestr(rec.Fieldname, rec.FilterType.ToString(), rec.Startvalue, rec.Endvalue);
            return (retval);
        }

        private HttpStatusCode BuildExportList(string sqlstr, ref List<ASCTracInterfaceModel.Model.Item.InventoryAuditExport> aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.NoContent;
            SqlConnection conn = new SqlConnection(myClass.myParse.Globals.myDBUtils.myConnString);
            SqlCommand cmd = new SqlCommand(sqlstr, conn);
            conn.Open();
            SqlDataReader drAudit = cmd.ExecuteReader();

            myClass.myParse.Globals.mydmupdate.InitUpdate();
            try
            {
                while (drAudit.Read())
                {
                    retval = HttpStatusCode.OK;
                    var rec = new ASCTracInterfaceModel.Model.Item.InventoryAuditExport();

                    string ascItemId = drAudit["ASCITEMID"].ToString();
                    double totQty = ascLibrary.ascUtils.ascStrToDouble(drAudit["TOTQTY"].ToString(), 0);
                    double qtyDualUnit = ascLibrary.ascUtils.ascStrToDouble(drAudit["QTYDUALUNIT"].ToString(), 0);
                    string tmpStr = string.Empty;
                    myClass.myParse.Globals.myGetInfo.GetASCItemInfo( ascItemId, "DUAL_UNIT_ITEM, BILL_UOM, MFG_ID, STANDARDCOST", ref tmpStr);
                    bool dualUnitItem =  ascLibrary.ascStrUtils.GetNextWord( ref tmpStr).Equals("T");
                    string billUom = ascLibrary.ascStrUtils.GetNextWord( ref tmpStr);
                    string mfgId = ascLibrary.ascStrUtils.GetNextWord( ref tmpStr);  //added 03-24-17 (JXG)
                    string stdCost = ascLibrary.ascStrUtils.GetNextWord( ref tmpStr);  //added 03-24-17 (JXG)

                    if (currExportConfig.InvAuditExportExcludeZeroQuantity && totQty == 0)
                        continue;

                    //rec.PROCESS_FLAG = "R";
                    //rec.CREATE_DATETIME = DateTime.Now.ToString();
                    rec.FACILITY = drAudit["HOST_SITE_ID"].ToString();
                    rec.PRODUCT_CODE = drAudit["ITEMID"].ToString();
                    rec.QUANTITY = totQty;

                    DateTime dtExpDate;
                    if (DateTime.TryParse(drAudit["EXPDATE"].ToString(), out dtExpDate))
                        rec.EXPDATE = dtExpDate;

                        rec.QTY_ALLOCATED  = ascLibrary.ascUtils.ascStrToDouble( drAudit["ALLOCQTY"].ToString(), 0);
                        rec.QTY_ONHOLD = ascLibrary.ascUtils.ascStrToDouble(drAudit["HOLDQTY"].ToString(), 0);
                        rec.DATE_TIME = DateTime.Now;
                        rec.UOM = drAudit["STOCK_UOM"].ToString();
                        rec.VMI_CUSTID = drAudit["VMI_CUSTID"].ToString();
                        rec.LOTID = drAudit["LOTID"].ToString();
                    

                        rec.VMI_CUSTID = drAudit["VMI_CUSTID"].ToString();

                        rec.ALT_LOTID = drAudit["ALT_LOTID"].ToString();  //added 08-02-16 (JXG) for Driscoll's

                        if (!String.IsNullOrEmpty(mfgId))
                            rec.MFG_ID = mfgId;
                    if (!string.IsNullOrEmpty(stdCost))  //added 03-24-17 (JXG)
                    {
                        //if (Decimal.TryParse(stdCost, out tmpDecimal))
                        rec.COST_EACH = ascLibrary.ascUtils.ascStrToDouble( stdCost, 0);
                    }

                    if (!String.IsNullOrEmpty(billUom) || dualUnitItem)
                    {
                        rec.CW_UOM = billUom;
                        rec.CW_QTY = qtyDualUnit;
                    }

                    aData.Add(rec);

                }

            }
            finally
            {
                drAudit.Close();
                cmd.Dispose();
                conn.Close();
                conn.Dispose();
            }
            return (retval);
        }


    }
}
