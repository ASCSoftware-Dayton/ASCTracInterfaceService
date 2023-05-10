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
        public static readonly object LockObject = new object();

        /// <summary>
        /// Get List of WCS Picks for Order Type.  Content contains list of WCSPicks.
        /// </summary>
        [HttpGet]
        public HttpResponseMessage doExportWCSPicks(string aOrderType)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            var retval = new HttpResponseMessage(statusCode);
            var aData = new List<ASCTracInterfaceModel.Model.WCS.WCSPick>();
            string errMsg = string.Empty;
            lock (LockObject)
            {
                var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/WCS/" + FuncID;
                ASCTracInterfaceDll.Class1 myClass = null;
                try
                {
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    myClass = new ASCTracInterfaceDll.Class1();
                    ASCTracInterfaceDll.Class1.InitParse(myClass, baseUrl, FuncID, ref errMsg);
                    if (myClass == null)
                        statusCode = HttpStatusCode.InternalServerError;
                    else
                    {
                        myClass.myLogRecord.HttpFunctionID = "Get";
                        myClass.myLogRecord.OrderNum = aOrderType;
                        myClass.myLogRecord.InData = "aOrderType=" + aOrderType;

                        ReadMyAppSettings.ReadAppSettings(FuncID);
                        statusCode = ASCTracInterfaceDll.WCS.WCSProcess.doWCSPickExport(myClass, aOrderType, ref aData, ref errMsg);
                    }
                }
                catch (Exception ex)
                {
                    statusCode = HttpStatusCode.BadRequest;
                    errMsg = ex.Message;
                    if (myClass != null)
                        myClass.LogException(ex);
                    else
                        LoggingUtil.LogEventView(FuncID, aOrderType, ex.ToString(), ref errMsg);
                }
                retval = new HttpResponseMessage(statusCode);
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
            Models.ModelResponse resp;
            lock (LockObject)
            {
                ASCTracInterfaceDll.Class1 myClass = null;
                try
                {
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    myClass = new ASCTracInterfaceDll.Class1();
                    ASCTracInterfaceDll.Class1.InitParse(myClass, baseUrl, FuncID, ref errMsg);
                    if (myClass == null)
                        statusCode = HttpStatusCode.InternalServerError;
                    else
                    {
                        myClass.myLogRecord.HttpFunctionID = "Post";
                        myClass.myLogRecord.OrderNum = aData.ORDERNUMBER;
                        myClass.myLogRecord.ItemID = aData.ITEMID;
                        myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aData);

                        ReadMyAppSettings.ReadAppSettings(FuncID);
                        statusCode = ASCTracInterfaceDll.WCS.WCSProcess.doWCSPickImport(myClass, "C", aData, ref errMsg);
                    }
                }
                catch (Exception ex)
                {
                    statusCode = HttpStatusCode.BadRequest;
                    errMsg = ex.Message;
                    if (myClass != null)
                        myClass.LogException(ex);
                    else
                        LoggingUtil.LogEventView(FuncID, aData.ORDERNUMBER, ex.ToString(), ref errMsg);
                }
                if (statusCode == HttpStatusCode.OK)
                {
                    resp = ASCResponse.BuildResponse(statusCode, null);
                }
                else
                {
                    resp = ASCResponse.BuildResponse(statusCode, errMsg);
                   
                }
                //ASCTracInterfaceDll.Class1.LogTransaction(FuncID, aData.ORDERNUMBER, Newtonsoft.Json.JsonConvert.SerializeObject(aData), Newtonsoft.Json.JsonConvert.SerializeObject(resp), statusCode != HttpStatusCode.OK);
                if (myClass != null)
                {
                    myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(resp);
                    myClass.PostLog(statusCode, errMsg);
                }
            }
            HttpResponseMessage retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);

            return (retval);
        }

    }
}
