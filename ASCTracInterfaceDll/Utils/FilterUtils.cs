using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceDll.Utils
{
    internal class FilterUtils
    {
        internal static void AppendToExportFilter( ref string sqlstr, List<ASCTracInterfaceModel.Model.ModelExportFilter> ExportFilterList, string defaultTable, string aOtherTables)
        {
            if (ExportFilterList.Count > 0)
            {
                List<String> tablelist = new List<string>();
                tablelist.Add(defaultTable.ToUpper());
                string OtherTables = aOtherTables;
                while( !String.IsNullOrEmpty( OtherTables))
                {
                    string tblname = ascLibrary.ascStrUtils.GetNextWord(ref OtherTables).ToUpper();
                    if (!tablelist.Contains(tblname))
                        tablelist.Add(tblname);
                }

                foreach( var rec in ExportFilterList )
                {
                    string tblname = rec.Tablename;
                    if (String.IsNullOrEmpty(tblname))
                        tblname = defaultTable;
                    if (tablelist.Contains(tblname))
                    {
                        string fieldname = tblname + "." + rec.Fieldname;
                        string wherestr = ascLibrary.ascStrUtils.buildwherestr(fieldname, rec.FilterType.ToString(), rec.Startvalue, rec.Endvalue);
                        if (!String.IsNullOrEmpty(wherestr))
                            sqlstr += " AND " + wherestr;
                    }
                }
            }
        }
    }
}
