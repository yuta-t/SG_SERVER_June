using LocalCommons.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalCommons.Utilities;
using AgentServer.Database;
using AgentServer.Structuring;
using AgentServer.Network.Connections;
using MySql.Data.MySqlClient;
using System.Data;

namespace AgentServer.Packet.Send
{

    public sealed class AddFriendSuccess : NetPacket
    {
        public AddFriendSuccess(Account User, string nickname,short groupnum, byte last)
        {
            ns.Write((byte)0x74); //op
            ns.Write((byte)0x03); //subop
            ns.WriteBIG5Fixed_intSize(nickname);
            ns.Write(0);
            ns.Write(groupnum);
            ns.Write(last);
        }
    }
    public sealed class AddFriendFail : NetPacket
    {
        public AddFriendFail(Account User, string nickname, byte last)
        {
            ns.Write((byte)0x74); //op
            ns.Write((byte)0x04); //subop
            ns.WriteBIG5Fixed_intSize(nickname);
            ns.Write((byte)0); //???
            ns.Write(last);
        }
    }

    public sealed class CancelAddFriendOK : NetPacket
    {
        public CancelAddFriendOK(Account User, string nickname, byte last)
        {
            ns.Write((byte)0x74); //op
            ns.Write((byte)0x1B); //subop
            ns.WriteBIG5Fixed_intSize(nickname);
            ns.Write(last);
        }
    }
    public sealed class DeclineAddFriendOK : NetPacket
    {
        public DeclineAddFriendOK(Account User, string nickname, byte last)
        {
            ns.Write((byte)0x74); //op
            ns.Write((byte)0x0C); //subop
            ns.WriteBIG5Fixed_intSize(nickname);
            ns.Write(last);
        }
    }

    public sealed class AcceptFriendOK : NetPacket
    {
        public AcceptFriendOK(Account User, string nickname, byte last)
        {
            ns.Write((byte)0x74); //op
            ns.Write((byte)0x09); //subop
            ns.WriteBIG5Fixed_intSize(nickname);
            ns.Write(0);
            ns.Write(last);
        }
    }

