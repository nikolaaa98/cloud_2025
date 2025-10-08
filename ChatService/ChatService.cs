using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ChatService.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Fabric;

namespace ChatService
{
    internal sealed class ChatService : StatefulService
    {
        public ChatService(StatefulServiceContext context) : base(context) { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[]
            {
                new ServiceReplicaListener(context =>
                    new KestrelCommunicationListener(context, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(context, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();
                        builder.Services.AddSingleton<StatefulServiceContext>(context);
                        builder.Services.AddSingleton<ChatDatabaseService>();
                        builder.Services.AddControllers();

                        builder.WebHost
                               .UseKestrel()
                               .UseContentRoot(Directory.GetCurrentDirectory())
                               .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                               .UseUrls(url);

                        var app = builder.Build();

                        app.MapControllers();
                        app.MapGet("/", () => "ChatService is running!");

                        return app;
                    }))
            };
        }
    }
}
