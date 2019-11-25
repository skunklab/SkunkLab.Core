using Piraeus.Core.Messaging;
using System;

namespace Piraeus.Grains
{
    [Serializable]
    public class MetricNotificationEventArgs 
    {
        public MetricNotificationEventArgs()
        {

        }
        public MetricNotificationEventArgs(CommunicationMetrics metrics)
        {
            Metrics = metrics;
        }

        public CommunicationMetrics Metrics { get; internal set; }
    }
}
