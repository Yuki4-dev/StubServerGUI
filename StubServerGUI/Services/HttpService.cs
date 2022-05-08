using Newtonsoft.Json;
using StubServerGUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StubServerGUI.Services
{
    public class HttpService : IHttpService
    {
        private bool _IsDisposed = false;

        private readonly ILogger logger;

        private HttpListener? _HttpListener = null;

        public bool IsListening => _HttpListener?.IsListening == true;

        public HttpService(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task ListenAsync(string prefix, Func<HttpRequest, Task<HttpResponse>> server)
        {
            if (_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (IsListening)
            {
                throw new InvalidOperationException("Aleady Start.");
            }

            logger.Info($"Open Server -> {prefix}");
            _HttpListener = new HttpListener();
            _HttpListener.Prefixes.Add(prefix);
            _HttpListener.Start();

            logger.Info($"Listen Server -> {prefix}");
            await Task.Run(async () =>
            {
                while (IsListening)
                {
                    try
                    {
                        var context = await _HttpListener.GetContextAsync();
                        await Server(context, server);
                    }
                    catch (Exception ex)
                    {
                        Error(ex);
                        throw;
                    }
                }
            });
        }

        private async Task Server(HttpListenerContext context, Func<HttpRequest, Task<HttpResponse>> server)
        {
            var request = context.Request;
            logger.Info($"Recieve Url -> {request.Url}");
            logger.Info($"Remote EndPoint -> {request.RemoteEndPoint}");
            logger.Info($"Local EndPoint -> {request.LocalEndPoint}");
            logger.Info($"Content Length -> {request.ContentLength64}");

            string requestBody = string.Empty;
            if (request.HasEntityBody)
            {
                using (var stream = request.InputStream)
                using (var reader = new StreamReader(stream, request.ContentEncoding))
                {
                    requestBody = reader.ReadToEnd();
                }
            }

            var headers = new List<HttpValuePair>();
            foreach (var key in request.Headers.AllKeys)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    var values = (request.Headers[key] ?? string.Empty).Split(',');
                    headers.Add(new HttpValuePair(key, values));
                }
            }

            var cookies = new List<HttpValuePair>();
            if (request.Cookies != null)
            {
                foreach (var cookie in request.Cookies.Cast<Cookie>())
                {
                    cookies.Add(new HttpValuePair(cookie.Name, cookie.Value));
                }
            }

            var parameters = new List<HttpValuePair>();
            if (request.QueryString.HasKeys())
            {
                foreach (var key in request.QueryString.AllKeys)
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        var values = request.QueryString.GetValues(key) ?? Array.Empty<string>();
                        parameters.Add(new HttpValuePair(key, values));
                    }
                }
            }

            var httpRequest = new HttpRequest(request.HttpMethod, request.Url?.AbsolutePath ?? string.Empty, requestBody, headers, cookies, parameters);
            logger.Info($"Request -> {JsonConvert.SerializeObject(httpRequest)}");

            var httpResponse = await server.Invoke(httpRequest);
            logger.Info($"Response -> {JsonConvert.SerializeObject(httpResponse)}");

            using (var response = context.Response)
            {
                response.StatusCode = httpResponse.Status;

                foreach (var headerPair in httpResponse.Headers)
                {
                    foreach (var value in headerPair.Values)
                    {
                        response.Headers.Add(headerPair.Key, value);
                    }
                }

                foreach (var cookiePair in httpResponse.Cookies)
                {
                    foreach (var value in cookiePair.Values)
                    {
                        response.Cookies.Add(new Cookie(cookiePair.Key, value));
                    }
                }

                using (var stream = response.OutputStream)
                using (var writer = new StreamWriter(stream, response?.ContentEncoding ?? Encoding.UTF8))
                {
                    writer.Write(httpResponse.Body);
                }
            }
        }

        public void Stop()
        {
            if (_IsDisposed || _HttpListener == null)
            {
                return;
            }

            try
            {
                _HttpListener.Stop();
                _HttpListener.Close();
            }
            catch (Exception ex)
            {
                Error(ex);
            }
            finally
            {
                _HttpListener = null;
            }
        }

        public void Dispose()
        {
            Stop();
            _IsDisposed = true;
        }

        private void Error(Exception ex)
        {
            logger.Error(ex.Message);
#if DEBUG
            logger.Error(ex.StackTrace ?? string.Empty);
#endif
        }
    }
}
