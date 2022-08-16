using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.Count
{
    public class ModelCountHeader
    {
        public ModelCountHeader()
        {
            DetailList = new List<ModelCountDetail>();
            USERLEVELNUMBER = 0;
        }
        public string DESCRIPTION { get; set; }
        public DateTime SCHED_START_DATE { get; set; }
        public DateTime SCHED_END_DATE { get; set; }
        public string FACILITY { get; set; }
        public string COUNT_TYPE { get; set; }
        public long USERLEVELNUMBER { get; set; }

        public List<ModelCountDetail> DetailList { get; set; }
    }
}
