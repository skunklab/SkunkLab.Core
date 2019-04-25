using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Protocols.Mqtt;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samples.Mqtt.Client
{
    /* NOTE:  This sample works with the sample configuration script "SampleClient.ps1" */

    class Program
    {
        static CancellationTokenSource cts;
        static PiraeusMqttClient mqttClient;
        static int index;
        static IChannel channel;
        static string name;
        static string role;
        static bool send;
        static int channelNum;
        static string hostname;

        static void Main(string[] args)
        {           
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            cts = new CancellationTokenSource();

            if (args == null || args.Length == 0)
            {
                UseUserInput();
            }
            else
            {
                Console.WriteLine("Invalid user input");
                Console.ReadKey();
                return;
            }
                

            string token = GetSecurityToken(name, role);

            //create the channel
            channel = CreateChannel(token, cts);

            //add some channel events
            channel.OnClose += Channel_OnClose;
            channel.OnError += Channel_OnError; 
            channel.OnOpen += Channel_OnOpen;

            mqttClient = new PiraeusMqttClient(new SkunkLab.Protocols.Mqtt.MqttConfig(180.0), channel);

            Task task = StartMqttClientAsync(token);
            Task.WaitAll(task);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            cts.Cancel();
        }

        #region User Input
        static void UseUserInput()
        {
            WriteHeader();
            hostname = SelectHostname();
            name = SelectName();
            role = SelectRole();
            channelNum = SelectChannel();

            
        }

        #endregion

        #region Utilities

        static void WriteHeader()
        {
            PrintMessage("-------------------------------------------------------------------", ConsoleColor.White);
            PrintMessage("                       MQTT Sample Client", ConsoleColor.Cyan);
            PrintMessage(" Run SampleClientConfig.ps1 script prior to running this sample", ConsoleColor.Yellow);
            PrintMessage("-------------------------------------------------------------------", ConsoleColor.White);
            PrintMessage("press any key to contiune...", ConsoleColor.White);
            Console.ReadKey();
        }
        static void PrintMessage(string message, ConsoleColor color, bool section = false, bool input = false)
        {
            Console.ForegroundColor = color;
            if(section)
            {
                Console.WriteLine($"---   {message} ---");
            }
            else
            {
                if (!input)
                {
                    Console.WriteLine(message);
                }
                else
                {
                    Console.Write(message);
                }
            }


            Console.ResetColor();
        }
        static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception.Flatten().InnerException.Message);
        }

        #endregion

        #region MQTT Client
        static async Task StartMqttClientAsync(string token)
        {            
            ConnectAckCode code = await MqttConnectAsync(token);          
            if (code != ConnectAckCode.ConnectionAccepted)
                return;

            string observableEvent = role == "A" ? "http://www.skunklab.io/resource-b" : "http://www.skunklab.io/resource-a";

            try
            {                
                await mqttClient.SubscribeAsync(observableEvent, QualityOfServiceLevelType.AtLeastOnce, ObserveEvent).ContinueWith(SendMessages);
                
            }
            catch(Exception ex)
            {
                PrintMessage("Error", ConsoleColor.Red, true);
                PrintMessage(ex.Message, ConsoleColor.Red);
                Console.ReadKey();
            }
        }

        static void SendMessages(Task task)
        { 
            try
            {
                if (!send)
                {
                    PrintMessage("Do you want to send messages (Y/N) ? ", ConsoleColor.Cyan, false, true);
                    string sendVal = Console.ReadLine();
                    if (sendVal.ToUpperInvariant() != "Y")
                        return;
                }
                send = true;

                PrintMessage("Enter # of messages to send ? ", ConsoleColor.Cyan, false, true);
                string nstring = Console.ReadLine();

                int numMessages = Int32.Parse(nstring);
                PrintMessage("Enter delay between messages in milliseconds ? ", ConsoleColor.Cyan, false, true);
                string dstring = Console.ReadLine().Trim();

                int delayms = Int32.Parse(dstring);

                DateTime startTime = DateTime.Now;
                for (int i = 0; i < numMessages; i++)
                {
                    index++;
                    string payloadString = String.Format($"{DateTime.Now.Ticks}:{name}-message {index}");
                    byte[] payload = Encoding.UTF8.GetBytes(payloadString);
                    string publishEvent = role == "A" ? "http://www.skunklab.io/resource-a" : "http://www.skunklab.io/resource-b";
                    Task pubTask = mqttClient.PublishAsync(QualityOfServiceLevelType.AtMostOnce, publishEvent, "text/plain", payload);
                    Task.WhenAll(pubTask);

                    if (delayms > 0)
                    {
                        Task t = Task.Delay(delayms);
                        Task.WaitAll(t);
                    }
                }

                DateTime endTime = DateTime.Now;
                PrintMessage($"Total send time {endTime.Subtract(startTime).TotalMilliseconds} ms", ConsoleColor.White);

                PrintMessage("Send more messages (Y/N) ? ", ConsoleColor.Cyan, false, true);
                string val = Console.ReadLine();
                if (val.ToUpperInvariant() == "Y")
                {
                    SendMessages(task);
                }
            }
            catch(Exception ex)
            {
                PrintMessage("Error", ConsoleColor.Red, true);
                PrintMessage(ex.Message, ConsoleColor.Red);
                Console.ReadKey();
            }

        }

        static async Task<ConnectAckCode> MqttConnectAsync(string token)
        {
            PrintMessage("Trying to connect", ConsoleColor.Cyan, true);
            string sessionId = Guid.NewGuid().ToString();
            ConnectAckCode code = await mqttClient.ConnectAsync(sessionId, "JWT", token, 90);
            PrintMessage($"MQTT connection code {code}", code == ConnectAckCode.ConnectionAccepted ? ConsoleColor.Green : ConsoleColor.Red, false);

            return code;
        }
               
        
        static void ObserveEvent(string topic, string contentType, byte[] message)
        {
            long nowTicks = DateTime.Now.Ticks;
            Console.ForegroundColor = ConsoleColor.Green;
            string msg = Encoding.UTF8.GetString(message);
            string[] split = msg.Split(":", StringSplitOptions.RemoveEmptyEntries);
            string ticksString = split[0];
            long sendTicks = Convert.ToInt64(ticksString);
            long ticks = nowTicks - sendTicks;
            TimeSpan latency = TimeSpan.FromTicks(ticks);
            string messageText = msg.Replace(split[0], "").Trim(new char[] { ':', ' ' });

            Console.WriteLine($"Latency {latency.TotalMilliseconds} ms - Received message '{messageText}'");
        }
               

        #endregion
        
        #region Channel Events
        private static void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Channel '{e.ChannelId}' is open");
            Console.ResetColor();
        }

      

        private static void Channel_OnStateChange(object sender, ChannelStateEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Channel '{e.ChannelId}' state changed to '{e.State}'");
            Console.ResetColor();
        }

        private static void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Channel '{e.ChannelId}' error '{e.Error.Message}'");
            Console.ResetColor();
        }

        private static void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Channel '{e.ChannelId}' is closed");
        }

        #endregion

        #region Inputs
        static string SelectName()
        {
            Console.Write("Enter name for this client ? ");
            return Console.ReadLine();
        }

        static string SelectRole()
        {
            Console.Write("Enter role for the client (A/B) ? ");
            string role = Console.ReadLine().ToUpperInvariant();
            if (role == "A" || role == "B")
                return role;
            else
                return SelectRole();
        }

        static string SelectHostname()
        {
            Console.Write("Enter hostname, IP, or Enter for localhost ? ");
            string hostname = Console.ReadLine();
            if(string.IsNullOrEmpty(hostname))
            {
                return "localhost";
            }
            else
            {
                return hostname;
            }
        }

        static int SelectChannel()
        {
            //Console.WriteLine("--- Select Channel ---");
            //Console.WriteLine("(1) WebSocket");
            //Console.WriteLine("(2) TCP");
            //Console.WriteLine("---------- -----------");
            //Console.Write("Enter channel selection (1/2) ? ");

            //string chn = Console.ReadLine();
            string chn = "1";
            if (chn == "1")// || chn == "2")
                return Convert.ToInt32(chn);
            else
                return SelectChannel();
        }

        
        #endregion
        
        #region Security Token
        static string GetSecurityToken(string name, string role)
        {
            //Normally a security token would be obtained externally
            //For the sample we are going to build a token that can
            //be authn'd and authz'd for this sample

            string issuer = "http://skunklab.io/";
            string audience = issuer;
            string nameClaimType = "http://skunklab.io/name";
            string roleClaimType = "http://skunklab.io/role";
            string symmetricKey = "//////////////////////////////////////////8=";

            List<Claim> claims = new List<Claim>()
            {
                new Claim(nameClaimType, name),
                new Claim(roleClaimType, role)
            };

            return CreateJwt(audience, issuer, claims, symmetricKey, 60.0);
        }

        static string CreateJwt(string audience, string issuer, List<Claim> claims, string symmetricKey, double lifetimeMinutes)
        {
            SkunkLab.Security.Tokens.JsonWebToken jwt = new SkunkLab.Security.Tokens.JsonWebToken(new Uri(audience), symmetricKey, issuer, claims, lifetimeMinutes);
            return jwt.ToString();
        }

        #endregion

        #region Channels

        public static IChannel CreateChannel(string token, CancellationTokenSource src)
        {
            if (channelNum == 1)
            {
                string uriString = hostname == "localhost" ? "ws://localhost:8081/api/connect" : String.Format("wss://{0}/ws/api/connect", hostname);
              
                Uri uri = new Uri(uriString);
                return ChannelFactory.Create(uri, token, "mqtt", new WebSocketConfig(), src.Token);
            }
            else
            {
                if(hostname != "localhost")
                {
                    hostname = String.Format("{0}/tcp", hostname);
                }

                return ChannelFactory.Create(false, hostname, 8883, 2048, 2048 * 10, src.Token);
            }
        }

        #endregion




    }
}
