using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OeynetSocket.SocketFramework
{
    public class ClientThreadEventArgs : EventArgs
    {
        public ClientThreadEventType EventType
        {
            get;
            set;
        }
        public ClientThreadEventArgs()
        {


        }
    }
}
