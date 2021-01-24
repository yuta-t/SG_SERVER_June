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
using AgentServer.Structuring.Item;
using NestedDictionaryLib;

namespace AgentServer.Packet
{
    public class EnchantSystem
    {
        public static void Handle_GetEnchantItemInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 8A 05 FF FF FF FF 80
            Account User = Client.CurrentAccount;
            int ItemNum = reader.ReadLEInt32();//-1

            if (Enchant_GetItemInfo(User.UserNum, out var infos))
                Client.SendAsync(new EnchantItemInfo(ItemNum, infos, last));
        }

        public static void Handle_Hardening(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 94 05 61 D1 00 00 01 40
            //FF 94 05 69 D1 00 00 02 02
            Account User = Client.CurrentAccount;
            int StoneNum = reader.ReadLEInt32();
            byte type = reader.ReadByte(); //1=提鍊 2=合成 3=強化
            if (Enchant_Hardening(User, StoneNum, type, out int ResultStoneNum))
                Client.SendAsync(new HardeningDone(User.TR, StoneNum, ResultStoneNum, last));
        }

        public static void Handle_StoneMount(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 8C 05 77 E5 00 00 02 11 D4 00 00 80
            //FF 8C 05 77 E5 00 00 03 0D D2 00 00 80
            Account User = Client.CurrentAccount;
            int ItemNum = reader.ReadLEInt32();
            byte seqnum = reader.ReadByte();
            int StoneNum = reader.ReadLEInt32();
            if (Enchant_StoneMount(User, ItemNum, StoneNum, seqnum, out var infos, out var Attrs))
            {
                Client.SendAsync(new StoneMountSuccess(User.TR, seqnum, ItemNum, StoneNum, infos, last));
                User.UserItemAttr.TryRemove(ItemNum, out _);
                User.UserItemAttr.TryAdd(ItemNum, Attrs);
            }
            else
                Client.SendAsync(new StoneMountFail(last));

        }
        public static void Handle_StoneRemove(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 90 05 00 00 00 00 77 E5 00 00 02 10
            Account User = Client.CurrentAccount;
            int unk = reader.ReadLEInt32();
            int ItemNum = reader.ReadLEInt32();
            byte seqnum = reader.ReadByte();

            if (Enchant_StoneRemove(User, ItemNum, seqnum, out int ReturnStoneNum, out var infos, out var Attrs))
            {
                Client.SendAsync(new StoneRemoveSuccess(ReturnStoneNum, ItemNum, infos, last));
                User.UserItemAttr.TryRemove(ItemNum, out _);
                if (Attrs.Count > 0)
                    User.UserItemAttr.TryAdd(ItemNum, Attrs);
            }

        }
        public static void Handle_SealErase(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 92 05 48 D1 00 00 3B 91 00 00 03 08
            Account User = Client.CurrentAccount;
            int EraseItemNum = reader.ReadLEInt32();
            int ItemNum = reader.ReadLEInt32();
            byte seqnum = reader.ReadByte();

        }

