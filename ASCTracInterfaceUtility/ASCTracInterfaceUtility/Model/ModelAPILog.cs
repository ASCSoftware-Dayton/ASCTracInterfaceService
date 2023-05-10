using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.UI.Xaml.Controls.Chart;

namespace ASCTracInterfaceUtility.Model
{
    internal class ModelAPILog
    {
        internal string ID { get; set; }
        internal string URL { get; set; }
        internal string LogType { get; set; }
        internal string HttpFunctionID { get; set; }
        internal string FunctionID { get; set; }
        internal DateTime StartDatetime { get; set; }
        internal DateTime StopDatetime { get; set; }
        internal int ReturnStatus { get; set; }
        internal string RetryFlag { get; set; }
        internal DateTime RetryDatetime { get; set; }

        internal string RetryUserID { get; set; }
        internal string OrderNum{ get; set; }
        internal string ItemID{ get; set; }
        internal string EmailErrorSent{ get; set; }
        internal string ASCErrorType { get; set; }

        internal string InputData { get; set; } // API_MEMO_DETA.RecType='I'
        internal string MessageData { get; set; } // API_MEMO_DETA.RecType='M'
        internal string OutputData { get; set; } // API_MEMO_DETA.RecType='O'
        internal string StackTraceData { get; set; } // API_MEMO_DETA.RecType='S'
        internal string QueryData { get; set; } // API_MEMO_DETA.RecType='Q'

    }
}
