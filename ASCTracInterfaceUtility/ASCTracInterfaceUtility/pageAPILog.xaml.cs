// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using ASCTracInterfaceModel.Model.WCS;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.AccessControl;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Telerik.UI.Xaml.Controls.Data.DataForm;
using Telerik.UI.Xaml.Controls.Grid;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Popups;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ASCTracInterfaceUtility
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class pageAPILog : Page
    {
        private string DEFAULT_VIEW_NAME = "***LAST***";
        private string curUserID = "ADMIN";
        internal ViewModel.APILogViewModel myViewModel { get; set; }
        //private DataTable myDataTable; // = new DataTable();
        string myConnString = ""; // password=WeH73w;workstation id=dev;packet size=4096;user id=app_user;data source=asc-cin-app01;persist security info=False;initial catalog=ASCTrac904Dev";
        ASCTracInterfaceDll.Class1 myClass;

        public pageAPILog()
        {
            this.InitializeComponent();
            try
            {
                if (Data.DataGlobals.myDBGlobals.myDBConnectionInfo != null)
                    myConnString = Data.DataGlobals.myDBGlobals.myDBConnection.myConnString;
                myViewModel = new ViewModel.APILogViewModel();
                // GridMain.DataContext = myViewModel;
                myDataGrid.AutoGenerateColumns = false;
                myDataGrid.UserEditMode = DataGridUserEditMode.None;
                myDataGrid.UserGroupMode = DataGridUserGroupMode.Disabled;

                SetupDateFilter();

                if (!String.IsNullOrEmpty(myConnString))
                {
                    string errmsg = string.Empty;
                    myClass = ASCTracInterfaceDll.Class1.InitParse2(myConnString, "Retry API", "Retry", ref errmsg);
                }
            }
            catch (Exception ex)
            {
                ASCErrorMessageBox("APILog Create Exception", ex.ToString());
            }
        }

        private void SetupDateFilter()
        {
            List<String> list = new List<String>();
            list.Add("Today");
            list.Add("Last 2 Days");
            list.Add("Last Week");
            list.Add("Last 30 Days");
            list.Add("Select Date");
            list.Add("All Records");
            cbDateFilter.ItemsSource = list;
            cbDateFilter.SelectedIndex = 0;

            dtpStartDate.Value = DateTime.Now.Date;
        }

        private string GetDateFilter()
        {
            string sql = string.Empty;
            if( cbDateFilter.SelectedIndex == 4 )
            {
                sql = "START_DATETIME >= '" + dtpStartDate.Value.Value.Date.ToString() + "' AND START_DATETIME<'" + dtpStartDate.Value.Value.Date.AddDays(1).ToString() + "'";
            }
            else if( cbDateFilter.SelectedIndex < 5 ) 
            {
                int numDays = 1;
                if (cbDateFilter.SelectedIndex == 1)
                    numDays = 2;
                if (cbDateFilter.SelectedIndex == 2)
                    numDays = 7;
                if (cbDateFilter.SelectedIndex == 3)
                    numDays = 30;
                sql = "START_DATETIME >= '" + DateTime.Now.Date.AddDays(-1 * numDays).ToShortDateString() + "'";
            }
            return (sql);

        }

        private void FillGrid()
        {
            try
            {
                if (Data.DataGlobals.myDBGlobals.myDBConnectionInfo != null)
                    myConnString = Data.DataGlobals.myDBGlobals.myDBConnection.myConnString;

                myDataGrid.ItemsSource = null;
                string sql = "select API_LOG.*, " +
                    " CASE WHEN MI.ID IS NULL THEN 'False' ELSE 'True' END AS INPUT_DATA_FLAG, " +
                    " CASE WHEN MM.ID IS NULL THEN 'False' ELSE 'True' END AS MSG_DATA_FLAG, " +
                    " CASE WHEN MO.ID IS NULL THEN 'False' ELSE 'True' END AS OUTPUT_DATA_FLAG, " +
                    " CASE WHEN MS.ID IS NULL THEN 'False' ELSE 'True' END AS STACK_DATA_FLAG, " +
                    " CASE WHEN MQ.ID IS NULL THEN 'False' ELSE 'True' END AS QUERY_DATA_FLAG" +
                    " from API_LOG" +
                    " LEFT JOIN API_LOG_MEMO_DATA MI ON MI.ID = API_LOG.ID AND MI.RECTYPE = 'I'" +
                    " LEFT JOIN API_LOG_MEMO_DATA MM ON MM.ID = API_LOG.ID AND MM.RECTYPE = 'M'" +
                    " LEFT JOIN API_LOG_MEMO_DATA MO ON MO.ID = API_LOG.ID AND MO.RECTYPE = 'O'" +
                    " LEFT JOIN API_LOG_MEMO_DATA MS ON MS.ID = API_LOG.ID AND MS.RECTYPE = 'S'" +
                    " LEFT JOIN API_LOG_MEMO_DATA MQ ON MQ.ID = API_LOG.ID AND MQ.RECTYPE = 'Q'";

                sql += " WHERE " + GetDateFilter();

                SqlConnection conn = new SqlConnection(myConnString);
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();

                myViewModel.myDataTable.Clear();
                //myDataTable = new DataTable();
                //myDataTable.DefaultView
                // create data adapter
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(myViewModel.myDataTable);
                conn.Close();
                da.Dispose();

                //var myDataList = myDataTable.Rows.Cast<DataRow>().ToList();

                //myDataGrid.Columns.CollectionChanged += Columns_CollectionChanged;
                //GridMain.DataContext = myDataTable;
                //myDataGrid.AutoGenerateColumns = true;
                myDataGrid.ItemsSource = myViewModel.myDataTable;

                //myDataGrid.ItemsSource = myDataList;
                //collectionNavigator.DataContext = myDataTable;
                //collectionNavigator.Source = (System.Collections.IList)myDataTable;
                //collectionNavigator.CollectionView = myDataTable;
                myDataGrid.Columns.Clear();

                if (!GetGridSettings(myDataGrid, "API_LOG"))
                {
                    foreach (DataColumn col in myViewModel.myDataTable.Columns)
                    {
                        var gridcol = new DataGridTextColumn();
                        gridcol.PropertyName = col.ColumnName;
                        string titleStr = col.ColumnName.ToString().ToLower().Replace("_", " ");
                        gridcol.Header = titleStr.Substring(0, 1).ToUpper() + titleStr.Substring(1);
                        gridcol.CanUserReorder = true;
                        gridcol.CanUserGroup = false;

                        myDataGrid.Columns.Add(gridcol);
                    }
                }
                GridMain.DataContext = myViewModel;
                /*
                for( int i = 0; i< myDataGrid.Columns.Count; i++ )
                {
                    myDataGrid.Columns[i].Header = myDataGrid.Columns[i].Header.ToString().ToLower().Replace("_", " ");
                }
                foreach( var col in myDataGrid.Columns ) 
                {
                    col.Header = col.Header.ToString().ToLower().Replace("_", " ");
                }
                */
            }
            catch (Exception ex)
            {
                ASCErrorMessageBox("Fill Grid Exception", ex.ToString());
            }
        }

        private bool GetGridSettings(RadDataGrid aGrid, string aSettingName)
        {
            string aData = string.Empty;
            bool retval = false;
            string sql = "SELECT DISPLAYFIELDS FROM TBLVIEW2" +
                " WHERE TBLNAME='" + aSettingName + "' AND VIEWNAME='" + DEFAULT_VIEW_NAME + "' AND USERID='" + curUserID + "'";
            if( Data.DataGlobals.myDBGlobals.ReadFieldFromDBWithPipes(sql, "", ref aData))
            {
                string fielddata = Utils.ASCUtils.GetNextWord(ref aData);
                while (!String.IsNullOrEmpty(fielddata))
                {
                    retval = true;
                    string fieldtype = Utils.ASCUtils.ascGetNextWord(ref fielddata, ",");
                    string colName = Utils.ASCUtils.ascGetNextWord(ref fielddata, ",");
                    string fieldname = Utils.ASCUtils.ascGetNextWord(ref fielddata, ",");
                    int maxwidth = Convert.ToInt32(Utils.ASCUtils.ascStrToInt(Utils.ASCUtils.ascGetNextWord(ref fielddata, ","), 1));
                    int maxlen = Convert.ToInt32(Utils.ASCUtils.ascStrToInt(Utils.ASCUtils.ascGetNextWord(ref fielddata, ","), 1));
                    int colIndex = Convert.ToInt32(Utils.ASCUtils.ascStrToInt(Utils.ASCUtils.ascGetNextWord(ref fielddata, ","), 1));

                    var gridcol = new DataGridTextColumn();
                    gridcol.PropertyName = fieldname;
                    string titleStr = fieldname.ToString().ToLower().Replace("_", " ");
                    gridcol.Header = titleStr.Substring(0, 1).ToUpper() + titleStr.Substring(1);
                    gridcol.CanUserReorder = true;
                    gridcol.CanUserGroup = false;
                    gridcol.Width = maxwidth;

                    myDataGrid.Columns.Add(gridcol);

                    fielddata = Utils.ASCUtils.GetNextWord(ref aData);
                }
            }
            return (retval);
        }



        private void SetGridSettings(RadDataGrid aGrid, string aSettingName)
        {
            string aData = string.Empty;
            int idx = 0;
            foreach (DataGridTextColumn gridCol in aGrid.Columns)
            {
                if (!String.IsNullOrEmpty(aData))
                    aData += "|";
                aData += "T," + gridCol.PropertyName + "," + gridCol.PropertyName + "," + gridCol.Width.ToString() + ",0," + idx.ToString();
                idx += 1;
            }

            string tmp = string.Empty;
            string sql = "SELECT DISPLAYFIELDS FROM TBLVIEW2" +
                " WHERE TBLNAME='" + aSettingName + "' AND VIEWNAME='" + DEFAULT_VIEW_NAME + "' AND USERID='" + curUserID + "'";
            if (Data.DataGlobals.myDBGlobals.ReadFieldFromDBWithPipes(sql, "", ref aData))
            {
                Data.DataGlobals.myDBGlobals.RunSqlCommand("UPDATE TBLVIEW2 SET DISPLAYFIELDS = '" + aData.Replace("'", "''") + "'" +
                        " WHERE TBLNAME='" + aSettingName + "' AND VIEWNAME='" + DEFAULT_VIEW_NAME + "' AND USERID='" + curUserID + "'");
            }
            else
            {
                sql = "INSERT INTO TBLVIEW2 (TBLNAME, VIEWNAME, USERID, DISPLAYFIELDS)" +
                    " VALUES ( '" + aSettingName + "','" + DEFAULT_VIEW_NAME + "','" + curUserID + "','" + aData.Replace("'", "''") + "')";
                Data.DataGlobals.myDBGlobals.RunSqlCommand(sql);
            }

        }

        private void RetryTransaction(string ID, string functionID)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            if( myClass == null) 
                myClass = ASCTracInterfaceDll.Class1.InitParse2(myConnString, "Retry API", "Retry", ref errMsg);
            if (myClass == null)
                statusCode = HttpStatusCode.InternalServerError;
            else
            {
                string jsonData = string.Empty;
                string ordernum = string.Empty;
                try
                {
                    string sql = "SELECT MEMO_DATA FROM API_LOG_MEMO_DATA WHERE ID = " + ID + " AND RECTYPE = 'I'";
                    SqlConnection conn = new SqlConnection(myConnString);
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    conn.Open();
                    SqlDataReader myReader = cmd.ExecuteReader();
                    if (myReader.Read())
                    {
                        jsonData = myReader["MEMO_DATA"].ToString();
                        switch (functionID)
                        {
                            case "IM_ASN":
                                {
                                    var aData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracInterfaceModel.Model.ASN.ASNHdrImport>(jsonData);
                                    ordernum = aData.ASN;
                                    statusCode = ASCTracInterfaceDll.Imports.ImportASN.doImportASN(myClass, aData, ref errMsg);
                                }
                                break;
                            case "IM_COUNT":
                                {
                                    var aData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracInterfaceModel.Model.Count.ModelCountHeader>(jsonData);
                                    ordernum = aData.COUNTID.ToString();
                                    statusCode = ASCTracInterfaceDll.Imports.ImportControlledCount.doImportControlledCount(myClass, aData, ref errMsg);
                                }
                                break;
                            case "CONFSHIP":
                                {
                                    var aData = jsonData; // JUST ORDERNUMBER Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracInterfaceModel.Model.Count.ModelCountHeader>(jsonData);
                                    ordernum = aData;
                                    statusCode = ASCTracInterfaceDll.Imports.ImportCustOrder.doImportCustOrderConfirmShip(myClass, aData, ref errMsg);
                                }
                                break;
                            case "IM_ORDER":
                                {
                                    var aData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracInterfaceModel.Model.CustOrder.OrdrHdrImport>(jsonData);
                                    ordernum = aData.ORDERNUMBER;
                                    statusCode = ASCTracInterfaceDll.Imports.ImportCustOrder.doImportCustOrder(myClass, aData, ref errMsg);
                                }
                                break;
                            case "IM_ITEM":
                                {
                                    var aData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracInterfaceModel.Model.Item.ItemMasterImport>(jsonData);
                                    ordernum = aData.PRODUCT_CODE;
                                    statusCode = ASCTracInterfaceDll.Imports.ImportItemMaster.doImportItem(myClass, aData, ref errMsg);
                                }
                                break;
                            case "IM_RECV":
                                {
                                    var aData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracInterfaceModel.Model.PO.POHdrImport>(jsonData);
                                    ordernum = aData.PONUMBER;
                                    statusCode = ASCTracInterfaceDll.Imports.ImportPO.doImportPO(myClass, aData, ref errMsg);
                                }
                                break;
                            case "IM_VENDOR":
                                {
                                    var aData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracInterfaceModel.Model.Vendor.VendorImport>(jsonData);
                                    ordernum = aData.VENDOR_CODE;
                                    statusCode = ASCTracInterfaceDll.Imports.ImportVendor.doImportVendor(myClass, aData, ref errMsg);
                                }
                                break;
                            case "WCSPicks":
                            case "WCSRepick":
                            case "WCSUnpick":
                                {
                                    string fImportType = "C";
                                    if (functionID.Equals("WCSRepick"))
                                        fImportType = "R";
                                    if (functionID.Equals("WCSUnpick"))
                                        fImportType = "N";
                                    var aData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracInterfaceModel.Model.WCS.WCSPick>(jsonData);
                                    ordernum = aData.ORDERNUMBER;
                                    statusCode = ASCTracInterfaceDll.WCS.WCSProcess.doWCSPickImport(myClass, fImportType, aData, ref errMsg);
                                }
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    statusCode = HttpStatusCode.InternalServerError;
                    errMsg = ex.Message;
                    myClass.WriteException(functionID, jsonData, ordernum, errMsg, ex.StackTrace);
                }
            }
            string updstr;
            if (statusCode == HttpStatusCode.OK)
            {
                updstr = "RETRY_FLAG='Y'";
            }
            else
            {
                ASCErrorMessageBox("Retry Transaction Failed", errMsg);

                updstr = "RETRY_FLAG='E'";
            }
            updstr = "UPDATE API_LOG SET RETRY_DATETIME = GetDate(), " + updstr + " where id = " + ID;
            {
                {
                    SqlConnection myConnection = new SqlConnection(myConnString);

                    myConnection.Open();
                    SqlCommand myCommand = myConnection.CreateCommand();
                    myCommand.CommandTimeout = 30;
                    SqlTransaction myTransaction = myConnection.BeginTransaction();
                    myCommand.Transaction = myTransaction;
                    int retval = 0;

                    myCommand.CommandText = "SET ANSI_NULLS ON";
                    myCommand.ExecuteNonQuery();
                    myCommand.CommandText = "SET NOCOUNT OFF";
                    myCommand.ExecuteNonQuery();

                    myCommand.CommandText = updstr;

                    retval = myCommand.ExecuteNonQuery();
                    myTransaction.Commit();
                }
            }
        }

        async private void ASCErrorMessageBox( string aTitle, string aMessage)
        {
            var dlg = new ContentDialog
            {
                Title = aTitle,
                Content = aMessage,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dlg.ShowAsync();
        }

        private void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var col in e.NewItems)
                {
                    if (col is DataGridTemplateColumn tempcol)
                        tempcol.Header = "Column 1";
                }
            }
        }

        async private void btnCount_Click(object sender, RoutedEventArgs e)
        {

            var dlg = new ContentDialog
            {
                Title = "API Log",
                Content = "RecordCount is " + myViewModel.myDataTable.Rows.Count.ToString(),
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dlg.ShowAsync();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            FillGrid();
        }

        private async void btnRetry_Click(object sender, RoutedEventArgs e)
        {
            if (myDataGrid.SelectedItems.Count > 0)
            {
                DataRowView row = (DataRowView)myDataGrid.SelectedItem;
                if (row["RETRY_FLAG"].ToString() == "Y")
                    ASCErrorMessageBox("Retry Transaction", "Transaction ID " + row["ID"].ToString() + " has already been reprocessed");
                else
                {
                    var dlg = new ContentDialog
                    {
                        Title = "API Log",
                        Content = "Do you wish to Retry Transaction ID " + row["ID"] + "?",
                        CloseButtonText = "No",
                        PrimaryButtonText = "Yes",
                        XamlRoot = this.XamlRoot
                    };

                    ContentDialogResult ans = await dlg.ShowAsync();
                    if (ans == ContentDialogResult.Primary)
                    {
                        RetryTransaction(row["ID"].ToString(), row["FUNCTION_ID"].ToString());
                        FillGrid();
                    }
                }
            }
        }

        private void dtpStartDate_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {

        }


        private void tabViewMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if((tabViewMain.SelectedItem as TabViewItem ) == tabDetail )
            {
                //ASCErrorMessageBox("Debug", "Detail selected");

            }

        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
          //  myViewModel.SelectedRow = myDataGrid.rows.SelectedItem.
        }

        async private void btnShowData_Click(object sender, RoutedEventArgs e)
        {
            if (myDataGrid.SelectedItems.Count > 0)
            {
                DataRowView row = (DataRowView)myDataGrid.SelectedItem;

                Button button = (Button)sender;
                string recType = button.Content.ToString().Substring(0, 1);
                string ID = row["ID"].ToString();
                string sql = "SELECT MEMO_DATA FROM API_LOG_MEMO_DATA WHERE ID = " + ID + " AND RECTYPE = '" + recType + "'";
                SqlConnection conn = new SqlConnection(myConnString);
                SqlCommand cmd = new SqlCommand(sql, conn);
                try
                {
                    conn.Open();
                    SqlDataReader myReader = cmd.ExecuteReader();
                    if (myReader.Read())
                    {
                        string jsonData = myReader["MEMO_DATA"].ToString();

                        var dlg = new pageDisplayAPIInfo(row, button.Content.ToString(), jsonData);
                        dlg.XamlRoot = this.XamlRoot;

                        await dlg.ShowAsync();
                    }
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        private void cbDateFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( cbDateFilter.SelectedIndex == 4 )
            {
                dtpStartDate.Visibility = Visibility.Visible;
            }
            else
                dtpStartDate.Visibility = Visibility.Collapsed;
        }
    }
}
