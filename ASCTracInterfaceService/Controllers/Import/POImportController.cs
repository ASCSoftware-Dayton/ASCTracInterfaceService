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
            HttpResponseMessage retval; // = ASCResponse.BuildResponse( statusCode, errMsg);

            if (statusCode == HttpStatusCode.OK)
            {
                if (String.IsNullOrEmpty(errMsg))
                {
                    var resp = ASCResponse.BuildResponse(statusCode, null);
                    retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
                }
                else
                {
                    //var resp = ASCResponse.BuildMissingItemsResponse(statusCode, errMsg);
                    var resp = ASCResponse.BuildResponse(statusCode, "Missing Items:" + errMsg.Replace( "|", ", "));
                    retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
                }
                //retval = Request.CreateResponse(statusCode, errMsg);
            }
            else
            {
                var resp = ASCResponse.BuildResponse(statusCode, errMsg, Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Import/POImport", "Post");
                retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
            }
            return (retval);

        }
    }
}
