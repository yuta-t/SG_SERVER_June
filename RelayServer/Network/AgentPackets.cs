using LocalCommons.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayServer.Network
{
    /// <summary>
    /// Sends Request To Login Server For Authorization.
    /// </summary>
    public sealed class Net_RegisterRelayServer : NetPacket
    {
        public Net_RegisterRelayServer() : base(0)
        {
            ns.Write((byte)0); //op
            ns.Write((byte)1);
            ns.Write((short)9155);

            ns.WriteDynamicASCII(Conf.ServerIP);
            ns.WriteDynamicASCII("");
        }
    }

    public sealed class Send_UDP_Info : NetPacket
    {
        //TCJoinResponse - ответ Логин сервера на пакет от Гейм сервера
        public Send_UDP_Info(int session, short port, string ip) : base(0)
        {
            ns.Write((byte)1); //op
            ns.Write(session);
            ns.Write(port);
            ns.WriteASCIIFixed_intSize(ip);
        }
    }
}
