using System.Collections.Generic;

namespace StubServerGUI.Models
{
    public class HttpResponseJson
    {
        public int? status { get; set; }

        public string? body { get; set; }

        public IDictionary<string, string[]>? header { get; set; }

        public IDictionary<string, string[]>? cookie { get; set; }
    }
}
