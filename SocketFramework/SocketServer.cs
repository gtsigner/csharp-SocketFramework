using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Data;
using System.Collections.Generic;

namespace OeynetSocket.SocketFramework
{

    /// <summary>
    /// 服务器组件
    /// </summary>
    public class SocketServer
    {
        private Socket server = null;
        private string address = null;
        private int port = 0;
        private Thread listenThread = null;
        //保存客户端信息
        private List<ClientThread> clients = new List<ClientThread>();

        //链接上事件
        public event SocketConnectEvent OnClientConnected = null;
        //断开链接事件
        public event SocketConnectEvent OnClientDisconnected = null;
        //接受到数据事件
        public event ReceiveEventHandler OnServerRecevied = null;


        #region File

        /// <summary>
        /// 监听地址
        /// </summary>
        public string Address
        {
            get
            {
                return address;
            }
            set
            {
                address = value;
            }
        }

        /// <summary>
        /// 监听端口
        /// </summary>
        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
            }
        }

        /// <summary>
        /// 当前连接到服务器的客户数量
        /// </summary>
        public int OnlineCount
        {
            get
            {
                return clients.Count;
            }
        }

        /// <summary>
        /// 连接到服务器的全部客户
        /// </summary>
        public List<ClientThread> Clients
        {
            get
            {
                return clients;
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
            address = _ip.ToString();
            port = _port;
            try
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            }
            catch (Exception ex)
            {
                throw new Exception("端口问题");
            }
        }

        /// <summary>
        /// 开始监听
        /// </summary>
        public void StartListen()
        {
            try
            {
                listenThread = new Thread(new ThreadStart(this._listen));
                listenThread.Name = "服务器监听线程";
                listenThread.Start();
                Console.WriteLine("Server Start.\n");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 停止监听
        /// </summary>
        public void StopListen()
        {
            //断开每一个客户端现成链接
            for (int i = 0; i < clients.Count; i++)
            {
                ((ClientThread)clients[i]).Stop();
                //关闭connect
                clients[i].ClientSocket.Close();
            }
            clients.Clear();
            //关闭监听线程
            if (listenThread != null)
                listenThread.Abort();
            //关闭serversocket
            if (server != null)
                server.Close();
            Console.WriteLine("Server Close.\n");
        }

        /// <summary>
        /// 向指定的客户发送信息
        /// </summary>
        /// <param name="remoteAddress">客户端地址</param>
        /// <param name="msg">要发送的信息</param>
        public void Write(string remoteAddress, Packet packet)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                ClientThread clientThread = (ClientThread)clients[i];
                if (clientThread.RemoteAddress.Equals(remoteAddress))
                {
                    clientThread.ClientWriter.WritePacket(packet);
                    break;
                }
            }
        }

        public void WriteAll(Packet packet, String[] noSendRemote)
        {
            foreach (ClientThread item in clients)
            {
                item.ClientWriter.WritePacket(packet);
            }
        }

        public byte[] ReadData(IPAddress remoteAddress)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                ClientThread clientThread = (ClientThread)clients[i];
                if (clientThread.RemoteAddress.Equals(remoteAddress))
                {

                    return System.Text.Encoding.Default.GetBytes("123");
                }

            }
            return System.Text.Encoding.Default.GetBytes("123");
        }

        #region
        /// <summary>
        /// 移除某个客户端
        /// </summary>
        /// <param name="client"></param>
        internal void RemoveClient(ClientThread client)
        {
            clients.Remove(client);
        }

        //监听
        private void _listen()
        {
            server.Listen(20);
            while (true)
            {
                //循环accept客户端的请求，然后开启新的现成进行数据交互
                Socket client = server.Accept();
                ClientThread clientThread = new ClientThread(client);

                //客户端收到数据包
                clientThread.OnReceviedPacket += clientThread_OnServerReceive;
                //链接断开
                clientThread.OnClientDisconnected += clientThread_OnClientDisconnected;
                clientThread.OnAbortingEvent += clientThread_OnAbortingEvent;
                clients.Add(clientThread);
                //触发链接事件
                if (this.OnClientConnected != null)
                {
                    //链接成功
                    this.OnClientConnected(ConnectEventType.connected, new SocketEventArgs(client));
                }
                clientThread.Start();
            }
        }

        void clientThread_OnAbortingEvent(object sender)
        {
            //进行删除客户端
        }

        void clientThread_OnClientDisconnected(ConnectEventType type, SocketEventArgs args)
        {
            Console.WriteLine("disconnect test");
            if (this.OnClientDisconnected != null)
            {
                this.OnClientDisconnected(type, args);
            }

        }

        void clientThread_OnServerReceive(object sender, ReceiveEventArgs e)
        {
            if (this.OnServerRecevied != null)
            {
                this.OnServerRecevied(sender, e);
            }
        }

        #endregion
    }

}
