using System;
using System.Diagnostics.Tracing;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;

namespace DocumentService
{
    [EventSource(Name = "MyCompany-CloudApp-DocumentService")]
    internal sealed class ServiceEventSource : EventSource
    {
        public static readonly ServiceEventSource Current = new ServiceEventSource();
        static ServiceEventSource() => Task.Run(() => { });
        private ServiceEventSource() : base() { }

        public static class Keywords
        {
            public const EventKeywords Requests = (EventKeywords)0x1L;
            public const EventKeywords ServiceInitialization = (EventKeywords)0x2L;
        }

        [NonEvent]
        public void ServiceMessage(ServiceContext context, string message)
        {
            if (IsEnabled())
            {
                WriteEvent(1, context.ServiceName.ToString(), context.ServiceTypeName, message);
            }
        }

        [Event(2, Level = EventLevel.Informational)]
        public void ServiceTypeRegistered(int hostProcessId, string serviceType) => WriteEvent(2, hostProcessId, serviceType);

        [Event(3, Level = EventLevel.Error)]
        public void ServiceHostInitializationFailed(string exception) => WriteEvent(3, exception);
    }
}
