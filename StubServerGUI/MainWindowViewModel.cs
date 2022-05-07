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

namespace StubServerGUI
{
    [INotifyPropertyChanged]
    public partial class MainWindowViewModel
    {
        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string uri = string.Empty;

        [ObservableProperty]
        private string responseStatus = string.Empty;

        [ObservableProperty]
        private string responseCookie = string.Empty;

        [ObservableProperty]
        private string responseHeader = string.Empty;

        [ObservableProperty]
        private string responseBody = string.Empty;

        [ObservableProperty]
        private string responseBodyFilePath = string.Empty;

        [ObservableProperty]
        private bool responseJavaScriptEnable = true;

        public MonacoModel Model { get; }

        private readonly IHttpService httpService;

        private readonly IJavaScriptRunner javaScriptRunner;

        public MainWindowViewModel() : this(DI.Get<IHttpService>(), DI.Get<IJavaScriptRunner>()) { }

        public MainWindowViewModel(IHttpService httpService, IJavaScriptRunner javaScriptRunner)
        {
            Model = new MonacoModel()
            {
                Language = "javascript",
                Text = "function server(request){\r\n    return {\"status\" : 200, \"body\" : \"OK\"}\r\n}"
            };

            this.httpService = httpService;
            this.javaScriptRunner = javaScriptRunner;
        }

        [ICommand]
        private void StartServer()
        {
            if (httpService.IsRunning)
            {
                return;
            }

            httpService.StartAsync("http://localhost:8080/", async (request) =>
           {
               if (!Regex.IsMatch(request.Uri, Uri))
               {
                   return null;
               }

               if (ResponseJavaScriptEnable)
               {
                   var script = GetJavaScript(request.ToJson());
                   var jsonResult = await javaScriptRunner.RunAsync(script);
                   var response = JsonConvert.DeserializeObject<HttpResponseJson>(jsonResult);
                   if(response == null)
                   {
                       return null;
                   }
                   return HttpResponse.FromJson(response);
               }
               else
               {
                   return new HttpResponse(int.Parse(responseStatus), responseBody);
               }
           });
        }

        private string GetJavaScript(HttpRequestJson requestJson)
        {
            var script = "(function(){" +
                $"var request = {JsonConvert.SerializeObject(requestJson)};" +
                $"return ({Model.Text}(request))" +
                "}());";

            return script;
        }
    }
}
