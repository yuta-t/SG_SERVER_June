using AgentServer.Structuring;
using LocalCommons.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentServer.Packet.Send
{
    public sealed class GetMonthlyGacha : NetPacket
    {
        public GetMonthlyGacha(Account User, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x530);
            ns.Write(2);
            ns.Write(last);
        }
    }
}
