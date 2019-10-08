using Common.LogUtil;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Common.Normal
{
    public class NormalClient : NetBase
    {
        private static readonly LogHelp log = LogHelp.GetLogger(typeof(NormalClient));
        public NormalClient(int numConnections, int receiveBufferSize) : base(numConnections, receiveBufferSize)
        {
        }

        public override void ProcessConnect(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
            }
            e.AcceptSocket = e.ConnectSocket;
            ((AsyncUserToken)e.UserToken).Socket = e.ConnectSocket;
            ((AsyncUserToken)e.UserToken).Guid = Guid.NewGuid();

            StartReceive(e);
        }

        protected override void CloseClientSocket(SocketAsyncEventArgs e)
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
            m_readWritePool.Push(e);
        }
    }
}
