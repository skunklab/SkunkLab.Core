using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt.Handlers
{
    public class MqttPingRespHandler : MqttMessageHandler
    {
        public MqttPingRespHandler(MqttSession session, MqttMessage message)
            : base(session, message)
        {

        }

        public override async Task<MqttMessage> ProcessAsync()
        {
            Session.IncrementKeepAlive();
            return await Task.FromResult<MqttMessage>(null);
        }
    }
}
