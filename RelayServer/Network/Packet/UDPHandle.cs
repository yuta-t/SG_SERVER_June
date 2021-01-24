using LocalCommons.Network;
using RelayServer.Network.Connections;
using RelayServer.Network.Packet.Send;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RelayServer;
using RelayServer.Network.Packet.AgentServer;
using RelayServer.Structuring;
using LocalCommons.Utilities;

namespace RelayServer.Network.Packet
{

    public class UDPHandle
    {
        public static ConcurrentDictionary<int, AccountInfo> CurrentAccounts { get; } = new ConcurrentDictionary<int, AccountInfo>();

        public static void Handle_Connect_04FFD403(ClientConnection Client, PacketReader reader, IPEndPoint endPoint)
        {
            int session = reader.ReadLEInt32();
            Client.SendAsync(new Connect_04FFD403(endPoint), endPoint);

            short port = (short)endPoint.Port;
            string ip = endPoint.Address.ToString();
            AccountInfo acinfo = new AccountInfo
            {
                Session = session,
                UDPPort = port,
                IP = endPoint.Address.ToString(),
                Connection = Client,
                EndPoint = endPoint
            };
            //Console.WriteLine(Utility.ByteArrayToString(reader.Buffer()));
            /*if (!CurrentAccounts.Exists(a => a.Session == session))
            {
                CurrentAccounts.Add(acinfo);
                AgentServerHandle.CurrentAgentServer.SendAsync(new Send_UDP_Info(session, port, ip));
            }*/
            if (!CurrentAccounts.Any(ac => ac.Key == session))
            {
                CurrentAccounts.TryAdd(session, acinfo);
                AgentServerHandle.CurrentAgentServer.SendAsync(new Send_UDP_Info(session, port, ip));
            }


        }

        public static void Handle_Connect_04FFD803(ClientConnection Client, PacketReader reader, IPEndPoint endPoint)
        {
            Client.SendAsync(new Connect_04FFD803(), endPoint);
        }

        public static void Handle_ConnectUser_04FFD603(ClientConnection Client, PacketReader reader, IPEndPoint endPoint)
        {
            int session = reader.ReadLEInt32();
            byte[] userreturn = reader.ReadByteArray(reader.Size - 8);

            //IPEndPoint TendPoint = CurrentAccounts.FirstOrDefault(ac => ac.Key == session).Value.endPoint;
            AccountInfo acinfo = CurrentAccounts.FirstOrDefault(ac => ac.Key == session).Value;
            acinfo.Connection.SendAsync(new Connect_04FFD603(userreturn), acinfo.EndPoint);
        }

    }
}
