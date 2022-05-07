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

        public async Task StartAsync(string prefix, Func<HttpRequest, Task<HttpResponse>> server)
        {
            if (_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (IsListening)
            {
                throw new InvalidOperationException("Aleady Start.");
            }

            logger.Info($"Start -> {prefix}");
            _HttpListener = new HttpListener();
            _HttpListener.Prefixes.Add(prefix);
            _HttpListener.Start();

            await Task.Run(async () =>
            {
                while (IsListening)
                {
                    var context = await _HttpListener.GetContextAsync();
                    logger.Info($"Recieve -> {context.Request.Url}");
                    try
                    {
                        await Server(context, server);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                        logger.Error(ex.StackTrace ?? string.Empty);
                    }
                }
            });

        }

        private async Task Server(HttpListenerContext context, Func<HttpRequest, Task<HttpResponse>> server)
        {
            var request = context.Request;
            string requestBody = string.Empty;
            if (request.HasEntityBody)
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    requestBody = reader.ReadToEnd();
                }
                request.InputStream.Close();
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

            var httpRequest = new HttpRequest(request.HttpMethod, request?.Url.AbsolutePath ?? string.Empty, requestBody, headers, cookies, parameters);
            logger.Info($"Request -> {httpRequest}");

            HttpResponse httpResponse;
            try
            {
                httpResponse = await server.Invoke(httpRequest);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace ?? string.Empty);
                return;
            }

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

                using (var writer = new StreamWriter(response.OutputStream, response?.ContentEncoding ?? Encoding.UTF8))
                {
                    writer.Write(httpResponse.Body);
                }

                response.OutputStream.Close();
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
                logger.Warn(ex.Message);
                logger.Warn(ex.StackTrace ?? string.Empty);
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
    }
}
