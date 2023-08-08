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
        public static readonly object LockObject = new object();

        private static string FuncID = "CustOrderImport";
        private static string funcType = "IM_ORDER";
        /// <summary>
        /// Import a Customer Order (Header and Details)
        /// </summary>
        [System.Web.Http.HttpPost]
        public HttpResponseMessage PostCustOrder(ASCTracInterfaceModel.Model.CustOrder.OrdrHdrImport aData)
        {
            HttpResponseMessage retval; // = ASCResponse.BuildResponse( statusCode, errMsg);
            HttpStatusCode retvalstatusCode = HttpStatusCode.Accepted;
            Models.ModelResponse resp;
            lock (LockObject)
            {
                string errMsg = string.Empty;
                HttpStatusCode statusCode = HttpStatusCode.Accepted;
                ASCTracInterfaceDll.Class1 myClass = null;
                var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Import/CustOrderImport";
                try
                {
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    myClass = new ASCTracInterfaceDll.Class1();
                    ASCTracInterfaceDll.Class1.InitParse(ref myClass, baseUrl, funcType, ref errMsg);
                    if (myClass == null)
                        statusCode = HttpStatusCode.InternalServerError;
                    else
                    {
                        myClass.myLogRecord.HttpFunctionID = "Post";
                        myClass.myLogRecord.OrderNum = aData.ORDERNUMBER;
                        myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
                        statusCode = ASCTracInterfaceDll.Imports.ImportCustOrder.doImportCustOrder(myClass, aData, ref errMsg);
                    }
                }
                catch (Exception ex)
                {
                    statusCode = HttpStatusCode.BadRequest;
                    errMsg = ex.Message;
                    if (myClass != null)
                        myClass.LogException(ex);
                    else
                        LoggingUtil.LogEventView(funcType, aData.ORDERNUMBER, ex.ToString(), ref errMsg);
                }
                retvalstatusCode = statusCode;
                if (statusCode == HttpStatusCode.OK)
                {
                    if (String.IsNullOrEmpty(errMsg))
                    {
                        resp = ASCResponse.BuildResponse(statusCode, null);
                    }
                    else
                    {
                        errMsg = "Customer Order: " + aData.ORDERNUMBER + ", Missing Items: " + errMsg.Replace("|", ", ").Trim();
                        resp = ASCResponse.BuildResponse(HttpStatusCode.PreconditionFailed, errMsg);
                    }
                }
                else
                {
                    resp = ASCResponse.BuildResponse(statusCode, errMsg, baseUrl, "Post");
                }
                if (myClass != null)
                {
                    myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(resp);
                    myClass.PostLog(statusCode, errMsg);
                }
                else
                    LoggingUtil.LogEventView(funcType, aData.ORDERNUMBER, errMsg, ref errMsg);
            }
            retval = Request.CreateResponse<Models.ModelResponse>(retvalstatusCode, resp);

            //ASCTracInterfaceDll.Class1.LogTransaction(FuncID, aData.ORDERNUMBER, , , fError);
            return (retval);
        }
    }
}
