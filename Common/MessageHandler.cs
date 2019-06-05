using System;
using System.Collections.Concurrent;
using System.Text;
using Common.SimpleSocket;

namespace Common
{
    /// <summary>
    /// 处理消息. 从Buffer池中拆分出完整消息包 塞入待处理的消息队列中 由上层进行处理
    /// </summary>
    public class MessageHandler
    {
        /// <summary>
        /// 消息队列
        /// </summary>
        public ConcurrentQueue<string> MessageQueue;

        public MessageHandler()
        {
            MessageQueue = new ConcurrentQueue<string>();
        }

        /// <summary>
        /// 反序列化消息
        /// </summary>
        /// <returns>The message.</returns>
        /// <param name="message">Message.</param>
        public string DeserializeMessage(byte[] message)
        {
            return Encoding.ASCII.GetString(message);
        }

        /// <summary>
        /// 从buffer池中读取消息 即拆包
        /// </summary>
        /// <returns>剩余待处理字节数</returns>
        /// <param name="bufferPool">Buffer pool.</param>
        /// <param name="connection">Connection.</param>
        /// <param name="remainLength">Remain length.</param>
        public int ReadBufferFromPool(byte[] bufferPool, SocketData connection, int remainLength)
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
