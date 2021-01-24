using RelayServer.Network.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RelayServer.Structuring
{
    public class AccountInfo
    {
        public int Session { get; set; }
        public short UDPPort { get; set; }
        public string IP { get; set; }
        public ClientConnection Connection { get; set; }
        public IPEndPoint EndPoint { get; set; }
    }
}
