﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Export
{
    [Filters.ApiAuthenticationFilter]
    public class POLinesExportController : ApiController
    {
        private static string FuncID = "POLinesExport";
        /// <summary>
        /// Return list of Receipts by Line
        /// </summary>
        [HttpGet]
        public HttpResponseMessage GetPOLines(ASCTracInterfaceModel.Model.PO.POExportFilter aData)
        {
            List<ASCTracInterfaceModel.Model.PO.POExportLines> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Export/" + FuncID;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = new ASCTracInterfaceDll.Class1();
                ASCTracInterfaceDll.Class1.InitParse(ref myClass, baseUrl, "EX_RECV_LINES", ref errMsg);
                if (myClass == null)
                    statusCode = HttpStatusCode.InternalServerError;
                else
                {
                    myClass.myLogRecord.HttpFunctionID = "Get";
                    myClass.myLogRecord.OrderNum = "";
                    myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Exports.ExportPOLines.doExportPOLines(myClass, aData, ref outdata, ref errMsg);
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
            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
            {
                retval = new HttpResponseMessage(statusCode);
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(outdata));
                if (myClass != null)
                    myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
            }
            else if (statusCode == HttpStatusCode.NoContent)
            {
                errMsg = "No Records Found";
                var resp = ASCResponse.BuildResponse(HttpStatusCode.OK, errMsg);
                retval = Request.CreateResponse<Models.ModelResponse>(HttpStatusCode.OK, resp);
                if (myClass != null)
                    myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(resp);
            }
            else
            {
                retval = Request.CreateErrorResponse(statusCode, errMsg);
                if (myClass != null)
                    myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
            }
            if (myClass != null)
            {
                myClass.PostLog(statusCode, errMsg);
            }

            return (retval);
        }

        /// <summary>
        /// Return list of Receipts by Line  (for Completed Receipts, if parameter is set)
        /// </summary>
        [HttpGet]
        public HttpResponseMessage GetPOLines(bool aOnlySendCompletedReceipt)
        {
            ASCTracInterfaceModel.Model.PO.POExportFilter aData = new ASCTracInterfaceModel.Model.PO.POExportFilter( aOnlySendCompletedReceipt);
            List<ASCTracInterfaceModel.Model.PO.POExportLines> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Export/" + FuncID;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = new ASCTracInterfaceDll.Class1();
                ASCTracInterfaceDll.Class1.InitParse(ref myClass, baseUrl, "EX_RECV_LINES", ref errMsg);
                if (myClass == null)
                    statusCode = HttpStatusCode.InternalServerError;
                else
                {
                    myClass.myLogRecord.HttpFunctionID = "Get";
                    myClass.myLogRecord.OrderNum = "";
                    myClass.myLogRecord.InData = "aOnlySendCompletedReceipt=" + aOnlySendCompletedReceipt.ToString();
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Exports.ExportPOLines.doExportPOLines(myClass, aData, ref outdata, ref errMsg);
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
            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
            {
                retval = new HttpResponseMessage(statusCode);
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(outdata));
                if (myClass != null)
                    myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
            }
            else if (statusCode == HttpStatusCode.NoContent)
            {
                errMsg = "No Records Found";
                var resp = ASCResponse.BuildResponse(HttpStatusCode.OK, errMsg);
                retval = Request.CreateResponse<Models.ModelResponse>(HttpStatusCode.OK, resp);
                if (myClass != null)
                    myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(resp);
            }
            else
            {
                retval = Request.CreateErrorResponse(statusCode, errMsg);
                if (myClass != null)
                    myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
            }
            if (myClass != null)
            {
                myClass.PostLog(statusCode, errMsg);
            }
            return (retval);
        }

        /// <summary>
        /// Update list of Receipts by Line to Processed
        /// </summary>
        [HttpPut]
        public HttpResponseMessage UpdatePOExport( List <ASCTracInterfaceModel.Model.PO.POExportLines> aList)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Export/" + FuncID;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = new ASCTracInterfaceDll.Class1();
                ASCTracInterfaceDll.Class1.InitParse(ref myClass, baseUrl, "EX_RECV_LINES", ref errMsg);
                if (myClass == null)
                    statusCode = HttpStatusCode.InternalServerError;
                else
                {
                    myClass.myLogRecord.HttpFunctionID = "Put";
                    myClass.myLogRecord.OrderNum = "";
                    myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aList);
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Exports.ExportPOLines.updateExportPOLines(myClass, aList, ref errMsg);
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