using System;
using System.Data;
using System.Net.Sockets;
using System.Threading;

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
        public Writer ClientWriter
        {
            get;
            set;
        }
        public Reader ClientReader
        {
            get;
            set;
        }

        public event ReceiveEventHandler OnReceviedPacket = null;
        //客户端断开链接
        public event SocketConnectEvent OnClientDisconnected = null;

        public event ClientThreadAbortingEvent OnAbortingEvent = null;

        private bool IsConnect = false;
        public string RemoteAddress
        {
            get
            {
                return ClientSocket.RemoteEndPoint.ToString();
            }
        }

        public ClientThread(Socket client)
        {
            this.IsConnect = true;
            this.ClientSocket = client;
            this.ClientWriter = new Writer(this.ClientSocket);
            this.ClientReader = new Reader(this.ClientSocket);
        }

        /// <summary>
        /// 开始客户线程
        /// </summary>
        public void Start()
        {
            thread = new Thread(new ThreadStart(_startThread));
            thread.Name = "客户线程";
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
                this.OnClientDisconnected(ConnectEventType.disconnected, null);
            }
            this.ClientReader.Close();
            this.ClientWriter.Close();
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
                    Packet packet = this.ClientReader.ReadPackSync();
                    if (this.OnReceviedPacket != null)
                    {
                        this.OnReceviedPacket(ClientSocket, new ReceiveEventArgs(packet));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //如果异常就关闭
                    this.Stop();
                    return;
                }
            }
            //如果跳出来了也要将链接关闭
            this.Stop();
        }
    }
}
