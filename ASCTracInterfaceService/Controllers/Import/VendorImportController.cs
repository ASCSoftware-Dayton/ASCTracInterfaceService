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
        private static string FuncID = "VendorImport";
        private string funcType = "IM_VENDOR";

        /// <summary>
        /// Import a Vendor Record of structure ASCTracInterfaceModel.Model.Vendor.VendorImport 
        /// </summary>
        /// <param name="data">The data to be imported.</param>
        [HttpPost]
        public HttpResponseMessage PostVendor(ASCTracInterfaceModel.Model.Vendor.VendorImport aData)
        {
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Import/VendorImport";
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = ASCTracInterfaceDll.Class1.InitParse(baseUrl, funcType, ref errMsg);
                myClass.myLogRecord.HttpFunctionID = "Post";
                myClass.myLogRecord.OrderNum = aData.VENDOR_CODE;
                myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
                try
                {
                    ReadMyAppSettings.ReadAppSettings(FuncID);

                    statusCode = ASCTracInterfaceDll.Imports.ImportVendor.doImportVendor(myClass, aData, ref errMsg);
                }
                catch (Exception ex)
                {
                    myClass.LogException(ex);

                    statusCode = HttpStatusCode.BadRequest;
                    errMsg = ex.Message;
                    //LoggingUtil.LogEventView("PostPO", aData.PONUMBER, ex.ToString(), ref errMsg);
                }
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView(funcType, aData.VENDOR_CODE, ex.ToString(), ref errMsg);
            }

            HttpResponseMessage retval; // = ASCResponse.BuildResponse( statusCode, errMsg);
            Models.ModelResponse resp;
            if (statusCode == HttpStatusCode.OK)
            {
                resp = ASCResponse.BuildResponse(statusCode, null);
                retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
                //retval = Request.CreateResponse(statusCode, errMsg);
            }
            else
            {
                resp = ASCResponse.BuildResponse(statusCode, errMsg, baseUrl, "Post");
                retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
            }
            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(resp);
                myClass.PostLog(statusCode, errMsg);
            }

            return (retval);
        }
    }
}