using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.WCS
{
    [Filters.ApiAuthenticationFilter]
    public class WCSPicksController : ApiController
    {
        /// <summary>
        /// Get List of WCS Picks for Order Type.  Content contains list of WCSPicks.
        /// </summary>
        [HttpGet]
        public HttpResponseMessage doExportWCSPicks(string aOrderType)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            var aData = new List<ASCTracInterfaceModel.Model.WCS.WCSPick>();
            string errmsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings();
                statusCode = ASCTracInterfaceDll.WCS.WCSProcess.doWCSPickExport(aOrderType, ref aData, ref errmsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errmsg = "(ExportWCSPick) " + ex.Message;
            }
            var retval = new HttpResponseMessage(statusCode);
            if (statusCode == HttpStatusCode.OK)
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(aData));
            else
                retval.Content = new StringContent(errmsg);

            return (retval);
        }


        /// <summary>
        /// Process a Customer Order Pick Record
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
                statusCode = ASCTracInterfaceDll.WCS.WCSProcess.doWCSPickImport( "C", aData, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = "(PostWCSPick) " +  ex.Message;
                LoggingUtil.LogEventView("PostPick", aData.ORDERNUMBER, ex.ToString(), ref errMsg);
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
