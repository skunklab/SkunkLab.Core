using SkunkLab.Protocols.Coap;
using System;
using System.Threading.Tasks;

namespace SkunkLab.Clients.Coap
{
    public class ClientRequestDispatcher : ICoapRequestDispatch
    {
        public ClientRequestDispatcher(CoapClientRequestRegistry registry)
        {
            this.registry = registry;
        }

        private CoapClientRequestRegistry registry;
        private bool disposedValue;
        private string identity;

        public string Identity
        {
            set
            {
                this.identity = value;
            }
        }

        public Task<CoapMessage> PostAsync(CoapMessage message)
        {
            TaskCompletionSource<CoapMessage> tcs = new TaskCompletionSource<CoapMessage>();
            CoapUri uri = new CoapUri(message.ResourceUri.ToString());
            ResponseMessageType rmt = message.MessageType == CoapMessageType.Confirmable ? ResponseMessageType.Acknowledgement : ResponseMessageType.NonConfirmable;


            registry.GetAction("POST", uri.Resource)?.Invoke(MediaTypeConverter.ConvertFromMediaType(message.ContentType), message.Payload);
            CoapMessage response = new CoapResponse(message.MessageId, rmt, ResponseCodeType.Created, message.Token);
            tcs.SetResult(response);
            return tcs.Task;
        }

        /// <summary>
        /// Not implemented in client 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<CoapMessage> GetAsync(CoapMessage message)
        {
            TaskCompletionSource<CoapMessage> tcs = new TaskCompletionSource<CoapMessage>();
            CoapMessage msg = new CoapResponse(message.MessageId, ResponseMessageType.Reset, ResponseCodeType.EmptyMessage, message.Token);
            tcs.SetResult(msg);
            return tcs.Task;
        }

        /// <summary>
        /// Not implemented in client
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<CoapMessage> PutAsync(CoapMessage message)
        {
            TaskCompletionSource<CoapMessage> tcs = new TaskCompletionSource<CoapMessage>();
            CoapMessage msg = new CoapResponse(message.MessageId, ResponseMessageType.Reset, ResponseCodeType.EmptyMessage, message.Token);
            tcs.SetResult(msg);
            return tcs.Task;
        }

        /// <summary>
        /// Not implemented in client
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<CoapMessage> DeleteAsync(CoapMessage message)
        {
            TaskCompletionSource<CoapMessage> tcs = new TaskCompletionSource<CoapMessage>();
            CoapMessage msg = new CoapResponse(message.MessageId, ResponseMessageType.Reset, ResponseCodeType.EmptyMessage, message.Token);
            tcs.SetResult(msg);
            return tcs.Task;
        }

        /// <summary>
        /// Not implemented in client
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<CoapMessage> ObserveAsync(CoapMessage message)
        {
            TaskCompletionSource<CoapMessage> tcs = new TaskCompletionSource<CoapMessage>();
            CoapMessage msg = new CoapResponse(message.MessageId, ResponseMessageType.Reset, ResponseCodeType.EmptyMessage, message.Token);
            tcs.SetResult(msg);
            return tcs.Task;
        }

        #region IDisposable Support


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    registry.Clear();
                    registry = null;
                }

                disposedValue = true;
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
