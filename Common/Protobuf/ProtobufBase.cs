using Google.Protobuf;
using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.IO;

namespace Common.Protobuf
{
    public interface ProtocolBufBase
    {
        public byte[] Serialize();
    }
    public class ProtobufBase<T> : ProtocolBufBase where T : class, IMessage<T>, new()
    {
        public static int ProtoId;

        public void SetProtoId(int id)
        {
            ProtoId = id;
        }

        public byte[] Serialize()
        {
            var stream = new MemoryStream();
            (this as T).WriteTo(stream);
            var sendBytes = stream.ToArray();
            var sendHead = BitConverter.GetBytes(sendBytes.Length);
            var sendData = new byte[sendHead.Length + sendBytes.Length];
            Array.Copy(sendHead, 0, sendData, 0, sendHead.Length);
            Array.Copy(sendBytes, 0, sendData, sendHead.Length, sendBytes.Length);
            return sendData;
        }
    }

    public class ProtocolPaseBase
    {
        Func<IMessage> _func;
        public ProtocolPaseBase(Func<IMessage> func)
        {
            _func = func;
        }

        public virtual void SetProtoId(int protoId) { }

        public IMessage ParseFrom(byte[] stream)
        {
            IMessage msg = _func();
            msg.MergeFrom(stream);
            return msg;
        }
    }
    public class ProtocolParse<T> : ProtocolPaseBase where T : ProtobufBase<T>, IMessage<T>, new()
    {
        public ProtocolParse(Func<IMessage> func) : base(func)
        {
        }

        public override void SetProtoId(int protoId)
        {
            ProtobufBase<T>.ProtoId = protoId;
        }
    }

    public class ProtocolParser
    {
        private static ProtocolParser _instance = new ProtocolParser();
        public static ProtocolParser Instance => _instance ?? new ProtocolParser();

        private Dictionary<int, ProtocolPaseBase> _dicParser = new Dictionary<int, ProtocolPaseBase>();

        public ProtocolPaseBase GetParser(int proId)
        {
            _dicParser.TryGetValue(proId, out var res);
            return res;
        }

        public static ProtocolParse<T> CreateParse<T>() where T : ProtobufBase<T>, IMessage<T>, new()
        {
            return new ProtocolParse<T>(() => new T());
        }

        public ProtocolParser()
        {

            push((int)S2C_PROTOCOL_TYPE.S2CLogin, CreateParse<S2CLogin>());
            push((int)C2S_PROTOCOL_TYPE.C2SLogin, CreateParse<C2SLogin>());

            foreach (var (protoId, parser) in _dicParser)
            {
                parser.SetProtoId(protoId);
            }
        }

        void push(int protoId, ProtocolPaseBase parse)
        {
            _dicParser.Add((int)S2C_PROTOCOL_TYPE.S2CLogin, CreateParse<S2CLogin>());
        }
    }
}
