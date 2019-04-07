﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProgramaPontos.Domain.Core.Events;
using ProgramaPontos.Infra.Ioc.AspNetCore;
using System;
using System.Reflection;

namespace ProgramaPontos.EventHandler.Sinc
{
    class Program
    {
        private static IConfigurationRoot configuration;
        private static IServiceProvider serviceProvider;

        static void Main(string[] args)
        {
            SetTitle();
            LoadConfiguration();
            BuildServiceProvider();
            StarBus();

            Console.WriteLine("Waiting for events...");
            Console.Read();

        }

        private static void SetTitle()
        {
            Console.Title = Assembly.GetExecutingAssembly().GetName().Name;
        }

        private static void StarBus()
        {
            var bus = serviceProvider.GetService<IEventBus>();
            bus.OnRaiseEvent = (e) => { onRaiseEvent(e); };
            bus.Consume();
        }

        private static void onRaiseEvent(IDomainEvent e)
        {
            Console.WriteLine(e.GetType().ToString());
            var handlerInterfaceType = typeof(IDomainEventHandler<>).MakeGenericType(e.GetType());
            var handler = serviceProvider.GetService(handlerInterfaceType);
            var method = handler?.GetType().GetMethod("Handle", new Type[] { e.GetType() });
            method?.Invoke(handler, new[] { e });

        }

        private static void BuildServiceProvider()
        {
            serviceProvider = new ServiceCollection()
                    .AddProgramaPontosServices(configuration)
                    .AddDomainEventHandlers()
                    .BuildServiceProvider();

        }

        static void LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            configuration = builder.Build();





        }
    }
}
