﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                cookie = Cookies.ToDictionary( c => c.Key, c => c.Values.ToArray()),
                parameter = Parameters.ToDictionary(p => p.Key, p => p.Values.ToArray()),
            };

        }

        public override string ToString()
        {
            return $"Method : {Method} {Environment.NewLine}" +
                   $"Uri : {Uri} {Environment.NewLine}" +
                   $"Header : {Headers} {Environment.NewLine}" +
                   $"Cookie : {Cookies} {Environment.NewLine}" +
                   $"Parameter : {Parameters} {Environment.NewLine}" +
                   $"Body : {Body} {Environment.NewLine}";
        }
    }
}
