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
        [HttpGet]
        public HttpResponseMessage GetInventoryAuditRecords(string aVMICustID, string aSiteID, string aItemID)
        {
            List<ASCTracInterfaceModel.Model.Item.InventoryAuditExport> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings();
                statusCode = ASCTracInterfaceDll.Exports.ExportInventoryAudits.DoExportInventoryAudits(aVMICustID, aSiteID, aItemID, ref outdata, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("GetInventoryAuditRecords", aVMICustID + "," + aSiteID + "," + aItemID, ex.ToString(), ref errMsg);
            }
            var retval = new HttpResponseMessage(statusCode);
            if (statusCode == HttpStatusCode.OK)
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(outdata));
            else
                retval.Content = new StringContent(errMsg);
            //var retval = new Models.ModelReturnType(errMsg);
            return (retval);
        }

    }
}
