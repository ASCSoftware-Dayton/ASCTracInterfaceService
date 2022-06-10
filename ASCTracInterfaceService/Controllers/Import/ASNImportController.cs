using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Import
{
    [Filters.ApiAuthenticationFilter]
    public class ASNImportController : ApiController
    {
        [System.Web.Http.HttpPost]
        /// <summary>
        /// Import a ASN Order (Header and Details)
        /// </summary>
        public HttpResponseMessage PostASN(ASCTracInterfaceModel.Model.ASN.ASNHdrImport aData)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            try
            {
                statusCode = ASCTracInterfaceDll.Imports.ImportASN.doImportASN(aData, ref errMsg);
                //statusCode = ASCTracInterfaceDll.Imports.ImportPO.doImportPO(aData, ref errMsg);

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
