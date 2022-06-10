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
                statusCode = ASCTracInterfaceDll.Exports.ExportTranfile.doExportTranfile(aData, ref outdata, ref errMsg);
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
                statusCode = ASCTracInterfaceDll.Exports.ExportTranfile.doExportTranfile(aData, ref outdata, ref errMsg);
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
        /// Update list of Inventory Transactions to Processed
        /// </summary>
        [HttpPut]
        public HttpResponseMessage UpdateTranfileExport(List<ASCTracInterfaceModel.Model.TranFile.TranfileExport> aList)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                statusCode = ASCTracInterfaceDll.Exports.ExportTranfile.UpdateExport(aList, ref errMsg);
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
