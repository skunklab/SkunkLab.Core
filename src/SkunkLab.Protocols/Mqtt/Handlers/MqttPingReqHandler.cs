using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt.Handlers
{
    public class MqttPingReqHandler : MqttMessageHandler
    {
        public MqttPingReqHandler(MqttSession session, MqttMessage message)
            : base(session, message)
        {

        }

        public override async Task<MqttMessage> ProcessAsync()
        {
            if (!Session.IsConnected)
            {
                Session.Disconnect(Message);
                return null;
            }

            Session.IncrementKeepAlive();
            return await Task.FromResult<MqttMessage>(new PingResponseMessage());
        }
    }
}
