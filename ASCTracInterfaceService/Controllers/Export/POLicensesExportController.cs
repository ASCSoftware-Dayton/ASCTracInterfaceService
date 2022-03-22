using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Export
{
    public class POLicensesExportController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage GetPOLines(ASCTracInterfaceModel.Model.PO.POExportFilter aData)
        {
            List<ASCTracInterfaceModel.Model.PO.POExportLicenses> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                statusCode = ASCTracInterfaceDll.Exports.ExportPOLicenses.doExportPOLicenses(aData, ref outdata, ref errMsg);
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
    public HttpResponseMessage GetPOLines(bool aOnlySendCompletedReceipt)
    {
            ASCTracInterfaceModel.Model.PO.POExportFilter aData = new ASCTracInterfaceModel.Model.PO.POExportFilter(aOnlySendCompletedReceipt);
        List<ASCTracInterfaceModel.Model.PO.POExportLicenses> outdata = null;
        HttpStatusCode statusCode = HttpStatusCode.Accepted;
        string errMsg = string.Empty;
        try
        {
            statusCode = ASCTracInterfaceDll.Exports.ExportPOLicenses.doExportPOLicenses(aData, ref outdata, ref errMsg);
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
    public HttpResponseMessage UpdatePOExport(List<ASCTracInterfaceModel.Model.PO.POExportLicenses> aList)
    {
        HttpStatusCode statusCode = HttpStatusCode.Accepted;
        string errMsg = string.Empty;
        try
        {
            statusCode = ASCTracInterfaceDll.Exports.ExportPOLicenses.updateExportPOLicenses(aList, ref errMsg);
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