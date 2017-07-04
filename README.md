## Csharp TCP协议的简单封装

报头：  (int)密码 + (int)包总长
包体：  一个序列化过后的字符串

######

感谢MSDN,感谢Google上面找到了很多有用的知识点.

## Test
创建一个服务端
```c#
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


```

创建一个简单的客户端
```c#
  class Test_Client
    {

        public static void Main(String[] args)
        {
            SocketClient client = new SocketClient("127.0.0.1", 6666);
            client.OnConnected += client_OnConnected;
            client.OnReceived += client_OnReceived;
            client.OnConnectFailed += client_OnConnectFailed;
            client.OnDisConnected += client_OnDisConnected;
            client.Connect();
            Console.Read();
            client.DisConnect();
        }

        static void client_OnDisConnected(ConnectEventType type, SocketEventArgs args)
        {
            //客户端丢失和服务端的链接
            Console.WriteLine("服务端关闭");
        }

        static void client_OnConnectFailed(ConnectEventType type, SocketEventArgs args)
        {
            Console.WriteLine("链接服务端失败");
        }

        static void client_OnReceived(object sender, ReceiveEventArgs e)
        {
            Console.WriteLine("收到数据包个数:" + e.Packets.Count);
        }

        static void client_OnConnected(ConnectEventType type, SocketEventArgs args)
        {
            Console.WriteLine("链接服务端成功");
        }
    }

```

当中大部分都是封装了事件,方便用户回调使用
