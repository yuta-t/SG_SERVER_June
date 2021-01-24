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
        public GetRankInfo(byte type, int start, int icount, int rankkind, byte last)
        {
            //8D 01 error
            ns.Write((byte)0x8C); //op code
            ns.Write(type);
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
    public sealed class GetItemCollectionRankInfo : NetPacket
    {
        public GetItemCollectionRankInfo(byte type, int start, int icount, byte last)
        {
            //8D 01 error
            ns.Write((byte)0x8C); //op code
            ns.Write(type);
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
                    cmd.CommandText = "usp_itemCollection_getRankRange";
                    cmd.Parameters.Add("startRank", MySqlDbType.Int32).Value = start;
                    cmd.Parameters.Add("showCount", MySqlDbType.Int32).Value = icount;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ns.Write(Convert.ToInt32(reader["rank"]));
                            ns.WriteBIG5Fixed_intSize(reader["nickName"].ToString());
                            ns.Write(Convert.ToInt32(reader["point"]));
                            ns.Write(0L);
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
        public GetMyRankInfo(byte type, string NickName, int rankkind, byte last)
        {
            /*8F 00 00 00 00 00 F1 64 01 00 07 00 00 00 70 6F 70 
             6F 36 37 38 05 AD 08 00 00 00 00 00 05 AD 08 00 
             00 00 00 00 20*/
            ns.Write((byte)0x8F); //op code
            ns.Write(0);
            ns.Write(type);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_Rank_getNickname";
                    cmd.Parameters.Add("nickname", MySqlDbType.VarString).Value = NickName;
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
    public sealed class GetItemCollectionMyRankInfo : NetPacket
    {
        public GetItemCollectionMyRankInfo(byte type, string NickName, byte last)
        {
            /*8F 00 00 00 00 00 F1 64 01 00 07 00 00 00 70 6F 70 
             6F 36 37 38 05 AD 08 00 00 00 00 00 05 AD 08 00 
             00 00 00 00 20*/
            //8F 3F 00 00 00 07 01
            ns.Write((byte)0x8F); //op code
            ns.Write(0);//0x3F
            ns.Write(type);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_itemCollection_getRankSearch";
                    cmd.Parameters.Add("nickName", MySqlDbType.VarString).Value = NickName;
                    cmd.Parameters.Add("showCount", MySqlDbType.Int32).Value = 2;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            ns.Write(Convert.ToInt32(reader["rank"]));
                            ns.WriteBIG5Fixed_intSize(reader["nickName"].ToString());
                            ns.Write(Convert.ToInt32(reader["point"]));
                            ns.Write(0);
                        }
                    }
                }
            }
            ns.Write(last);
        }
    }
    public sealed class SearchRank : NetPacket
    {
        public SearchRank(byte type, string nickname, int icount, int rankkind, byte last)
        {
            ns.Write((byte)0x8C); //op code
            ns.Write(type);
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
    public sealed class SearchItemCollectionRank : NetPacket
    {
        public SearchItemCollectionRank(byte type, string nickname, int showcount, byte last)
        {
            ns.Write((byte)0x8C); //op code
            ns.Write(type);
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
                    cmd.CommandText = "usp_itemCollection_getRankSearch";
                    cmd.Parameters.Add("nickName", MySqlDbType.VarString).Value = nickname;
                    cmd.Parameters.Add("showCount", MySqlDbType.Int32).Value = showcount;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ns.Write(Convert.ToInt32(reader["rank"]));
                                ns.WriteBIG5Fixed_intSize(reader["nickName"].ToString());
                                ns.Write(Convert.ToInt32(reader["point"]));
                                ns.Write(0L);
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
