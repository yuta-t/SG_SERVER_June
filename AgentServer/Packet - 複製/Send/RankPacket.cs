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
using System.IO;

namespace AgentServer.Packet.Send
{
    public sealed class GetRankInfo : NetPacket
    {
        public GetRankInfo(Account User, int start, int icount, int rankkind, byte last) : base(1, User.EncryptKey)
        {
            //8D 01 error
            ns.Write((byte)0x8C); //op code
            ns.Write((byte)0);
            int countpos = (int)ns.Position;
            byte count = 0;
            ns.Write(count); //count
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_Rank_getRange";
                    cmd.Parameters.Add("startRank", MySqlDbType.Int32).Value = start;
                    cmd.Parameters.Add("showCount", MySqlDbType.Int32).Value = icount;
                    cmd.Parameters.Add("detailRank", MySqlDbType.Int32).Value = rankkind;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ns.Write(Convert.ToInt32(reader["Rank"]));
                            ns.WriteBIG5Fixed_intSize(reader["nickname"].ToString());
                            ns.Write(Convert.ToInt64(reader["EXP"]));
                            ns.Write(Convert.ToInt64(reader["EXP"]));
                            ns.Write(Convert.ToInt32(reader["Rank"]));
                            count++;
                        }
                    }
                }
            }
            ns.Write(last);
            ns.Seek(countpos, SeekOrigin.Begin);
            ns.Write(count);
        }
    }
    public sealed class GetMyRankInfo : NetPacket
    {
        public GetMyRankInfo(Account User, int rankkind, byte last) : base(1, User.EncryptKey)
        {
            /*8F 00 00 00 00 00 F1 64 01 00 07 00 00 00 70 6F 70 
             6F 36 37 38 05 AD 08 00 00 00 00 00 05 AD 08 00 
             00 00 00 00 20*/
            ns.Write((byte)0x8F); //op code
            ns.Write((byte)0);
            ns.Write(0);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_Rank_getNickname";
                    cmd.Parameters.Add("nickname", MySqlDbType.VarString).Value = User.NickName;
                    cmd.Parameters.Add("detailRank", MySqlDbType.Int32).Value = rankkind;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            ns.Write(Convert.ToInt32(reader["Rank"]));
                            ns.WriteBIG5Fixed_intSize(reader["nickname"].ToString());
                            ns.Write(Convert.ToInt64(reader["EXP"]));
                            ns.Write(Convert.ToInt64(reader["EXP"]));
                        }
                    }
                }
            }
            ns.Write(last);
        }
    }
    public sealed class SearchRank : NetPacket
    {
        public SearchRank(Account User, string nickname, int icount, int rankkind, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0x8C); //op code
            ns.Write((byte)0);
            int countpos = (int)ns.Position;
            byte count = 0;
            ns.Write(count); //count
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_Rank_Search";
                    cmd.Parameters.Add("usernickname", MySqlDbType.VarString).Value = nickname;
                    cmd.Parameters.Add("showcount", MySqlDbType.Int32).Value = icount;
                    cmd.Parameters.Add("detailRank", MySqlDbType.Int32).Value = rankkind;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ns.Write(Convert.ToInt32(reader["Rank"]));
                                ns.WriteBIG5Fixed_intSize(reader["nickname"].ToString());
                                ns.Write(Convert.ToInt64(reader["EXP"]));
                                ns.Write(Convert.ToInt64(reader["EXP"]));
                                ns.Write(Convert.ToInt32(reader["Rank"]));
                                count++;
                            }
                        }
                    }
                }
            }
            ns.Write(last);
            ns.Seek(countpos, SeekOrigin.Begin);
            ns.Write(count);
        }
    }
}
