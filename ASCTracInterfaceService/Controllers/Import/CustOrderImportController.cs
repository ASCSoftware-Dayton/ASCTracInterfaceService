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
        private static string funcType = "IM_ORDER";
        /// <summary>
        /// Import a Customer Order (Header and Details)
        /// </summary>
        [System.Web.Http.HttpPost]
        public HttpResponseMessage PostCustOrder(ASCTracInterfaceModel.Model.CustOrder.OrdrHdrImport aData)
        {
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Import/CustOrderImport";
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = ASCTracInterfaceDll.Class1.InitParse(baseUrl, funcType, ref errMsg);
                myClass.myLogRecord.HttpFunctionID = "Post";
                myClass.myLogRecord.OrderNum = aData.ORDERNUMBER;
                myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
                try
                {
                    statusCode = ASCTracInterfaceDll.Imports.ImportCustOrder.doImportCustOrder( myClass, aData, ref errMsg);
                }
                catch (Exception ex)
                {
                    myClass.LogException(ex);
                    statusCode = HttpStatusCode.BadRequest;
                    errMsg = ex.Message;
                    //LoggingUtil.LogEventView("PostCustOrder", aData.ORDERNUMBER, ex.ToString(), ref errMsg);
                }
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView(funcType, aData.ORDERNUMBER, ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval; // = ASCResponse.BuildResponse( statusCode, errMsg);
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
                    resp = ASCResponse.BuildResponse(HttpStatusCode.PreconditionFailed, "Missing Items:" + errMsg.Replace("|", ", ").Trim());
                    retval = Request.CreateResponse<Models.ModelResponse>(HttpStatusCode.OK, resp);
                }
            }
            else
            {
                resp = ASCResponse.BuildResponse(statusCode, errMsg, baseUrl, "Post");
                retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
            }
            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(resp);
                myClass.PostLog(statusCode, errMsg);
            }
            //ASCTracInterfaceDll.Class1.LogTransaction(FuncID, aData.ORDERNUMBER, , , fError);
            return (retval);
        }
    }
}
