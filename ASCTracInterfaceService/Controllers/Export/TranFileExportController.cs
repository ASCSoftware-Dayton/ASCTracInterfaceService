using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Export
{
    [Filters.ApiAuthenticationFilter]
    public class TranFileExportController : ApiController
    {
        private static string FuncID = "TranFileExport";
        /// <summary>
        /// Return list of Inventory Transactions
        /// </summary>

        [HttpGet]
        public HttpResponseMessage GetTranfileTransactions(ASCTracInterfaceModel.Model.TranFile.TranFileExportFilter aData)
        {
            List<ASCTracInterfaceModel.Model.TranFile.TranfileExport> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                statusCode = ASCTracInterfaceDll.Exports.ExportTranfile.doExportTranfile(aData, ref outdata, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("GetTranfileTransactions", aData.CustID, ex.ToString(), ref errMsg);
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
        /// Return list of Inventory Transactions for a Customer
        /// </summary>
        [HttpGet]
        public HttpResponseMessage GetTranfileTransactions( string aCustID, string aExcludeTrantype )
        {
            ASCTracInterfaceModel.Model.TranFile.TranFileExportFilter aData = new ASCTracInterfaceModel.Model.TranFile.TranFileExportFilter(aExcludeTrantype, aCustID);
            List<ASCTracInterfaceModel.Model.TranFile.TranfileExport> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                statusCode = ASCTracInterfaceDll.Exports.ExportTranfile.doExportTranfile(aData, ref outdata, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("GetTranfileTransactions", aCustID, ex.ToString(), ref errMsg);
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
        /// Update list of Inventory Transactions to Processed
        /// </summary>
        [HttpPut]
        public HttpResponseMessage UpdateTranfileExport(List<ASCTracInterfaceModel.Model.TranFile.TranfileExport> aList)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                statusCode = ASCTracInterfaceDll.Exports.ExportTranfile.UpdateExport(aList, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("UpdateTranfileExport", aList.Count.ToString(), ex.ToString(), ref errMsg);
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
