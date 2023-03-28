using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.WCS
{
    [Filters.ApiAuthenticationFilter]
    public class WCSUnpickController : ApiController
    {
        private static string FuncID = "WCSUnpick";

        /// <summary>
        /// Process a UnPick of a Customer Order Line
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
                    statusCode = ASCTracInterfaceDll.WCS.WCSProcess.doWCSPickImport(myClass, "N", aData, ref errMsg);
                }
                catch (Exception ex)
                {
                    statusCode = HttpStatusCode.BadRequest;
                    errMsg = ex.Message;
                    myClass.LogException(ex);
                }
            }
            catch (Exception ex)
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

            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
                myClass.PostLog(statusCode, errMsg);
            }
            return (retval);
        }
    }
}

