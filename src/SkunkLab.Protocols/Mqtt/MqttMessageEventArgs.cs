using System;

namespace SkunkLab.Protocols.Mqtt
{
    public class MqttMessageEventArgs : EventArgs
    {
        public MqttMessageEventArgs(MqttMessage message)
        {
            Message = message;
        }

        public MqttMessage Message { get; internal set; }
    }
}
