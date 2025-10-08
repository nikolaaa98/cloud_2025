using System;
using System.Diagnostics.Tracing;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;

namespace GatewayService
{
    [EventSource(Name = "MyCompany-CloudApp-GatewayService")]
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
        public void Message(string message, params object[] args)
        {
            if (IsEnabled())
            {
                string finalMessage = string.Format(message, args);
                Message(finalMessage);
            }
        }

        private const int MessageEventId = 1;
        [Event(MessageEventId, Level = EventLevel.Informational, Message = "{0}")]
        public void Message(string message) => WriteEvent(MessageEventId, message);

        [NonEvent]
        public void ServiceMessage(ServiceContext serviceContext, string message, params object[] args)
        {
            if (IsEnabled())
            {
                string finalMessage = string.Format(message, args);
                ServiceMessage(
                    serviceContext.ServiceName.ToString(),
                    serviceContext.ServiceTypeName,
                    GetReplicaOrInstanceId(serviceContext),
                    serviceContext.PartitionId,
                    serviceContext.CodePackageActivationContext.ApplicationName,
                    serviceContext.CodePackageActivationContext.ApplicationTypeName,
                    serviceContext.NodeContext.NodeName,
                    finalMessage);
            }
        }

        private const int ServiceMessageEventId = 2;
        [Event(ServiceMessageEventId, Level = EventLevel.Informational, Message = "{7}")]
        private void ServiceMessage(string serviceName, string serviceTypeName, long replicaOrInstanceId, Guid partitionId,
            string applicationName, string applicationTypeName, string nodeName, string message)
        {
            WriteEvent(ServiceMessageEventId, serviceName, serviceTypeName, replicaOrInstanceId, partitionId, applicationName, applicationTypeName, nodeName, message);
        }

        private static long GetReplicaOrInstanceId(ServiceContext context)
        {
            if (context is StatelessServiceContext stateless) return stateless.InstanceId;
            if (context is StatefulServiceContext stateful) return stateful.ReplicaId;
            throw new NotSupportedException("Context type not supported.");
        }

        private const int ServiceTypeRegisteredEventId = 3;
        [Event(ServiceTypeRegisteredEventId, Level = EventLevel.Informational, Message = "Service host process {0} registered service type {1}", Keywords = Keywords.ServiceInitialization)]
        public void ServiceTypeRegistered(int hostProcessId, string serviceType) => WriteEvent(ServiceTypeRegisteredEventId, hostProcessId, serviceType);

        private const int ServiceHostInitializationFailedEventId = 4;
        [Event(ServiceHostInitializationFailedEventId, Level = EventLevel.Error, Message = "Service host initialization failed", Keywords = Keywords.ServiceInitialization)]
        public void ServiceHostInitializationFailed(string exception) => WriteEvent(ServiceHostInitializationFailedEventId, exception);
    }
}
