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
            Common.SocketIsDebug = true;
            IPAddress addr = IPAddress.Parse("10.1.56.117");
            SocketServer server = new SocketServer(addr, 6666);
            server.OnClientConnected += server_OnClientConnected;
            server.OnClientThreadStop += server_OnClientThreadStop;
            server.OnClientDisConnected += server_OnClientDisConnected;
            server.Start();
            Console.Read();
            server.Stop();
        }

        static void server_OnClientDisConnected(object sender, SocketEventArgs data)
        {
            Console.WriteLine("One Client DisConnected: ip" + ((ClientThread)sender).RemoteAddress);

        }

        static void server_OnClientConnected(object sender, SocketEventArgs data)
        {
            Console.WriteLine("One Client Connected: ip" + ((ClientThread)sender).RemoteAddress);
        }

        static void server_OnClientThreadStop(object sender)
        {
            Console.WriteLine("One ClientThread Stop.");
        }

    }
}
