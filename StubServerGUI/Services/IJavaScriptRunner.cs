using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StubServerGUI.Services
{
    public interface IJavaScriptRunner
    {
        void SetWebView(WebView2 webView);

        Task<string> RunAsync(string javaScript);
    }
}
