using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Sockets;
using System.Threading;

/// Author: https://github.com/zhaojunlike
namespace OeynetSocket.SocketFramework
{

    /// <summary>
    /// 用于服务器处理每个客户端的线程
    /// </summary>
    public class ClientThread
    {
        public Socket ClientSocket
        {
            get;
            set;
        }
        private Thread thread = null;
        private Writer ClientWriter
        {
            get;
            set;
        }
        private Reader ClientReader
        {
            get;
            set;
        }

        public event ReceiveEventHandler OnReceviedPacket = null;
        //客户端断开链接
        public event SocketConnectEvent OnClientDisconnected = null;

        public event ClientThreadAbortingEvent OnAbortingEvent = null;

        private bool IsConnect = false;
        private String _remoteAddress;
        public string RemoteAddress
        {
            set
            {
                this._remoteAddress = value;
            }
            get
            {
                return this._remoteAddress;
            }
        }
        public ClientThread(Socket client)
        {
            this.IsConnect = true;
            this.ClientSocket = client;
            this.ClientWriter = new Writer(this.ClientSocket);
            this.ClientReader = new Reader(this.ClientSocket);
            this._remoteAddress = ClientSocket.RemoteEndPoint.ToString();
        }

        /// <summary>
        /// 开始客户线程
        /// </summary>
        public void Start()
        {
            thread = new Thread(new ThreadStart(_startThread));
            thread.Name = "客户线程";
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 关闭客户端,但是流任然没有关闭
        /// </summary>
        public void Stop()
        {
            this._abortThread();
            //发送事件，封装数据
            if (this.OnClientDisconnected != null)
            {
                SocketEventArgs args = new SocketEventArgs();
                args.RemoteAddress = this.RemoteAddress;
                args.ClientThread = this;
                this.OnClientDisconnected(ConnectEventType.disconnected, args);
            }
            this.ClientReader.Close();
            this.ClientWriter.Close();
            this.IsConnect = false;
        }

        /// <summary>
        /// 停止客户线程
        /// </summary>
        private void _abortThread()
        {
            //关闭线程
            if (this.OnAbortingEvent != null)
            {
                this.OnAbortingEvent(null);
            }
            Console.WriteLine("Thread One Exit.");
            thread.Abort();
        }

        private void _startThread()
        {
            while (ClientSocket.Connected && this.ClientReader.stream.CanRead)
            {
                try
                {
                    //去接受一个完整的数据包
                    List<Packet> packets = this.ClientReader.ReadPackSync();
                    if (this.OnReceviedPacket != null)
                    {
                        ReceiveEventArgs args = new ReceiveEventArgs(packets);
                        args.RemoteAddress = RemoteAddress;
                        this.OnReceviedPacket(ClientSocket, args);
                    }
                }
                catch (Exception ex)
                {

                    //如果异常就关闭
                    Console.WriteLine("ERROR:" + ex.Message);
                    if (this.OnClientDisconnected != null)
                    {
                        SocketEventArgs args = new SocketEventArgs();
                        args.RemoteAddress = this.RemoteAddress;
                        args.ClientThread = this;
                        this.OnClientDisconnected(ConnectEventType.disconnected, args);
                    }
                    //并且让客户端吧自己删除掉
                    this._abortThread();
                    this.ClientReader.Close();
                    this.ClientWriter.Close();
                    this.ClientSocket.Close();
                    this.IsConnect = false;
                    return;
                }
            }
            //如果跳出来了也要将链接关闭
            this.Stop();
        }

        public void WritePacket(Packet packet)
        {
            try
            {
                this.ClientWriter.WritePacket(packet);
            }
            catch (Exception ex)
            {
                //如果异常就关闭
                Console.WriteLine("ERROR:" + ex.Message);
                if (this.OnClientDisconnected != null)
                {
                    SocketEventArgs args = new SocketEventArgs();
                    args.RemoteAddress = this.RemoteAddress;
                    args.ClientThread = this;
                    this.OnClientDisconnected(ConnectEventType.disconnected, args);
                }
                //并且让客户端吧自己删除掉
                this._abortThread();
                this.ClientReader.Close();
                this.ClientWriter.Close();
                this.ClientSocket.Close();
                this.IsConnect = false;
            }
        }
    }
}
