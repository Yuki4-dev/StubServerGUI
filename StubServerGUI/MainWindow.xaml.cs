using StubServerGUI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

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
