using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Data;
using System.Collections.Generic;

/// Author: https://github.com/zhaojunlike
namespace OeynetSocket.SocketFramework
{

    /// <summary>
    /// 服务器组件
    /// </summary>
    public class SocketServer
    {
        private Socket _server = null;
        private string _address = null;
        private int _port = 0;
        private Thread listenThread = null;
        //保存客户端信息
        private List<ClientThread> _clients = new List<ClientThread>();
        //链接上事件
        public event SocketConnectEvent OnClientConnected = null;
        //客户端关闭链接事件
        public event ClientThreadStopEvent OnClientThreadStop = null;
        //添加一个客户端
        public event SocketConnectEvent OnClientDisConnected = null;
        //接受到数据事件
        public event ReceiveEventHandler OnServerRecevied = null;

        private Thread daemonThread;

        #region Public

        /// <summary>
        /// 监听地址
        /// </summary>
        public string Address
        {
            get
            {
                return this._address;
            }
            set
            {
                this._address = value;
            }
        }

        /// <summary>
        /// 监听端口
        /// </summary>
        public int Port
        {
            get
            {
                return this._port;
            }
            set
            {
                this._port = value;
            }
        }

        /// <summary>
        /// 当前连接到服务器的客户数量
        /// </summary>
        public int OnlineCount
        {
            get
            {
                return this._clients.Count;
            }
        }

        #endregion

        #region 获取本机IP
        public static IPAddress getIpAddress()
        {
            //获取本地的IP地址
            string AddressIP = string.Empty;
            IPAddress ip = null;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                    ip = _IPAddress;
                }
            }
            return ip;
        }
        #endregion


        public SocketServer(IPAddress _ip, int _port)
        {
            this._address = _ip.ToString();
            this._port = _port;
            try
            {
                this._server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this._server.Bind(new IPEndPoint(IPAddress.Parse(this._address), this._port));
            }
            catch (Exception ex)
            {
                throw new Exception("端口问题");
            }
        }

        /// <summary>
        /// 开始监听
        /// </summary>
        public void Start()
        {
            try
            {
                listenThread = new Thread(new ThreadStart(this._listen));
                listenThread.Name = "服务器监听线程";
                listenThread.Start();
                Console.WriteLine("Server Start.\n");
                daemonThread = new Thread(new ThreadStart(this._sendHeartActive));
                daemonThread.Name = "心跳线程";
                daemonThread.IsBackground = true;
                daemonThread.Start();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void _sendHeartActive()
        {

        }

        /// <summary>
        /// 停止监听
        /// </summary>
        public void Stop()
        {
            //断开每一个客户端现成链接
            for (int i = 0; i < this._clients.Count; i++)
            {
                //关闭会关闭链接和关闭线程
                ((ClientThread)this._clients[i]).Stop();
            }
            this._clients.Clear();
            //关闭监听线程
            if (listenThread != null)
                listenThread.Abort();
            //关闭监听线程
            if (daemonThread != null)
                daemonThread.Abort();
            //关闭serversocket
            if (this._server != null)
                this._server.Close();
            Console.WriteLine("Framework log：Server Close.\n");
        }

        /// <summary>
        /// 向指定的客户发送信息
        /// </summary>
        /// <param name="remoteAddress">客户端地址</param>
        /// <param name="msg">要发送的信息</param>
        public bool Write(string remoteAddress, Packet packet)
        {
            for (int i = 0; i < this._clients.Count; i++)
            {
                ClientThread clientThread = (ClientThread)this._clients[i];
                if (clientThread.RemoteAddress.Equals(remoteAddress))
                {
                    //这里
                    return clientThread.WritePacket(packet);
                }
            }
            return false;
        }

        /// <summary>
        /// 给所有客户端发送数据包
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="noSendRemote"></param>
        public bool WriteAll(Packet packet, String[] noSendRemote)
        {
            foreach (ClientThread item in _clients)
            {
                #region 排除不发送数据得客户端
                if (noSendRemote != null)
                {
                    foreach (String noItem in noSendRemote)
                    {
                        if (noItem == item.RemoteAddress)
                        {
                            if (Common.SocketIsDebug) { Console.WriteLine(Common.Log_Prefix + "Skip Send To:" + item.RemoteAddress); }
                            continue;
                        }
                    }
                }
                #endregion
                if (Common.SocketIsDebug) { Console.WriteLine(Common.Log_Prefix + "Send To:" + item.RemoteAddress); }
                //这里如果发生异常，那么会先调用Exception，然后调用Stop，所以会减掉1，所以只要一个发送失败了，我们就跳出循环
                if (!item.WritePacket(packet)) { return false; }
            }
            return true;
        }


        #region
        /// <summary>
        /// 移除某个客户端
        /// </summary>
        /// <param name="client"></param>
        internal void RemoveClient(ClientThread client)
        {
            this._clients.Remove(client);
            //移除客户端后发送事件
            if (this.OnClientDisConnected != null)
            {
                //sender
                this.OnClientDisConnected(client, null);
            }
        }


        //监听
        private void _listen()
        {
            this._server.Listen(20);
            while (true)
            {
                //循环accept客户端的请求，然后开启新的现成进行数据交互
                Socket client = this._server.Accept();
                ClientThread clientThread = new ClientThread(client);
                //客户端线程异常事件
                clientThread.OnThreadException += clientThread_OnThreadException;
                //客户端线程停止事件
                clientThread.OnThreadStop += clientThread_OnThreadStop;
                clientThread.OnReceviedPacket += clientThread_OnReceviedPacket;
                this._clients.Add(clientThread);
                //触发链接事件
                if (this.OnClientConnected != null)
                {
                    //链接成功
                    this.OnClientConnected(clientThread, null);
                }
                clientThread.Start();
            }
        }
        /// <summary>
        /// 客户端线程接受到数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientThread_OnReceviedPacket(object sender, ReceiveEventArgs data)
        {
            if (this.OnServerRecevied != null)
            {
                this.OnServerRecevied(sender, data);
            }
        }

        /// <summary>
        /// 断开和异常是同时发生
        /// </summary>
        /// <param name="sender"></param>
        void clientThread_OnThreadStop(object sender)
        {
            if (this.OnClientThreadStop != null)
            {
                this.OnClientThreadStop(sender);
            }
            this.RemoveClient((ClientThread)sender);
        }

        /// <summary>
        /// 客户端异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        void clientThread_OnThreadException(object sender, ClientThreadEventArgs data)
        {

        }
        #endregion
    }

}
