// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Audio;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ASCTracInterfaceUtility
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class pageDBConnection : ContentDialog
    {
        string alias;
        Model.ModelDBConnection DBConnection;
        public pageDBConnection(string _alias, Model.ModelDBConnection aDBConnection)
        {
            this.InitializeComponent();
            alias = _alias;
            Title += "(" + alias + ")";
            DBConnection = aDBConnection;
            DataContext = DBConnection;
            var mylist = Data.DataGlobals.myDBGlobals.myDBConnectionInfo.myDBConnectionList;
            listDatabases.ItemsSource = mylist;
            string dbname = Data.DataGlobals.myDBGlobals.myDBConnectionInfo.GetDBAliasConnectionName(alias);
            int idx = mylist.IndexOf(dbname);
            if (idx >= 0)
                listDatabases.SelectedIndex = idx;
            SetMessages("Current Connection for Alias is " + dbname, "");
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Data.DataGlobals.myDBGlobals.myDBConnectionInfo.SaveDBAlias(alias, DBConnection);
        }

        private void listDatabases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string dbName = listDatabases.SelectedItem as string;
            DataContext = null;

            DBConnection = Data.DataGlobals.myDBGlobals.myDBConnectionInfo.GetDBConnection(dbName);
            DataContext = DBConnection;
        }

        private void SetMessages(string ainfomsg, string errmsg)
        {
            if( String.IsNullOrEmpty(ainfomsg ))
            {
                lblErrorMessage.Text = errmsg;
                lblErrorMessage.Visibility = Visibility.Visible;
                lblInfoMessage.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblInfoMessage.Text = ainfomsg;
                lblInfoMessage.Visibility = Visibility.Visible;
                lblErrorMessage.Visibility = Visibility.Collapsed;
            }
        }

        private bool TestConnection()
        {
            bool retval = true;
            try
            {
                SqlConnection conn = new SqlConnection(DBConnection.myConnString);
                SqlCommand cmd = new SqlCommand("SELECT GETDATE()", conn);
                conn.Open();
            }
            catch (Exception ex)
            {
                retval = false;
                lblErrorMessage.Text = ex.Message;  
            }
            return (retval);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(DBConnection.ConnectionName))
                SetMessages( "", "Connection Name must be entered.");
            else if (TestConnection())
            {
                Data.DataGlobals.myDBGlobals.myDBConnectionInfo.SaveConnection(DBConnection);
                lblErrorMessage.Text = string.Empty;
                SetMessages("Successful", "");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            listDatabases.ItemsSource = null;
            Data.DataGlobals.myDBGlobals.myDBConnectionInfo.DeleteConnection(DBConnection);
            listDatabases.ItemsSource = Data.DataGlobals.myDBGlobals.myDBConnectionInfo.myDBConnectionList;
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            DataContext = null;

            DBConnection = new Model.ModelDBConnection("");
            DataContext = DBConnection;
        }
    }
}
