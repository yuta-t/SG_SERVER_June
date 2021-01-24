using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentServer.Packet
{
    public class GachaHandle
    {
        public static void Handle_GetMonthlyGacha(ClientConnection Client, byte last)
        {
            Account User = Client.CurrentAccount;
            Client.SendAsync(new GetMonthlyGacha(User, last));
        }
    }
}
