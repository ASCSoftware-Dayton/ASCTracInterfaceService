using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Export
{
    [Filters.ApiAuthenticationFilter]

    public class InventoryAuditController : ApiController
    {
        private static string FuncID = "InventoryAudit";
        [HttpGet]
        public HttpResponseMessage GetInventoryAuditRecords(string aVMICustID, string aSiteID, string aItemID)
        {
            List<ASCTracInterfaceModel.Model.Item.InventoryAuditExport> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            string errMsg = string.Empty;

            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Export/" + FuncID;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = ASCTracInterfaceDll.Class1.InitParse(baseUrl, "EX_INAUD", ref errMsg);
                myClass.myLogRecord.HttpFunctionID = "Get";
                myClass.myLogRecord.OrderNum = aVMICustID;
                myClass.myLogRecord.ItemID = aItemID;
                myClass.myLogRecord.InData = "aVMICustID=" + aVMICustID + "&aSiteID=" + aSiteID + "&aItemID=" + aItemID;

                try
                {
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Exports.ExportInventoryAudits.DoExportInventoryAudits(myClass, aVMICustID, aSiteID, aItemID, ref outdata, ref errMsg);
                }
                catch (Exception ex)
                {
                    statusCode = HttpStatusCode.BadRequest;
                    errMsg = ex.Message;
                    myClass.LogException(ex);

                }
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView(FuncID, aVMICustID, ex.ToString(), ref errMsg);
            }

            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
            {
                retval = new HttpResponseMessage(statusCode);
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(outdata));
            }
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
                myClass.PostLog(statusCode, errMsg);
            }

            return (retval);
        }

    }
}
