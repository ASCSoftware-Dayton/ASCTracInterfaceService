using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Export
{
    [Filters.ApiAuthenticationFilter]
    public class CustOrderExportController : ApiController
    {
        /// <summary>
        /// Return list of unprocessed Customer Orders Pick Records
        /// </summary>
        [HttpGet]
        public HttpResponseMessage GetCustOrderPicks(ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter aData)
        {
            List<ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                statusCode = ASCTracInterfaceDll.Exports.ExportCustOrder.doExportCustOrders(aData, ref outdata, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
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
        /// Return list of unprocessed Customer Orders Pick Records for a Customer
        /// </summary>
        [HttpGet]
        public HttpResponseMessage GetCustOrderPicks(string aCustID)
        {
            List<ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter aData = new ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter("C", aCustID, string.Empty);
                statusCode = ASCTracInterfaceDll.Exports.ExportCustOrder.doExportCustOrders(aData, ref outdata, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
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
        /// Update list of Customer Orders Pick Records to Processed 
        /// </summary>

        [HttpPut]
        public HttpResponseMessage UpdateCustOrderExport(List<ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport> aList)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                statusCode = ASCTracInterfaceDll.Exports.ExportCustOrder.updateExportCustOrder(aList, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
            }
            var retval = new HttpResponseMessage(statusCode);
            retval.Content = new StringContent(errMsg);
            //var retval = new Models.ModelReturnType(errMsg);
            return (retval);

        }
    }
}