using Piraeus.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Piraeus.TcpGateway
{
    [Serializable]
    public class TcpGatewayOptions
    {
        public TcpGatewayOptions()
        {

        }

        public bool IsLocal { get; set; } = false;
    }
}
