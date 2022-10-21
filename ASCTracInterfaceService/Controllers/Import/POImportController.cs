using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Import
{
    [Filters.ApiAuthenticationFilter]
    public class POImportController : ApiController
    {
        /// <summary>
        /// Import a Purchase Order (Header and Details)
        /// </summary>
        [HttpPost]
        public HttpResponseMessage PostPO(ASCTracInterfaceModel.Model.PO.POHdrImport aData)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings();
                statusCode = ASCTracInterfaceDll.Imports.ImportPO.doImportPO(aData, ref errMsg);
                //statusCode = ASCTracInterfaceDll.Imports.ImportPO.doImportPO(aData, ref errMsg);

            }
            catch( Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("PostPO", aData.PONUMBER, ex.ToString(), ref errMsg);
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
