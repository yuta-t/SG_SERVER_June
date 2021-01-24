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
            //8A 07 01 00 00 00 0E 00 00 00 00 80
            Account User = Client.CurrentAccount;
            byte type = reader.ReadByte();
            int startindex = reader.ReadLEInt32();
            int showcount = reader.ReadLEInt32();
            byte rankkind = reader.ReadByte();
            if (type == 0)
                Client.SendAsync(new GetRankInfo(type, startindex, showcount, 0, last));
            else if (type == 7)
                Client.SendAsync(new GetItemCollectionRankInfo(type, startindex, showcount, last));
        }

        public static void Handle_GetMyRankInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            //8E 00 00 10
            //8E 07 00 01
            Account User = Client.CurrentAccount;
            byte type = reader.ReadByte();
            byte rankkind = reader.ReadByte();
            if (type == 0)
                Client.SendAsync(new GetMyRankInfo(type, User.NickName, 0, last));
            else if (type == 7)
                Client.SendAsync(new GetItemCollectionMyRankInfo(type, User.NickName, last));
        }

        public static void Handle_SearchRank(ClientConnection Client, PacketReader reader, byte last)
        {
            //8B 00 0C 00 00 00 43 68 65 61 74 45 6E 67 69 6E 65 37 0E 00 00 00 03 08
            //8B 07 08 00 00 00 43 68 65 6E 63 68 65 6E 0E 00 00 00 00 10
            Account User = Client.CurrentAccount;
            byte type = reader.ReadByte();
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            int showcount = reader.ReadLEInt32();
            byte rankkind = reader.ReadByte();
            if (type == 0)
                Client.SendAsync(new SearchRank(type, nickname, showcount, 0, last));
            else if (type == 7)
                Client.SendAsync(new SearchItemCollectionRank(type, nickname, showcount, last));
        }
    }
}
