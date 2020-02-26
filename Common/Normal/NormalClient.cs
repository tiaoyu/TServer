using Common.LogUtil;
using Common.Protobuf;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Common.Normal
{
    public class NormalClient : NetBase
    {
        private static readonly LogHelp log = LogHelp.GetLogger(typeof(NormalClient));
        private SocketAsyncEventArgs ClientEventArgs;
        private IPAddress RemoteAddress;
        private Socket connectSocket;

        public NormalClient(int numConnections, int receiveBufferSize) : base(numConnections, receiveBufferSize)
        {
        }

        public void StartConnect(string ipOrHost, int port)
        {
            var address = Dns.GetHostAddresses(ipOrHost);
            RemoteAddress = address[address.Length - 1];
            log.Debug($"HostOrIp:{ipOrHost} RemoteAddress:{RemoteAddress.ToString()}");
            var remoteEndPoint = new IPEndPoint(RemoteAddress, port);
            connectSocket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            var e = CreateNewSocketAsyncEventArgsForConnect();
            e.RemoteEndPoint = remoteEndPoint;

            connectSocket.ConnectAsync(e);
        }

        private void ProcessConnect(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                return;
            }

            ClientEventArgs = CreateNewSocketAsyncEventArgsFromPool();
            ClientEventArgs.AcceptSocket = e.ConnectSocket;
            var token = ((AsyncUserToken)ClientEventArgs.UserToken);
            token.Socket = e.ConnectSocket;
            token.Guid = Guid.NewGuid();
            token.SendSocket = CreateNewSocketAsyncEventArgsFromPool();

            StartReceive(ClientEventArgs);
        }

        public void StartSend(object msg)
        {
            StartSend(ClientEventArgs, msg);
        }

        public void StartSend(byte[] msg)
        {
            StartSend(ClientEventArgs, msg);
        }

        private SocketAsyncEventArgs CreateNewSocketAsyncEventArgsForConnect()
        {
            var e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(Connect_Completed);
            e.UserToken = new AsyncUserToken();
            return e;
        }

        public virtual void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ProcessConnect(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        protected override void CloseSocket(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = e.UserToken as AsyncUserToken;

            // close the socket associated with the client
            try
            {
                token.Socket.Shutdown(SocketShutdown.Send);
            }
            // throws if client process has already closed
            catch (Exception err)
            {
                log.Info($"Exception:{err.Message}");
            }
            token.Socket?.Close();

            // Free the SocketAsyncEventArg so they can be reused by another client
            FreeSocketAsyncEventArgsToPool(e);

            log.Info("Connection break");
        }
    }
}
