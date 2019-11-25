using System.Threading.Tasks;

namespace SkunkLab.Protocols.Coap.Handlers
{
    public class CoapObserveHandler : CoapMessageHandler
    {
        public CoapObserveHandler(CoapSession session, CoapMessage message, ICoapRequestDispatch dispatcher = null)
            : base(session, message, dispatcher)
        {
            session.EnsureAuthentication(message);
        }

        public override async Task<CoapMessage> ProcessAsync()
        {
            if (!Session.CoapReceiver.IsDup(Message.MessageId))
            {
                Session.CoapReceiver.CacheId(Message.MessageId);
            }

            CoapMessage message = await Dispatcher.ObserveAsync(Message);

            return await Task.FromResult<CoapMessage>(message);
        }
    }
}
