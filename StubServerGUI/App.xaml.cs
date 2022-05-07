using System.Windows;

namespace StubServerGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DI.Injection();
        }
    }
}
