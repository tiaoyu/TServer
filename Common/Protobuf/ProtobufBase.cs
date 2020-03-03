using Common.Normal;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;

namespace Common.Protobuf
{
    public interface ProtocolBufBase
    {
        public byte[] Serialize();

        public void OnProcess(Guid guid);

        public void OnProcess();
    }
    public class ProtobufBase<T> : ProtocolBufBase where T : class, IMessage<T>, new()
    {
        public static int TypeId { set; get; }
        public int ProtoId { get { return TypeId; } }
        public ExtSocket ExtSocket { get; set; }

        public void SetProtoId(int id)
        {
            TypeId = id;
        }

        public byte[] Serialize()
        {
            var stream = new MemoryStream();
            var t = (this as T);
            t.WriteTo(stream);
            var sendBytes = stream.ToArray();
            var sendProtoId = BitConverter.GetBytes(this.ProtoId);
            var sendHead = BitConverter.GetBytes(sendBytes.Length + sendProtoId.Length);
            var sendData = new byte[sendHead.Length + sendProtoId.Length + sendBytes.Length];
            Array.Copy(sendHead, 0, sendData, 0, sendHead.Length);
            Array.Copy(sendProtoId, 0, sendData, sendHead.Length, sendProtoId.Length);
            Array.Copy(sendBytes, 0, sendData, sendHead.Length + sendProtoId.Length, sendBytes.Length);
            return sendData;
        }

        public virtual void OnProcess(Guid guid)
        {
        }

        public virtual void OnProcess() { }
    }

    public class ProtocolPaseBase
    {
        private Func<IMessage> _func;
        public ProtocolPaseBase(Func<IMessage> func)
        {
            _func = func;
        }

        public virtual void SetProtoId(int protoId) { }

        public IMessage ParseFrom(byte[] stream)
        {
            IMessage msg = _func();
            msg.MergeFrom(stream, 4, stream.Length - 4);
            return msg;
        }
    }
    public class ProtocolParse<T> : ProtocolPaseBase where T : ProtobufBase<T>, IMessage<T>, new()
    {
        private Func<T> _func;
        public ProtocolParse(Func<T> func) : base(() => func())
        {
            _func = func;
        }

        public override void SetProtoId(int protoId)
        {
            ProtobufBase<T>.TypeId = protoId;
        }
    }

    public class ProtocolParser
    {
        private static ProtocolParser _instance = new ProtocolParser();
        public static ProtocolParser Instance => _instance ?? new ProtocolParser();

        private static Dictionary<int, ProtocolPaseBase> _dicParser = new Dictionary<int, ProtocolPaseBase>();

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
        }

        public static void Register()
        {
            push((int)C2S_PROTOCOL_TYPE.C2STest, CreateParse<C2STest>());
            push((int)S2C_PROTOCOL_TYPE.S2CTest, CreateParse<S2CTest>());

            push((int)C2S_PROTOCOL_TYPE.C2SLogin, CreateParse<C2SLogin>());
            push((int)S2C_PROTOCOL_TYPE.S2CLogin, CreateParse<S2CLogin>());

            push((int)C2S_PROTOCOL_TYPE.C2SRegister, CreateParse<C2SRegister>());
            push((int)S2C_PROTOCOL_TYPE.S2CRegister, CreateParse<S2CRegister>());

            push((int)S2C_PROTOCOL_TYPE.S2CMove, CreateParse<S2CMove>());
            push((int)C2S_PROTOCOL_TYPE.C2SMove, CreateParse<C2SMove>());

            push((int)S2C_PROTOCOL_TYPE.S2CSight, CreateParse<S2CSight>());

            push((int)C2S_PROTOCOL_TYPE.C2SNavAuto, CreateParse<C2SNavAuto>());


            foreach (var (protoId, parser) in _dicParser)
            {
                parser.SetProtoId(protoId);
            }
        }


        private static void push(int protoId, ProtocolPaseBase parse)
        {
            _dicParser.Add(protoId, parse);
        }
    }
}
