using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using LocalCommons.Network;
using LocalCommons.Cookie;
using LocalCommons.Cryptography;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Net;
using AgentServer.Database;
using LocalCommons.Utilities;
using AgentServer.Structuring.Map;

namespace AgentServer.Packet
{
    public class LobbyHandle
    {
        public static void Handle_ShowPage(ClientConnection Client, PacketReader reader, byte last)
        {
            byte type = reader.ReadByte();
            Client.SendAsync(new ShowPage(Client.CurrentAccount, type, last));
        }

        public static void HandlePingTime(ClientConnection Client, byte last)
        {
            Client.SendAsync(new PingTime_0X41(Client.CurrentAccount, last));
        }

        public static void HandleSinglePlay(ClientConnection Client, PacketReader reader, byte last)
        {
            int mapnum = reader.ReadLEInt32();
            //Console.WriteLine("mapnum: {0}", mapnum);
            MapHolder.MapInfos.TryGetValue(mapnum, out MapInfo mapinfo);
            if (mapinfo.CanTimeAttack)
                Client.SendAsync(new SinglePlay_0X1D4(Client.CurrentAccount, mapnum, last));
        }

        public static void Handle_GetUserInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);

            Client.SendAsync(new GetUserInfo(User, nickname, last));
        }

    }
}
