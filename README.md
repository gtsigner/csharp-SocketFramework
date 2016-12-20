##Csharp TCP协议的简单封装

报头：  (int)密码 + (int)包总长
包体：  一个序列化过后的字符串

######

感谢MSDN,感谢Google上面找到了很多有用的知识点.


##Test


创建一个服务端
```c#
 try
            {
                myServer = new SocketServer(ip, prot);
            }
            catch (Exception ex)
            {
                MessageBox.Show("监听端口失败");
                return;
            }
            lb_IpAddress.Content = "";
            lb_Prot.Content = prot.ToString();
            //this.Start_Server();
            myServer.StartListen();
            //这些事件应该是第一次进行加载
            myServer.OnClientConnected += myServer_OnClientConnected;
            myServer.OnClientDisconnected += myServer_OnClientDisconnected;
            myServer.OnServerRecevied += new ReceiveEventHandler(myServer_OnReceive);
            pb_bar.IsIndeterminate = true;

```

创建一个简单的客户端
```
         this._socketClient = new SocketClient(this._server_host, this._server_port);
            this._socketClient.OnConnected += _socketClient_OnConnected;
            this._socketClient.OnDisConnected += _socketClient_OnDisConnected;
            this._socketClient.OnReceived += _socketClient_OnReceived;
            this._socketClient.OnConnectFailed += _socketClient_OnConnectFailed;
            this._socketClient.Connect();

```

当中大部分都是封装了事件
