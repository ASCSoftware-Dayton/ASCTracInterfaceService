using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model
{
    public class ModelExportFilter
    {
        public ModelExportFilter( string afieldname, long aFilterType, string aValue, string aEndValue )
        {
            Fieldname = afieldname;
            FilterType = aFilterType;
            Startvalue = aValue;
            Endvalue = aEndValue;
        }
        public string Fieldname { get; }
        public long FilterType { get; }
        public string Startvalue { get; }
        public string Endvalue { get; }
    }
}
