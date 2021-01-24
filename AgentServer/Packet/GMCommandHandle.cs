using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using LocalCommons.Logging;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentServer.Packet
{
    public static class GMCommandHandle
    {
        public static void Handle_Notice(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            int noticetype = reader.ReadLEInt32();
            int noticekind = reader.ReadLEInt32();
            int contentlen = reader.ReadLEInt32();
            string content = reader.ReadBig5StringSafe(contentlen);
            if (User.Attribute == 0)
            {
                Log.Warning("!!!!!! [{0}] try send notice [{1}]", User.NickName, content);                
            }
            else
            {
                foreach (Account Users in ClientConnection.CurrentAccounts.Values)
                {
                    ClientConnection current = Users.Connection;
                    current.SendAsync(new NoticePacket(Users, content, last));
                }
                Log.Info("[{1}] Send notice NoticeType : 0, noticeKind : 1,  {0}", User.NickName, content);
            }
        }
        public static void Handle_DisconnectUser(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            if (User.Attribute == 0)
            {
                Log.Warning("!!!!!! [{0}] try to disconnect a user with no auth", User.NickName);
            }
            else
            {
                bool isExist = ClientConnection.CurrentAccounts.Values.Any(p => p.NickName == nickname);
                if (isExist)
                {
                    Account obj = ClientConnection.CurrentAccounts.ToList().Find(p => p.Value.NickName == nickname).Value;
                    obj.Connection.Disconnect();
                }
            }
        }
        public static void Handle_FindGo(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            bool isExist = ClientConnection.CurrentAccounts.Values.Any(p => p.NickName == nickname);
            if (isExist)
            {
                Account obj = ClientConnection.CurrentAccounts.ToList().Find(p => p.Value.NickName == nickname).Value;
                if (User.Attribute == 0 && obj.Attribute == 0)
                {
                    obj.Connection.Disconnect();
                }
            }
        }

        public static void Handle_ClientCheckAutoBan(ClientConnection Client, PacketReader reader, byte last)
        {
            //19 01 00 00 00 0A 00 00 00 00 00 00 00 02
            Log.Warning("Bad User Detected!!");
            Account User = Client.CurrentAccount;
            int unk1 = reader.ReadLEInt32(); //blockreason?
            int unk2 = reader.ReadLEInt32(); //blocktime?
            AutoBan(User);
            Client.SendAsync(new ClientCheckAutoBanACK(User, unk1, last));
            Client.Disconnect();
        }

        private static void AutoBan(Account User)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_insertBlackList";
                    cmd.Parameters.Add("commandusernum", MySqlDbType.Int32).Value = 0;
                    cmd.Parameters.Add("blackUsernum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("blockreason", MySqlDbType.Int32).Value = 1;
                    cmd.Parameters.Add("blocktime", MySqlDbType.Int32).Value = 10;
                    cmd.Parameters.Add("remoteIP", MySqlDbType.VarString).Value = User.LastIp;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                    }
                }
            }
        }


        public class ClientCheckAutoBanACK : NetPacket
        {
            public ClientCheckAutoBanACK(Account User, int unk1, byte last)
            {
                //1A 01 00 00 00 02
                ns.Write((byte)0x1A);
                ns.Write(unk1);
                ns.Write(last);
            }
        }
    }
}
