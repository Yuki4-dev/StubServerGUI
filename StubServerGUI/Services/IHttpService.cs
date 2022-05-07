using StubServerGUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StubServerGUI.Services
{
    public interface IHttpService : IDisposable
    {
        bool IsRunning { get; }

        Task StartAsync(string prefix, Func<HttpRequest, Task<HttpResponse?>> server);

        void Close();
    }
}
