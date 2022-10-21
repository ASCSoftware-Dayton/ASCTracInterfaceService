using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.WCS
{
    [Filters.ApiAuthenticationFilter]
    public class WCSUnpickController : ApiController
    {
        /// <summary>
        /// Process a UnPick of a Customer Order Line
        /// </summary>
        /// <param name="data">The data to be imported.</param>
        [HttpPost]
        public HttpResponseMessage PostPick(ASCTracInterfaceModel.Model.WCS.WCSPick aData)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings();
                statusCode = ASCTracInterfaceDll.WCS.WCSProcess.doWCSPickImport("N", aData, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("PostUnPick", aData.ORDERNUMBER, ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval;

            if (statusCode == HttpStatusCode.Accepted)
                retval = Request.CreateResponse(statusCode, errMsg);
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            return (retval);
        }

    }
}

