using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Models
{
    [ComVisible(true)]
    public class MonacoModel
    {
        public event Action<MonacoModel>? TextChanged;

        private string _Text = string.Empty;
        public string Text
        {
            get => _Text;
            set
            {
                if (_Text != value)
                {
                    _Text = value;
                    TextChanged?.Invoke(this);
                }
            }
        }

        public string Language { get; set; } = string.Empty;
    }
}
