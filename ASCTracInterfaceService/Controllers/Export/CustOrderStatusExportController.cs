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
        private static string FuncID = "CustOrderStatusExport";
        /// <summary>
        /// Return list of Customer Orders Status Changes
        /// </summary>

        [HttpGet]
        public HttpResponseMessage GetCustOrderStatus(ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter aData)
        {
            List<ASCTracInterfaceModel.Model.CustOrder.CustOrderStatusExport> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;

            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Export/" + FuncID;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = ASCTracInterfaceDll.Class1.InitParse(baseUrl, "EX_ORDER_STATUS", ref errMsg);
                myClass.myLogRecord.HttpFunctionID = "Get";
                myClass.myLogRecord.OrderNum = aData.CustID;
                myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aData);

                try
                {
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Exports.ExportCustOrderStatus.doExportCustOrderStatus(myClass, aData, ref outdata, ref errMsg);
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
        /// Return list of Customer Orders Status Changes for a Customer
        /// </summary>
        [HttpGet]
        public HttpResponseMessage GetCustOrderStatus(string aCustID)
        {
            List<ASCTracInterfaceModel.Model.CustOrder.CustOrderStatusExport> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Export/" + FuncID;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = ASCTracInterfaceDll.Class1.InitParse(baseUrl, "EX_ORDER_STATUS", ref errMsg);
                myClass.myLogRecord.HttpFunctionID = "Get";
                myClass.myLogRecord.OrderNum = aCustID;
                myClass.myLogRecord.InData = "aCustID=" + aCustID;
                try
                {
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter aData = new ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter("C", aCustID, string.Empty);
                    statusCode = ASCTracInterfaceDll.Exports.ExportCustOrderStatus.doExportCustOrderStatus(myClass, aData, ref outdata, ref errMsg);
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
                LoggingUtil.LogEventView(FuncID, aCustID, ex.ToString(), ref errMsg);
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
        /// Update list of Customer Orders Status Changes
        /// </summary>
        [HttpPut]
        public HttpResponseMessage UpdateCustOrderExport(List<ASCTracInterfaceModel.Model.CustOrder.CustOrderStatusExport> aList)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Export/" + FuncID;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = ASCTracInterfaceDll.Class1.InitParse(baseUrl, "EX_ORDER_STATUS", ref errMsg);
                myClass.myLogRecord.HttpFunctionID = "Put";
                //myClass.myLogRecord.OrderNum = aCustID;
                myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aList);
                try
                {
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Exports.ExportCustOrderStatus.updateExportCustOrderStatus(myClass, aList, ref errMsg);
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
                LoggingUtil.LogEventView(FuncID, "", ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
                retval = Request.CreateResponse(statusCode, errMsg);
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
                myClass.PostLog(statusCode, errMsg);
            }

            return (retval);

        }
    }
}