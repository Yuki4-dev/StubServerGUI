using StubServerGUI.Models;
using System;
using System.Threading.Tasks;

namespace StubServerGUI.Services
{
    public interface IHttpService : IDisposable
    {
        bool IsListening { get; }

        Task StartAsync(string prefix, Func<HttpRequest, Task<HttpResponse>> server);

        void Stop();
    }
}
