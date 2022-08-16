using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ASCTracInterfaceTest
{
    internal class RestService
    {
        HttpClient client;

        public RestService()
        {
#if DEBUG
            client = new HttpClient();
            //client = new HttpClient(DependencyService.Get<IHttpClientHandlerService>().GetInsecureHandler());
#else
          client = new HttpClient();
#endif

            string authInfo = "k" + ":" + "34sddff";
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
        }


        public string fURL;

        
        public async Task<HttpResponseMessage> doItemImport(ASCTracInterfaceModel.Model.Item.ItemMasterImport aData)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/itemmasterimport"); // string.Format(RestUrl, string.Empty));
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.PostAsync(uri, content).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);
        }
        public async Task<HttpResponseMessage> doVendorImport(ASCTracInterfaceModel.Model.Vendor.VendorImport aData)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/vendorimport"); // string.Format(RestUrl, string.Empty));
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.PostAsync(uri, content).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);
        }

        public async Task<HttpResponseMessage> doASNImport(ASCTracInterfaceModel.Model.ASN.ASNHdrImport aData)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/asnimport"); // string.Format(RestUrl, string.Empty));
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.PostAsync(uri, content).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);
        }

        public async Task<HttpResponseMessage> doPOImport(ASCTracInterfaceModel.Model.PO.POHdrImport aData)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/poimport"); // string.Format(RestUrl, string.Empty));
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.PostAsync(uri, content).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);
        }

        public async Task<HttpResponseMessage> doPOLinesExport(ASCTracInterfaceModel.Model.PO.POExportFilter aData)
        {
            //string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            string msg = "?aOnlySendCompletedReceipt=false";
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/polinesexport/" + msg); // string.Format(RestUrl, string.Empty));

            //StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.GetAsync(uri).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);
        }
        public async Task<HttpResponseMessage> updatePOLinesExport(List<ASCTracInterfaceModel.Model.PO.POExportLines> aData)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/polinesexport"); // string.Format(RestUrl, string.Empty));
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.PutAsync(uri, content).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);
        }


        public async Task<HttpResponseMessage> doPOLicensesExport(ASCTracInterfaceModel.Model.PO.POExportFilter aData)
        {
            //string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            string msg = "?aOnlySendCompletedReceipt=false";
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/policensesexport/" + msg); // string.Format(RestUrl, string.Empty));

            //StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.GetAsync(uri).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);
        }
        public async Task<HttpResponseMessage> UpdatePOLicensesExport(List<ASCTracInterfaceModel.Model.PO.POExportLicenses> aData)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/policensesexport"); // string.Format(RestUrl, string.Empty));
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.PutAsync(uri, content).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);
        }


        public async Task<HttpResponseMessage> doControlledCountImport(ASCTracInterfaceModel.Model.Count.ModelCountHeader aData)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/controlledcount"); // string.Format(RestUrl, string.Empty));
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.PostAsync(uri, content).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);
        }

        public async Task<HttpResponseMessage> doCOImport(ASCTracInterfaceModel.Model.CustOrder.OrdrHdrImport aData)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/custorderimport"); // string.Format(RestUrl, string.Empty));
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.PostAsync(uri, content).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);
        }

        public async Task<HttpResponseMessage> doCOsExport(ASCTracInterfaceModel.Model.CustOrder.CustOrderExportFilter aData)
        {
            //string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            string msg = "?acustid=" + aData.CustID;
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/custorderexport/" + msg); // string.Format(RestUrl, string.Empty));

            //StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.GetAsync(uri).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);
        }


        public async Task<HttpResponseMessage> updateCOLinesExport(List<ASCTracInterfaceModel.Model.CustOrder.CustOrderHeaderExport> aData)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/custorderexport"); // string.Format(RestUrl, string.Empty));
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.PutAsync(uri, content).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);
        }

        public async Task<HttpResponseMessage> doParcelExport(ASCTracInterfaceModel.Model.CustOrder.ParcelExporFilter aData)
        {
            //string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            string msg = "?acustid=" + aData.CustID; ;
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/parcelexport/" + msg); // string.Format(RestUrl, string.Empty));

            //StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.GetAsync(uri).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);
        }

        
        public async Task<HttpResponseMessage> updateParcelExport(List<ASCTracInterfaceModel.Model.CustOrder.ParcelExport> aData)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/parcelexport"); // string.Format(RestUrl, string.Empty));
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.PutAsync(uri, content).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);
        }


        public async Task<HttpResponseMessage> doTranfileExport(ASCTracInterfaceModel.Model.TranFile.TranFileExportFilter aData)
        {
            //string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            string msg = "?acustid=" + aData.CustID + "&aExcludeTrantype=" + aData.ExcludeTranType;
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/tranfileexport/" + msg); // string.Format(RestUrl, string.Empty));

            //StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.GetAsync(uri).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);
        }


        public async Task<HttpResponseMessage> updateTranfileExport(List<ASCTracInterfaceModel.Model.TranFile.TranfileExport> aData)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/tranfileexport"); // string.Format(RestUrl, string.Empty));
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.PutAsync(uri, content).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);
        }


        public async Task<HttpResponseMessage> CallWCSPostPick(ASCTracInterfaceModel.Model.WCS.WCSPick aData)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            string funcString = "wcspicks";
            if (aData.ORDERTYPE.Equals("N"))
                funcString = "wcsunpick";
            if (aData.ORDERTYPE.Equals("R"))
                funcString = "wcsrepick";
            Uri uri = new Uri(baseuri, "/api/" + funcString); // string.Format(RestUrl, string.Empty));
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.PostAsync(uri, content).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.ToString());
            }
            return (response);

        }

        public async Task<HttpResponseMessage> doWCSGetPicks(string aOrderType)
        {
            //string json = Newtonsoft.Json.JsonConvert.SerializeObject(aData);
            string msg = "?aordertype=" + aOrderType;
            HttpResponseMessage response;
            Uri baseuri = new Uri(fURL);
            Uri uri = new Uri(baseuri, "/api/wcspicks/" + msg); // string.Format(RestUrl, string.Empty));

            //StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                //ascLibrary.ascUtils.ascWriteLog( "IMPORT", "Before Send", true);
                response = client.GetAsync(uri).Result; // .GetAsync( uri, .GetAsync(uri, content);
                //ascLibrary.ascUtils.ascWriteLog("IMPORT", "After Send", true);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(uri.ToString() + "\r\n" +  ex.ToString());
            }
            return (response);
        }

    }

}
