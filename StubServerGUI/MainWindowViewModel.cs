using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StubServerGUI.Services;
using StubServerGUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Models;
using Newtonsoft.Json;
using System.IO;

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

        public MainWindowViewModel() : this(DI.Get<ILogger>(), DI.Get<IHttpService>(), DI.Get<IJavaScriptRunner>()) { }

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
                await httpService.StartAsync(url, Server);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace ?? string.Empty);
            }
            finally
            {
                CanStart = true;
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

            httpService.Stop();
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
                catch(Exception ex)
                {
                    logger.Error(ex.Message);
                    logger.Error(ex.StackTrace ?? string.Empty);
                }
            }

            var script = GetJavaScript(request.ToJson(), new MetaJson() { file = fileValue });
            var javaScriptResult = await javaScriptRunner.RunAsync(script);
            logger.Info($"JavaScript Result -> {javaScriptResult ?? "null"}");
            if(javaScriptResult == null)
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
                       $" *     file : string (Local File)\r\n" +
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
    }
}
