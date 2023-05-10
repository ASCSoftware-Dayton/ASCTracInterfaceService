using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Core;

namespace ASCTracInterfaceUtility.ViewModel
{
    public class APILogViewModel : ViewModelBase
    {
        private DataRowView selectedRow;
        private DataTable dataTable;
        public DataTable myDataTable
        {
            get { return dataTable; }
            set
            {
                this.dataTable = value;
                this.OnPropertyChanged(nameof(myDataTable));
            }
        }

        public DataRowView SelectedRow
        {
            get { return selectedRow; }
            set
            {
                this.selectedRow = value;
                this.OnPropertyChanged(nameof(SelectedRow));
            }
        }

        public APILogViewModel()
        {
            dataTable = new DataTable();
        }

    }
}