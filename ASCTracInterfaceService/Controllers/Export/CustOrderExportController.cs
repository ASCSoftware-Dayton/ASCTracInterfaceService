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

            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Export/" + FuncID;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = new ASCTracInterfaceDll.Class1();
                ASCTracInterfaceDll.Class1.InitParse(myClass, baseUrl, "EX_ORDER", ref errMsg);
                if (myClass == null)
                    statusCode = HttpStatusCode.InternalServerError;
                else
                {
                    myClass.myLogRecord.HttpFunctionID = "Get";
                    myClass.myLogRecord.ItemID = aData.CustID;
                    myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aData);

                    statusCode = ASCTracInterfaceDll.Exports.ExportCustOrder.doExportCustOrders(myClass, aData, ref outdata, ref errMsg);
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
            //ASCTracInterfaceDll.Class1.LogTransaction(FuncID, "", Newtonsoft.Json.JsonConvert.SerializeObject(aData), Newtonsoft.Json.JsonConvert.SerializeObject(retval), statusCode != HttpStatusCode.OK);
            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
                myClass.PostLog(statusCode, errMsg);
            }

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
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Export/" + FuncID;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = new ASCTracInterfaceDll.Class1();
                ASCTracInterfaceDll.Class1.InitParse(myClass, baseUrl, "EX_ORDER", ref errMsg);
                if (myClass == null)
                    statusCode = HttpStatusCode.InternalServerError;
                else
                {
                    myClass.myLogRecord.HttpFunctionID = "Get";
                myClass.myLogRecord.ItemID = aCustID;
                myClass.myLogRecord.InData = "aCustID=" + aCustID;

                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter aData = new ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter("C", aCustID, string.Empty);
                    statusCode = ASCTracInterfaceDll.Exports.ExportCustOrder.doExportCustOrders(myClass, aData, ref outdata, ref errMsg);
                }
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                if (myClass != null)
                    myClass.LogException(ex);
                else
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
        /// Update list of Customer Orders Pick Records to Processed 
        /// </summary>

        [HttpPut]
        public HttpResponseMessage UpdateCustOrderExport(List<ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport> aList)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Export/" + FuncID;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = new ASCTracInterfaceDll.Class1();
                ASCTracInterfaceDll.Class1.InitParse(myClass, baseUrl, "EX_ORDER", ref errMsg);
                if (myClass == null)
                    statusCode = HttpStatusCode.InternalServerError;
                else
                {
                    myClass.myLogRecord.HttpFunctionID = "Put";
                    //myClass.myLogRecord.ItemID = ;
                    myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aList);
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Exports.ExportCustOrder.updateExportCustOrder(myClass, aList, ref errMsg);
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