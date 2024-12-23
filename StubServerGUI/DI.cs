﻿using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using StubServerGUI.Services;
using System;

namespace StubServerGUI
{
    public class DI
    {
        public static T Get<T>() where T : class
        {
            var services = Ioc.Default.GetService<T>() ?? throw new InvalidOperationException($"UnRegister : {typeof(T)}.");
            return services;
        }

        public static void Injection()
        {
            var sc = new ServiceCollection();
            sc.AddSingleton<MainWindowViewModel>();
            sc.AddSingleton<ILogger, BindableLogger>();
            sc.AddSingleton<IHttpService, HttpService>();
            sc.AddSingleton<IJavaScriptRunner, JavaScriptRunner>();

            Ioc.Default.ConfigureServices(sc.BuildServiceProvider());
        }
    }
}
