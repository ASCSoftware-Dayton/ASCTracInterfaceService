using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceDll.Model
{
    public class ModelLog
	{
		public ModelLog( string aURL, string aFunctionID)
        {
			URL = aURL;
			FunctionID = aFunctionID;
			LogType = "I";
        }

		public string URL { get; set; }
		public string LogType { get; set; }  // X=Exception, E=Error(Data), I=Informational
		public string FunctionID { get; set; }
		public string HttpFunctionID { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime StopDateTime { get; set; }
		public int ReturnStatus { get; set; }
		public string OrderNum { get; set; }
		public string ItemID { get; set; }

		public string SQLData { get; set;}
		public string InData { get; set; }
		public string OutData { get; set; }
		public string StackTrace { get; set; }
		public string infoMsg { get; set; }
	}
}
