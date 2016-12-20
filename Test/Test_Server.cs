using OeynetSocket.SocketFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace OeynetSocket.Test
{
    class Test_Server
    {
        public static void Main(String[] args)
        {
            IPAddress addr = IPAddress.Parse("127.0.0.1");
            SocketServer server = new SocketServer(addr, 6666);
            server.OnClientConnected += server_OnClientConnected;
            server.StartListen();
            Console.Read();
        }
        static void server_OnClientConnected(ConnectEventType type, SocketEventArgs args)
        {
            Console.WriteLine("One Client Connected: ip" + args.RemoteAddress);
        }
    }
}
