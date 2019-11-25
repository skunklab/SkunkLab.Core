using System.Threading.Tasks;

namespace SkunkLab.Protocols.Coap.Handlers
{
    public class CoapResponseHandler : CoapMessageHandler
    {
        public CoapResponseHandler(CoapSession session, CoapMessage message)
            : base(session, message, null)
        {
            session.EnsureAuthentication(message);
        }

        public override async Task<CoapMessage> ProcessAsync()
        {
            Session.CoapSender.DispatchResponse(Message);

            if (Message.MessageType == CoapMessageType.Acknowledgement)
            {
                return await Task.FromResult<CoapResponse>(new CoapResponse(Message.MessageId, ResponseMessageType.Acknowledgement, ResponseCodeType.EmptyMessage));
            }
            else
            {
                return null;
            }

        }
    }
}
