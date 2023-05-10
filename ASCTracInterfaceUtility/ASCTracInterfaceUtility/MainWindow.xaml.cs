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
using System.IO;
using System.Linq;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Telerik.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Microsoft.UI;
using Windows.ApplicationModel;
using Microsoft.UI.Windowing;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ASCTracInterfaceUtility
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        public const int ICON_SMALL2 = 2;

        public const int WM_GETICON = 0x007F;
        public const int WM_SETICON = 0x0080;

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, uint msg, int wParam, IntPtr lParam);
        private bool fInit = false;
        public MainWindow()
        {
            this.InitializeComponent();
            LoadIcon("asclogo64sq_ico2.ico");
            Title = "ASCTac API Utility";
            Data.DataGlobals.myDBGlobals = new Data.DataGlobals("AliasASCTrac");
            this.Activated += MainWindow_Activated;
        }

            // Handle needs to stay alive as long as this window
            Microsoft.Win32.SafeHandles.SafeFileHandle iIcon;

        private void LoadIcon(string aIconName)
        {
            /*
            IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = AppWindow.GetFromWindowId(windowId); // AppWindow.GetFromWindowId(windowId);
            appWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, aIconName));
            */

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);

            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            appWindow.SetIcon(aIconName);

        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (!fInit)
            {
                if (Data.DataGlobals.myDBGlobals.myDBConnection != null)
                {
                    fInit = true;
                    AddAPILogPage();
                }
                else if (this.Content.XamlRoot != null)
                {
                    fInit = true;
                    OpenDBConnectionSetup("AliasASCTrac");
                }
            }
        }

        private void AddAPILogPage()
        {
            try
            {

            
            var newTab = new RadTabItem();
            newTab.Header = "API Log";
            newTab.PinButtonVisibility = Visibility.Visible;
            newTab.CloseButtonVisibility = Visibility.Visible;

            Frame frame = new Frame();
            newTab.Content = frame;
            frame.Navigate(typeof(pageAPILog));

            mytabview.Items.Add(newTab);
            }
            catch (Exception ex)
            {
                ASCErrorMessageBox("Open API Log Exception", ex.ToString());
            }
        }

        async private void ASCErrorMessageBox(string aTitle, string aMessage)
        {
            var dlg = new ContentDialog
            {
                Title = aTitle,
                Content = aMessage,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dlg.ShowAsync();
        }

        async private void OpenDBConnectionSetup(string alias)
        {
            var dbConnection = Data.DataGlobals.myDBGlobals.myDBConnection;
            if (dbConnection == null)
            {
                dbConnection = new Model.ModelDBConnection("Main");
            }
            var dlg = new pageDBConnection(alias, dbConnection);
            dlg.XamlRoot = this.Content.XamlRoot;

            var ans = await dlg.ShowAsync();
        }

        private void onExit(object sender, RoutedEventArgs e)
        {
            Close();
        }



        private void mnuProcessesDBConnection_Click(object sender, RoutedEventArgs e)
        {
            OpenDBConnectionSetup("AliasASCTrac");
        }

        private void mnuProcessesAPILog_Click(object sender, RoutedEventArgs e)
        {
            foreach( RadTabItem tab in mytabview.Items)
            {
                if (tab.Header.ToString() == "API Log")
                {
                    mytabview.SelectedItem = tab;
                    return;
                }
            }
            AddAPILogPage();
        }
    }
}
