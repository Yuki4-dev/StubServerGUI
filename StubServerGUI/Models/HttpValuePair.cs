﻿using System.Collections.Generic;
using System.Linq;

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
            return string.Join(',', Values.Select(value => Key + ":" + value));
        }
    }
}
