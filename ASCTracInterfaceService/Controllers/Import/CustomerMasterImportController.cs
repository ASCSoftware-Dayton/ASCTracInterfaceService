using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Import
{
    [Filters.ApiAuthenticationFilter]

    public class CustomerMasterImportController : ApiController
    {
        private static string FuncID = "CustomerMasterImport";
        private static string funcType = "IM_CUST";
        /// <summary>
        /// Import an Customer Record
        /// </summary>
        [HttpPost]
        public HttpResponseMessage PostCustomerMaster(ASCTracInterfaceModel.Model.Customer.CustomerMasterImport aData)
        {
            string errMsg = string.Empty;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Import/CustomerMasterImport";
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = new ASCTracInterfaceDll.Class1();
                ASCTracInterfaceDll.Class1.InitParse(ref myClass, baseUrl, funcType, ref errMsg);
                if (myClass == null)
                    statusCode = HttpStatusCode.InternalServerError;
                else
                {
                    myClass.myLogRecord.HttpFunctionID = "Post";
                    myClass.myLogRecord.ItemID = aData.CUST_ID;
                    myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Imports.ImportCustomerMaster.doImportCustomer(myClass, aData, ref errMsg);
                }
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                if (myClass != null)
                    myClass.LogException(ex);
                else
                    LoggingUtil.LogEventView(FuncID, aData.CUST_ID, ex.ToString(), ref errMsg);
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
                resp = ASCResponse.BuildResponse(statusCode, errMsg, Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Import/CustomerMasterImport", "Post");
                retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
            }
            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(resp);
                myClass.PostLog(statusCode, errMsg);
            }
            else
                LoggingUtil.LogEventView(funcType, aData.CUST_ID, errMsg, ref errMsg);

            //ASCTracInterfaceDll.Class1.LogTransaction(FuncID, aData.PRODUCT_CODE, Newtonsoft.Json.JsonConvert.SerializeObject(aData), Newtonsoft.Json.JsonConvert.SerializeObject(resp), statusCode != HttpStatusCode.OK);

            return (retval);
        }
    }
}
