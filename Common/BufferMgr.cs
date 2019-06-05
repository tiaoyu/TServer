using System;
using System.Collections.Generic;
using System.Text;

namespace Common.SimpleSocket
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
        private int _capabilityBytes;
        /// <summary>
        /// 池中总共包含的block个数
        /// </summary>
        private int _capabilityBlocks;
        /// <summary>
        /// buffer池 所有Socket使用的buffer缓存都在一个大数组中
        /// </summary>
        public byte[] ByteBufferPool;

        public BufferMgr()
        {
        }

        /// <summary>
        /// 初始化Buffer池
        /// </summary>
        /// <param name="maxBlockCount">Max block count.</param>
        /// <param name="eachBlockBytes">Each block bytes.</param>
        public void Init(int maxBlockCount = 8, int eachBlockBytes = 8 * 1024)
        {
            _capabilityBlocks = maxBlockCount;
            EachBlockBytes = eachBlockBytes;
            _capabilityBytes = _capabilityBlocks * EachBlockBytes;
            ByteBufferPool = new byte[_capabilityBytes];
            _freeIndexStack = new Stack<int>();
            for (var i = 0; i < _capabilityBlocks; ++i)
            {
                _freeIndexStack.Push(i);
            }
        }


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
        /// 从buffer池中读取消息 即拆包
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="data">Data.</param>
        public int ReadBufferFromPool(SocketData data, int remainLength)
        {
            if (data.BytesOfDoneHead < 4)
            {
                // 读取消息头：若当前消息头未读取完整则继续读取
                var remainHeadLength = 4 - data.BytesOfDoneHead;
                if (remainLength - remainHeadLength >= 0)
                {
                    for (var i = 0; i < remainHeadLength; ++i)
                    {
                        data.MessageHead[data.BytesOfDoneHead + i] = ByteBufferPool[data.OffsetInBufferPool + data.SkipBufferBytes + i];
                    }
                    remainLength -= remainHeadLength;
                    data.BytesOfDoneHead += remainHeadLength;
                    data.SkipBufferBytes += remainHeadLength;
                    data.MessageLength = BitConverter.ToInt32(data.MessageHead);

                }
                else
                {
                    for (var i = 0; i < remainLength; ++i)
                    {
                        data.MessageHead[data.BytesOfDoneHead + i] = ByteBufferPool[data.OffsetInBufferPool + data.SkipBufferBytes + i];
                    }
                    remainLength = 0;
                    data.BytesOfDoneHead += remainHeadLength;
                    data.SkipBufferBytes = 0;
                }
            }

            // 读取消息体：若当前剩余buffer长度大于待读取的消息长度 则读取的消息长度为 待读取的消息长度
            if (data.BytesOfDoneBody == 0)
            {
                data.MessageBody = new byte[data.MessageLength];
            }
            var waitDoneBytes = data.MessageLength - data.BytesOfDoneBody;
            if (remainLength >= waitDoneBytes)
            {
                Array.Copy(ByteBufferPool, data.OffsetInBufferPool + data.SkipBufferBytes
                    , data.MessageBody, data.BytesOfDoneBody, data.MessageLength - data.BytesOfDoneBody);
                data.BytesOfDoneBody = data.MessageLength;
                data.SkipBufferBytes += waitDoneBytes;
                remainLength -= waitDoneBytes;
            }
            else
            {
                Array.Copy(ByteBufferPool, data.OffsetInBufferPool + data.SkipBufferBytes
                    , data.MessageBody, data.BytesOfDoneBody, remainLength);
                data.BytesOfDoneBody += remainLength;
                remainLength = 0;
            }

            // 处理读取到到消息
            if (data.MessageLength == data.BytesOfDoneBody)
            {
                Console.WriteLine("Recv: {0}", Encoding.ASCII.GetString(data.MessageBody));
                if (remainLength == 0)
                {
                    data.SkipBufferBytes = 0;
                }
                data.MessageLength = 0;
                data.BytesOfDoneBody = 0;
                data.BytesOfDoneHead = 0;
            }
            else
            {
                data.SkipBufferBytes = 0;
            }
            return remainLength;
        }
    }
}
