using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace ASCTracInterfaceDll.Utils
{
    internal class AllocUtil
    {
        internal static void AddLotAllocForOrderDet(string orderNum, long lineNum, string ascItemId, string aLot, double qtyToAlloc, ParseNet.GlobalClass Globals)
        {
            /*
                // If table exists, we are using lot allocation
                sqlStr = "SELECT S1.* FROM SYSOBJECTS S1 (NOLOCK) " +
                    "WHERE S1.NAME='TBL_TOASC_CUST_ORDR_ALLOCATION'";
                if (AscDbUtils.ReadFieldsFromAscIntrfceDb(sqlStr, ref tmpStr))
                {
                    sqlStr = "SELECT ORDERNUMBER FROM TBL_TOASC_CUST_ORDR_ALLOCATION (NOLOCK) " +
                        "WHERE ORDERNUMBER='" + orderNum + "' AND LINENUMBER=" + lineNum + " " +
                        "AND ORDERTYPE='C' AND PROCESS_FLAG='R' AND LOTID='" + reqLot + "'";
                    if (!AscDbUtils.ReadFieldsFromAscIntrfceDb(sqlStr, ref tmpStr))
                    {
                        // Lot allocation host record does not exist, so create
                        AscUpdate lotAlloc = new AscUpdate();
                        lotAlloc.Add("CREATE_DATETIME", "GETDATE()");
                        lotAlloc.Add("FACILITY", siteId);
                        lotAlloc.Add("ITEMID", itemId);
                        lotAlloc.Add("ORDERNUMBER", orderNum);
                        lotAlloc.Add("LINENUMBER", lineNum);
                        lotAlloc.Add("ORDERTYPE", "C");
                        lotAlloc.Add("PROCESS_FLAG", "R");
                        lotAlloc.Add("LOTID", reqLot);
                        lotAlloc.Add("QTY_PREALLOC", qty.ToString());
                        lotAlloc.InsertRec("TBL_TOASC_CUST_ORDR_ALLOCATION",
                            new SqlConnection(Globals.ascIntrfceConnStr));
                    }
                }
            */
        }

        internal static void BackoutPreAllocationForOrderDet(string orderNum, long lineNum, ParseNet.GlobalClass Globals)
        {
            string ascItemId = string.Empty;

            Globals.myGetInfo.GetOrderDetInfo(orderNum, "PICK_SKIDID, ASCITEMID", lineNum.ToString(), ref ascItemId);

            var skidId = ascLibrary.ascStrUtils.GetNextWord(ref ascItemId);

            if (!String.IsNullOrEmpty(ascItemId))
            {
                string sqlStr = "SELECT LOTID FROM LOTALLOC (NOLOCK) WHERE ORDERNUMBER='" + orderNum + "' " +
                    "AND ORDERTYPE='C' AND LINENUMBER=" + lineNum;
                using (SqlConnection conn = new SqlConnection(Globals.myDBUtils.myConnString))
                using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            sqlStr = "DELETE FROM LOTALLOC WHERE ORDERNUMBER='" + orderNum + "' " +
                                "AND LOTID='" + dr["LOTID"].ToString() + "' AND ORDERTYPE='C' " +
                                "AND LINENUMBER=" + lineNum;
                            Globals.mydmupdate.AddToUpdate(sqlStr);

                            SetLotPreallocForLot(ascItemId, dr["LOTID"].ToString(), Globals);
                        }
                    }
                }
            }

            if (skidId.Length > 1)
            {
                string sqlStr = "UPDATE LOCITEMS SET PREALLOC_ORDERNUMBER=NULL, PREALLOC_WORKORDER_ID=NULL " +
                    "WHERE SKIDID='" + skidId + "'";
                Globals.mydmupdate.AddToUpdate(sqlStr);
            }
        }

        internal static void SetLotPreallocForLot(string ascItemId, string lotId, ParseNet.GlobalClass Globals)
        {
            string sql = "UPDATE LOT SET LOT.QTY_PREALLOC = " +
                "(SELECT ISNULL(SUM(ISNULL(LOTALLOC.QTY_PREALLOC, 0) - ISNULL(LOTALLOC.QTY_PICKED,0)),0) " +
                "FROM LOTALLOC WHERE LOTALLOC.ASCITEMID='" + ascItemId + "' AND LOTALLOC.LOTID='" + lotId + "') " +
                "WHERE LOT.ASCITEMID='" + ascItemId + "' AND LOT.LOTID='" + lotId + "'";
            Globals.mydmupdate.AddToUpdate(sql);

            Globals.mydmupdate.SetItemMasterQty(ascItemId, "QTYHARDALLOC");
        }

    }
}
