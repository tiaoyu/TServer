using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Common.Normal
{
    /// <summary>
    /// BufferManager管理了一整块内存空间称之为BufferPool,BufferPool的大小称之为BufferPoolSize
    /// 将BufferPool平均切分为若干块, 每一块称之为BufferBlock, 将BufferBlock按顺序标上序号，称之为BufferInx
    /// BufferBlock数量称之为BufferCount, BufferBlock的大小称之为BufferSize，
    /// </summary>
    class BufferManager
    {
        /// <summary> BufferPool 总大小 BufferPoolSize </summary>
        int m_numBytes;                 // the total number of bytes controlled by the buffer pool
        /// <summary> BufferPool </summary>
        public byte[] m_buffer;                // the underlying byte array maintained by the Buffer Manager
        /// <summary>
        /// 待分配的Buffer池序号栈
        /// </summary>
        Stack<int> m_freeIndexPool;     // 
        /// <summary>
        /// 当前序号
        /// </summary>
        int m_currentIndex;
        /// <summary>
        /// 每个Buffer块大小
        /// </summary>
        int m_bufferSize;

        public BufferManager(int totalBytes, int bufferSize)
        {
            m_numBytes = totalBytes;
            m_currentIndex = 0;
            m_bufferSize = bufferSize;
            m_freeIndexPool = new Stack<int>();
        }

        // Allocates buffer space used by the buffer pool
        /// <summary>
        /// 初始化BufferPool
        /// </summary>
        public void InitBuffer()
        {
            m_buffer = new byte[m_numBytes];
        }

        // Assigns a buffer from the buffer pool to the 
        // specified SocketAsyncEventArgs object
        //
        // <returns>true if the buffer was successfully set, else false</returns>
        /// <summary>
        /// 给SocketAsyncEventArgs添加Buffer
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool SetBuffer(SocketAsyncEventArgs args)
        {
            if (m_freeIndexPool.Count > 0)
            {
                args.SetBuffer(m_buffer, m_freeIndexPool.Pop(), m_bufferSize);
            }
            else
            {
                if ((m_numBytes - m_bufferSize) < m_currentIndex)
                {
                    return false;
                }
                args.SetBuffer(m_buffer, m_currentIndex, m_bufferSize);
                m_currentIndex += m_bufferSize;
            }
            return true;
        }

        // Removes the buffer from a SocketAsyncEventArg object.  
        // This frees the buffer back to the buffer pool
        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            m_freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }
}
