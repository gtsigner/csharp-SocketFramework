using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// Author: https://github.com/zhaojunlike
namespace OeynetSocket.SocketFramework
{
    public class PacketHelper
    {

        /// <summary>
        /// 封装一个完整的数据包
        /// </summary>
        /// <param name="key"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static byte[] CreatePacket(int key, String msg)
        {
            byte[] data = new byte[0];
            //把自己的密码加入
            byte[] md5 = BitConverter.GetBytes(key);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(msg);
            int totalLength = md5.Length + 4 + bodyBytes.Length;
            byte[] lengthBytes = BitConverter.GetBytes(totalLength);//包长度
            data = data.Concat(md5).Concat(lengthBytes).Concat(bodyBytes).ToArray();
            return data;
        }


    }
}
