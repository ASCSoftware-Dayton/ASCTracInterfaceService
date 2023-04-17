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
using System.Runtime.InteropServices.WindowsRuntime;
using Telerik.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ASCTracInterfaceUtility
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            Title = "ASC API Utility";
            AddAPILogPage();
        }

        private void AddAPILogPage()
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

    }
}
