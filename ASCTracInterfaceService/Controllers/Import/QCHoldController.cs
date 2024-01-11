using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Import
{
    public class QCHoldController : ApiController
    {
        private static string FuncID = "QCImport";
        private string funcType = "IM_QCHOLDS";

        /// <summary>
        /// Import a QC Hold Record of structure ASCTracInterfaceModel.Model.QC.QCHoldImport 
        /// </summary>
        /// <param name="data">The data to be imported.</param>
        [HttpPost]
        public HttpResponseMessage PostVendor(ASCTracInterfaceModel.Model.QC.QCHoldImport aData)
        {
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Import/QCHoldImport";
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string TranID = string.Empty;
            if (!String.IsNullOrEmpty(aData.License_ID))
                TranID = aData.License_ID;
            else if (!String.IsNullOrEmpty(aData.Host_String_ID))
                TranID = aData.Host_String_ID;
            ASCTracInterfaceDll.Class1 myClass = null;
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
                    myClass.myLogRecord.OrderNum = TranID;
                    myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
                    ReadMyAppSettings.ReadAppSettings(FuncID);

                    statusCode = ASCTracInterfaceDll.Imports.ImportQCHold.doImportQCHold(myClass, aData, ref errMsg);
                }
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                if (myClass != null)
                    myClass.LogException(ex);
                else
                    LoggingUtil.LogEventView(funcType, TranID, ex.ToString(), ref errMsg);
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
            else
                LoggingUtil.LogEventView(funcType, TranID, errMsg, ref errMsg);

            return (retval);
        }
    }
}