    public sealed class GetAcceptedFriendList_7406 : NetPacket
    {
        public GetAcceptedFriendList_7406(Account User, byte last)
        {
            ns.Write((byte)0x74); //op
            ns.Write((byte)0x06); //subop
            int countpos = (int)ns.Position;
            int count = 0;
            ns.Write(count);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_cm_getFriendListAccepted";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ns.WriteBIG5Fixed_intSize(reader["nickname"].ToString());
                        ns.Write(Convert.ToBoolean(reader["blocked"]));
                        //ns.Write(Convert.ToBoolean(reader["deleted"]));
                        ns.Write((short)0);
                        ns.Write(Convert.ToInt16(reader["groupNum"]));
                        ns.Write(0);
                        ns.Write(Convert.ToInt32(reader["FarmUniqueNum"]));
                        ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["ExpireDateTime"])));
                        ns.Write((byte)1); //fdtype?
                        ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["lastLogoutTime"])));
                        count++;
                    }
                }

                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write(last);
            ns.Seek(countpos, System.IO.SeekOrigin.Begin);
            ns.Write(count);
        }
    }

    public sealed class GetWaitAcceptFriendList_7407 : NetPacket
    {
        public GetWaitAcceptFriendList_7407(Account User, byte last)
        {
            ns.Write((byte)0x74); //op
            ns.Write((byte)0x07); //subop
            int countpos = (int)ns.Position;
            int count = 0;
            ns.Write(count);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_cm_getFriendListWaitAccept";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ns.WriteBIG5Fixed_intSize(reader["nickname"].ToString());
                        ns.Write(Convert.ToInt16(reader["groupNum"]));
                        count++;
                    }
                }

                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write(last);
            ns.Seek(countpos, System.IO.SeekOrigin.Begin);
            ns.Write(count);
        }
    }

    public sealed class GetRequestedToMe_741E : NetPacket
    {
        public GetRequestedToMe_741E(Account User, byte last)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_cm_getRequestedToMe";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.HasRows)
                {
                    ns.Write((byte)0x74); //op
                    ns.Write((byte)0x1E); //subop
                    reader.Read();
                    ns.WriteBIG5Fixed_intSize(reader["nickname"].ToString());
                    ns.WriteBIG5Fixed_intSize(reader["invitationmessage"].ToString());
                    ns.Write(last);
                }

                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
    }

    public sealed class GetFriendGroupList_7420 : NetPacket
    {
        public GetFriendGroupList_7420(Account User, byte last)
        {
            ns.Write((byte)0x74); //op
            ns.Write((byte)0x20); //subop
            ns.Write((byte)0);
            int countpos = (int)ns.Position;
            int count = 0;
            ns.Write(count);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_cm_groupOperate";
                cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("operationType", MySqlDbType.Int16).Value = 0;
                cmd.Parameters.Add("groupNum", MySqlDbType.Int16).Value = 0;
                cmd.Parameters.Add("isFolding", MySqlDbType.Byte).Value = 0;
                cmd.Parameters.Add("groupName", MySqlDbType.VarString).Value = "";
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ns.Write(Convert.ToInt16(reader["fdGroupNum"]));
                        ns.Write(Convert.ToBoolean(reader["fdIsFolding"]));
                        ns.WriteBIG5Fixed_intSize(reader["fdGroupName"].ToString());
                        count++;
                    }
                }

                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write(last);
            ns.Seek(countpos, System.IO.SeekOrigin.Begin);
            ns.Write(count);
        }
    }
    public sealed class AddFriendGroup_742001 : NetPacket
    {
        public AddFriendGroup_742001(Account User, string groupname, byte last)
        {
            ns.Write((byte)0x74); //op
            ns.Write((byte)0x20); //subop
            ns.Write((byte)1);
            int countpos = (int)ns.Position;
            int count = 0;
            ns.Write(count);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_cm_groupOperate";
                cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("operationType", MySqlDbType.Int16).Value = 1;
                cmd.Parameters.Add("groupNum", MySqlDbType.Int16).Value = 0;
                cmd.Parameters.Add("isFolding", MySqlDbType.Byte).Value = 0;
                cmd.Parameters.Add("groupName", MySqlDbType.VarString).Value = groupname;
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ns.Write(Convert.ToInt16(reader["fdGroupNum"]));
                        ns.Write(Convert.ToBoolean(reader["fdIsFolding"]));
                        ns.WriteBIG5Fixed_intSize(reader["fdGroupName"].ToString());
                        count++;
                    }
                }

                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write(last);
            ns.Seek(countpos, System.IO.SeekOrigin.Begin);
            ns.Write(count);
        }
    }
    public sealed class ModifytFriendGroup_742002 : NetPacket
    {
        public ModifytFriendGroup_742002(Account User, short groupnum, string groupname, byte isfolding, byte last)
        {
            ns.Write((byte)0x74); //op
            ns.Write((byte)0x20); //subop
            ns.Write((byte)2);
            int countpos = (int)ns.Position;
            int count = 0;
            ns.Write(count);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_cm_groupOperate";
                cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("operationType", MySqlDbType.Int16).Value = 2;
                cmd.Parameters.Add("groupNum", MySqlDbType.Int16).Value = groupnum;
                cmd.Parameters.Add("isFolding", MySqlDbType.Byte).Value = isfolding;
                cmd.Parameters.Add("groupName", MySqlDbType.VarString).Value = groupname;
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ns.Write(Convert.ToInt16(reader["fdGroupNum"]));
                        ns.Write(Convert.ToBoolean(reader["fdIsFolding"]));
                        ns.WriteBIG5Fixed_intSize(reader["fdGroupName"].ToString());
                        count++;
                    }
                }

                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write(last);
            ns.Seek(countpos, System.IO.SeekOrigin.Begin);
            ns.Write(count);
        }
    }
    public sealed class DelFriendGroup_742003 : NetPacket
    {
        public DelFriendGroup_742003(Account User, short groupnum, byte last)
        {
            ns.Write((byte)0x74); //op
            ns.Write((byte)0x20); //subop
            ns.Write((byte)3);
            int countpos = (int)ns.Position;
            int count = 0;
            ns.Write(count);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_cm_groupOperate";
                cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("operationType", MySqlDbType.Int16).Value = 3;
                cmd.Parameters.Add("groupNum", MySqlDbType.Int16).Value = groupnum;
                cmd.Parameters.Add("isFolding", MySqlDbType.Byte).Value = 0;
                cmd.Parameters.Add("groupName", MySqlDbType.VarString).Value = "";
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ns.Write(Convert.ToInt16(reader["fdGroupNum"]));
                        ns.Write(Convert.ToBoolean(reader["fdIsFolding"]));
                        ns.Write(0);
                        count++;
                    }
                }

                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write(last);
            ns.Seek(countpos, System.IO.SeekOrigin.Begin);
            ns.Write(count);
        }
    }

    public sealed class GroupMoveMember : NetPacket
    {
        public GroupMoveMember(Account User, short groupnum, string nickname, byte last)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_cm_groupMoveMember";
                cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("groupNum", MySqlDbType.Int16).Value = groupnum;
                cmd.Parameters.Add("nickname", MySqlDbType.VarString).Value = nickname;
                MySqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write((byte)0x74); //op
            ns.Write((byte)0x23); //subop
            ns.Write((byte)0);
            ns.Write(last);
        }
    }

    public sealed class BlockFriendOK : NetPacket
    {
        public BlockFriendOK(Account User,  string nickname, byte last)
        {
            ns.Write((byte)0x74); //op
            ns.Write((byte)0x0F); //subop
            ns.WriteBIG5Fixed_intSize(nickname);
            ns.Write(last);
        }
    }
    public sealed class UnBlockFriendOK : NetPacket
    {
        public UnBlockFriendOK(Account User, string nickname, byte last)
        {
            ns.Write((byte)0x74); //op
            ns.Write((byte)0x12); //subop
            ns.WriteBIG5Fixed_intSize(nickname);
            ns.Write(last);
        }
    }
    public sealed class DeleteFriendOK : NetPacket
    {
        public DeleteFriendOK(Account User, string nickname, byte last)
        {
            ns.Write((byte)0x74); //op
            ns.Write((byte)0x15); //subop
            ns.WriteBIG5Fixed_intSize(nickname);
            ns.Write(last);
        }
    }


    public sealed class GetUserAlarmInfo : NetPacket
    {
        public GetUserAlarmInfo(Account User, byte last)
        {
            ns.Write((byte)0xFF); 
            ns.Write((short)0x108); //op
            int countpos = (int)ns.Position;
            short count = 0;
            ns.Write(count);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_CM_getUserAlarmInfo";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ns.Write(Convert.ToInt16(reader["eventNum"]));
                        ns.WriteBIG5Fixed_intSize(reader["fromNickname"].ToString());
                        ns.Write(Convert.ToInt32(reader["ext1"]));
                        ns.Write(Convert.ToInt32(reader["ext2"]));
                        ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["eventtime"])));
                        ns.Write(0);
                        count++;
                    }
                }

                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write(last);
            ns.Seek(countpos, System.IO.SeekOrigin.Begin);
            ns.Write(count);
        }
    }
}
