using System;
using System.IO;
using System.Net.Sockets;

/// Author: https://github.com/zhaojunlike
namespace OeynetSocket.SocketFramework
{

    /// <summary>
    /// 为OnConnect和OnDisconnect事件提供数据
    /// </summary>
    public class SocketEventArgs : System.EventArgs
    {
        public Socket socket
        {
            get;
            set;
        }
        private string address = null;
        public ClientThread ClientThread
        {
            get;
            set;
        }

        /// <summary>
        /// 远程计算机IP地址和端口
        /// </summary>
        public string RemoteAddress
        {
            set
            {
                this.address = value;
            }
            get
            {
                return address;
            }
        }
        public SocketEventArgs()
        {

        }
        public SocketEventArgs(Socket socket)
        {
            this.socket = socket;
            this.address = socket.RemoteEndPoint.ToString();
        }

        public SocketEventArgs(string ip, int port)
        {
            this.address = string.Format("{0}:{1}", ip, port);
        }
    }
}
