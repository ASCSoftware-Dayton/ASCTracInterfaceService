using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace ASCTracInterfaceService.Models
{
    public class ModelResponse
    {
        public HttpStatusCode ReturnCode { get; set; }
        public string ReturnCodeDescription { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}