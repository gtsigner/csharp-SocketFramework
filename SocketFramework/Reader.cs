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
            byte[] buffer = new byte[512];
            //key 验证客户端密码 4位
            //header length  4位
            int recSize = this.stream.Read(buffer, 0, buffer.Length);
            if (recSize != 0)
            {
                //放到数据组中
                byte[] realBuffer = new byte[recSize];
                //除去没用的一段
                Array.Copy(buffer, 0, realBuffer, 0, recSize);
                totalBytes = totalBytes.Concat(realBuffer).ToArray();//C#神器，byte[]会自动延长
                //如果当前包中有包头的数据得时候
                if (totalBytes.Length <= 8)
                {
                    //还不够一个
                    return nowPackets;
                }

                while (totalBytes.Length > 8)
                {
                    Packet nowPacket = new Packet();
                    //TODO 把 int型的key换成MD5
                    int keyLength = 4;
                    byte[] md5Key = new byte[4];
                    //缓存中的4位int copy出来
                    Array.Copy(totalBytes, 0, md5Key, 0, 4);
                    //int
                    int serverKey = BitConverter.ToInt32(md5Key, 0);
                    nowPacket.Key = serverKey;
                    if (serverKey.ToString() == "19960615")//是我的包,我的密码，哈哈哈，开始循环读
                    {
                        //接下是整个数据包的大小
                        byte[] packSizeBuffer = new byte[4];//先读整个包的大小(不包含MD5)
                        //取出数据包中的包含数据长度的
                        Array.Copy(totalBytes, 4, packSizeBuffer, 0, 4);
                        //取出整个包的大小
                        int packSize = BitConverter.ToInt32(packSizeBuffer, 0);
                        //当前数据缓存中长度
                        int currentSize = totalBytes.Length - keyLength - packSizeBuffer.Length;//标记整个包有多少，默认减掉包头Md5+PackSize
                        //判断如果是否有一个完整的数据包
                        if (totalBytes.Length >= packSize)
                        {
                            //包体的长度
                            int bodyShouldSize = packSize - 8;
                            //Console.Write(String.Format("TotalBytes:{0},当前数据包：{1} \n", totalBytes.Length, packSize));
                            byte[] nowPacketBodyBytes = new byte[bodyShouldSize];
                            //读出包体
                            Array.Copy(totalBytes, 8, nowPacketBodyBytes, 0, bodyShouldSize);
                            nowPacket.Body = Encoding.UTF8.GetString(nowPacketBodyBytes);
                            //删除掉已经读了得数据
                            byte[] newTotalBytes = new byte[totalBytes.Length - packSize];
                            Array.Copy(totalBytes, packSize, newTotalBytes, 0, totalBytes.Length - packSize);
                            //Array.Clear(totalBytes, 0, packSize); //此方法无法删除bytes数组中得数据
                            totalBytes = newTotalBytes;
                            this.nowPackets.Add(nowPacket);
                            //Console.WriteLine(String.Format("移除数据包后：{0},Packts数量:{1}", totalBytes.Length, nowPackets.Count));
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

        private void ParsePacket()
        {
            Packet nowPacket = new Packet();
            //TODO 把 int型的key换成MD5
            int keyLength = 4;
            byte[] md5Key = new byte[4];
            //缓存中的4位int copy出来
            Array.Copy(totalBytes, 0, md5Key, 0, 4);
            //int
            int serverKey = BitConverter.ToInt32(md5Key, 0);
            nowPacket.Key = serverKey;
            if (serverKey.ToString() == "19960615")//是我的包,我的密码，哈哈哈，开始循环读
            {
                //接下是整个数据包的大小
                byte[] packSizeBuffer = new byte[4];//先读整个包的大小(不包含MD5)
                //取出数据包中的包含数据长度的
                Array.Copy(totalBytes, 4, packSizeBuffer, 0, 4);
                //取出整个包的大小
                int packSize = BitConverter.ToInt32(packSizeBuffer, 0);
                //当前数据缓存中长度
                int currentSize = totalBytes.Length - keyLength - packSizeBuffer.Length;//标记整个包有多少，默认减掉包头Md5+PackSize
                //判断如果是否有一个完整的数据包
                if (totalBytes.Length >= packSize)
                {
                    //包体的长度
                    int bodyShouldSize = packSize - 8;
                    Console.Write(String.Format("TotalBytes:{0},当前数据包：{1} \n", totalBytes.Length, packSize));
                    byte[] nowPacketBodyBytes = new byte[bodyShouldSize];
                    //读出包体
                    Array.Copy(totalBytes, 8, nowPacketBodyBytes, 0, bodyShouldSize);
                    nowPacket.Body = Encoding.UTF8.GetString(nowPacketBodyBytes);
                    //删除掉已经读了得数据
                    Array.Clear(totalBytes, 0, packSize);
                    this.nowPackets.Add(nowPacket);
                }
                else
                {
                    return;
                }
            }
        }

    }
}
