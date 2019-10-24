using Common.Protobuf;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Common.Normal
{
    public class ExtSocket
    {
        public Guid Guid;
        public SocketAsyncEventArgs SocketEventArgs;
        public ProtocolBufBase Protocol;
    }
}
