using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Common.Simple
{
    /// <summary>
    /// Sample:
    /// <p>
    /// var server = new SimpleSocketServer<string>("127.0.0.1", 11000);
    /// server.SetDeserializeFunc((socket, bytes) => Encoding.UTF8.GetString(bytes));
    /// server.SetSerializeFunc((socket, strings) =>
    /// {
    ///     var sendBytes = Encoding.UTF8.GetBytes(strings);
    ///     var sendHead = BitConverter.GetBytes(sendBytes.Length);
    ///     var sendData = new byte[sendHead.Length + sendBytes.Length];
    ///     Array.Copy(sendHead, 0, sendData, 0, sendHead.Length);
    ///     Array.Copy(sendBytes, 0, sendData, sendHead.Length, sendBytes.Length);
    ///     return sendData;
    /// });
    /// server.Start();
    /// /// 处理消息
    /// Task.Run(() =>
    /// {
    ///     while (server.IsRunning)
    ///     {
    ///         var count = server.MessageHandler.MessageQueue.Count;
    ///         for (var i = 0; i < count; ++i)
    ///         {
    ///             server.MessageHandler.MessageQueue.TryDequeue(out string message);
    ///             Console.WriteLine(message);
    ///         }
    ///     }
    /// });
    /// /// 发送消息
    /// while (server.IsRunning)
    /// {
    ///     var str = Console.ReadLine();
    ///     if ("exit".Equals(str))
    ///     {
    ///         server.IsRunning = false;
    ///         continue;
    ///     }
    ///     var connectionList = new List<int>(server.DicConnection.Keys);
    ///     if (connectionList.Count <= 0) continue;
    ///     foreach (var connectionId in connectionList)
    ///     {
    ///         if (!server.DicConnection.TryGetValue(connectionId, out var connection)) continue;
    ///         if (connection.Socket.Connected)
    ///         {
    ///             server.SendMessage(connection.Socket, str);
    ///         }
    ///         else
    ///         {
    ///             server.DicConnection.TryRemove(connectionId, out connection);
    ///         }
    ///     }
    /// }
    /// server.ServerSocket.Close();   
    /// </p>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete("Don't use OldMethod, use NewMethod instead", true)]
    public class SimpleSocketServer<T> : SimpleSocket<T>
    {
        public int Port;
        public string Host;
        public Socket ServerSocket;

        public SimpleSocketServer(string host, int port)
        {
            Host = host;
            Port = port;
            DicConnection = new ConcurrentDictionary<int, SocketData>();
            BufferMgr.Instance.Init();
        }

        public void Start()
        {
            IPAddress ip = IPAddress.Parse(Host);
            IPEndPoint ipe = new IPEndPoint(ip, Port);

            ServerSocket = new Socket(AddressFamily.InterNetwork
                , SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(ipe);
            ServerSocket.Listen(0);

            // accept
            ServerSocket.BeginAccept(AcceptCallback, ServerSocket);
        }

        public void SendMessage()
        {

        }

    }
}
