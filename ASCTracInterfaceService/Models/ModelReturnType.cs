using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASCTracInterfaceService.Models
{
    public class ModelReturnType
    {
        public ModelReturnType( string aErrorMessage)
        {
            successful = string.IsNullOrEmpty(aErrorMessage);
            ErrorMessage = aErrorMessage;
        }
        public bool successful { get; set; }
        public string ErrorMessage { get; set; }
       // public string DataMessage { get; set; }

    }
}