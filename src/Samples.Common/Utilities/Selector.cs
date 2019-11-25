using Samples.Common.Channels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Samples.Common.Protocols;

namespace Samples.Common.Utilities
{
    public static class Selector
    {
        public static ChannelType GetChannel()
        {
            string channelNo = null;
            string[] array = new string[] { "1", "2", "3", "4" };

            while (channelNo == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("----- Select Channel -----");
                Console.WriteLine("(1) Web Socket");
                Console.WriteLine("(2) TCP");
                Console.WriteLine("(3) UDP");
                Console.WriteLine("(4) HTTP");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Enter # of Channel ? ");
                channelNo = Console.ReadLine();
                Console.ResetColor();
                if (array.Contains(channelNo))
                {
                    break;
                }
                else
                {
                    channelNo = null;
                }
            }

            return Enum.Parse<ChannelType>(channelNo);
        }

        public static ProtocolType GetProtocol(ChannelType channel)
        {
            string protocolNo = null;
            string[] array = null;

            while (protocolNo == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("----- Select Protocol -----");
                if (channel == ChannelType.Http)
                {
                    array = new string[] { "3" };
                    Console.WriteLine("(3) REST");
                }
                else if (channel == ChannelType.WebSocket)
                {
                    array = new string[] { "1", "2", "4" };
                    Console.WriteLine("(1) MQTT");
                    Console.WriteLine("(2) CoAP");
                    Console.WriteLine("(4) Web Socket Native");
                }
                else if (channel == ChannelType.TCP)
                {
                    array = new string[] { "1", "2" };
                    Console.WriteLine("(1) MQTT");
                    Console.WriteLine("(2) CoAP");
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Enter # of Protocol ? ");
                Console.ResetColor();

                if (array.Contains(protocolNo))
                {
                    break;
                }
                else
                {
                    protocolNo = null;
                }
            }

            return Enum.Parse<ProtocolType>(protocolNo);
        }
    }
}
