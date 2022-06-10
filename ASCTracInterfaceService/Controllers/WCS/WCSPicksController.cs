using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.WCS
{
    [Filters.ApiAuthenticationFilter]
    public class WCSPicksController : ApiController
    {
        /// <summary>
        /// Process a Customer Order Pick
        /// </summary>
        /// <param name="data">The data to be imported.</param>
        [HttpPost]
        public HttpResponseMessage PostPick(ASCTracInterfaceModel.Model.WCS.WCSPick aData)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            string errMsg = string.Empty;
            try
            {
                statusCode = ASCTracInterfaceDll.WCS.WCSProcess.doWCSPickImport( "C", aData, ref errMsg);
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

        [HttpGet]
        public static HttpStatusCode doExportWCSPicks( ref List<ASCTracInterfaceModel.Model.WCS.WCSPick> aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            aData = new List<ASCTracInterfaceModel.Model.WCS.WCSPick>();
            try
            {
                retval = ASCTracInterfaceDll.WCS.WCSProcess.doWCSPickExport(ref aData, ref errmsg);
            }
            catch (Exception ex)
            {
                retval = HttpStatusCode.BadRequest;
                errmsg = ex.Message;
            }
            return (retval);
        }


    }
}
