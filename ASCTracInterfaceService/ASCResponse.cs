using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace ASCTracInterfaceService
{
    internal class ASCResponse
    {
        static internal Models.ModelResponse BuildResponse(HttpStatusCode statusCode, string aerrmsg)
        {
            string errmsg = string.Empty;
            if( !String.IsNullOrEmpty( aerrmsg))
                errmsg = aerrmsg.Replace( "~", "\r\n");
            Models.ModelResponse resp = new Models.ModelResponse();
            resp.ReturnCode = statusCode;
            resp.ReturnCodeDescription = statusCode.ToString();

            resp.Message = errmsg;
            if (statusCode == HttpStatusCode.PreconditionFailed)
            {
                resp.ReturnCodeDescription = "Accepted";
                resp.Status = "PARTIAL";
                if (errmsg.EndsWith(","))
                    resp.Message = errmsg.Substring(0, errmsg.Length - 1);
                else
                    resp.Message = errmsg;
            }
            else if (statusCode == HttpStatusCode.Accepted)
            {
                resp.Status = "PARTIAL";
            }
            else if (statusCode == HttpStatusCode.NoContent)
            {
                resp.ReturnCodeDescription = "Success";
                resp.Status = "Success";
                if (String.IsNullOrEmpty(errmsg))
                    resp.Message = "No Records Found";

            }
            else if (statusCode != HttpStatusCode.OK)
            {
                resp.Status = "ERROR";
            }
            else
            {
                resp.Status = "Success";
                if (String.IsNullOrEmpty(errmsg))
                    resp.Message = "SUCCESSFULLY PROCESSED";
            }
            return (resp);
        }
        static internal Models.ModelResponse BuildResponse(HttpStatusCode statusCode, string errmsg, string url, string aCommand)
        {
            Models.ModelResponse resp = new Models.ModelResponse();
            resp.ReturnCode = statusCode;
            resp.ReturnCodeDescription = statusCode.ToString();
            resp.Message = "HTTP " + aCommand + " on resource \"" + url + "\failed: " + errmsg;
            if (statusCode != HttpStatusCode.OK)
            {
                resp.Status = "ERROR";
            }
            else
            {
                resp.Status = "Success";
                if (String.IsNullOrEmpty(errmsg))
                    resp.Message = "SUCCESSFULLY PROCESSED";
            }
            return (resp);
        }

        static internal Models.ModelMissingItemsResponse BuildMissingItemsResponse(HttpStatusCode statusCode, string errmsg)
        {
            Models.ModelMissingItemsResponse resp = new Models.ModelMissingItemsResponse();
            resp.ReturnCode = statusCode;
            resp.ReturnCodeDescription = statusCode.ToString();
                resp.Status = "Partial";
            string tmp = errmsg;
            while (!String.IsNullOrEmpty(tmp))
                resp.MissingItems.Add(ascLibrary.ascStrUtils.GetNextWord(ref tmp));
            return (resp);
        }
    }
}