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


        [HttpPost]
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
