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
        private static string FuncID = "CustOrderExport";
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
                ReadMyAppSettings.ReadAppSettings(FuncID);
                statusCode = ASCTracInterfaceDll.Exports.ExportCustOrder.doExportCustOrders(aData, ref outdata, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("GetCustOrderPicks", aData.CustID, ex.ToString(), ref errMsg);
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
                ReadMyAppSettings.ReadAppSettings(FuncID);
                ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter aData = new ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter("C", aCustID, string.Empty);
                statusCode = ASCTracInterfaceDll.Exports.ExportCustOrder.doExportCustOrders(aData, ref outdata, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("GetCustOrderPicks", aCustID, ex.ToString(), ref errMsg);
            }

            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
            {
                retval = new HttpResponseMessage(statusCode);
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(outdata));
            }
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            ASCTracInterfaceDll.Class1.LogTransaction(FuncID, "", aCustID, Newtonsoft.Json.JsonConvert.SerializeObject(retval), statusCode != HttpStatusCode.OK);

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
                ReadMyAppSettings.ReadAppSettings(FuncID);
                statusCode = ASCTracInterfaceDll.Exports.ExportCustOrder.updateExportCustOrder(aList, ref errMsg);
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
            ASCTracInterfaceDll.Class1.LogTransaction(FuncID, "", Newtonsoft.Json.JsonConvert.SerializeObject(aList), Newtonsoft.Json.JsonConvert.SerializeObject(retval), statusCode != HttpStatusCode.OK);

            return (retval);

        }
    }
}