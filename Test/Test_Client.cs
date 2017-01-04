using OeynetSocket.SocketFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OeynetSocket.Test
{
    class Test_Client
    {

        public static void Main(String[] args)
        {
            SocketClient client = new SocketClient("192.168.32.1", 8888);
            client.OnConnected += client_OnConnected;
            client.OnReceived += client_OnReceived;
            client.OnConnectFailed += client_OnConnectFailed;
            client.OnDisConnected += client_OnDisConnected;
            client.Connect();
            Console.Read();
            client.Stop();
        }

        static void client_OnDisConnected(object sender, SocketEventArgs data)
        {
            Console.WriteLine("服务端关闭");
        }

        static void client_OnConnectFailed(object sender, SocketEventArgs data)
        {
            Console.WriteLine("链接服务端失败");
        }

        static void client_OnConnected(object sender, SocketEventArgs data)
        {
            Console.WriteLine("链接服务端成功");
        }


        static void client_OnReceived(object sender, ReceiveEventArgs data)
        {
            for (var i = 0; i < data.Packets.Count; i++)
            {
                Console.WriteLine(data.Packets[i].Body);
            }
        }
    }
}
