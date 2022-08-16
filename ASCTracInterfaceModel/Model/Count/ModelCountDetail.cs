using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.Count
{
    public class ModelCountDetail
    {
        public ModelCountDetail()
        {
            GROUP_SEQ = 0;
        }
        public string FIELDNAME { get; set; }
        public string FILTER_TYPE { get; set; }
        public string START_VALUE { get; set; }
        public string END_VALUE { get; set; }
        public string FUNCTIONID { get; set; }
        public long GROUP_SEQ { get; set; }
    }
}