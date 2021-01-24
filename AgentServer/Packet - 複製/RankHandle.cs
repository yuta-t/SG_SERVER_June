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

namespace AgentServer.Packet
{
    public class RankHandle
    {
        public static void Handle_GetRankInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            //8A 00 01 00 00 00 0E 00 00 00 00 10
            //8A 00 0F 00 00 00 0E 00 00 00 00 10
            Account User = Client.CurrentAccount;
            reader.Offset += 1;
            int startindex = reader.ReadLEInt32();
            int showcount = reader.ReadLEInt32();
            byte rankkind = reader.ReadByte();
            Client.SendAsync(new GetRankInfo(User, startindex, showcount, 0, last));
        }

        public static void Handle_GetMyRankInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            //8E 00 00 10
            Account User = Client.CurrentAccount;
            reader.Offset += 1;
            byte rankkind = reader.ReadByte();

            Client.SendAsync(new GetMyRankInfo(User, 0, last));

        }

        public static void Handle_SearchRank(ClientConnection Client, PacketReader reader, byte last)
        {
            //8B 00 0C 00 00 00 43 68 65 61 74 45 6E 67 69 6E 65 37 0E 00 00 00 03 08
            Account User = Client.CurrentAccount;
            reader.Offset += 1;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            int showcount = reader.ReadLEInt32();
            byte rankkind = reader.ReadByte();

            Client.SendAsync(new SearchRank(User, nickname, showcount, 0, last));
        }
    }
}
