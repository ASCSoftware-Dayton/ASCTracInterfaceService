﻿using System;
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
        /// <summary>
        /// Import a Vendor Record of structure ASCTracInterfaceModel.Model.Vendor.VendorImport 
        /// </summary>
        /// <param name="data">The data to be imported.</param>
        [HttpPost]
        public HttpResponseMessage PostVendor(ASCTracInterfaceModel.Model.Vendor.VendorImport aData)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            string errMsg = string.Empty;
            try
            {
                ReadMyAppSettings.ReadAppSettings();
                statusCode = ASCTracInterfaceDll.Imports.ImportVendor.doImportVendor(aData, ref errMsg);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView("PostVendor", aData.VENDOR_CODE, ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval; // = ASCResponse.BuildResponse( statusCode, errMsg);

            if (statusCode == HttpStatusCode.OK)
            {
                var resp = ASCResponse.BuildResponse(statusCode, null);
                retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
                //retval = Request.CreateResponse(statusCode, errMsg);
            }
            else
            {
                var resp = ASCResponse.BuildResponse(statusCode, errMsg, Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Import/VendorImport", "Post");
                retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
            }
            return (retval);
        }
    }
}