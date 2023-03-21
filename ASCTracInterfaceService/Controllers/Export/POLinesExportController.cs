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
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                statusCode = ASCTracInterfaceDll.Exports.ExportPOLines.doExportPOLines(aData, ref outdata, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("GetPOLines", aData.OnlySendCompletedReceipts.ToString(), ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
            {
                retval = new HttpResponseMessage(statusCode);
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(outdata));
            }
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            ASCTracInterfaceDll.Class1.LogTransaction(FuncID, "", Newtonsoft.Json.JsonConvert.SerializeObject(aData), Newtonsoft.Json.JsonConvert.SerializeObject(retval), statusCode != HttpStatusCode.OK);

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
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                statusCode = ASCTracInterfaceDll.Exports.ExportPOLines.doExportPOLines(aData, ref outdata, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("GetPOLines", aOnlySendCompletedReceipt.ToString(), ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
            {
                retval = new HttpResponseMessage(statusCode);
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(outdata));
            }
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            ASCTracInterfaceDll.Class1.LogTransaction(FuncID, "", aOnlySendCompletedReceipt.ToString(), Newtonsoft.Json.JsonConvert.SerializeObject(retval), statusCode != HttpStatusCode.OK);

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
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                statusCode = ASCTracInterfaceDll.Exports.ExportPOLines.updateExportPOLines(aList, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("UpdatePOExport", aList.Count.ToString(), ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
                retval = Request.CreateResponse(statusCode, errMsg);
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            ASCTracInterfaceDll.Class1.LogTransaction(FuncID, "", Newtonsoft.Json.JsonConvert.SerializeObject(aList), Newtonsoft.Json.JsonConvert.SerializeObject(retval), statusCode != HttpStatusCode.OK);

            return (retval);
        }
    }
}