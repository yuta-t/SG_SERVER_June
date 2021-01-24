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
using AgentServer.Structuring.Park;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AgentServer.Packet
{
    public class CommandHandle
    {
        public static void Handle_UseShoutItem(ClientConnection Client, PacketReader reader, byte last)
        {
            //7C 0C 0D 00 00 0A 00 00 00 74 65 73 74 31 32 33 34 35 36 01
            Account User = Client.CurrentAccount;
            int ShoutItemNum = reader.ReadLEInt32();
            int msglen = reader.ReadLEInt32();
            string Msg = reader.ReadBig5StringSafe(msglen);

            if (UseShoutItem(User, ShoutItemNum, Msg)) {
                Client.SendAsync(new ShoutOK(User, ShoutItemNum, last));
                foreach (Account Online in ClientConnection.CurrentAccounts.Values)
                {
                    Online.Connection.SendAsync(new ShoutToAll(Online, User.NickName, ShoutItemNum, Msg, User.Exp, last));
                }
            }
        }

        private static bool UseShoutItem(Account User, int ShoutItemNum, string ShoutMsg)
        {      
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_useShoutItem";
                cmd.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("ShoutItemNum", MySqlDbType.Int32).Value = ShoutItemNum;
                cmd.Parameters.Add("ShoutMsg", MySqlDbType.VarString).Value = ShoutMsg;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                if (reader.GetInt32("retval") == 0)
                    return true;
                cmd.Dispose();
                reader.Close();
                con.Close();
            }

            return false;
        }

        public class ShoutOK : NetPacket
        {
            public ShoutOK(Account User, int ShoutItemNum, byte last) : base(1, User.EncryptKey)
            {
                //7D 00 00 00 00 0C 0D 00 00 04
                ns.Write((byte)0x7D);
                ns.Write(0);
                ns.Write(ShoutItemNum);
                ns.Write(last);
            }
        }

        public class ShoutToAll : NetPacket
        {
            public ShoutToAll(Account User, string Nickname, int ShoutItemNum, string ShoutMsg, long EXP, byte last) : base(1, User.EncryptKey)
            {
                /*7E 0C 0D 00 00 00 0A 00 00 00 74 65 73 74 31 
                 32 33 34 35 36 08 00 00 00 70 6F 70 6F 34 35
                 36 49 28 5A AE 1B 00 00 00 00 01*/
                ns.Write((byte)0x7E);
                ns.Write(ShoutItemNum);
                ns.Write((byte)0);
                ns.WriteBIG5Fixed_intSize(ShoutMsg);
                ns.WriteBIG5Fixed_intSize(Nickname);
                ns.Write(EXP);
                ns.Write(last);
            }
        }

    }
}
