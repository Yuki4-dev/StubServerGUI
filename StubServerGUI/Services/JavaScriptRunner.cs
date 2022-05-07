using Microsoft.Web.WebView2.Wpf;
using System;
using System.Threading.Tasks;

namespace StubServerGUI.Services
{
    public class JavaScriptRunner : IJavaScriptRunner
    {
        private WebView2? _WebView2 = null;

        private readonly ILogger logger;

        public JavaScriptRunner(ILogger logger)
        {
            this.logger = logger;
        }

        public void SetWebView(WebView2 webView)
        {
            _WebView2 = webView ?? throw new ArgumentNullException(nameof(webView));
        }

        public async Task<string?> RunAsync(string javaScript)
        {
            if (_WebView2 == null)
            {
                throw new InvalidOperationException("WebView is null.");
            }

            logger.Info($"Run Script -> {javaScript}");
            return await _WebView2.Dispatcher.Invoke(() =>
            {
                return _WebView2.ExecuteScriptAsync(javaScript);
            });
        }
    }
}
