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
        private Socket ClientSocket
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

        //客户端线程接受数据事件
        public event ClientThreadReceivedEvent OnReceviedPacket = null;

        //客户端线程关闭事件
        public event ClientThreadStopEvent OnThreadStop = null;

        //接受异常事件
        public event ClientThreadExceptionEvent OnThreadException = null;

        //客户端线程停止事件
        public event ClientThreadAborEvent OnThreadAbort = null;

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
            //发送事件，封装数据
            if (this.OnThreadStop != null)
            {
                SocketEventArgs args = new SocketEventArgs();
                args.RemoteAddress = this.RemoteAddress;
                args.ClientThread = this;
                //发送事件参数
                this.OnThreadStop(this);
            }
            this.ClientReader.Close();
            this.ClientWriter.Close();
            this.IsConnect = false;
            //自动关闭socket链接
            this.ClientSocket.Close();
            this._abortThread();
        }

        /// <summary>
        /// 停止客户线程
        /// </summary>
        private void _abortThread()
        {
            //关闭线程
            if (this.OnThreadAbort != null)
            {
                this.OnThreadAbort(this);
            }
            Console.WriteLine("Thread Exit .... Remote:" + this.RemoteAddress);
            thread.Abort();
        }

        /// <summary>
        /// 线程函数
        /// </summary>
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
                        ReceiveEventArgs args = new ReceiveEventArgs(packets, this.RemoteAddress);
                        this.OnReceviedPacket(ClientSocket, args);
                    }
                }
                catch (Exception ex)
                {
                    //如果异常就关闭
                    Console.WriteLine("客户端读取数据异常ERROR:" + ex.Message);
                    if (this.OnThreadException != null)
                    {
                        ClientThreadEventArgs args = new ClientThreadEventArgs();
                        args.EventType = ClientThreadEventType.read_error;
                        this.OnThreadException(this, args);
                    }
                    //自动关闭
                    this.Stop();
                    return;
                }
            }
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool WritePacket(Packet packet)
        {
            try
            {
                this.ClientWriter.WritePacket(packet);
                return true;
            }
            catch (Exception ex)
            {
                //如果异常就关闭
                if (Common.SocketIsDebug) { Console.WriteLine(Common.Log_Prefix + "写操作异常捕获ERROR:" + ex.Message); }
                if (this.OnThreadException != null)
                {
                    ClientThreadEventArgs args = new ClientThreadEventArgs();
                    args.EventType = ClientThreadEventType.write_error;
                    this.OnThreadException(this, args);
                }
                //并且让客户端吧自己删除掉
                this.Stop();
            }
            return false;
        }
    }
}
