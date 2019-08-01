using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Common.Simple
{
    public class SimpleSocket<T>
    {
        public bool IsRunning = true;
        public bool WaitToReconnect = false;
        public MessageHandler<T> MessageHandler;
        public ConcurrentDictionary<int, SocketData> DicConnection;

        public SimpleSocket()
        {
            MessageHandler = new MessageHandler<T>();
            DicConnection = new ConcurrentDictionary<int, SocketData>();
        }

        public void SetDeserializeFunc(Func<Socket, byte[], T> funcDeserialize)
        {
            MessageHandler.SetDeserializeFunc(funcDeserialize);
        }

        public void SetSerializeFunc(Func<Socket, T, byte[]> funcSerialize)
        {
            MessageHandler.SetSerializeFunc(funcSerialize);
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

            try
            {
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


                    ss.Socket.BeginReceive(BufferMgr.Instance.ByteBufferPool, ss.OffsetInBufferPool,
                        BufferMgr.Instance.EachBlockBytes
                        , SocketFlags.None, ReceiveCallBack, ss);
                }
                else
                {
                    Console.WriteLine("Connection closed, ConnectionId : " + ss.GetHashCode());
                    ss.Socket.Close();
                    WaitToReconnect = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error:{e.Message}");
                ss.Socket.Close();
                WaitToReconnect = true;
            }
        }

        public void SendMessage(Socket socket, T message)
        {
            SendMessage(socket, MessageHandler.SerializeMessage(socket, message));
        }
        private void SendMessage(Socket socket, byte[] message)
        {
            Console.WriteLine($"Send message, length:{message.Length}~");
            socket.BeginSend(message, 0, message.Length, SocketFlags.None,
                (ar) => { (ar.AsyncState as Socket)?.EndSend(ar); }, socket);
        }
    }
}
