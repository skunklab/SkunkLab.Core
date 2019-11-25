using Piraeus.Core.Metadata;
using System;
using System.Collections.Generic;

namespace Piraeus.GrainInterfaces
{
    [Serializable]
    public class PiSystemState
    {
        public PiSystemState()
        {
        }

        #region Metrics
        public long MessageCount { get; set; }
        public long ByteCount { get; set; }
        public long ErrorCount { get; set; }
        public DateTime? LastErrorTimestamp { get; set; }
        public DateTime? LastMessageTimestamp { get; set; }
        #endregion

        public EventMetadata Metadata { get; set; }

        public Dictionary<string, ISubscription> Subscriptions;

        public Dictionary<string, IMetricObserver> MetricLeases { get; set; }

        public Dictionary<string, IErrorObserver> ErrorLeases { get; set; }

        public Dictionary<string, Tuple<DateTime, string>> LeaseExpiry { get; set; }
    }
}
