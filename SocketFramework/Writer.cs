using System;
using System.Data;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Linq;

/// Author: https://github.com/zhaojunlike
namespace OeynetSocket.SocketFramework
{
    /// <summary>
    /// Writer 的摘要说明。
    /// </summary>
    public class Writer
    {

        private NetworkStream stream = null;
        private Socket socket;

        public Writer(Socket socket)
        {
            this.socket = socket;
            stream = new NetworkStream(socket);
        }

        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="msg"></param>
        public void WriteString(string msg)
        {
            Console.WriteLine("Write一条消息:" + msg);

            byte[] data = new byte[0];
            //把自己的密码加入
            byte[] md5 = Encoding.UTF8.GetBytes("cba6c83625b358bf562e624fc903c244");
            byte[] bodyBytes = Encoding.UTF8.GetBytes(msg);
            int totalLength = md5.Length + md5.Length + bodyBytes.Length;
            byte[] lengthBytes = BitConverter.GetBytes(totalLength);//包长度
            Console.Write(String.Format("数据包长度：{0},数据:", totalLength, msg));

            //尝试包头和包体分开发
            //socket.Send(data.Concat(md5).Concat(lengthBytes).ToArray());
            data = data.Concat(md5).Concat(lengthBytes).Concat(bodyBytes).ToArray();
            this.stream.Write(data, 0, data.Length);
            //flush
            //包体
            //socket.Send(bodyBytes);
        }

        public void WriteBytes(byte[] msg)
        {
            this.stream.Write(msg, 0, msg.Length);
        }

        public void WriteInt(int msg)
        {

        }
        public void WriteFlot(float msg)
        {

        }
        public void Close()
        {
            stream.Close();
        }
        public void WritePacket(Packet packet)
        {
            //把自己的密码加入
            byte[] md5 = Encoding.UTF8.GetBytes(packet.Key);
            //包体
            byte[] bodyBytes = Encoding.UTF8.GetBytes(packet.Body);
            //总长度
            int totalLength = md5.Length + 4 + bodyBytes.Length;
            //包含长度的数据包
            byte[] lengthBytes = BitConverter.GetBytes(totalLength);
            //发送Data
            this.WriteBytes(md5.Concat(lengthBytes).Concat(bodyBytes).ToArray());
        }
    }
}
