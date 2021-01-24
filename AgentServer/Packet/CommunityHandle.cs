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
    public class CommunityHandle
    {
        public static void Handle_AddFriend(ClientConnection Client, PacketReader reader, byte last)
        {
            /*74 02 0A 00 00 00 47 4D 32 30 31 39 31 32 30 33 06 00 00 00 34 35 36 33 32 31 01 00 08*/
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            int msglen = reader.ReadLEInt32();
            string msg = string.Empty;
            if (msglen > 0)
            {
                msg = reader.ReadBig5StringSafe(msglen);
            }
            short groupnum = reader.ReadLEInt16();

            if (User.NickName != nickname)
            {
                AddFriendCheck(User, nickname, msg, groupnum, out byte ret);
                if (ret == 0)
                    Client.SendAsync(new AddFriendSuccess(User, nickname, groupnum, last));
                else
                    Client.SendAsync(new AddFriendFail(User, nickname, last));
            }
            else
            {
                Client.SendAsync(new AddFriendFail(User, nickname, last));
            }

            /*74 03 0A 00 00 00 47 4D 32 30 31 39 31 32 30 33 00 00 00 00 01 00 08*/
            /*74 04 0A 00 00 00 72 6D 6E 61 77 6D 72 6E 61 77 00 10*/

        }

        public static void Handle_DeleteFriend(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);

            if (User.NickName != nickname)
            {
                DeleteFriend(User, nickname, out byte ret);
                if (ret == 0)
                    Client.SendAsync(new DeleteFriendOK(User, nickname, last));
            }

        }

        public static void Handle_CancelAddFriend(ClientConnection Client, PacketReader reader, byte last)
        {
            //74 1A 0A 00 00 00 47 4D 32 30 31 39 31 32 30 33 04
            //74 1B 0A 00 00 00 47 4D 32 30 31 39 31 32 30 33 04
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);

            CancleAddFriend(User, nickname, out byte ret);
            if (ret == 0)
                Client.SendAsync(new CancelAddFriendOK(User, nickname, last));


        }
        public static void Handle_DeclineFriend(ClientConnection Client, PacketReader reader, byte last)
        {
            //74 0B 07 00 00 00 70 6F 70 6F 36 37 38 20
            //74 0C 07 00 00 00 70 6F 70 6F 36 37 38 20
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);

            DeclineAddFriend(User, nickname, out byte ret);
            if (ret == 0)
                Client.SendAsync(new DeclineAddFriendOK(User, nickname, last));


        }
        public static void Handle_AcceptFriend(ClientConnection Client, PacketReader reader, byte last)
        {
            //74 08 0A 00 00 00 47 4D 32 30 31 39 31 32 30 33 40
            //74 09 0A 00 00 00 47 4D 32 30 31 39 31 32 30 33 00 00 00 00 40
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);

            AcceptFriend(User, nickname, out byte ret);
            if (ret == 0)
                Client.SendAsync(new AcceptFriendOK(User, nickname, last));


        }

        public static void Handle_GetFriendListAccepted(ClientConnection Client, byte last)
        {
            Account User = Client.CurrentAccount;
            Client.SendAsync(new GetAcceptedFriendList_7406(User, last));
            Client.SendAsync(new GetWaitAcceptFriendList_7407(User, last));
            //74 06 usp_cm_getFriendListAccepted
            /*74 06 0C 00 00 00 07 00 00 00 70 6F 70 6F 34 35 36 00 
             00 00 00 00 00 00 00 00 81 07 00 00 A0 60 70 BC FB 03 
             00 00 00 30 76 C6 80 66 01 00 00 08 00 00 00 C5 DC C5 
             DC C5 DC BB E6 00 00 00 00 00 00 00 00 00 DA 8B 0B 00 
             90 E2 33 E1 0A 04 00 00 01 B8 3D 3B F4 33 01 00 00 06 
             00 00 00 41 6C 74 4B 6F 59 00 00 00 00 00 00 00 00 00 
             F0 12 11 00 30 B1 4D 34 11 04 00 00 01 B0 D1 7B 9E 67 
             01 00 00 07 00 00 00 41 6C 74 4B 6F 59 32 00 00 00 00 
             00 00 00 00 00 A9 9D 11 00 90 F0 C1 15 12 04 00 00 01 
             A0 01 E4 EE 66 01 00 00 08 00 00 00 70 6F 70 6F 34 35 
             36 49 00 00 00 00 00 00 00 00 00 FD CD 11 00 F8 16 7C 
             59 12 04 00 00 01 48 2A 9E D6 67 01 00 00 0A 00 00 00 
             54 52 48 61 63 6B 65 72 30 31 00 00 00 00 00 00 00 00 
             00 ED F3 11 00 E0 2F 42 AB 12 04 00 00 01 C8 49 57 7E 
             67 01 00 00 07 00 00 00 70 6F 70 6F 42 42 42 00 00 00 
             00 00 00 00 00 00 D8 F7 11 00 C0 F6 3A B6 12 04 00 00 
             01 68 8D 86 CF 67 01 00 00 08 00 00 00 BE BC C9 40 A4 
             70 A4 49 00 00 00 00 00 00 00 00 00 97 E1 14 00 F8 FA 
             DB 8D 17 04 00 00 01 70 53 59 A2 3A 01 00 00 0B 00 00 
             00 B1 4D C4 DD B2 C2 A8 E0 61 30 61 00 00 00 00 00 00 
             00 00 00 1B 54 15 00 58 57 6F 2E 18 04 00 00 01 18 2D 
             A8 EB 39 01 00 00 04 00 00 00 B0 49 AD F4 00 00 00 00 
             00 00 00 00 00 F0 58 15 00 F0 48 66 35 18 04 00 00 01 
             88 88 EC C2 39 01 00 00 0B 00 00 00 74 77 6E B1 6D AD 
             69 A4 A7 A5 FA 00 00 00 00 00 00 00 00 00 C0 4A 1E 00 
             50 AC 50 FD 0B 04 00 00 01 80 F3 38 E0 67 01 00 00 0A 
             00 00 00 A7 DA AC 4F AF AB 5A 31 32 33 00 00 00 00 00 
             00 00 00 00 3F 02 22 00 50 93 1F 67 2D 04 00 00 01 90 
             FC 57 B8 66 01 00 00 20*/
            //74 07 00 00 00 00 20
        }

        public static void Handle_GetFriendGroup(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            byte opcode = reader.ReadByte();
            switch (opcode)
            {
                case 0:
                    Client.SendAsync(new GetFriendGroupList_7420(User, last));
                    break;
                case 1:
                    int namelen = reader.ReadLEInt32();
                    string groupname = reader.ReadBig5StringSafe(namelen);
                    if (groupname != "DEFAULT")
                        Client.SendAsync(new AddFriendGroup_742001(User, groupname, last));
                    break;
                case 2:
                    short groupnum = reader.ReadLEInt16();
                    byte isfolding = reader.ReadByte();
                    int namelen2 = reader.ReadLEInt32();
                    string groupname2 = reader.ReadBig5StringSafe(namelen2);
                    if (groupname2 != "DEFAULT" && groupnum != 0)
                        Client.SendAsync(new ModifytFriendGroup_742002(User, groupnum, groupname2, isfolding, last));
                    break;
                case 3:
                    short groupnum2 = reader.ReadLEInt16();
                    if (groupnum2 != 0)
                        Client.SendAsync(new DelFriendGroup_742003(User, groupnum2, last));
                    break;

            }
            //Client.SendAsync(new GetetFriendGroupList_7420(User, last));
            //74 20 00 01 00 00 00 00 00 01 07 00 00 00 44 45 46 41 55 4C 54 10
            /*74 20 00 02 00 00 00 00 00 01 07 00 00 00 44 45 46 41 55 4C 54 01 00 00 04 00 00 00 31 32 33 34 04*/
        }

        public static void Handle_GroupMoveMember(ClientConnection Client, PacketReader reader, byte last)
        {
            //74 22 01 00 0A 00 00 00 A7 DA AC 4F AF AB 5A 31 32 33 02
            Account User = Client.CurrentAccount;
            short groupnum = reader.ReadLEInt16();
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);

            Client.SendAsync(new GroupMoveMember(User, groupnum, nickname, last));
        }

        public static void Handle_BlockFriend(ClientConnection Client, PacketReader reader, byte last)
        {
            //74 0F 0E 00 00 00 74 77 6E B1 6D AD 69 A4 A7 A5 FA 02
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            BlockFriend(User, nickname, out byte ret);
            if(ret == 0)
                Client.SendAsync(new BlockFriendOK(User, nickname, last));
        }
        public static void Handle_UnBlockFriend(ClientConnection Client, PacketReader reader, byte last)
        {
            //741109000000CE78A741A6D1A5C03210
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            UnBlockFriend(User, nickname, out byte ret);
            if (ret == 0)
                Client.SendAsync(new UnBlockFriendOK(User, nickname, last));
        }

        public static void Handle_GetRequestedToMe(ClientConnection Client, byte last)
        {
            Account User = Client.CurrentAccount;
            Client.SendAsync(new GetRequestedToMe_741E(User, last));
            //74 1E 0A 00 00 00 47 4D 32 30 31 39 31 32 30 33 00 00 00 00 10
            //74 1E 07 00 00 00 70 6F 70 6F 36 37 38 06 00 00 00 34 35 36 33 32 31 20
        }

        public static void Handle_GetUserAlarmInfo(ClientConnection Client, byte last)
        {
            Account User = Client.CurrentAccount;
            Client.SendAsync(new GetUserAlarmInfo(User, last));
            /*FF 08 01 01 00 0E 00 07 00 00 00 70 6F 70 6F 36 37 38 FF FF FF FF FF FF FF FF 00 BE 2D EC 67 01 00 00 00 00 00 00 02*/
        }

        public static void Handle_0x7426(ClientConnection Client, byte last)
        {
            Account User = Client.CurrentAccount;
            Client.SendAsync(new GameRoom_Hex("742700000000", last));
            //74 27 00 00 00 00 10
        }

        public static void Handle_0x7428(ClientConnection Client, byte last)
        {
            Account User = Client.CurrentAccount;
            Client.SendAsync(new GameRoom_Hex("5A47FFFFFFFF00000000CB781167B68B0600FFFFFFFFFFFFFFFF", last));
            //5A 47 FF FF FF FF 00 00 00 00 CB 78 11 67 B6 8B 06 00 FF FF FF FF FF FF FF FF 10
        }

        private static void AddFriendCheck(Account User,string requestednickname, string invitationmessage, short groupNum, out byte ret)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_cm_addfriend";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("requestednickname", MySqlDbType.VarString).Value = requestednickname;
                cmd.Parameters.Add("invitationmessage", MySqlDbType.VarString).Value = invitationmessage;
                cmd.Parameters.Add("groupNum", MySqlDbType.Int16).Value = groupNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                ret = Convert.ToByte(reader["retval"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }

        }

        private static void CancleAddFriend(Account User, string requestednickname, out byte ret)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_cm_cancelAddFriend";
                cmd.Parameters.Add("myusernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("requestednickname", MySqlDbType.VarString).Value = requestednickname;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                ret = Convert.ToByte(reader["retval"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }

        }
        private static void DeclineAddFriend(Account User, string requestnickname, out byte ret)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_cm_declinefriend";
                cmd.Parameters.Add("requestNickname", MySqlDbType.VarString).Value = requestnickname;
                cmd.Parameters.Add("myusernum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                ret = Convert.ToByte(reader["retval"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }

        }
        private static void AcceptFriend(Account User, string requestnickname, out byte ret)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_cm_acceptFriend";
                cmd.Parameters.Add("requestNickname", MySqlDbType.VarString).Value = requestnickname;
                cmd.Parameters.Add("requestedusernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("friendType", MySqlDbType.Int32).Value = 0;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                ret = Convert.ToByte(reader["retval"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }

        }

        private static void BlockFriend(Account User, string nickname, out byte ret)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_cm_blockFriend";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("nickname", MySqlDbType.VarString).Value = nickname;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                ret = Convert.ToByte(reader["retval"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }

        }
        private static void UnBlockFriend(Account User, string nickname, out byte ret)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_cm_unblockFriend";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("nickname", MySqlDbType.VarString).Value = nickname;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                ret = Convert.ToByte(reader["retval"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }

        }
        private static void DeleteFriend(Account User, string nickname, out byte ret)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_cm_deleteFriend";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("nickname", MySqlDbType.VarString).Value = nickname;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                ret = Convert.ToByte(reader["retval"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }

        }

    }
}
