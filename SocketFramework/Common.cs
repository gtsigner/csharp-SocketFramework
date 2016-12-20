using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/// Author: https://github.com/zhaojunlike
namespace OeynetSocket.SocketFramework
{
    public delegate void SocketEventHandler(object sender, SocketEventArgs e);
    public delegate void ReceiveEventHandler(object sender, ReceiveEventArgs e);
    public enum ConnectEventType
    {
        connected = 1,
        disconnected = 2,
        connect_failed = 3,
    }
    public delegate void SocketConnectEvent(ConnectEventType type, SocketEventArgs args);
    //客户端线程abord之前触发的事件
    public delegate void ClientThreadAbortingEvent(object sender);
    //客户端socket关闭
    public delegate void ClientThreadCloseEvent(object sender);
    public class Common
    {

    }
}
