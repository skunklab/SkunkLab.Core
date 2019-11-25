using System;

namespace SkunkLab.Protocols.Mqtt
{
    public class MqttConnectionArgs : EventArgs
    {
        public MqttConnectionArgs(ConnectAckCode code)
        {
            Code = code;
        }

        public ConnectAckCode Code { get; internal set; }
    }
}
