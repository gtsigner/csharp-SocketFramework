using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OeynetSocket.SocketFramework
{
    //我的数据包
    public class Packet
    {

        //4个字节
        public int Key
        {
            get;
            set;
        }

        public String Body
        {
            set;
            get;
        }
    }
}
