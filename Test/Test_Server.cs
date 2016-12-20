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
            server.OnClientDisconnected += server_OnClientDisconnected;
            server.StartListen();
            Console.Read();
            server.StopListen();
        }

        static void server_OnClientDisconnected(ConnectEventType type, SocketEventArgs args)
        {
            Console.WriteLine("One Client DisConnected: ip" + args.RemoteAddress);
        }
        static void server_OnClientConnected(ConnectEventType type, SocketEventArgs args)
        {
            Console.WriteLine("One Client Connected: ip" + args.RemoteAddress);
        }
    }
}
