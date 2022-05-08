using CommunityToolkit.Mvvm.Messaging;
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
    public partial class MainWindow : Window, IRecipient<ShowMessageBoxMessage>
    {
        private readonly ILogger logger;

        private readonly IHttpService httpService;

        private readonly IJavaScriptRunner javaScriptRunner;

        public MainWindow()
        {
            DataContext = DI.Get<MainWindowViewModel>();
            logger = DI.Get<ILogger>();
            httpService = DI.Get<IHttpService>();
            javaScriptRunner = DI.Get<IJavaScriptRunner>();

            InitializeComponent();
            WeakReferenceMessenger.Default.RegisterAll(this);

            JavaScriptWebView.Source = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\html\index.html", UriKind.Absolute);
            javaScriptRunner.SetWebView(JavaScriptWebView);

            if (logger is BindableLogger bindable)
            {
                bindable.Bind(Write);
            }
        }

        public void Receive(ShowMessageBoxMessage message)
        {
            MessageBox.Show(message.Message, message.Title);
        }

        private void JavaScriptWebView_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            if (JavaScriptWebView.CoreWebView2 != null)
            {
                var viewModel = (MainWindowViewModel)DataContext;
                JavaScriptWebView.CoreWebView2.AddHostObjectToScript("model", viewModel.Model);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
            httpService.Dispose();
        }

        private void Write(string text)
        {
            LogTextBox.Dispatcher.InvokeAsync(() =>
            {
                LogTextBox.AppendText(text + "\r\n");
            });
        }
    }

    public class ShowMessageBoxMessage
    {
        public string Message { get; }

        public string Title { get; }

        public ShowMessageBoxMessage(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }

}
