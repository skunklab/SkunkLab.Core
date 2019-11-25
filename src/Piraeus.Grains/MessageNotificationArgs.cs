using Piraeus.Core.Messaging;
using System;

namespace Piraeus.Grains
{
    [Serializable]
    public class MessageNotificationArgs 
    {
        public MessageNotificationArgs(EventMessage message)
        {
            Message = message;
            Timestamp = DateTime.UtcNow;
        }

        public EventMessage Message { get; internal set; }

        public DateTime? Timestamp { get; internal set; }
    }
}
