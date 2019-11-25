using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt.Handlers
{
    public class MqttPubCompHandler : MqttMessageHandler
    {
        public MqttPubCompHandler(MqttSession session, MqttMessage message)
            : base(session, message)
        {

        }

        public override async Task<MqttMessage> ProcessAsync()
        {
            Session.ReleaseMessage(Message.MessageId);
            Session.Unquarantine(Message.MessageId);
            return await Task.FromResult<MqttMessage>(null);
        }
    }
}
