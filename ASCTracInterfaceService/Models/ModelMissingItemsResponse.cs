using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace ASCTracInterfaceService.Models
{
    public class ModelMissingItemsResponse
    {
        public ModelMissingItemsResponse()
        {
            MissingItems = new List<string>();
        }

        public HttpStatusCode ReturnCode { get; set; }
        public string ReturnCodeDescription { get; set; }
        public string Status { get; set; }
        public List<string> MissingItems { get; set; }

    }
}