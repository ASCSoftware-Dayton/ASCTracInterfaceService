using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Import
{
    [Filters.ApiAuthenticationFilter]
    public class VendorImportController : ApiController
    {
        [HttpPost]
        //public Models.ModelReturnType PostPO( ASCTracInterfaceDll.Model.PO.POHdrImport aPOHdrData)
        public HttpResponseMessage PostVendor(ASCTracInterfaceModel.Model.Vendor.VendorImport aData)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            string errMsg = string.Empty;
            try
            {
                statusCode = ASCTracInterfaceDll.Imports.ImportVendor.doImportVendor(aData, ref errMsg);
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