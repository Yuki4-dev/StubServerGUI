using System;
using System.Collections.Generic;
using System.Linq;

namespace StubServerGUI.Models
{
    public class HttpRequest
    {
        public string Method { get; }

        public string Uri { get; }

        public string Body { get; }

        public IEnumerable<HttpValuePair> Headers { get; }

        public IEnumerable<HttpValuePair> Cookies { get; }

        public IEnumerable<HttpValuePair> Parameters { get; }

        public HttpRequest(string method, string uri, string body, IEnumerable<HttpValuePair> headers, IEnumerable<HttpValuePair> cookies, IEnumerable<HttpValuePair> parameters)
        {
            Method = method;
            Uri = uri;
            Body = body;
            Headers = headers;
            Cookies = cookies;
            Parameters = parameters;
        }

        public HttpRequestJson ToJson()
        {
            return new HttpRequestJson()
            {
                method = Method,
                body = Body,
                header = Cookies.ToDictionary(h => h.Key, h => h.Values.ToArray()),
                cookie = Cookies.ToDictionary(c => c.Key, c => c.Values.ToArray()),
                parameter = Parameters.ToDictionary(p => p.Key, p => p.Values.ToArray()),
            };

        }

        public override string ToString()
        {
            return $"Method : {Method} {Environment.NewLine}" +
                   $"Uri : {Uri} {Environment.NewLine}" +
                   $"Header : {JoinToString(Headers)} {Environment.NewLine}" +
                   $"Cookie : {JoinToString(Cookies)} {Environment.NewLine}" +
                   $"Parameter : {JoinToString(Parameters)} {Environment.NewLine}" +
                   $"Body : {Body} {Environment.NewLine}";
        }

        private string JoinToString(IEnumerable<HttpValuePair> httpValuePairs)
        {
            return string.Join(Environment.NewLine, httpValuePairs.Select(p => p.ToString()));
        }
    }
}
