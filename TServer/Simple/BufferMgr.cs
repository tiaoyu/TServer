using System;
using System.Collections.Generic;

namespace TServer.Simple
{
    /// <summary>
    /// buffer 的管理类. 一个Server使用一个byte数组作为所有Socket接收数据的buffer缓存
    /// 每个Socket分配一段buffe 称为一个block 
    /// 每当有Socket收到数据时 都会写入对应的block中
    /// </summary>
    public class BufferMgr
    {
        private static BufferMgr _instance = new BufferMgr();

        public static BufferMgr Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BufferMgr();
                return _instance;
            }
        }

        /// <summary>
        /// 未使用的block下标
        /// </summary>
        private Stack<int> _freeIndexStack;
        /// <summary>
        /// 每个Socket占用的buffer大小
        /// </summary>
        public int EachBlockBytes;
        /// <summary>
        /// buffer池的总大小
        /// </summary>
        private int _capability;
        /// <summary>
        /// 池中总共包含的block个数
        /// </summary>
        private int _countOfBlock;
        /// <summary>
        /// buffer池 所有Socket使用的buffer缓存都在一个大数组中
        /// </summary>
        private byte[] _byteBufferPool;

        public BufferMgr()
        {
            // 默认设置最大保持连接数为8个 每个block大小为8*1024字节
            _countOfBlock = 8;
            EachBlockBytes = 8 * 1024;
            _capability = 8 * 8 * 1024;
            _byteBufferPool = new byte[_capability];
            _freeIndexStack = new Stack<int>();
            for (var i = 0; i < _countOfBlock; ++i)
            {
                _freeIndexStack.Push(i);
            }
        }


        //public void Init()
        //{
        //    _byteBuffer = new byte[_capability];
        //    _countOfBlock = _capability / EachBlockBytes;
        //    for (var i = 0; i < _countOfBlock; ++i)
        //    {
        //        _freeIndex.Push(i);
        //    }
        //}

        /// <summary>
        /// Gets the index of the buffer.
        /// </summary>
        /// <returns>The buffer index.</returns>
        public int GetBlockIndex()
        {
            lock (_freeIndexStack)
            {
                return _freeIndexStack.Pop();
            }
        }

        /// <summary>
        /// 将Socket接收到的数据写入Buffer池中
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="length">Length.</param>
        /// <param name="socketData">Socket data.</param>
        public void WriteBufferIntoPool(byte[] buffer, int length, SocketData socketData)
        {
            Array.Copy(buffer, 0, _byteBufferPool, socketData._curInx, length);
            socketData._curInx += length;
        }

        /// <summary>
        /// 从buffer池中读取一个4字节数据 作为消息头
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="data">Data.</param>
        public void ReadHeadBufferFromPool(out byte[] buffer, SocketData data)
        {
            buffer = new byte[4];
            if (data._offInx + 4 > data._curInx)
            {
                buffer = null;
                return;
            }
            Array.Copy(_byteBufferPool, data._offInx, buffer, 0, 4);
        }

        /// <summary>
        /// 从buffer池中读取消息体
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="data">Data.</param>
        public void ReadBufferFromPool(out byte[] buffer, SocketData data)
        {
            if (data._waitReadLength == 0)
            {
                BufferMgr.Instance.ReadHeadBufferFromPool(out byte[] buffHead, data);
                if (buffHead != null)
                {
                    data._waitReadLength = BitConverter.ToInt32(buffHead);
                    data._messageLength = data._waitReadLength;
                    data._offInx += 4;
                    data.Message = new byte[data._messageLength];
                    data.MessageOffet = 0;
                }
            }
            if (data._waitReadLength > 0)
            {
                buffer = new byte[data._waitReadLength];
                if (data._offInx + data._waitReadLength > data._curInx)
                {
                    if (data._offInx < data._curInx)
                    {
                        Array.Copy(_byteBufferPool, data._offInx, buffer, buffer.Length, data._curInx - data._offInx);
                        data._waitReadLength -= (data._curInx - data._offInx);
                        data._offInx = data._curInx;
                    }
                    else
                    {
                        buffer = null;
                        return;
                    }
                }
                else
                {
                    Array.Copy(_byteBufferPool, data._offInx, buffer, 0, data._waitReadLength);
                    data._offInx += data._waitReadLength;
                    data._waitReadLength = 0;
                }
            }
            else
            {
                buffer = null;
            }

        }
    }
}
