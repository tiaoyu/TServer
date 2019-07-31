using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Common.SimpleSocket
{
    public class SimpleSocket
    {
        public MessageHandler MessageHandler;
        public ConcurrentDictionary<int, SocketData> DicConnection;

        public SimpleSocket() {
            MessageHandler = new MessageHandler();
        }
        
        /// <summary>
        /// Accepts the callback.
        /// </summary>
        /// <param name="ar">Ar.</param>
        public void AcceptCallback(IAsyncResult ar)
        {
            if (!(ar.AsyncState is Socket socket)) return;

            var connection = socket.EndAccept(ar);

            var socketData = new SocketData(connection);

            DicConnection.TryAdd(connection.GetHashCode(), socketData);

            socketData.Socket.BeginReceive(BufferMgr.Instance.ByteBufferPool, socketData.OffsetInBufferPool, BufferMgr.Instance.EachBlockBytes
                , SocketFlags.None, ReceiveCallBack, socketData);
            Console.WriteLine("Connected end, ConnectionId : " + connection.GetHashCode());
            socket.BeginAccept(AcceptCallback, socket);

        }

        /// <summary>
        /// Receives the callback.
        /// </summary>
        /// <param name="ar">Ar.</param>
        public void ReceiveCallBack(IAsyncResult ar)
        {
            if (!(ar.AsyncState is SocketData ss)) return;

            // 当前接收到的字节数
            var recLength = ss.Socket.EndReceive(ar);
            if (recLength > 0)
            {
                Console.WriteLine("endLength: " + recLength);
                // 当前剩余需要处理的字节长度
                var remainProcessLength = recLength;
                do
                {
                    remainProcessLength = MessageHandler.ReadBufferFromPool(BufferMgr.Instance.ByteBufferPool, ss, remainProcessLength);

                } while (remainProcessLength != 0);


                ss.Socket.BeginReceive(BufferMgr.Instance.ByteBufferPool, ss.OffsetInBufferPool, BufferMgr.Instance.EachBlockBytes
                    , SocketFlags.None, ReceiveCallBack, ss);
            }
            else
            {
                Console.WriteLine("Connection closed, ConnectionId : " + ss.GetHashCode());
                ss.Socket.Close();
            }
        }
    }
}
