using System;
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
                ReadMyAppSettings.ReadAppSettings();
                statusCode = ASCTracInterfaceDll.Exports.ExportPOLines.doExportPOLines(aData, ref outdata, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("GetPOLines", aData.OnlySendCompletedReceipts.ToString(), ex.ToString(), ref errMsg);
            }
            var retval = new HttpResponseMessage(statusCode);
            if (statusCode == HttpStatusCode.OK)
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(outdata));
            else
                retval.Content = new StringContent(errMsg);
            //var retval = new Models.ModelReturnType(errMsg);
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
                ReadMyAppSettings.ReadAppSettings();
                statusCode = ASCTracInterfaceDll.Exports.ExportPOLines.doExportPOLines(aData, ref outdata, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("GetPOLines", aOnlySendCompletedReceipt.ToString(), ex.ToString(), ref errMsg);
            }
            var retval = new HttpResponseMessage(statusCode);
            if (statusCode == HttpStatusCode.OK)
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(outdata));
            else
                retval.Content = new StringContent(errMsg);
            //var retval = new Models.ModelReturnType(errMsg);
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
                ReadMyAppSettings.ReadAppSettings();
                statusCode = ASCTracInterfaceDll.Exports.ExportPOLines.updateExportPOLines(aList, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("UpdatePOExport", aList.Count.ToString(), ex.ToString(), ref errMsg);
            }
            var retval = new HttpResponseMessage(statusCode);
                retval.Content = new StringContent(errMsg);
            //var retval = new Models.ModelReturnType(errMsg);
            return (retval);

        }
    }
}