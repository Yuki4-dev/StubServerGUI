using System.Runtime.InteropServices;

namespace Models
{
    [ComVisible(true)]
    public class MonacoModel
    {
        public string Text { get; set; } = string.Empty;

        public string Language { get; set; } = string.Empty;
    }
}
