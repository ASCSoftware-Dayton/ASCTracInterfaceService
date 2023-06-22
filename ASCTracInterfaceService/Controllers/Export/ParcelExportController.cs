using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Export
{
    [Filters.ApiAuthenticationFilter]
    public class ParcelExportController : ApiController
    {
        private static string FuncID = "ParcelExport";

        /// <summary>
        /// Return list of Parcel Records
        /// </summary>
        [HttpGet]
        public HttpResponseMessage GetParcelTransactions(ASCTracInterfaceModel.Model.CustOrder.ParcelExporFilter aData)
        {
            List<ASCTracInterfaceModel.Model.CustOrder.ParcelExport> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Export/" + FuncID;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = new ASCTracInterfaceDll.Class1();
                ASCTracInterfaceDll.Class1.InitParse(myClass, baseUrl, "EX_PARC", ref errMsg);
                if (myClass == null)
                    statusCode = HttpStatusCode.InternalServerError;
                else
                {
                    myClass.myLogRecord.HttpFunctionID = "Get";
                    myClass.myLogRecord.OrderNum = aData.CustID;
                    myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aData);

                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Exports.ExportParcel.doExportParcel(myClass, aData, ref outdata, ref errMsg);
                }
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                if (myClass != null)
                    myClass.LogException(ex);
                else
                    LoggingUtil.LogEventView(FuncID, aData.CustID, ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
            {
                retval = new HttpResponseMessage(statusCode);
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(outdata));
            }
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
                myClass.PostLog(statusCode, errMsg);
            }
            return (retval);
        }


        /// <summary>
        /// Return list of Parcel Records for a Customer
        /// </summary>
        [HttpGet]
        public HttpResponseMessage GetParcelTransactions(string aCustID)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            ASCTracInterfaceModel.Model.CustOrder.ParcelExporFilter aData = new ASCTracInterfaceModel.Model.CustOrder.ParcelExporFilter( aCustID, "");
            List<ASCTracInterfaceModel.Model.CustOrder.ParcelExport> outdata = null;
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Export/" + FuncID;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = new ASCTracInterfaceDll.Class1();
                ASCTracInterfaceDll.Class1.InitParse(myClass, baseUrl, "EX_PARC", ref errMsg);
                if (myClass == null)
                    statusCode = HttpStatusCode.InternalServerError;
                else
                {

                    myClass.myLogRecord.HttpFunctionID = "Get";
                    myClass.myLogRecord.OrderNum = aCustID;
                    myClass.myLogRecord.InData = "aCustID=" + aCustID;

                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Exports.ExportParcel.doExportParcel(myClass, aData, ref outdata, ref errMsg);
                }
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                if (myClass != null)
                    myClass.LogException(ex);
                else
                    LoggingUtil.LogEventView(FuncID, aData.CustID, ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
            {
                retval = new HttpResponseMessage(statusCode);
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(outdata));
            }
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
                myClass.PostLog(statusCode, errMsg);
            }
            return (retval);
        }

        /// <summary>
        /// Update list of Parcel Records to Processed
        /// </summary>
        [HttpPut]
        public HttpResponseMessage UpdateParcelExport(List<ASCTracInterfaceModel.Model.CustOrder.ParcelExport> aList)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Export/" + FuncID;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = new ASCTracInterfaceDll.Class1();
                ASCTracInterfaceDll.Class1.InitParse(myClass, baseUrl, "EX_PARC", ref errMsg);
                if (myClass == null)
                    statusCode = HttpStatusCode.InternalServerError;
                else
                {
                    myClass.myLogRecord.HttpFunctionID = "Put";
                    myClass.myLogRecord.OrderNum = "";
                    myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aList);

                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Exports.ExportParcel.UpdateExport(myClass, aList, ref errMsg);
                }
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                if (myClass != null)
                    myClass.LogException(ex);
                else
                    LoggingUtil.LogEventView(FuncID, "", ex.ToString(), ref errMsg);
            }
            Models.ModelResponse resp;
            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
                resp = ASCResponse.BuildResponse(statusCode, null);
            else
                resp = ASCResponse.BuildResponse(statusCode, errMsg);
            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(resp);
                myClass.PostLog(statusCode, errMsg);
            }
            retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp); 

            return (retval);

        }

    }
}
