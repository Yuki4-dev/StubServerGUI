using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StubServerGUI.Models
{
    public class HttpValuePair
    {
        public string Key { get; }

        public IEnumerable<string> Values { get; }

        public HttpValuePair(string key, string value) : this(key, new string[] { value }) { }

        public HttpValuePair(string key, IEnumerable<string> values)
        {
            Key = key;
            Values = values;
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Values.Select(value => Key + ":" + value));
        }
    }
}
