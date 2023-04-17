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
        private DataTable myDataTable; // = new DataTable();
        string myConnString = "password=WeH73w;workstation id=dev;packet size=4096;user id=app_user;data source=asc-cin-app01;persist security info=False;initial catalog=ASCTrac904Dev";

        public pageAPILog()
        {
            this.InitializeComponent();
            dtpStartDate.Date = DateTime.Now.Date;
            //FillGrid();
        }

        private void FillGrid()
        {
            string sql = "SELECT * FROM API_LOG ";
            sql += " WHERE START_DATETIME>='" + dtpStartDate.Date.Date.ToString() + "' AND START_DATETIME<'" + dtpStartDate.Date.Date.AddDays(1).ToString() + "'";

            SqlConnection conn = new SqlConnection(myConnString);
            SqlCommand cmd = new SqlCommand(sql, conn);
            conn.Open();

            myDataTable = new DataTable();
            //myDataTable.DefaultView
            // create data adapter
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            // this will query your database and return the result to your datatable
            da.Fill(myDataTable);
            conn.Close();
            da.Dispose();

            var myDataList = myDataTable.Rows.Cast<DataRow>().ToList();

            myDataGrid.AutoGenerateColumns = false;
            //myDataGrid.Columns.CollectionChanged += Columns_CollectionChanged;
            //GridMain.DataContext = myDataTable;
            //myDataGrid.ItemsSource = myDataTable;
            myDataGrid.ItemsSource = myDataList;
            //collectionNavigator.DataContext = myDataTable;
            // collectionNavigator.Source = myDataList;
            //collectionNavigator.CollectionView = myDataTable;
            myDataGrid.UserEditMode = DataGridUserEditMode.None;

           
            foreach (DataColumn col in myDataTable.Columns)
            {
                var gridcol = new DataGridTextColumn();
                gridcol.PropertyName = col.ColumnName;
                string titleStr = col.ColumnName.ToString().ToLower().Replace("_", " ");
                gridcol.Header = titleStr.Substring(0, 1).ToUpper() + titleStr.Substring(1);

                myDataGrid.Columns.Add(gridcol);
            }
           
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

        private void RetryTransaction(string ID, string functionID)
        {
            HttpStatusCode statusCode = HttpStatusCode.Accepted;
            string errMsg = string.Empty;
            var myClass = ASCTracInterfaceDll.Class1.InitParse("Retry " + functionID, functionID, ref errMsg);
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
                            case "IM_ORDER":
                                {
                                    var aData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracInterfaceModel.Model.CustOrder.OrdrHdrImport>(jsonData);
                                    ordernum = aData.ORDERNUMBER;
                                    statusCode = ASCTracInterfaceDll.Imports.ImportCustOrder.doImportCustOrder(myClass, aData, ref errMsg);
                                }
                                break;
                            case "WCSPicks":
                                {
                                    var aData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracInterfaceModel.Model.WCS.WCSPick>(jsonData);
                                    ordernum = aData.ORDERNUMBER;
                                    statusCode = ASCTracInterfaceDll.WCS.WCSProcess.doWCSPickImport(myClass, "C", aData, ref errMsg);
                                }
                                break;
                            case "IM_RECV":
                                {
                                    var aData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracInterfaceModel.Model.PO.POHdrImport>(jsonData);
                                    ordernum = aData.PONUMBER;
                                    statusCode = ASCTracInterfaceDll.Imports.ImportPO.doImportPO(myClass, aData, ref errMsg);
                                }
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    statusCode = HttpStatusCode.InternalServerError;
                    errMsg = ex.Message;
                    ASCTracInterfaceDll.Class1.WriteException(functionID, jsonData, ordernum, errMsg, ex.StackTrace);
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
                Content = "RecordCount is " + myDataTable.Rows.Count.ToString(),
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
    }
}
