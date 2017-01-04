using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/// Author: https://github.com/zhaojunlike
namespace OeynetSocket.SocketFramework
{
    //cba6c83625b358bf562e624fc903c244
    //我的数据包
    public class Packet
    {

        //32个字节
        public String Key
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
