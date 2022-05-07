using StubServerGUI.Services;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace StubServerGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            JavaScriptWebView.Source = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\html\index.html", UriKind.Absolute);
            DI.Get<IJavaScriptRunner>().SetWebView(JavaScriptWebView);
        }

        private void JavaScriptWebView_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            if (JavaScriptWebView.CoreWebView2 != null)
            {
                var viewModel = (MainWindowViewModel)DataContext;
                JavaScriptWebView.CoreWebView2.AddHostObjectToScript("model", viewModel.Model);
            }
        }
    }
}
