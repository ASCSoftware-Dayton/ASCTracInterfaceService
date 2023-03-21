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
        private static string FuncID = "CustOrderImport";
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
                ReadMyAppSettings.ReadAppSettings(FuncID);
                statusCode = ASCTracInterfaceDll.Imports.ImportCustOrder.doImportCustOrder(aData, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("PostCustOrder", aData.ORDERNUMBER, ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval; // = ASCResponse.BuildResponse( statusCode, errMsg);
            bool fError = false;

            Models.ModelResponse resp;
            if (statusCode == HttpStatusCode.OK)
            {
                if (String.IsNullOrEmpty(errMsg))
                {
                    resp = ASCResponse.BuildResponse(statusCode, null);
                    retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
                }
                else
                {
                    fError = true;
                    resp = ASCResponse.BuildResponse(HttpStatusCode.PreconditionFailed, "Missing Items:" + errMsg.Replace("|", ", ").Trim());
                    retval = Request.CreateResponse<Models.ModelResponse>(HttpStatusCode.OK, resp);
                }
            }
            else
            {
                fError = true;
                var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority);

                resp = ASCResponse.BuildResponse(statusCode, errMsg, baseUrl + "/Import/CustOrderImport", "Post");
                retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
            }
            ASCTracInterfaceDll.Class1.LogTransaction(FuncID, aData.ORDERNUMBER, Newtonsoft.Json.JsonConvert.SerializeObject(aData), Newtonsoft.Json.JsonConvert.SerializeObject(resp), fError);
            return (retval);
        }
    }
}
