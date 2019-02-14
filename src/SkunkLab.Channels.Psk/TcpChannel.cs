
using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels.Tcp
{
    public abstract partial class TcpChannel : IChannel
    {
       


        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <param name="usePrefixLength"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="localEP"></param>
        /// <param name="pskIdentity"></param>
        /// <param name="psk"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBufferSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, IPAddress address, int port, IPEndPoint localEP, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(address, port, localEP, pskIdentity, psk, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(address, port, localEP, pskIdentity, psk, blockSize, maxBufferSize, token);
            }
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <param name="usePrefixLength"></param>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="localEP"></param>
        /// <param name="pskIdentity"></param>
        /// <param name="psk"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBufferSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, string hostname, int port, IPEndPoint localEP, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(hostname, port, localEP, pskIdentity, psk, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(hostname, port, localEP, pskIdentity, psk, blockSize, maxBufferSize, token);
            }
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <param name="usePrefixLength"></param>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="pskIdentity"></param>
        /// <param name="psk"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBufferSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, string hostname, int port, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(hostname, port, null, pskIdentity, psk, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(hostname, port, null, pskIdentity, psk, blockSize, maxBufferSize, token);
            }
        }


        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <param name="usePrefixLength"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="pskIdentity"></param>
        /// <param name="psk"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBufferSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, IPAddress address, int port, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(address, port, null, pskIdentity, psk, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(address, port, null, pskIdentity, psk, blockSize, maxBufferSize, token);
            }
        }


        public static TcpChannel Create(bool usePrefixLength, IPEndPoint remoteEndpoint, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(remoteEndpoint, null, pskIdentity, psk, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(remoteEndpoint, null, pskIdentity, psk, blockSize, maxBufferSize, token);
            }
        }

        public static TcpChannel Create(bool usePrefixLength, IPEndPoint remoteEndpoint, IPEndPoint localEP, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(remoteEndpoint, localEP, pskIdentity, psk, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(remoteEndpoint, localEP, pskIdentity, psk, blockSize, maxBufferSize, token);
            }
        }
       

        /// <summary>
        /// Create new TCP server channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pskIdentity"></param>
        /// <param name="psk"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, TcpClient client, TlsPskIdentityManager pskManager, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpServerChannel(client, pskManager, maxBufferSize, token);
            }
            else
            {
                return new TcpServerChannel2(client, pskManager, blockSize, maxBufferSize, token);
            }
        }

        public abstract bool RequireBlocking { get; }
        public abstract int Port { get; internal set; }
        public abstract string TypeId { get; }
        public abstract bool IsConnected { get; }
        public abstract string Id { get; internal set; }

        public abstract bool IsEncrypted { get; internal set; }

        public abstract bool IsAuthenticated { get; internal set; }

        public abstract ChannelState State { get; internal set; }

        public abstract event EventHandler<ChannelReceivedEventArgs> OnReceive;
        public abstract event EventHandler<ChannelCloseEventArgs> OnClose;
        public abstract event EventHandler<ChannelOpenEventArgs> OnOpen;
        public abstract event EventHandler<ChannelErrorEventArgs> OnError;
        public abstract event EventHandler<ChannelStateEventArgs> OnStateChange;

        public abstract Task CloseAsync();

        public abstract void Dispose();

        public abstract Task OpenAsync();

        public abstract Task ReceiveAsync();

        public abstract Task SendAsync(byte[] message);

        public abstract Task AddMessageAsync(byte[] message);
    }
}
