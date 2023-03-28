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
        private static string FuncID = "WCSPicks";

        /// <summary>
        /// Get List of WCS Picks for Order Type.  Content contains list of WCSPicks.
        /// </summary>
        [HttpGet]
        public HttpResponseMessage doExportWCSPicks(string aOrderType)
        {
            var aData = new List<ASCTracInterfaceModel.Model.WCS.WCSPick>();
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/WCS/" + FuncID;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = ASCTracInterfaceDll.Class1.InitParse(baseUrl, FuncID, ref errMsg);
                myClass.myLogRecord.HttpFunctionID = "Get";
                myClass.myLogRecord.OrderNum = aOrderType;
                myClass.myLogRecord.InData = "aOrderType=" + aOrderType;

                try
                {
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.WCS.WCSProcess.doWCSPickExport(myClass, aOrderType, ref aData, ref errMsg);
                }
                catch (Exception ex)
                {
                    myClass.LogException(ex);
                    statusCode = HttpStatusCode.BadRequest;
                    errMsg = "(ExportWCSPick) " + ex.Message;
                }
            }
            catch( Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView(FuncID, aOrderType, ex.ToString(), ref errMsg);
            }
            var retval = new HttpResponseMessage(statusCode);
            if (statusCode == HttpStatusCode.OK)
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(aData));
            else
                retval.Content = new StringContent(errMsg);
            // ASCTracInterfaceDll.Class1.LogTransaction(FuncID, "", aOrderType, Newtonsoft.Json.JsonConvert.SerializeObject(retval), statusCode != HttpStatusCode.OK);
            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
                myClass.PostLog(statusCode, errMsg);
            }

            return (retval);
        }


        /// <summary>
        /// Process a Customer Order Pick Record
        /// </summary>
        /// <param name="data">The data to be imported.</param>
        [HttpPost]
        public HttpResponseMessage PostPick(ASCTracInterfaceModel.Model.WCS.WCSPick aData)
        {
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/WCS/" + FuncID;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = ASCTracInterfaceDll.Class1.InitParse(baseUrl, FuncID, ref errMsg);
                myClass.myLogRecord.HttpFunctionID = "Post";
                myClass.myLogRecord.OrderNum = aData.ORDERNUMBER;
                myClass.myLogRecord.ItemID = aData.ITEMID;
                myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aData);

                try
                {
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.WCS.WCSProcess.doWCSPickImport( myClass, "C", aData, ref errMsg);
                }
                catch (Exception ex)
                {
                    myClass.LogException(ex);
                    statusCode = HttpStatusCode.BadRequest;
                    errMsg = "(PostWCSPick) " + ex.Message;
                    //LoggingUtil.LogEventView("PostPick", aData.ORDERNUMBER, ex.ToString(), ref errMsg);
                }
            }
            catch( Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView(FuncID, aData.ORDERNUMBER, ex.ToString(), ref errMsg);
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
                resp = ASCResponse.BuildResponse(statusCode, errMsg);
                retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
            }
            //ASCTracInterfaceDll.Class1.LogTransaction(FuncID, aData.ORDERNUMBER, Newtonsoft.Json.JsonConvert.SerializeObject(aData), Newtonsoft.Json.JsonConvert.SerializeObject(resp), statusCode != HttpStatusCode.OK);
            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
                myClass.PostLog(statusCode, errMsg);
            }

            return (retval);
        }

    }
}
