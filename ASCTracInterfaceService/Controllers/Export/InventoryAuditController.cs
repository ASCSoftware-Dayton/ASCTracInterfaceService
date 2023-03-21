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
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                statusCode = ASCTracInterfaceDll.Exports.ExportInventoryAudits.DoExportInventoryAudits(aVMICustID, aSiteID, aItemID, ref outdata, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("GetInventoryAuditRecords", aVMICustID + "," + aSiteID + "," + aItemID, ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
            {
                retval = new HttpResponseMessage(statusCode);
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(outdata));
            }
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            ASCTracInterfaceDll.Class1.LogTransaction(FuncID, "", aVMICustID + "," + aSiteID + "," + aItemID, Newtonsoft.Json.JsonConvert.SerializeObject(retval), statusCode != HttpStatusCode.OK);

            return (retval);
        }

    }
}
