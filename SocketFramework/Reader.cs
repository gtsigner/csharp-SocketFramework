using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

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


        //同步阻塞读取一个完整的数据包
        public Packet ReadPackSync()
        {
            Packet packet = new Packet();

            //开始循环去读取数据到缓冲区
            byte[] buffer = new byte[512];
            byte[] totalBytes = new byte[0];
            //key 验证客户端密码 4位
            //header length  4位
            int recSize = this.stream.Read(buffer, 0, buffer.Length);
            if (recSize != 0)
            {
                //放到数据组中
                totalBytes = totalBytes.Concat(buffer).ToArray();//C#神器，byte[]会自动延长
                //TODO 把 int型的key换成MD5
                int keyLength = 4;
                byte[] md5Key = new byte[4];
                //把包头的4位int copy出来
                Array.Copy(totalBytes, 0, md5Key, 0, 4);
                //int
                int serverKey = BitConverter.ToInt32(md5Key, 0);
                Console.WriteLine("MD5包头Key:" + serverKey);
                if (serverKey.ToString() == "19960615")//是我的包,我的密码，哈哈哈，开始循环读
                {
                    //接下是整个数据包的大小
                    byte[] packSizeBuffer = new byte[4];//先读整个包的大小(不包含MD5)
                    //取出数据包中的包含数据长度的
                    Array.Copy(totalBytes, 4, packSizeBuffer, 0, 4);
                    //取出整个包的大小
                    int packSize = BitConverter.ToInt32(packSizeBuffer, 0);

                    //当前包体长度
                    int currentSize = recSize - keyLength - packSizeBuffer.Length;//标记整个包有多少，默认减掉包头Md5+PackSize
                    int bodyShouldSize = packSize - 8;
                    Console.Write(String.Format("包总长度：{0},Body当前：{1},Body剩余:{2} \n", packSize, currentSize, bodyShouldSize - currentSize));
                    //如果当前的长度继续读取数据,需要把整个数据包读完
                    while (this.stream.DataAvailable && currentSize < bodyShouldSize)
                    {
                        recSize = this.stream.Read(buffer, 0, 512);
                        if (recSize > 0)
                        {
                            currentSize += recSize;//读取成功，进行叠加
                            totalBytes = totalBytes.Concat(buffer).ToArray();//将读取的内容添加到容器后面
                            Console.WriteLine("current size:" + currentSize + "\n");
                        }
                        else
                        {
                            throw new Exception("connect read error");
                        }
                    }
                    //读取完了，拼装整个数据包
                    byte[] data = new byte[packSize - 8];
                    //便宜
                    Array.Copy(totalBytes, 8, data, 0, packSize - 8);//获取数据
                    //进行数据通知事件
                    Console.WriteLine("数据：" + Encoding.UTF8.GetString(data));
                    //需要再这里解开包
                    //解析数据
                    String dataStr = Encoding.UTF8.GetString(data);
                    packet.Key = serverKey;
                    packet.Body = dataStr;
                }
            }
            else
            {
                throw new Exception("connect read error");
            }
            return packet;
        }
    }
}
