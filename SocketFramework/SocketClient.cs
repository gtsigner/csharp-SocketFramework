using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OeynetSocket.SocketFramework
{

    public class SocketClient
    {
        private Socket _socket;
        //只拥有客户端进程
        public ClientThread clientThread
        {
            get;
            set;
        }

        //接受数据
        public event ReceiveEventHandler OnReceived = null;
        //链接成功
        public event SocketConnectEvent OnConnected = null;
        //链接失败
        public event SocketConnectEvent OnConnectFailed = null;

        //服务器断开链接
        public event SocketConnectEvent OnDisConnected = null;
        private String _host;
        private int _port;
        public SocketClient(String host, int port)
        {
            this._host = host;
            this._port = port;
            //TCP
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        //阻塞进行链接
        public void Connect()
        {
            try
            {
                this._socket.Connect(new IPEndPoint(IPAddress.Parse(this._host), this._port));
            }
            catch (Exception ex)
            {
                if (this.OnConnectFailed != null)
                {
                    //链接失败
                    this.OnConnectFailed(ConnectEventType.connect_failed, null);
                    return;
                }
                else
                {
                    throw ex;
                }
            }
            if (!this._socket.Connected)
            {
                throw new Exception("error");
            }

            this.clientThread = new ClientThread(this._socket);
            //链接成功触发事件
            if (this.OnConnected != null)
            {
                this.OnConnected(ConnectEventType.connected, null);
            }
            this.clientThread.OnClientDisconnected += clientThread_OnClientDisconnected;
            this.clientThread.OnReceviedPacket += clientThread_OnReceviedPacket;
            this.clientThread.Start();
        }

        /// <summary>
        /// 传递ClientThread事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientThread_OnReceviedPacket(object sender, ReceiveEventArgs e)
        {
            if (this.OnReceived != null)
            {
                this.OnReceived(sender, e);
            }
        }

        /// <summary>
        /// 传递事件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        void clientThread_OnClientDisconnected(ConnectEventType type, SocketEventArgs args)
        {
            if (this.OnDisConnected != null)
            {
                this.OnDisConnected(type, args);
            }
        }

        //释放链接
        public void DisConnect()
        {
            //关闭线程
            this.clientThread.Stop();
            //自己要关闭数据流
            this._socket.Shutdown(SocketShutdown.Both);
            this._socket.Close();
        }

    }
}
