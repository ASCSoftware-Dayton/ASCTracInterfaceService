﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracInterfaceService.Controllers.Export
{
    [Filters.ApiAuthenticationFilter]
    public class TranFileExportController : ApiController
    {
        private static string FuncID = "TranFileExport";
        /// <summary>
        /// Return list of Inventory Transactions
        /// </summary>

        [HttpGet]
        public HttpResponseMessage GetTranfileTransactions(ASCTracInterfaceModel.Model.TranFile.TranFileExportFilter aData)
        {
            List<ASCTracInterfaceModel.Model.TranFile.TranfileExport> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Export/" + FuncID;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = ASCTracInterfaceDll.Class1.InitParse(baseUrl, "EX_TRAN", ref errMsg);
                myClass.myLogRecord.HttpFunctionID = "Get";
                myClass.myLogRecord.OrderNum = "";
                myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
                try
                {
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Exports.ExportTranfile.doExportTranfile(myClass, aData, ref outdata, ref errMsg);
                }
                catch (Exception ex)
                {
                    statusCode = HttpStatusCode.BadRequest;
                    errMsg = ex.Message;
                    myClass.LogException(ex);

                }
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView(FuncID, "", ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
            {
                retval = new HttpResponseMessage(statusCode);
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(outdata));
            }
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
                myClass.PostLog(statusCode, errMsg);
            }
            return (retval);
        }


        /// <summary>
        /// Return list of Inventory Transactions for a Customer
        /// </summary>
        [HttpGet]
        public HttpResponseMessage GetTranfileTransactions( string aCustID, string aExcludeTrantype )
        {
            ASCTracInterfaceModel.Model.TranFile.TranFileExportFilter aData = new ASCTracInterfaceModel.Model.TranFile.TranFileExportFilter(aExcludeTrantype, aCustID);
            List<ASCTracInterfaceModel.Model.TranFile.TranfileExport> outdata = null;
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Export/" + FuncID;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = ASCTracInterfaceDll.Class1.InitParse(baseUrl, "EX_TRAN", ref errMsg);
                myClass.myLogRecord.HttpFunctionID = "Get";
                myClass.myLogRecord.OrderNum = "";
                myClass.myLogRecord.InData = "aCustID=" + aCustID + "&aExcludeTrantype=" + aExcludeTrantype;
                try
                {
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Exports.ExportTranfile.doExportTranfile(myClass, aData, ref outdata, ref errMsg);
                }
                catch (Exception ex)
                {
                    statusCode = HttpStatusCode.BadRequest;
                    errMsg = ex.Message;
                    myClass.LogException(ex);

                }
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView(FuncID, "", ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
            {
                retval = new HttpResponseMessage(statusCode);
                retval.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(outdata));
            }
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
                myClass.PostLog(statusCode, errMsg);
            }
            return (retval);
        }

        /// <summary>
        /// Update list of Inventory Transactions to Processed
        /// </summary>
        [HttpPut]
        public HttpResponseMessage UpdateTranfileExport(List<ASCTracInterfaceModel.Model.TranFile.TranfileExport> aList)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Export/" + FuncID;
            ASCTracInterfaceDll.Class1 myClass = null;
            try
            {
                ReadMyAppSettings.ReadAppSettings(FuncID);
                myClass = ASCTracInterfaceDll.Class1.InitParse(baseUrl, "EX_TRAN", ref errMsg);
                myClass.myLogRecord.HttpFunctionID = "Put";
                myClass.myLogRecord.OrderNum = "";
                myClass.myLogRecord.InData = Newtonsoft.Json.JsonConvert.SerializeObject(aList);

                try
                {
                    ReadMyAppSettings.ReadAppSettings(FuncID);
                    statusCode = ASCTracInterfaceDll.Exports.ExportTranfile.UpdateExport(myClass, aList, ref errMsg);
                }
                catch (Exception ex)
                {
                    statusCode = HttpStatusCode.BadRequest;
                    errMsg = ex.Message;
                    myClass.LogException(ex);

                }
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.BadRequest;
                errMsg = ex.Message;
                LoggingUtil.LogEventView(FuncID, "", ex.ToString(), ref errMsg);
            }
            HttpResponseMessage retval;
            if (statusCode == HttpStatusCode.OK)
                retval = Request.CreateResponse(statusCode, errMsg);
            else
                retval = Request.CreateErrorResponse(statusCode, errMsg);
            if (myClass != null)
            {
                myClass.myLogRecord.OutData = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
                myClass.PostLog(statusCode, errMsg);
            }
            return (retval);
        }

    }
}
