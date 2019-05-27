using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TServer.Simple;

namespace TServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Server!");

            SimpleSocketServer server = new SimpleSocketServer("127.0.0.1", 11000);
            server.Start();
        }


    }
}
