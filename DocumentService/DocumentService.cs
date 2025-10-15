using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using DocumentService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Data;
using System.Fabric;

namespace DocumentService
{
    internal sealed class DocumentService : StatefulService
    {
        public DocumentService(StatefulServiceContext context) : base(context) { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[]
            {
                new ServiceReplicaListener(context =>
                    new KestrelCommunicationListener(context, (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(context, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();

                        builder.Services.AddSingleton<StatefulServiceContext>(context);
                        builder.Services.AddSingleton<IReliableStateManager>(this.StateManager);
                        builder.Services.AddSingleton<DocumentDatabaseService>();
                        builder.Services.AddControllers();

                        builder.WebHost
                               .UseKestrel()
                               .UseContentRoot(Directory.GetCurrentDirectory())
                               .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                               .UseUrls(url);

                        var app = builder.Build();

                        app.MapControllers();

                        // Dodaj health check endpoint
                        app.MapGet("/", () => "DocumentService is running!");
                        app.MapGet("/api/health", () => new { Status = "Healthy", Service = "DocumentService", Timestamp = DateTime.UtcNow });

                        return app;
                    }))
            };
        }
    }
}
