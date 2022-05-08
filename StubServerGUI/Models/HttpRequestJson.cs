using System.Collections.Generic;

namespace StubServerGUI.Models
{
    public class HttpRequestJson
    {
        public string? method { get; set; }

        public string? uri { get; set; }

        public string? body { get; set; }

        public IDictionary<string, string[]>? header { get; set; }

        public IDictionary<string, string[]>? cookie { get; set; }

        public IDictionary<string, string[]>? parameter { get; set; }
    }
}
