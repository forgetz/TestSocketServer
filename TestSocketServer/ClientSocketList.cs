using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestSocketServer
{
    public class ClientSocketList
    {
        public string Address { get; set; }
        public int port { get; set; }
        public Socket SocketConnection { get; set; }
        public DateTime LastConnected { get; set; }

    }
}
