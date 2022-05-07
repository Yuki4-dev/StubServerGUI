using System;
using System.Collections.Generic;
using System.Linq;

namespace StubServerGUI.Models
{
    public class HttpResponse
    {
        public static HttpResponse Empty { get; } = new HttpResponse(200, "");

        public int Status { get; }

        public string Body { get; }

        public IEnumerable<HttpValuePair> Headers { get; }

        public IEnumerable<HttpValuePair> Cookies { get; }

        public HttpResponse(int status, string body) : this(status, body, new List<HttpValuePair>(), new List<HttpValuePair>()) { }

        public HttpResponse(int status, string body, IEnumerable<HttpValuePair> headers, IEnumerable<HttpValuePair> cookies)
        {
            Status = status;
            Body = body;
            Headers = headers;
            Cookies = cookies;
        }

        public override string ToString()
        {
            return $"Status : {Status} {Environment.NewLine}" +
                   $"Header : {JoinToString(Headers)} {Environment.NewLine}" +
                   $"Cookie : {JoinToString(Cookies)} {Environment.NewLine}" +
                   $"Body : {Body} {Environment.NewLine}";
        }

        private string JoinToString(IEnumerable<HttpValuePair> httpValuePairs)
        {
            return string.Join(Environment.NewLine, httpValuePairs.Select(p => p.ToString()));
        }

        public static HttpResponse FromJson(HttpResponseJson json)
        {
            return new HttpResponse(json.status ?? 200,
                json.body ?? string.Empty,
                json.header?.Select(h => new HttpValuePair(h.Key, h.Value)) ?? Array.Empty<HttpValuePair>(),
                json.cookie?.Select(h => new HttpValuePair(h.Key, h.Value)) ?? Array.Empty<HttpValuePair>());
        }
    }
}
