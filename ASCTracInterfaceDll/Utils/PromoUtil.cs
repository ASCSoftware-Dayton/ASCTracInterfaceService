using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceDll.Utils
{
    internal class PromoUtil
    {
        internal static void AddPromo(string promoCode, string siteid, string acustid, ParseNet.GlobalClass Globals)
        {
            string updStr = string.Empty;
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "PROMO_CODE", promoCode);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "SITE_ID", siteid);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "MASTER_CLIENT", acustid);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "DESCRIPTION", "Imported");
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "CREATE_USERID", Globals.curUserID);
            ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "CREATE_DATE", "GETDATE()");
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "LAST_UPDATE_USERID", "IMPORT");
            ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "LAST_UPDATE_DATE", "GETDATE()");
            Globals.mydmupdate.InsertRecord("PROMOS", updStr);

        }

        internal static void AddPromoItem(string promoCode, string siteid, string ascItemId, ParseNet.GlobalClass Globals)
        {
            string updStr = "SELECT PROMO_CODE FROM PROMO_ITEMS (NOLOCK) " +
                    "WHERE PROMO_CODE='" + promoCode + "' AND SITE_ID='" + siteid + "' " +
                    "AND ASCITEMID='" + ascItemId + "'";
            if (!Globals.myDBUtils.ifRecExists(updStr))
            {
                updStr = string.Empty;
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "PROMO_CODE", promoCode);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "SITE_ID", siteid);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ITEMID", Globals.dmMiscItem.getitemid(ascItemId));
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ASCITEMID", ascItemId);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "COMMENT", "Imported");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "FILLED_FLAG", "O");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "QTY_PROMO", "0");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "QTYRECEIVED", "0");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "QTYALLOCATED", "0");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "QTYFILLED", "0");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "CREATE_USERID", Globals.curUserID);
                ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "CREATE_DATE", "GETDATE()");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "LAST_UPDATE_USERID", Globals.curUserID);
                ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "LAST_UPDATE_DATE", "GETDATE()");
                Globals.mydmupdate.InsertRecord("PROMO_ITEMS", updStr);
            }
        }

        internal static void UpdatePromoOrder(string promoCode, string siteid, string ascItemId, string orderNum, long lineNum, double orderQty, string orderType, ParseNet.GlobalClass Globals)
        {
            string updStr;
            string wherestr = "WHERE PROMO_CODE='" + promoCode + "' AND SITE_ID='" + siteid + "' " +
                            "AND ASCITEMID='" + ascItemId + "' AND ORDERTYPE='C' " +
                            "AND ORDERNUMBER='" + orderNum + "' AND LINENUMBER=" + lineNum.ToString();
            bool fExists = Globals.myDBUtils.ifRecExists("SELECT PROMO_CODE FROM PROMO_ORDERS (NOLOCK) " + wherestr);
                if (!fExists)
            {
                updStr = string.Empty;
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "PROMO_CODE", promoCode);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "SITE_ID", siteid);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ASCITEMID", ascItemId);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ORDERTYPE", orderType);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ORDERNUMBER", orderNum);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "LINENUMBER", lineNum.ToString());
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "QTYORDERED", orderQty.ToString());
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "QTYFILLED", "0");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "CREATE_USERID", Globals.curUserID);
                ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "CREATE_DATE", "GETDATE()");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "LAST_UPDATE_USERID", Globals.curUserID);
                ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "LAST_UPDATE_DATE", "GETDATE()");
                Globals.mydmupdate.InsertRecord("PROMO_ORDERS", updStr);
            }
            else
            {
                updStr = string.Empty;
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "QTYORDERED", orderQty.ToString());
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "LAST_UPDATE_USERID", Globals.curUserID);
                ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "LAST_UPDATE_DATE", "GETDATE()");
                Globals.mydmupdate.UpdateFields("PROMO_ORDERS", updStr, wherestr);
            }
            UpdatePromoItemQty(promoCode, siteid, ascItemId, orderType, Globals);
        }


        internal static void UpdatePromoItemQty(string promoCode, string siteid, string ascItemId, string orderType, ParseNet.GlobalClass Globals)
        {
            string sqlStr = "PROMO_CODE='" + promoCode + "' AND SITE_ID='" + siteid + "' " +
                "AND ASCITEMID='" + ascItemId + "' AND ORDERTYPE='" + orderType + "'";
            double qtyOrdered = Globals.dmMiscFunc.GetDblSum("PROMO_ORDERS", "QTYORDERED", sqlStr);

            sqlStr = "UPDATE PROMO_ITEMS SET QTY_PROMO=" + qtyOrdered + " " +
                "WHERE PROMO_CODE='" + promoCode + "' AND SITE_ID='" + siteid + "' " +
                "AND ASCITEMID='" + ascItemId + "'";
            Globals.mydmupdate.AddToUpdate(sqlStr);

        }


    }
}
