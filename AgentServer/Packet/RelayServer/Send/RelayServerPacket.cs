using LocalCommons.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentServer.Packet.RelayServer.Send
{

    public class NET_RelayRegistrationResult : NetPacket
    {
        //TCJoinResponse - ответ Логин сервера на пакет от Гейм сервера
        public NET_RelayRegistrationResult(bool success) : base(0)
        {
            ns.Write((byte)0);
            ns.Write(success);
        }
    }

}
