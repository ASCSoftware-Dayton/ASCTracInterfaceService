using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Export
{
    [Filters.ApiAuthenticationFilter]
    public class CustOrderStatusExportController : ApiController
    {
        /// <summary>
        /// Return list of Customer Orders Status Changes
        /// </summary>
            
        [HttpGet]
        public HttpResponseMessage GetCustOrderStatus(ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter aData)
        {
            List<ASCTracInterfaceModel.Model.CustOrder.CustOrderStatusExport> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings();
                statusCode = ASCTracInterfaceDll.Exports.ExportCustOrderStatus.doExportCustOrderStatus(aData, ref outdata, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("GetCustOrderStatus",aData.CustID, ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
            {
                retval = new HttpResponseMessage(statusCode);
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(outdata));
            }
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            return (retval);
        }

        /// <summary>
        /// Return list of Customer Orders Status Changes for a Customer
        /// </summary>
        [HttpGet]
        public HttpResponseMessage GetCustOrderStatus(string aCustID)
        {
            List<ASCTracInterfaceModel.Model.CustOrder.CustOrderStatusExport> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings();
                ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter aData = new ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter("C", aCustID, string.Empty);
                statusCode = ASCTracInterfaceDll.Exports.ExportCustOrderStatus.doExportCustOrderStatus(aData, ref outdata, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("GetCustOrderStatus", aCustID, ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
            {
                retval = new HttpResponseMessage(statusCode);
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(outdata));
            }
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            return (retval);
        }

        /// <summary>
        /// Update list of Customer Orders Status Changes
        /// </summary>
        [HttpPut]
        public HttpResponseMessage UpdateCustOrderExport(List<ASCTracInterfaceModel.Model.CustOrder.CustOrderStatusExport> aList)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings();
                statusCode = ASCTracInterfaceDll.Exports.ExportCustOrderStatus.updateExportCustOrderStatus(aList, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("UpdateCustOrderExport", aList.Count.ToString(), ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
                retval = Request.CreateResponse(statusCode, errMsg);
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            return (retval);

        }
    }
}

