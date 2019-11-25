using System.Threading.Tasks;

namespace SkunkLab.Protocols.Coap.Handlers
{
    public class CoapPingHandler : CoapMessageHandler
    {
        public CoapPingHandler(CoapSession session, CoapMessage message)
            : base(session, message, null)
        {
            session.EnsureAuthentication(message);
        }
        public override async Task<CoapMessage> ProcessAsync()
        {
            return await Task.FromResult<CoapMessage>(new CoapResponse(Message.MessageId, ResponseMessageType.Reset, ResponseCodeType.EmptyMessage));
        }
    }
}
