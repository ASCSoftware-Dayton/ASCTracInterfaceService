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
        private static string FuncID = "POImport";
        private string funcType = "IM_RECV";
        /// <summary>
        /// Import a Purchase Order (Header and Details)
        /// </summary>

        [HttpPost]
        public HttpResponseMessage PostPO(ASCTracInterfaceModel.Model.PO.POHdrImport aData)
        {
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Import/POImport";
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = ASCTracInterfaceDll.Class1.InitParse(baseUrl, funcType, ref errMsg);
                if (myClass == null)
                    statusCode = HttpStatusCode.InternalServerError;
                else
                {
                    myClass.myLogRecord.HttpFunctionID = "Post";
                    myClass.myLogRecord.OrderNum = aData.PONUMBER;
                    myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aData);

                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Imports.ImportPO.doImportPO(myClass, aData, ref errMsg);
                }
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                if (myClass != null)
                    myClass.LogException(ex);
                else
                    LoggingUtil.LogEventView(funcType, aData.PONUMBER, ex.ToString(), ref errMsg);
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
                    resp = ASCResponse.BuildResponse( HttpStatusCode.PreconditionFailed, "Missing Items:" + errMsg.Replace("|", ", ").Trim());
                    retval = Request.CreateResponse<Models.ModelResponse>(HttpStatusCode.OK, resp);
                }
            }
            else
            {
                resp = ASCResponse.BuildResponse(statusCode, errMsg, baseUrl, "Post");
                retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
            }
            //ASCTracInterfaceDll.Class1.LogTransaction(FuncID, aData.PONUMBER, Newtonsoft.Json.JsonConvert.SerializeObject(aData), Newtonsoft.Json.JsonConvert.SerializeObject(resp), fError);
            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(resp);
                myClass.PostLog(statusCode, errMsg);
            }

            return (retval);
        }
        /*
        [HttpPost]
        public HttpResponseMessage PostPOList(List<ASCTracInterfaceModel.Model.PO.POHdrImport> aList)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            string errMsg = string.Empty;
            string errPONum = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings();
                foreach (var rec in aList)
                {
                    errPONum = rec.PONUMBER;
                    string tmpErrMsg = string.Empty;
                    var tmpstatusCode = ASCTracInterfaceDll.Imports.ImportPO.doImportPO(rec, ref tmpErrMsg);

                    if (statusCode == HttpStatusCode.OK)
                        statusCode = tmpstatusCode;

                    if (!String.IsNullOrEmpty(tmpErrMsg))
                        errMsg += "PO: " + tmpErrMsg + "\r\n";
                }
                //statusCode = ASCTracInterfaceDll.Imports.ImportPO.doImportPO(aData, ref errMsg);

            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("PostPO", errPONum, ex.ToString(), ref errMsg);
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
                    var resp = ASCResponse.BuildResponse(HttpStatusCode.PreconditionFailed, "Missing Items:" + errMsg.Replace("|", ", "));
                    retval = Request.CreateResponse<Models.ModelResponse>(HttpStatusCode.PreconditionFailed, resp);
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
        */
    }
}
