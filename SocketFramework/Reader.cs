using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
/// Author: https://github.com/zhaojunlike
namespace OeynetSocket.SocketFramework
{
    /// <summary>
    /// Reader 的摘要说明。
    /// </summary>
    public class Reader
    {
        public NetworkStream stream
        {
            get;
            set;
        }

        public Reader(Socket socket)
        {
            stream = new NetworkStream(socket);
        }

        public void Close()
        {
            stream.Close();
        }

        //从流中读取bytes
        public byte[] ReadBytes()
        {
            return null;
        }

        //记录上一次的包
        private byte[] totalBytes = new byte[0];
        private List<Packet> nowPackets = new List<Packet>();
        //同步阻塞读取一个或者多个完整的数据包
        public List<Packet> ReadPackSync()
        {
            this.nowPackets.Clear();
            Packet packet = new Packet();
            //开始循环去读取数据到缓冲区
            byte[] buffer = new byte[1024];
            //key 验证客户端密码 32位
            int md5KeyLength = 32;
            //header length  4位
            int recSize = this.stream.Read(buffer, 0, buffer.Length);
            if (recSize != 0)
            {
                //放到数据组中
                byte[] realBuffer = new byte[recSize];
                //除去没用的一段
                Array.Copy(buffer, 0, realBuffer, 0, recSize);
                totalBytes = totalBytes.Concat(realBuffer).ToArray();//C#神器，byte[]会自动延长
                //如果当前包中有整个包头的数据得时候
                if (totalBytes.Length <= (md5KeyLength + 4))
                {
                    //还不够一个
                    return nowPackets;
                }
                //循环去解析数据头
                while (totalBytes.Length > (md5KeyLength + 4))
                {
                    Packet nowPacket = new Packet();
                    byte[] md5Key = new byte[md5KeyLength];
                    //缓存中的md5 copy出来
                    Array.Copy(totalBytes, 0, md5Key, 0, md5KeyLength);
                    //int
                    String serverKey = Encoding.UTF8.GetString(md5Key);
                    nowPacket.Key = serverKey;
                    if (serverKey.ToString() == "cba6c83625b358bf562e624fc903c244")//是我的包,我的密码，哈哈哈，开始循环读
                    {
                        //接下是整个数据包的大小
                        byte[] packSizeBuffer = new byte[4];//先读整个包的大小(不包含MD5)
                        //取出数据包中的包含数据长度的,偏移32长度
                        Array.Copy(totalBytes, md5KeyLength, packSizeBuffer, 0, 4);
                        //取出整个包的大小
                        int packSize = BitConverter.ToInt32(packSizeBuffer, 0);
                        //当前数据缓存中长度
                        int currentSize = totalBytes.Length - md5KeyLength - packSizeBuffer.Length;//标记整个包有多少，默认减掉包头Md5+PackSize
                        //判断如果是否有一个完整的数据包
                        if (totalBytes.Length >= packSize)
                        {
                            //包体的长度
                            int bodyShouldSize = packSize - md5KeyLength - 4;
                            byte[] nowPacketBodyBytes = new byte[bodyShouldSize];
                            //读出包体
                            Array.Copy(totalBytes, md5KeyLength + 4, nowPacketBodyBytes, 0, bodyShouldSize);
                            nowPacket.Body = Encoding.UTF8.GetString(nowPacketBodyBytes);
                            //删除掉已经读了得数据
                            byte[] newTotalBytes = new byte[totalBytes.Length - packSize];
                            Array.Copy(totalBytes, packSize, newTotalBytes, 0, totalBytes.Length - packSize);
                            totalBytes = newTotalBytes;
                            this.nowPackets.Add(nowPacket);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        //断开客户端得链接
                        byte[] newTotalBytes = new byte[0];
                        break;
                    }
                }

            }
            else
            {
                throw new Exception("connect read error");
            }
            return nowPackets;
        }
    }
}
