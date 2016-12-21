using OeynetSocket.SocketFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OeynetSocket.Test
{
    class Test_Client
    {

        public static void test(String[] args)
        {
            SocketClient client = new SocketClient("127.0.0.1", 6666);
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


        static void client_OnReceived(object sender, ReceiveEventArgs e)
        {
            Console.WriteLine("收到数据包个数:" + e.Packets.Count);
        }
    }
}
