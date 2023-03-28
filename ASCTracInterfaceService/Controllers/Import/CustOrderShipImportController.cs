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
        private static string FuncID = "CustOrderShipImport";
        private static string funcType = "CONFSHIP";
        /// <summary>
        /// Import and Process confirm ship of a Customer Order record.
        /// </summary>
        [System.Web.Http.HttpPut]
        public HttpResponseMessage PostCustOrderShip(string aOrderNumber)
        {
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Import/CustOrderShipImport";
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = ASCTracInterfaceDll.Class1.InitParse(baseUrl, funcType, ref errMsg);
                myClass.myLogRecord.HttpFunctionID = "Post";
                myClass.myLogRecord.OrderNum = aOrderNumber;
                myClass.myLogRecord.InData = aOrderNumber;
                try
                {
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Imports.ImportCustOrder.doImportCustOrderConfirmShip( myClass, aOrderNumber, ref errMsg);

                }
                catch (Exception ex)
                {
                    myClass.myLogRecord.LogType = "X";
                    myClass.myLogRecord.StackTrace = ex.StackTrace;
                    myClass.myLogRecord.OutData = ex.Message;
                    statusCode = HttpStatusCode.BadRequest;
                    errMsg = ex.Message;
                    //LoggingUtil.LogEventView("PostCustOrder", aData.ORDERNUMBER, ex.ToString(), ref errMsg);
                }
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView(funcType, aOrderNumber, ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval; // = ASCResponse.BuildResponse( statusCode, errMsg);
            Models.ModelResponse resp;
            if (statusCode == HttpStatusCode.OK)
            {
                resp = ASCResponse.BuildResponse(statusCode, null);
                retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
                //retval = Request.CreateResponse(statusCode, errMsg);
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

            /*

            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                statusCode = ASCTracInterfaceDll.Imports.ImportCustOrder.doImportCustOrderConfirmShip(aOrderNumber, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("PostCustOrderShip", aOrderNumber, ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval; // = ASCResponse.BuildResponse( statusCode, errMsg);
            Models.ModelResponse resp;
            if (statusCode == HttpStatusCode.OK)
            {
                resp = ASCResponse.BuildResponse(statusCode, null);
                retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
                //retval = Request.CreateResponse(statusCode, errMsg);
            }
            else
            {
                resp = ASCResponse.BuildResponse(statusCode, errMsg, Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Import/CustOrderShipImport", "Post");
                retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
            }
            ASCTracInterfaceDll.Class1.LogTransaction(FuncID, aOrderNumber, aOrderNumber, Newtonsoft.Json.JsonConvert.SerializeObject(resp), statusCode != HttpStatusCode.OK);
            
            return (retval);
            */
        }
    }
}
