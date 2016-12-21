using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/// Author: https://github.com/zhaojunlike
namespace OeynetSocket.SocketFramework
{
    public delegate void SocketEventHandler(object sender, SocketEventArgs data);
    public delegate void ReceiveEventHandler(object sender, ReceiveEventArgs data);
    public enum ConnectEventType
    {
        connected = 1,
        disconnected = 2,
        connect_failed = 3,
    }
    //是否是调试


    public enum ClientThreadEventType
    {
        connected_error = 1,
        read_error = 4,
        write_error = 5,
        disconnected_error = 2,
        connect_failed = 3,
    }
    public delegate void SocketConnectEvent(object sender, SocketEventArgs data);
    //客户端线程abord之前触发的事件
    public delegate void ClientThreadAborEvent(object sender);
    //客户端线程socket关闭
    public delegate void ClientThreadStopEvent(object sender);
    //客户端线程接受到数据得事件,第一个是发送者，第二个是数据包
    public delegate void ClientThreadReceivedEvent(object sender, ReceiveEventArgs data);
    //客户端线程异常事件
    public delegate void ClientThreadExceptionEvent(object sender, ClientThreadEventArgs data);

    public static class Common
    {
        public static bool SocketIsDebug
        {
            get;
            set;
        }

        public static String Log_Prefix = "https://oeynet.com/ Log Message：";
    }
}
