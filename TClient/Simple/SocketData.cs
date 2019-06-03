using System;
using System.Net.Sockets;

namespace TClient.Simple
{
    public class SocketData
    {
        public int MessageLength;
        public byte[] MessageBody;
        public byte[] MessageHead;
        /// <summary>
        /// 已经处理完成的消息体长度
        /// </summary>
        public int BytesOfDoneBody;
        /// <summary>
        /// 已经处理完成的消息头长度
        /// </summary>
        public int BytesOfDoneHead;
        /// <summary>
        /// 在Buffer池中的index
        /// </summary>
        public int IndexInBufferPool;
        /// <summary>
        /// 基础偏移量 根据在Buffer池中到index计算得出
        /// </summary>
        public int OffsetInBufferPool;
        /// <summary>
        /// 需要跳过的字节数 例如前一次最后处理了3字节的消息头，这一次处理时需跳过第一字节
        /// </summary>
        public int SkipBufferBytes;
        public Socket Socket;

        public SocketData(Socket socket)
        {
            Socket = socket;
            IndexInBufferPool = BufferMgr.Instance.GetBlockIndex();
            OffsetInBufferPool = IndexInBufferPool * BufferMgr.Instance.EachBlockBytes;
            MessageHead = new byte[4];
        }
    }
}
