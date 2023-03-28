using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Import
{
    [Filters.ApiAuthenticationFilter]
    public class ItemMasterImportController : ApiController
    {
        private static string FuncID = "ItemMasterImport";
        private static string funcType = "IM_ITEM";
        /// <summary>
        /// Import an Item Record
        /// </summary>
        [HttpPost]
        public HttpResponseMessage PostItemMaster(ASCTracInterfaceModel.Model.Item.ItemMasterImport aData)
        {
            string errMsg = string.Empty;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Import/ItemMasterImport";
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = ASCTracInterfaceDll.Class1.InitParse(baseUrl, funcType, ref errMsg);
                myClass.myLogRecord.HttpFunctionID = "Post";
                myClass.myLogRecord.ItemID = aData.PRODUCT_CODE;
                myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
                try
                {
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Imports.ImportItemMaster.doImportItem(myClass, aData, ref errMsg);
                }
                catch (Exception ex)
                {
                    myClass.LogException(ex);
                    myClass.myLogRecord.StackTrace = ex.StackTrace;
                    myClass.myLogRecord.OutData = ex.Message;
                    statusCode = HttpStatusCode.BadRequest;
                    errMsg = ex.Message;
                    //LoggingUtil.LogEventView("PostItemMaster", aData.PRODUCT_CODE, ex.ToString(), ref errMsg);
                }
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView(FuncID, aData.PRODUCT_CODE, ex.ToString(), ref errMsg);
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
                resp = ASCResponse.BuildResponse(statusCode, errMsg, Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Import/ItemMasterImport", "Post");
                retval = Request.CreateResponse<Models.ModelResponse>(statusCode, resp);
            }
                if (myClass != null)
                {
                    myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(resp);
                    myClass.PostLog(statusCode, errMsg);
                }

                //ASCTracInterfaceDll.Class1.LogTransaction(FuncID, aData.PRODUCT_CODE, Newtonsoft.Json.JsonConvert.SerializeObject(aData), Newtonsoft.Json.JsonConvert.SerializeObject(resp), statusCode != HttpStatusCode.OK);

                return (retval);
        }
    }
}
