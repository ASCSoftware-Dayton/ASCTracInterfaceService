using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Import
{
    [Filters.ApiAuthenticationFilter]
    public class ControlledCountController : ApiController
    {
        /// <summary>
        /// Import a Controlled Count(Header and Details)
        /// </summary>
        [System.Web.Http.HttpPost]
        public HttpResponseMessage PostCount(ASCTracInterfaceModel.Model.Count.ModelCountHeader aData)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings();
                statusCode = ASCTracInterfaceDll.Imports.ImportControlledCount.doImportControlledCount(aData, ref errMsg);
                //statusCode = ASCTracInterfaceDll.Imports.ImportPO.doImportPO(aData, ref errMsg);

            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("PostCount", aData.COUNTID.ToString(), ex.ToString(), ref errMsg);
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
