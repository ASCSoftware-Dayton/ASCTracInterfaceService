using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Import
{
    [Filters.ApiAuthenticationFilter]
    public class CustOrderImportController : ApiController
    {
        /// <summary>
        /// Import a Customer Order (Header and Details)
        /// </summary>
        [System.Web.Http.HttpPost]
        public HttpResponseMessage PostCustOrder(ASCTracInterfaceModel.Model.CustOrder.OrdrHdrImport aData)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            //var SessionID = System.Web.HttpContext.Current.Session.SessionID;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings();
                statusCode = ASCTracInterfaceDll.Imports.ImportCustOrder.doImportCustOrder(aData, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("PostCustOrder", aData.ORDERNUMBER, ex.ToString(), ref errMsg);
            }
            var retval = new HttpResponseMessage(statusCode);
            retval.Content = new StringContent(errMsg);
            //var retval = new Models.ModelReturnType(errMsg);
            return (retval);
        }
    }
}
