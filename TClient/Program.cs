using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TClient.Simple;

namespace TClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Client!");

            SimpleSocketClient client = new SimpleSocketClient("127.0.0.1", 11000);
            client.Connect();
        }


    }
}
