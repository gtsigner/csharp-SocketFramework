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
        public static void Main1(String[] args)
        {
            Common.SocketIsDebug = true;
            IPAddress addr = IPAddress.Parse("192.168.32.1");
            SocketServer server = new SocketServer(addr, 8888);
            server.OnClientConnected += server_OnClientConnected;
            server.OnServerRecevied += server_OnServerRecevied;
            server.OnClientThreadStop += server_OnClientThreadStop;
            server.OnClientDisConnected += server_OnClientDisConnected;
            server.Start();
            Console.Read();
            server.Stop();
        }

        static void server_OnServerRecevied(object sender, ReceiveEventArgs data)
        {
            for (var i = 0; i < data.Packets.Count; i++)
            {
                Console.Write(data.Packets[i].Body);
            }
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
