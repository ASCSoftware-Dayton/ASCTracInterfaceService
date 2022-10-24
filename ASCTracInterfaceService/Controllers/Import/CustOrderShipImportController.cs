using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Import
{
    public class CustOrderShipImportController : ApiController
    {
        /// <summary>
        /// Import and Process confirm ship of a Customer Order record.
        /// </summary>
        [System.Web.Http.HttpPut]
        public HttpResponseMessage PostCustOrderShip(string aOrderNumber)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings();
                statusCode = ASCTracInterfaceDll.Imports.ImportCustOrder.doImportCustOrderConfirmShip(aOrderNumber, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("PostCustOrderShip", aOrderNumber, ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval;

            if (statusCode == HttpStatusCode.OK)
                retval = Request.CreateResponse(statusCode, errMsg);
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            return (retval);
        }
    }
}
