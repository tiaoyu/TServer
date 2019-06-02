using System;
using System.Net.Sockets;

namespace TServer.Simple
{
    public class SocketData
    {
        public int _messageLength;
        public int _waitReadLength;
        public byte[] Message;
        public int MessageOffet;
        private int _indexInBufferPool;
        /// <summary>
        /// The current index of writing into buffer pool.
        /// </summary>
        public int _curInx;
        /// <summary>
        /// The offset index of reading from buffer pool.
        /// </summary>
        public int _offInx;
        private Socket _socket;
        public Socket Socket => _socket;
        public int IndexInBufferPool => _indexInBufferPool;

        public SocketData(Socket socket)
        {
            _socket = socket;
            _indexInBufferPool = BufferMgr.Instance.GetBlockIndex();
            _offInx = _indexInBufferPool * BufferMgr.Instance.EachBlockBytes;
            _curInx = _offInx;
        }
    }
}
