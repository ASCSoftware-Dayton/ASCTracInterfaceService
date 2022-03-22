using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model
{
    public class ModelCustomData
    {
        public ModelCustomData( string aFieldname, string aValue)
        {
            FieldName = aFieldname;
            Value = aValue;
        }
        public string FieldName { get; set; }
        public string Value{ get; set; }
    }
}
