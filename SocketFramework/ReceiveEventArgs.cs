using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Sockets;
using System.Text;


/// Author: https://github.com/zhaojunlike
namespace OeynetSocket.SocketFramework
{
    /// <summary>
    /// 为OnReceive事件提供数据
    /// </summary>
    public class ReceiveEventArgs : EventArgs
    {
        public List<Packet> Packets
        {
            get;
            set;
        }
        public ReceiveEventArgs(List<Packet> packets, String remoteAddress)
        {
            this.Packets = packets;
            this.RemoteAddress = remoteAddress;
        }
        public String RemoteAddress
        {
            get;
            set;
        }
    }
}
