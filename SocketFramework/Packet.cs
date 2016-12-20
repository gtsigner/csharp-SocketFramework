using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/// Author: https://github.com/zhaojunlike
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
        private string _Body;
        public String Body
        {
            set
            {
                this._Body = value + "";
            }
            get
            {
                return this._Body;
            }
        }
    }
}
