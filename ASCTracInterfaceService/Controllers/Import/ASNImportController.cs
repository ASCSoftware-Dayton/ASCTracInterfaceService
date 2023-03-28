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
        private static string FuncID = "ASNImport";
        private string funcType = "IM_ASN";
        /// <summary>
        /// Import a ASN Order (Header and Details)
        /// </summary>
        [System.Web.Http.HttpPost]
        public HttpResponseMessage PostASN(ASCTracInterfaceModel.Model.ASN.ASNHdrImport aData)
        {

            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Import/ASNImport";
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = ASCTracInterfaceDll.Class1.InitParse(baseUrl, funcType, ref errMsg);
                myClass.myLogRecord.HttpFunctionID = "Post";
                myClass.myLogRecord.OrderNum = aData.PONUMBER;
                myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aData);


                try
                {
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Imports.ImportASN.doImportASN(myClass, aData, ref errMsg);
                    //statusCode = ASCTracInterfaceDll.Imports.ImportPO.doImportPO(aData, ref errMsg);

                }
                catch (Exception ex)
                {
                    myClass.LogException(ex);
                    statusCode = HttpStatusCode.BadRequest;
                    errMsg = ex.Message;
                    //LoggingUtil.LogEventView("PostASN", aData.ASN, ex.ToString(), ref errMsg);
                }
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView(funcType, aData.PONUMBER, ex.ToString(), ref errMsg);
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
            //ASCTracInterfaceDll.Class1.LogTransaction(FuncID, aData.ASN, Newtonsoft.Json.JsonConvert.SerializeObject(aData), Newtonsoft.Json.JsonConvert.SerializeObject(resp), statusCode != HttpStatusCode.OK);
            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(resp);
                myClass.PostLog(statusCode, errMsg);
            }

            return (retval);
        }
    }
}
