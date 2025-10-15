using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using GatewayService.Services;

namespace GatewayService
{
    internal sealed class GatewayService : StatelessService
    {
        public GatewayService(StatelessServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();

                        //  DODAJ CORS
                        builder.Services.AddCors(options =>
                        {
                            options.AddPolicy("AllowFrontend",
                                policy =>
                                {
                                    policy
                                        .WithOrigins("http://localhost:5173")
                                        .AllowAnyHeader()
                                        .AllowAnyMethod()
                                        .AllowCredentials();
                                });
                        });

                        //  DODAJ POTREBNE SERVISE
                        builder.Services.AddSingleton<StatelessServiceContext>(serviceContext);
                        
                        //  DODAJ HTTP CLIENT (JEDAN PUT!)
                        builder.Services.AddHttpClient<HttpForwardingService>(client =>
                        {
                            client.Timeout = TimeSpan.FromSeconds(30);
                        });
                        
                        //  DODAJ GENERAL HTTP CLIENT ZA CHATCONTROLLER
                        builder.Services.AddHttpClient(); // Ovo je za IHttpClientFactory

                        builder.Services.AddControllers();

                        //  KONFIGURIŠI WEB HOST
                        builder.WebHost
                               .UseKestrel()
                               .UseContentRoot(Directory.GetCurrentDirectory())
                               .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                               .UseUrls(url);

                        var app = builder.Build();

                        //  MIDDLEWARE REDOSLED
                        app.UseCors("AllowFrontend");   // CORS
                        app.UseRouting();               // Routing
                        app.MapControllers();           // Mapiraj sve API kontrolere

                        return app;
                    }))
            };
        }
    }
}