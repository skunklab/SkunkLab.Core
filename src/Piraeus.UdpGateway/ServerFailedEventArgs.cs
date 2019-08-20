using System;

namespace Piraeus.UdpGateway
{
    public class ServerFailedEventArgs : EventArgs
    {
        public ServerFailedEventArgs(Exception error, string channelType, int port)
        {
            Error = error;
            ChannelType = channelType;
            Port = port;
        }

        public Exception Error { get; internal set; }
        public string ChannelType { get; internal set; }

        public int Port { get; internal set; }
    }
}
