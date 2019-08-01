using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Simple
{
    /// <summary>
    /// buffer 的管理类. 一个Server使用一个byte数组作为所有Socket接收数据的buffer缓存
    /// 每个Socket分配一段buffe 称为一个block 
    /// 每当有Socket收到数据时 都会写入对应的block中
    /// </summary>
    public class BufferMgr
    {
        private static BufferMgr _instance = new BufferMgr();

        public static BufferMgr Instance => _instance ?? (_instance = new BufferMgr());

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
    }
}
