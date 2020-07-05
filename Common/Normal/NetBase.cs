using Common.LogUtil;
using Common.Protobuf;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Common.Normal
{
    /// <summary>
    /// 基础网络模块
    ///     提供网络流的异步收发
    /// 
    /// 客户端独有的Connect 和 服务端的Accept由子类另行实现
    /// Implements the connection logic for the socket server.  
    ///  
    /// </summary>
    public class NetBase
    {
        private static readonly LogHelp log = LogHelp.GetLogger(typeof(NetBase));

        private int m_numConnections;   // the maximum number of connections the sample is designed to handle simultaneously 
        protected int m_receiveBufferSize;// buffer size to use for each socket I/O operation 
        private BufferManager m_bufferManager;  // represents a large reusable set of buffers for all socket operations
        private const int opsToPreAlloc = 2;    // read, write (don't alloc buffer space for accepts)
                                                // pool of reusable SocketAsyncEventArgs objects for write, read and accept socket operations
        protected SocketAsyncEventArgsPool m_readWritePool;
        private int m_totalBytesRead;           // counter of the total # bytes received by the server

        /// <summary>
        /// 消息处理类
        ///   将字节流反序列化为协议
        ///   将协议序列化为字节流
        /// </summary>
        public MessageHandler<object> MessageHandler;

        // Create an uninitialized server instance.  
        // To start the server listening for connection requests
        // call the Init method followed by Start method 
        //
        // <param name="numConnections">the maximum number of connections the sample is designed to handle simultaneously</param>
        // <param name="receiveBufferSize">buffer size to use for each socket I/O operation</param>
        public NetBase(int numConnections, int receiveBufferSize)
        {
            m_totalBytesRead = 0;
            m_numConnections = numConnections;
            m_receiveBufferSize = receiveBufferSize;
            // allocate buffers such that the maximum number of sockets can have one outstanding read and 
            //write posted to the socket simultaneously  
            m_bufferManager = new BufferManager(receiveBufferSize * numConnections * opsToPreAlloc, receiveBufferSize);

            m_readWritePool = new SocketAsyncEventArgsPool(numConnections);
        }

        // Initializes the server by preallocating reusable buffers and 
        // context objects.  These objects do not need to be preallocated 
        // or reused, but it is done this way to illustrate how the API can 
        // easily be used to create reusable objects to increase server performance.
        //
        public void Init()
        {
            // Allocates one large byte buffer which all I/O operations use a piece of.  This gaurds 
            // against memory fragmentation
            m_bufferManager.InitBuffer();

            // preallocate pool of SocketAsyncEventArgs objects
            SocketAsyncEventArgs readWriteEventArg;

            for (int i = 0; i < m_numConnections; i++)
            {
                //Pre-allocate a set of reusable SocketAsyncEventArgs
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
                m_bufferManager.SetBuffer(readWriteEventArg);
                readWriteEventArg.UserToken = new AsyncUserToken { OffsetInBufferPool = readWriteEventArg.Offset };

                // add SocketAsyncEventArg to the pool
                m_readWritePool.Push(readWriteEventArg);
            }

            MessageHandler = new MessageHandler<object>();

            ProtocolParser.Register();
        }

        /// <summary>
        /// 当Socket完成一次IO操作后的回调
        /// This method is called whenever a receive or send operation is completed on a socket 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
        public virtual void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    break;
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                case SocketAsyncOperation.Disconnect:
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="e"></param>
        /// <param name="msg"></param>
        public void StartSend(SocketAsyncEventArgs e, object msg)
        {
            StartSend(e, MessageHandler.SerializeMessage(msg));
        }

        private void StartSend(SocketAsyncEventArgs e, byte[] buffer)
        {
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            if (token != null && token.Socket.Connected)
            {
                var sendSocketArgs = token.SendSocket;
                sendSocketArgs.AcceptSocket = e.AcceptSocket;

                sendSocketArgs.SetBuffer(buffer, 0, buffer.Length);
                bool willRaiseEvent = sendSocketArgs.AcceptSocket.SendAsync(sendSocketArgs);
                if (!willRaiseEvent)
                {
                    ProcessSend(sendSocketArgs);
                }
            }
        }

        /// <summary>
        /// 发送消息后的处理方法 
        /// This method is invoked when an asynchronous send operation completes.  
        /// The method issues another receive on the socket to read any additional 
        /// data sent from the client
        ///
        /// </summary>
        /// <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // done echoing data back to the client
                AsyncUserToken token = (AsyncUserToken)e.UserToken;
                // read the next block of data send from the client
                //FreeSocketAsyncEventArgsToPool(e);
            }
            else
            {
                CloseSocket(e);
            }
        }

        /// <summary>
        /// 接收消息
        /// 
        ///     对于客户端来说，在Connect到服务器后进行接收
        ///     对于服务器来说，在Accept到客户端后进行接收
        /// 
        /// 并且每次处理完Receive到的消息后，紧接着接收下一条消息
        /// </summary>
        /// <param name="e"></param>
        public void StartReceive(SocketAsyncEventArgs e)
        {
            var ret = e.AcceptSocket.ReceiveAsync(e);

            if (!ret)
            {
                ProcessReceive(e);
            }
        }

        /// <summary>
        /// 接收完消息后的处理
        /// 使用MessageHandler处理字节流 发序列化为消息并入队供消息执行处理线程处理
        /// This method is invoked when an asynchronous receive operation completes. 
        /// If the remote host closed the connection, then the socket is closed.  
        /// If data was received then the data is echoed back to the client.
        /// </summary>
        /// <param name="e"></param>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            AsyncUserToken token = (AsyncUserToken)e.UserToken;

            if (e.BytesTransferred <= 0)
            {
                CloseSocket(e);
            }

            if (e.SocketError == SocketError.Success)
            {
                //increment the count of the total bytes receive by the server
                Interlocked.Add(ref m_totalBytesRead, e.BytesTransferred);
                token.OffsetInBufferPool = e.Offset;
                // 当前剩余需要处理的字节长度
                var remainProcessLength = e.BytesTransferred;
                do
                {
                    remainProcessLength = MessageHandler.ReadBufferFromPool(m_bufferManager.m_buffer, token, remainProcessLength);

                } while (remainProcessLength != 0);

                // 每次处理完Receive到的消息后，紧接着接收下一条消息
                StartReceive(e);
            }
        }

        protected virtual void CloseSocket(SocketAsyncEventArgs e) { }

        /// <summary>
        /// 创建新的SocketAsyncEventArgs
        ///   对于客户端来说，在Connect到服务器后创建两个，一读一写
        ///   对于服务器来说，在Accept到客户端后创建两个，一读一写
        /// </summary>
        protected SocketAsyncEventArgs CreateNewSocketAsyncEventArgsFromPool()
        {
            return m_readWritePool.Pop();
        }

        /// <summary>
        /// 释放SocketAsyncEventArgs到池中
        /// </summary>
        /// <param name="e"></param>
        protected void FreeSocketAsyncEventArgsToPool(SocketAsyncEventArgs e)
        {
            m_bufferManager.FreeBuffer(e);
            m_bufferManager.SetBuffer(e);
            m_readWritePool.Push(e);
        }

        protected virtual void OnAccept(ExtSocket ss) { }

        protected virtual void OnConnect(ExtSocket ss) { }

        protected virtual void OnClose(ExtSocket ss) { }

        protected virtual void OnDisconnect(ExtSocket ss) { }

        protected virtual void OnReceive(ExtSocket ss)
        {
            ss.Protocol.OnProcess(ss.Guid);
        }

        public void ProcessMessage()
        {
            var count = MessageHandler.MessageQueue.Count;
            while (count-- > 0)
            {
                if (MessageHandler.MessageQueue.TryDequeue(out object msg))
                {
                    var ss = (msg as ExtSocket);
                    switch (ss.ESocketType)
                    {
                        case ESocketType.ESocketAccept:
                            OnAccept(ss);
                            break;
                        case ESocketType.ESocketDisconnect:
                            OnDisconnect(ss);
                            break;
                        case ESocketType.ESocketReceive:
                            OnReceive(ss);
                            break;
                        case ESocketType.ESocketClose:
                            OnClose(ss);
                            break;
                    }
                }
            }
        }
    }
}
