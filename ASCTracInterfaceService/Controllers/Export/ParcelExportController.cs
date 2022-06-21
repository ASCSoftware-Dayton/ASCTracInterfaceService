using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Export
{
    [Filters.ApiAuthenticationFilter]
    public class ParcelExportController : ApiController
    {
        /// <summary>
        /// Return list of Parcel Records
        /// </summary>
        [HttpGet]
        public HttpResponseMessage GetParcelTransactions(ASCTracInterfaceModel.Model.CustOrder.ParcelExporFilter aData)
        {
            List<ASCTracInterfaceModel.Model.CustOrder.ParcelExport> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings();
                statusCode = ASCTracInterfaceDll.Exports.ExportParcel.doExportParcel(aData, ref outdata, ref errMsg);
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
        /// Return list of Parcel Records for a Customer
        /// </summary>
        [HttpGet]
        public HttpResponseMessage GetParcelTransactions(string aCustID)
        {
            ASCTracInterfaceModel.Model.CustOrder.ParcelExporFilter aData = new ASCTracInterfaceModel.Model.CustOrder.ParcelExporFilter( aCustID, "");
            List<ASCTracInterfaceModel.Model.CustOrder.ParcelExport> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings();
                statusCode = ASCTracInterfaceDll.Exports.ExportParcel.doExportParcel(aData, ref outdata, ref errMsg);
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
        /// Update list of Parcel Records to Processed
        /// </summary>
        [HttpPut]
        public HttpResponseMessage UpdateParcelExport(List<ASCTracInterfaceModel.Model.CustOrder.ParcelExport> aList)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings();
                statusCode = ASCTracInterfaceDll.Exports.ExportParcel.UpdateExport(aList, ref errMsg);
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
