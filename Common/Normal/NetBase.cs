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
    // Implements the connection logic for the socket server.  
    // After accepting a connection, all data read from the client 
    // is sent back to the client. The read and echo back to the client pattern 
    // is continued until the client disconnects.
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
                readWriteEventArg.UserToken = new AsyncUserToken();

                // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
                m_bufferManager.SetBuffer(readWriteEventArg);

                // add SocketAsyncEventArg to the pool
                m_readWritePool.Push(readWriteEventArg);
            }

            MessageHandler = new MessageHandler<object>();
            
            ProtocolParser.Register();
        }

        // This method is called whenever a receive or send operation is completed on a socket 
        //
        // <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
        public virtual void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    log.Debug("Receive");
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    log.Debug("Send");
                    ProcessSend(e);
                    break;
                case SocketAsyncOperation.Disconnect:
                    log.Debug("Disconnect");
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        public void StartSend(SocketAsyncEventArgs e, object msg)
        {
            StartSend(e, MessageHandler.SerializeMessage(msg));
        }

        public void StartSend(SocketAsyncEventArgs e, byte[] buffer)
        {
            var sendSocketArgs = CreateNewSocketAsyncEventArgsFromPool();
            sendSocketArgs.AcceptSocket = e.AcceptSocket;

            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            sendSocketArgs.SetBuffer(buffer, 0, buffer.Length);

            bool willRaiseEvent = token.Socket.SendAsync(sendSocketArgs);
            if (!willRaiseEvent)
            {
                ProcessSend(sendSocketArgs);
            }
        }

        // This method is invoked when an asynchronous send operation completes.  
        // The method issues another receive on the socket to read any additional 
        // data sent from the client
        //
        // <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // done echoing data back to the client
                AsyncUserToken token = (AsyncUserToken)e.UserToken;
                // read the next block of data send from the client
                FreeSocketAsyncEventArgsToPool(e);
            }
            else
            {
                CloseSocket(e);
            }

        }

        public void StartReceive(SocketAsyncEventArgs e)
        {
            var ret = e.AcceptSocket.ReceiveAsync(e);

            if (!ret)
            {
                ProcessReceive(e);
            }
        }

        // This method is invoked when an asynchronous receive operation completes. 
        // If the remote host closed the connection, then the socket is closed.  
        // If data was received then the data is echoed back to the client.
        //
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                //increment the count of the total bytes receive by the server
                Interlocked.Add(ref m_totalBytesRead, e.BytesTransferred);
                //log.Debug($"The server has read a total of {m_totalBytesRead} bytes, message: {Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred)}");
                token.OffsetInBufferPool = e.Offset;
                // 当前剩余需要处理的字节长度
                var remainProcessLength = e.BytesTransferred;
                do
                {
                    remainProcessLength = MessageHandler.ReadBufferFromPool(m_bufferManager.m_buffer, token, remainProcessLength);

                } while (remainProcessLength != 0);
                StartReceive(e);
            }
            else
            {
                CloseSocket(e);
            }
        }

        protected virtual void CloseSocket(SocketAsyncEventArgs e) { }

        /// <summary>
        /// 
        /// </summary>
        protected SocketAsyncEventArgs CreateNewSocketAsyncEventArgsFromPool()
        {
            return m_readWritePool.Pop();
        }

        protected void FreeSocketAsyncEventArgsToPool(SocketAsyncEventArgs e)
        {
            m_bufferManager.FreeBuffer(e);
            m_readWritePool.Push(e);
        }
    }
}
