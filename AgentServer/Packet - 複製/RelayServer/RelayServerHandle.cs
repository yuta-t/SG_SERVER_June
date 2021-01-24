using AgentServer.Controller;
using AgentServer.Network.Connections;
using AgentServer.Packet.RelayServer.Send;
using AgentServer.Structuring;
using LocalCommons.Logging;
using LocalCommons.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentServer.Packet.RelayServer
{
    public class RelayServerHandle
    {


        public static void Handle_RegisterRelayServer(RelayConnection net, PacketReader reader)
        {
            byte id = reader.ReadByte();
            short port = reader.ReadLEInt16();
            string ip = reader.ReadDynamicString();
            string password = reader.ReadDynamicString();
            bool success = RelayController.RegisterRelayServer(id, password, net, port, ip);
            net.SendAsync(new NET_RelayRegistrationResult(success));
        }

        public static void Handle_GetUDPInfo(RelayConnection net, PacketReader reader)
        {
            int session = reader.ReadLEInt32();
            short port = reader.ReadLEInt16();
            int iplen = reader.ReadLEInt32();
            string ip = reader.ReadStringSafe(iplen);
            //Console.WriteLine("Session: {0:X2}, UDPport: {1}, ip: {2}", session, port, ip);

            Account ac = ClientConnection.CurrentAccounts.First(p => p.Key == session).Value;
            ac.UDPPort = port;
        }
    }
}
