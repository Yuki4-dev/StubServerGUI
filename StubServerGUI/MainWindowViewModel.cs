using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Models;
using Newtonsoft.Json;
using StubServerGUI.Models;
using StubServerGUI.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StubServerGUI
{
    [INotifyPropertyChanged]
    public partial class MainWindowViewModel
    {
        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private bool canStart = true;

        [ObservableProperty]
        private string url = "http://localhost:8080/";

        [ObservableProperty]
        private string filePath = string.Empty;

        public MonacoModel Model { get; }

        private readonly ILogger logger;

        private readonly IHttpService httpService;

        private readonly IJavaScriptRunner javaScriptRunner;

        public MainWindowViewModel(ILogger logger, IHttpService httpService, IJavaScriptRunner javaScriptRunner)
        {
            this.logger = logger;
            this.httpService = httpService;
            this.javaScriptRunner = javaScriptRunner;
            Model = GetMonacoModel();
        }

        [ICommand]
        private async Task StartServer()
        {
            if (httpService.IsListening)
            {
                logger.Info("Server Opened.");
                return;
            }

            CanStart = false;
            try
            {
                logger.Info("Server Start.");
                await httpService.ListenAsync(url, Server);
            }
            catch (Exception ex)
            {
                Error(ex);
                WeakReferenceMessenger.Default.Send(new ShowMessageBoxMessage("Error", ex.Message));
            }
            finally
            {
                Stop();
            }
        }

        [ICommand]
        private void StopServer()
        {
            if (!httpService.IsListening)
            {
                logger.Info("Server Not Opened.");
                return;
            }

            Stop();
        }

        private async Task<HttpResponse> Server(HttpRequest request)
        {
            string fileValue = string.Empty;
            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    fileValue = File.ReadAllText(filePath);
                }
                catch (Exception ex)
                {
                    Error(ex);
                }
            }

            var script = GetJavaScript(request.ToJson(), new MetaJson() { file = fileValue });
            var javaScriptResult = await javaScriptRunner.RunAsync(script);
            logger.Info($"JavaScript Result -> {javaScriptResult ?? "null"}");
            if (javaScriptResult == null)
            {
                return HttpResponse.Empty;
            }

            var response = JsonConvert.DeserializeObject<HttpResponseJson>(javaScriptResult);
            if (response == null)
            {
                return HttpResponse.Empty;
            }

            return HttpResponse.FromJson(response);
        }

        private void Stop()
        {
            logger.Info("Server Stop.");
            httpService.Stop();
            CanStart = true;
        }

        private MonacoModel GetMonacoModel()
        {
            return new MonacoModel()
            {
                Language = "javascript",
                Text = "/**\r\n" +
                       " * request is\r\n" +
                       " * {\r\n" +
                       " *     uri : string \r\n" +
                       " *     method : 'GET'|'POST'|'PUT'|'DELETE' \r\n" +
                       " *     body : string \r\n" +
                       " *     header : map<string,string[]> \r\n" +
                       " *     cookie : map<string,string[]> \r\n" +
                       " *     parameter : map<string,string[]> (QueryString) \r\n" +
                       " * }\r\n" +
                       " * \r\n" +
                       " * meta is\r\n" +
                       " * {\r\n" +
                       $" *     file : string (LocalFile Value)\r\n" +
                       " * }\r\n" +
                       " * \r\n" +
                       " * return value is\r\n" +
                       " * {\r\n" +
                       " *     status : number \r\n" +
                       " *     body : string \r\n" +
                       " *     header : map<string,string[]> \r\n" +
                       " *     cookie : map<string,string[]> \r\n" +
                       " * }\r\n" +
                       " */\r\n" +
                       "function server(request, meta){\r\n    return {\"status\" : 200, \"body\" : \"OK\"}\r\n}"
            };
        }

        private string GetJavaScript(HttpRequestJson requestJson, MetaJson metaJson)
        {
            return "(function(){" +
                $"var request = {JsonConvert.SerializeObject(requestJson)};" +
                $"var meta = {JsonConvert.SerializeObject(metaJson)};" +
                $"return ({Model.Text}(request,meta))" +
                "}());";
        }

        private void Error(Exception ex)
        {
            logger.Error(ex.Message);
#if DEBUG
            logger.Error(ex.StackTrace ?? string.Empty);
#endif
        }
    }
}
