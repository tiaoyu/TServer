using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Common.Normal
{
    /// <summary>
    /// 处理消息. 从Buffer池中拆分出完整消息包 塞入待处理的消息队列中 由上层进行处理
    /// </summary>
    public class MessageHandler<T>
    {
        /// <summary>
        /// 消息队列
        /// </summary>
        public ConcurrentQueue<T> MessageQueue;

        /// <summary>
        /// 反序列消息方法 构造时传入
        /// </summary>
        private Func<byte[], T> _funcDeserialize;

        /// <summary>
        /// 序列化消息方法 构造时传入
        /// </summary>
        private Func<T, byte[]> _funcSerialize;

        public MessageHandler()
        {
            MessageQueue = new ConcurrentQueue<T>();
        }

        /// <summary>
        /// 设置反序列化消息方法
        /// </summary>
        /// <param name="funcDeserialize"></param>
        /// <returns></returns>
        public MessageHandler<T> SetDeserializeFunc(Func<byte[], T> funcDeserialize)
        {
            _funcDeserialize = funcDeserialize;
            return this;
        }

        /// <summary>
        /// 设置序列化消息方法
        /// </summary>
        /// <param name="funcSerialize"></param>
        /// <returns></returns>
        public MessageHandler<T> SetSerializeFunc(Func<T, byte[]> funcSerialize)
        {
            _funcSerialize = funcSerialize;
            return this;
        }

        /// <summary>
        /// 反序列化消息
        /// </summary>
        /// <returns>The message.</returns>
        /// <param name="message">Message.</param>
        private T DeserializeMessage(byte[] message)
        {
            return _funcDeserialize(message);
        }

        /// <summary>
        /// 序列化消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public byte[] SerializeMessage(T message)
        {
            return _funcSerialize(message);
        }

        /// <summary>
        /// 从buffer池中读取消息 即拆包
        /// </summary>
        /// <returns>剩余待处理字节数</returns>
        /// <param name="bufferPool">Buffer pool.</param>
        /// <param name="connection">Connection.</param>
        /// <param name="remainLength">Remain length.</param>
        public int ReadBufferFromPool(byte[] bufferPool, AsyncUserToken connection, int remainLength)
        {
            if (connection.BytesOfDoneHead < 4)
            {
                // 读取消息头：若当前消息头未读取完整则继续读取
                var remainHeadLength = 4 - connection.BytesOfDoneHead;
                if (remainLength - remainHeadLength >= 0)
                {
                    for (var i = 0; i < remainHeadLength; ++i)
                    {
                        connection.MessageHead[connection.BytesOfDoneHead + i] = bufferPool[connection.OffsetInBufferPool + connection.SkipBufferBytes + i];
                    }
                    remainLength -= remainHeadLength;
                    connection.BytesOfDoneHead += remainHeadLength;
                    connection.SkipBufferBytes += remainHeadLength;
                    connection.MessageLength = BitConverter.ToInt32(connection.MessageHead);

                }
                else
                {
                    for (var i = 0; i < remainLength; ++i)
                    {
                        connection.MessageHead[connection.BytesOfDoneHead + i] = bufferPool[connection.OffsetInBufferPool + connection.SkipBufferBytes + i];
                    }
                    remainLength = 0;
                    connection.BytesOfDoneHead += remainHeadLength;
                    connection.SkipBufferBytes = 0;
                }
            }

            // 读取消息体：若当前剩余buffer长度大于待读取的消息长度 则读取的消息长度为 待读取的消息长度
            if (connection.BytesOfDoneBody == 0)
            {
                connection.MessageBody = new byte[connection.MessageLength];
            }
            var waitDoneBytes = connection.MessageLength - connection.BytesOfDoneBody;
            if (remainLength >= waitDoneBytes)
            {
                Array.Copy(bufferPool, connection.OffsetInBufferPool + connection.SkipBufferBytes
                    , connection.MessageBody, connection.BytesOfDoneBody, connection.MessageLength - connection.BytesOfDoneBody);
                connection.BytesOfDoneBody = connection.MessageLength;
                connection.SkipBufferBytes += waitDoneBytes;
                remainLength -= waitDoneBytes;
            }
            else
            {
                Array.Copy(bufferPool, connection.OffsetInBufferPool + connection.SkipBufferBytes
                    , connection.MessageBody, connection.BytesOfDoneBody, remainLength);
                connection.BytesOfDoneBody += remainLength;
                remainLength = 0;
            }

            // 处理读取到到消息
            if (connection.MessageLength == connection.BytesOfDoneBody)
            {
                MessageQueue.Enqueue(DeserializeMessage(connection.MessageBody));

                if (remainLength == 0)
                {
                    connection.SkipBufferBytes = 0;
                }
                connection.MessageLength = 0;
                connection.BytesOfDoneBody = 0;
                connection.BytesOfDoneHead = 0;
            }
            else
            {
                connection.SkipBufferBytes = 0;
            }
            return remainLength;
        }
    }
}