        private static bool Enchant_GetItemInfo(int UserNum, out NestedDictionary<int, byte, byte, int, List<ItemAttr>> infos)
        {
            infos = new NestedDictionary<int, byte, byte, int, List<ItemAttr>>();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_enchant_getUserInfo";
                    cmd.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            byte SeqNum, SocketNum;
                            int ItemNum, StoneNum;
                            while (reader.Read())
                            {
                                ItemNum = Convert.ToInt32(reader["fdItemDescNum"]);
                                SeqNum = Convert.ToByte(reader["fdSeqNum"]);
                                SocketNum = Convert.ToByte(reader["fdSocketNum"]);
                                StoneNum = Convert.ToInt32(reader["fdStoneNum"]);
                                ItemAttr attr = new ItemAttr
                                {
                                    Attr = Convert.ToUInt16(reader["fdAttrType"]),
                                    AttrValue = Convert.ToSingle(reader["fdAttrValue"])
                                };

                                if (!infos.ContainsKey(ItemNum, SeqNum, SocketNum, StoneNum))
                                    infos.Add(ItemNum, SeqNum, SocketNum, StoneNum, new List<ItemAttr> { attr });
                                else
                                    infos[ItemNum][SeqNum][SocketNum][StoneNum].Add(attr);
                            }
                        }
                        return true;
                    }
                }
            }
        }
        private static bool Enchant_Hardening(Account User, int StoneNum, int type, out int ResultStoneNum)
        {
           ResultStoneNum = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_enchant_hardening";
                    cmd.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("StoneNum", MySqlDbType.Int32).Value = StoneNum;
                    cmd.Parameters.Add("type", MySqlDbType.Int32).Value = type;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            int CostType = reader.GetInt32("CostType");
                            int Cost = reader.GetInt32("Cost");
                            ResultStoneNum = reader.GetInt32("ReturnRealStoneNum");
                            if (CostType == 0)
                                User.TR -= Cost;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private static bool Enchant_StoneMount(Account User, int ItemNum, int pStoneNum, short pSeqNum, out NestedDictionary<byte, byte, int, List<ItemAttr>> infos, out List<ItemAttr> Attrs)
        {
            infos = new NestedDictionary<byte, byte, int, List<ItemAttr>>();
            Attrs = new List<ItemAttr>();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_enchant_mount";
                    cmd.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("ItemNum", MySqlDbType.Int32).Value = ItemNum;
                    cmd.Parameters.Add("StoneNum", MySqlDbType.Int32).Value = pStoneNum;
                    cmd.Parameters.Add("SeqNum", MySqlDbType.Int16).Value = pSeqNum;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            byte SeqNum, SocketNum;
                            int StoneNum;
                            while (reader.Read())
                            {
                                SeqNum = Convert.ToByte(reader["fdSeqNum"]);
                                SocketNum = Convert.ToByte(reader["fdSocketNum"]);
                                StoneNum = Convert.ToInt32(reader["fdStoneNum"]);
                                ItemAttr attr = new ItemAttr
                                {
                                    Attr = Convert.ToUInt16(reader["fdAttrType"]),
                                    AttrValue = Convert.ToSingle(reader["fdAttrValue"])
                                };

                                if (!infos.ContainsKey(SeqNum, SocketNum, StoneNum))
                                    infos.Add(SeqNum, SocketNum, StoneNum, new List<ItemAttr> { attr });
                                else
                                    infos[SeqNum][SocketNum][StoneNum].Add(attr);
                            }
                            reader.NextResult();
                            while (reader.Read())
                            {
                                ItemAttr attr = new ItemAttr
                                {
                                    Attr = Convert.ToUInt16(reader["AttrType"]),
                                    AttrValue = Convert.ToSingle(reader["AttrValue"])
                                };
                                Attrs.Add(attr);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private static bool Enchant_StoneRemove(Account User, int ItemNum, short pSeqNum, out int ReturnStoneNum, out NestedDictionary<byte, byte, int, List<ItemAttr>> infos, out List<ItemAttr> Attrs)
        {
            infos = new NestedDictionary<byte, byte, int, List<ItemAttr>>();
            Attrs = new List<ItemAttr>();
            ReturnStoneNum = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_enchant_remove";
                    cmd.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("ItemNum", MySqlDbType.Int32).Value = ItemNum;
                    cmd.Parameters.Add("SeqNum", MySqlDbType.Int16).Value = pSeqNum;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            byte SeqNum, SocketNum;
                            int StoneNum;
                            while (reader.Read())
                            {
                                SeqNum = Convert.ToByte(reader["fdSeqNum"]);
                                SocketNum = Convert.ToByte(reader["fdSocketNum"]);
                                StoneNum = Convert.ToInt32(reader["fdStoneNum"]);
                                ItemAttr attr = new ItemAttr
                                {
                                    Attr = Convert.ToUInt16(reader["fdAttrType"]),
                                    AttrValue = Convert.ToSingle(reader["fdAttrValue"])
                                };
                                ReturnStoneNum = Convert.ToInt32(reader["StoneItem"]);

                                if (!infos.ContainsKey(SeqNum, SocketNum, StoneNum))
                                    infos.Add(SeqNum, SocketNum, StoneNum, new List<ItemAttr> { attr });
                                else
                                    infos[SeqNum][SocketNum][StoneNum].Add(attr);
                            }
                            reader.NextResult();
                            while (reader.Read())
                            {
                                ItemAttr attr = new ItemAttr
                                {
                                    Attr = Convert.ToUInt16(reader["AttrType"]),
                                    AttrValue = Convert.ToSingle(reader["AttrValue"])
                                };
                                Attrs.Add(attr);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
