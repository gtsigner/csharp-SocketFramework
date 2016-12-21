using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

/// Author: https://github.com/zhaojunlike
namespace OeynetSocket.SocketFramework
{

    public class SocketClient
    {
        //服务端socket
        private Socket _socketClient;
        //只拥有客户端进程
        private ClientThread clientThread
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
        public bool IsConnection
        {
            get;
            set;
        }

        public SocketClient(String host, int port)
        {
            this._host = host;
            this._port = port;
            //TCP
            this._socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        //阻塞进行链接
        public void Connect()
        {
            try
            {
                this._socketClient.Connect(new IPEndPoint(IPAddress.Parse(this._host), this._port));
            }
            catch (Exception ex)
            {
                if (this.OnConnectFailed != null)
                {
                    //链接失败
                    this.OnConnectFailed(null, null);
                    return;
                }
                else
                {
                    throw ex;
                }
            }
            if (!this._socketClient.Connected)
            {
                throw new Exception("error");
            }
            this.clientThread = new ClientThread(this._socketClient);
            //链接成功触发事件
            if (this.OnConnected != null)
            {
                this.OnConnected(clientThread, null);
            }
            this.clientThread.OnThreadStop += clientThread_OnThreadStop;
            this.clientThread.OnReceviedPacket += clientThread_OnReceviedPacket;
            this.clientThread.Start();
            this.IsConnection = true;
        }

        /// <summary>
        /// 客户端线程关闭
        /// </summary>
        /// <param name="sender"></param>
        void clientThread_OnThreadStop(object sender)
        {
            this.IsConnection = false;
            if (this.OnDisConnected != null)
            {
                this.OnDisConnected(this.clientThread, null);
            }
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
        /// 发送数据包到服务端
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool WritePacket(Packet packet)
        {
            return this.clientThread.WritePacket(packet);
        }

        //释放链接
        public void Stop()
        {
            //断开客户端线程
            this.clientThread.Stop();
            //断开socket
            if (this._socketClient.Connected)
            {
                this._socketClient.Close();
            }
        }
    }
}
