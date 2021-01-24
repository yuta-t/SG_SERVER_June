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
using AgentServer.Structuring.Shu;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AgentServer.Packet
{
    public class ShuSystemHandle
    {
        public static void Handle_Shu_GetUserItemInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 2E 05 05 00 00 00 00 00 00 00 00 00 00 00 20
            //FF 2E 05 05 00 00 00 00 00 00 00 01 00 00 00 08
            //shu item state0?
            Account User = Client.CurrentAccount;
            reader.Offset += 8; //subopcode + 00 00 00 00
            int kind = reader.ReadLEInt32();
            Shu_GetUserItemByCharacter(User, kind, out var iteminfos2);
            Client.SendAsync(new Shu_GetUserItemInfo(kind, iteminfos2.ToList(), last));
        }

        public static void Handle_Shu_GetItemInfoByStr(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 2E 05 03 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 48 AA 00 00 08
            Account User = Client.CurrentAccount;
            reader.Offset += 8; //subopcode + 00 00 00 00
            reader.ReadLEInt32(); //00 00 00 00
            int count = reader.ReadLEInt32(); //01 00 00 00
            string strItemNum = string.Empty;
            for (int i = 0; i < count; i++)
            {
                int shuitemnum = reader.ReadLEInt32();
                strItemNum += string.Format("{0},", shuitemnum);
            }
            Shu_GetUserItemInfo(User, string.Empty, strItemNum, out var iteminfos);
            Client.SendAsync(new Shu_GetItemInfoStr(iteminfos.ToList(), last));
        }

        public static void Handle_Shu_Hatch(ClientConnection Client, PacketReader reader, byte last)
        {
            /*FF 2E 05 01 00 00 00 00 00 00 00 65 B8 37 00 
             00 00 00 00 47 AA 00 00 03 00 00 00 01 00 00
             00 3B AA 00 00 02 00 00 00 3D AA 00 00 00 00
             00 00 3A AA 00 00 01*/
            Account User = Client.CurrentAccount;
            reader.Offset += 8; //subopcode + 00 00 00 00
            long eggItemID = reader.ReadLEInt64();
            int eggItemNum = reader.ReadLEInt32();
            int strCount = reader.ReadLEInt32();
            string strPosition = string.Empty;
            string strItemNum = string.Empty;
            for (int i = 0; i < strCount; i++)
            {
                int pos = reader.ReadLEInt32();
                int itemnum = reader.ReadLEInt32();
                strPosition += string.Format("{0},", pos);
                strItemNum += string.Format("{0},", itemnum);
            }
            Shu_Hatch(User, eggItemID, eggItemNum, strPosition, strItemNum, out var infos);

            Client.SendAsync(new Shu_HatchOK(eggItemID, infos, last));
        }

        public static void Handle_Shu_ChangeCurrentShu(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 2E 05 0C 00 00 00 00 00 00 00 7F B8 37 00 00 00 00 00 04
            //FF 2E 05 0C 00 00 00 00 00 00 00 FF FF FF FF FF FF FF FF 01
            Account User = Client.CurrentAccount;
            reader.Offset += 8;
            long shuitemid = reader.ReadLEInt64();
            ChangeCurrentShu(User, shuitemid, out var beforeCharacterItemID, out var infos);
            infos.shuavatars.TryGetValue(shuitemid, out var avatarinfos);
            infos.shuchars.TryGetValue(shuitemid, out var charinfo);
            infos.shustatus.TryGetValue(shuitemid, out var statusinfo);
            UpdateUserShuInfo(User, shuitemid, avatarinfos, charinfo, statusinfo);
            Client.SendAsync(new Shu_ChangeCurrentShu(beforeCharacterItemID, shuitemid, infos, last));
        }

        public static void Handle_Shu_ManagerAction(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 2E 05 09 00 00 00 00 00 00 00 AE C1 1E 00 00 00 00 00 00 00 00 00 02
            //FF 2E 05 09 00 00 00 00 00 00 00 AE C1 1E 00 00 00 00 00 02 00 00 00 02
            Account User = Client.CurrentAccount;
            reader.Offset += 8;
            long shuitemid = reader.ReadLEInt64();
            int actiontype = reader.ReadLEInt32();

            if (ManagerAction(User, shuitemid, actiontype, out var infos))
            {
                if (User.CurrentShuID != -1)
                {
                    infos.shustatus.TryGetValue(User.CurrentShuID, out var statusinfo);
                    User.UserShuInfo.UpdateEachstatusinfo(statusinfo);
                }
                if (infos.beforeLevel != infos.afterLevel)
                    Client.SendAsync(new Shu_LevelUP(shuitemid, infos.beforeLevel, infos.afterLevel, last));
                Client.SendAsync(new Shu_ManagerAction(actiontype, shuitemid, infos, last));
            }
        }

        public static void Handle_Shu_ChangeAvatarInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            /*FF 2E 05 0B 00 00 00 00 00 00 00 AE C1 1E 00 00 00 00 00
             30 00 AE C1 1E 00 00 00 00 00 AF C1 1E 00 00 00 00 00 B0 C1
             1E 00 00 00 00 00 FF FF FF FF FF FF FF FF 46 25 22 00 00
             00 00 00 FF FF FF FF FF FF FF FF 20*/
             //server
            Account User = Client.CurrentAccount;
            reader.Offset += 8;
            long shuitemid = reader.ReadLEInt64();
            short size = reader.ReadLEInt16();
            string strPosition = string.Empty;
            string strItemID = string.Empty;
            for (int i = 0; i < 6; i++)
            {
                long itemid = reader.ReadLEInt64();
                strPosition += string.Format("{0},", i);
                strItemID += string.Format("{0},", itemid);
            }
            //Console.WriteLine("strPosition:{0}", strPosition);
            //Console.WriteLine("strItemID:{0}", strItemID);
            ChangeAvatarInfo(User, shuitemid, strPosition, strItemID, out var infos);
            infos.shuavatars.TryGetValue(shuitemid, out var avinfos);
            UpdateUserShuInfo(User, shuitemid, avinfos, null, null);
            Client.SendAsync(new Shu_ChangeAvatarInfo(User.CurrentShuID, infos, last));
        }
        public static void Handle_Shu_ChangeName(ClientConnection Client, PacketReader reader, byte last)
        {
            /*FF 2E 05 0A 00 00 00 00 00 00 00 AA 10 34 00 00
             00 00 00 0C 00 00 00 61 61 77 61 62 72 77 61 72
             62 77 61 08*/
            //server
            Account User = Client.CurrentAccount;
            reader.Offset += 8;
            long shuitemid = reader.ReadLEInt64();
            int len = reader.ReadLEInt32();
            if (len < 4 || len > 12)
                return;
            string name = string.Empty;
            name = reader.ReadBig5StringSafe(len);
            if (ChangeName(User, shuitemid, name, out var outname))
                Client.SendAsync(new Shu_ChangeNameOK(shuitemid, outname, last));
            if (User.CurrentShuID == shuitemid)
                User.UserShuInfo.ShuName = outname;
        }

        public static void Handle_Shu_ExploreCheck(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 2E 05 0F 00 00 00 00 00 00 00 00 40
            Account User = Client.CurrentAccount;
            reader.Offset += 8;
            byte zoneid = reader.ReadByte();
            ExploreCheck(User, zoneid, out var infos);
            Client.SendAsync(new Shu_ExploreCheck(infos, last));
        }
        public static void Handle_Shu_ExploreStart(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 2E 05 10 00 00 00 00 00 00 00 01 AA 10 34 00 00 00 00 00 02
            Account User = Client.CurrentAccount;
            reader.Offset += 8;
            byte zoneid = reader.ReadByte();
            long shuitemid = reader.ReadLEInt64();

            if (ExploreStart(User, zoneid, shuitemid, out var info))
                Client.SendAsync(new Shu_ExploreStartOK(info, last));
        }
        public static void Handle_Shu_ExploreStop(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 2E 05 11 00 00 00 00 00 00 00 01 10
            Account User = Client.CurrentAccount;
            reader.Offset += 8;
            byte zoneid = reader.ReadByte();

            if (ExploreStop(User, zoneid, out long charid))
                Client.SendAsync(new Shu_ExploreStopOK(zoneid, charid, last));
        }
        public static void Handle_Shu_ExploreReward(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 2E 05 12 00 00 00 00 00 00 00 01 40
            Account User = Client.CurrentAccount;
            reader.Offset += 8;
            byte zoneid = reader.ReadByte();

            if (ExploreReward(User, zoneid, out long charid, out var infos))
                Client.SendAsync(new Shu_ExploreReward(zoneid, charid, infos, last));
        }

        public static void Handle_Shu_GetGift(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 2E 05 0E 00 00 00 00 00 00 00 AA 10 34 00 00 00 00 00 20
            Account User = Client.CurrentAccount;
            reader.Offset += 8;
            long shuitemid = reader.ReadLEInt64();

            if (GetGift(User, shuitemid, out var exp, out var infos))
                Client.SendAsync(new Shu_GetGiftOK(shuitemid, exp, infos, last));
        }

        public static void Handle_Shu_UseItem(ClientConnection Client, PacketReader reader, byte last)
        {
            /*FF 2E 05 0D 00 00 00 00 00 00 00 AA 10 34 00 00
            00 00 00 D1 E3 34 00 00 00 00 00 48 AA 00 00 01 00
            00 00 04*/
            Account User = Client.CurrentAccount;
            reader.Offset += 8;
            long shucharitemid = reader.ReadLEInt64();
            long shuitemid = reader.ReadLEInt64();
            int itemnum = reader.ReadLEInt32();
            int usecount = reader.ReadLEInt32();

            if (ItemHolder.ShuItemCPKInfos.TryGetValue(itemnum, out var cpkinfo))
            {
                if (UseItem(User, cpkinfo.position, shucharitemid, shuitemid, itemnum, usecount, out var infos))
                {
                    if (cpkinfo.position == 2001)
                    {
                        if (infos.beforeLevel != infos.afterLevel)
                            Client.SendAsync(new Shu_LevelUP(shucharitemid, infos.beforeLevel, infos.afterLevel, last));
                        infos.shustatus.TryGetValue(shucharitemid, out var shustate);
                        UpdateUserShuInfo(User, shucharitemid, null, null, shustate);
                    }
                    Client.SendAsync(new Shu_UseItem(cpkinfo.position, shucharitemid, shuitemid, itemnum, usecount, infos, last));
                }
                else
                {
                    Client.SendAsync(new Shu_UseItemFail(shucharitemid, shuitemid, itemnum, usecount, last));
                }
            }
        }

        public static void Shu_GetUserCharacterItemList(Account User, out DBShuInfo infos)//out ConcurrentBag<ShuItemInfo> iteminfos, out DataTable shuinfo, out DataTable shuavatarinfo, out DataTable shustatus)
        {
            /*iteminfos = new ConcurrentBag<ShuItemInfo>();
            shuinfo = new DataTable();
            shuavatarinfo = new DataTable();
            shustatus = new DataTable();*/
            infos = new DBShuInfo();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_getUserCharacterItemList";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("characterItemID", MySqlDbType.Int64).Value = 0;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ShuItemInfo item = new ShuItemInfo
                                {
                                    itemdescnum = Convert.ToInt32(reader["avatarItemNum"]),
                                    itemID = Convert.ToInt64(reader["itemID"]),
                                    gotDateTime = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["gotDateTime"])),
                                    count = Convert.ToInt32(reader["count"]),
                                    state = Convert.ToInt32(reader["state"])
                                };
                                //iteminfos.Add(item);
                                long charid = Convert.ToInt64(reader["characterItemID"]);
                                infos.shuitems.AddOrUpdate(charid, new List<ShuItemInfo> { item }, (k, v) => { v.Add(item); return v; });
                            }
                            reader.NextResult();

                            /* reader.Read();
                             shuinfo.Load(reader);*/
                            while (reader.Read())
                            {
                                ShuCharInfo item = new ShuCharInfo
                                {
                                    avatarItemNum = Convert.ToInt32(reader["avatarItemNum"]),
                                    Name = reader["name"].ToString(),
                                    state = Convert.ToInt32(reader["state"]),
                                    MotionList = Convert.ToInt64(reader["motionList"]),
                                    PurchaseMotionList = Convert.ToInt64(reader["purchaseMotionList"]),
                                };
                                long charid = Convert.ToInt64(reader["characterItemID"]);
                                infos.shuchars.TryAdd(charid, item);
                                infos.characterItemID.Add(charid);
                                //infos.shuchars.AddOrUpdate(charid, new List<ShuCharInfo> { item }, (k, v) => { v.Add(item); return v; });
                            }
                            reader.NextResult();

                            /*reader.Read();
                            shuavatarinfo.Load(reader);*/
                            while (reader.Read())
                            {
                                ShuAvatarInfo item = new ShuAvatarInfo
                                {
                                    Position = Convert.ToInt32(reader["position"]),
                                    itemID = Convert.ToInt64(reader["itemID"]),
                                    avatarItemNum = Convert.ToInt32(reader["avatarItemNum"])
                                };
                                long charid = Convert.ToInt64(reader["characterItemID"]);
                                infos.shuavatars.AddOrUpdate(charid, new List<ShuAvatarInfo> { item }, (k, v) => { v.Add(item); return v; });
                            }
                            foreach (var av in infos.shuavatars)
                            {
                                for (int i = 3; i < 6; i++)
                                {
                                    if (!av.Value.Exists(e => e.Position == i))
                                    {
                                        ShuAvatarInfo item = new ShuAvatarInfo
                                        {
                                            Position = i,
                                            itemID = -1,
                                            avatarItemNum = -1
                                        };
                                        av.Value.Add(item);
                                    }
                                }
                            }
                            reader.NextResult();

                            /*reader.Read();
                            shustatus.Load(reader);*/
                            while (reader.Read())
                            {
                                ShuStatusInfo item = new ShuStatusInfo
                                {
                                    statustype = Convert.ToInt32(reader["statusType"]),
                                    value = Convert.ToInt32(reader["value"])
                                };
                                long charid = Convert.ToInt64(reader["characterItemID"]);
                                infos.shustatus.AddOrUpdate(charid, new List<ShuStatusInfo> { item }, (k, v) => { v.Add(item); return v; });
                            }

                        }
                    }
                }
            }
        }
        public static void Shu_UserStatusInfo(Account User, out ConcurrentDictionary<long, List<int>> shustatus)
        {
            shustatus = new ConcurrentDictionary<long, List<int>>();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_getUserStatusInfo";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("characterItemID", MySqlDbType.Int64).Value = 0;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                long id = Convert.ToInt64(reader["characterItemID"]);
                                int value = Convert.ToInt32(reader["value"]);
                                shustatus.AddOrUpdate(id, new List<int> { value }, (k, v) => { v.Add(value); return v; });
                            }
                        }
                    }
                }
            }
        }

        public static void Shu_CheckSatiety(Account User)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_checkSatiety";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("resultValue", MySqlDbType.Int32).Value = 1;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            Convert.ToDateTime(reader["nextCheckTime"]);
                        }
                    }
                }
            }
        }

        public static void UpdateUserShuInfo(Account User, long currentshuid, List<ShuAvatarInfo> avatarinfos, ShuCharInfo charinfo, List<ShuStatusInfo> statusinfo)
        {
            User.CurrentShuID = currentshuid;
            //User.isUsingShu = currentshuid != -1;
            if (User.CurrentShuID != -1)
            {
                if (avatarinfos != null)
                    User.UserShuInfo.updateavatar(avatarinfos);
                if (charinfo != null)
                    User.UserShuInfo.updatecharinfo(charinfo);
                if (statusinfo != null)
                    User.UserShuInfo.updatestatusinfo(statusinfo);
            }
        }

        public static void Shu_GetUserItemInfo(Account User, string strItemID, string strItemNums, out List<ShuItemInfo> iteminfos)
        {
            iteminfos = new List<ShuItemInfo>();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_getUserItemInfo";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("strItemID", MySqlDbType.VarString).Value = strItemID;
                    cmd.Parameters.Add("strItemNums", MySqlDbType.VarString).Value = strItemNums;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ShuItemInfo item = new ShuItemInfo
                                {
                                    itemdescnum = Convert.ToInt32(reader["avatarItemNum"]),
                                    itemID = Convert.ToInt64(reader["itemID"]),
                                    gotDateTime = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["gotDateTime"])),
                                    count = Convert.ToInt32(reader["count"]),
                                    state = Convert.ToInt32(reader["state"])
                                };
                                iteminfos.Add(item);
                            }
                        }
                    }
                }
            }
        }

        private static void Shu_GetUserItemByCharacter(Account User, int kind, out ConcurrentBag<ShuItemInfo> iteminfos)
        {
            iteminfos = new ConcurrentBag<ShuItemInfo>();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_getUserItemByCharacter";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("kind", MySqlDbType.Int32).Value = kind;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ShuItemInfo item = new ShuItemInfo
                                {
                                    itemdescnum = Convert.ToInt32(reader["avatarItemNum"]),
                                    itemID = Convert.ToInt64(reader["itemID"]),
                                    gotDateTime = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["gotDateTime"])),
                                    count = Convert.ToInt32(reader["count"]),
                                    state = Convert.ToInt32(reader["state"])
                                };
                                iteminfos.Add(item);
                            }
                        }
                    }
                }
            }
        }

        private static void Shu_Hatch(Account User, long eggItemID, int eggItemNum, string strPosition, string strItemNum, out DBShuInfo infos)
        {
            //egginfo = new DataTable();
            infos = new DBShuInfo();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_hatch";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("eggItemID", MySqlDbType.Int64).Value = eggItemID;
                    cmd.Parameters.Add("eggItemNum", MySqlDbType.Int32).Value = eggItemNum;
                    cmd.Parameters.Add("strPosition", MySqlDbType.VarString).Value = strPosition;
                    cmd.Parameters.Add("strItemNum", MySqlDbType.VarString).Value = strItemNum;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                           //egginfo.Load(reader);
                            ShuItemInfo eggitem = new ShuItemInfo
                            {
                                itemdescnum = 0,
                                itemID = eggItemID,
                                gotDateTime = Convert.IsDBNull(reader["gotDateTime"]) ? 0 : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["gotDateTime"])),
                                count = Convert.IsDBNull(reader["count"]) ? 0 : Convert.ToInt32(reader["count"]),
                                state = Convert.IsDBNull(reader["state"]) ? 0 : Convert.ToInt32(reader["state"])
                            };
                            long charid = 0;
                            infos.shuitems.AddOrUpdate(charid, new List<ShuItemInfo> { eggitem }, (k, v) => { v.Add(eggitem); return v; });
                            
                        }
                        reader.NextResult();

                        while (reader.Read())
                        {
                            ShuItemInfo item = new ShuItemInfo
                            {
                                itemdescnum = Convert.ToInt32(reader["avatarItemNum"]),
                                itemID = Convert.ToInt64(reader["itemID"]),
                                gotDateTime = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["gotDateTime"])),
                                count = Convert.ToInt32(reader["count"]),
                                state = Convert.ToInt32(reader["state"])
                            };
                            long charid = Convert.ToInt64(reader["characterItemID"]);
                            infos.shuitems.AddOrUpdate(charid, new List<ShuItemInfo> { item }, (k, v) => { v.Add(item); return v; });
                        }
                        reader.NextResult();

                        while (reader.Read())
                        {
                            ShuCharInfo item = new ShuCharInfo
                            {
                                avatarItemNum = Convert.ToInt32(reader["avatarItemNum"]),
                                Name = reader["name"].ToString(),
                                state = Convert.ToInt32(reader["state"]),
                                MotionList = Convert.ToInt64(reader["motionList"]),
                                PurchaseMotionList = Convert.ToInt64(reader["purchaseMotionList"]),
                            };
                            long charid = Convert.ToInt64(reader["characterItemID"]);
                            infos.shuchars.TryAdd(charid, item);
                            infos.characterItemID.Add(charid);
                        }
                        reader.NextResult();

                        while (reader.Read())
                        {
                            ShuAvatarInfo item = new ShuAvatarInfo
                            {
                                Position = Convert.ToInt32(reader["position"]),
                                itemID = Convert.ToInt64(reader["itemID"]),
                                avatarItemNum = Convert.ToInt32(reader["avatarItemNum"])
                            };
                            long charid = Convert.ToInt64(reader["characterItemID"]);
                            infos.shuavatars.AddOrUpdate(charid, new List<ShuAvatarInfo> { item }, (k, v) => { v.Add(item); return v; });
                        }
                        foreach (var av in infos.shuavatars)
                        {
                            for (int i = 3; i < 6; i++)
                            {
                                if (!av.Value.Exists(e => e.Position == i))
                                {
                                    ShuAvatarInfo item = new ShuAvatarInfo
                                    {
                                        Position = i,
                                        itemID = -1,
                                        avatarItemNum = -1
                                    };
                                    av.Value.Add(item);
                                }
                            }
                        }
                        reader.NextResult();

                        while (reader.Read())
                        {
                            ShuStatusInfo item = new ShuStatusInfo
                            {
                                statustype = Convert.ToInt32(reader["statusType"]),
                                value = Convert.ToInt32(reader["value"])
                            };
                            long charid = Convert.ToInt64(reader["characterItemID"]);
                            infos.shustatus.AddOrUpdate(charid, new List<ShuStatusInfo> { item }, (k, v) => { v.Add(item); return v; });
                        }

                    }
                }
            }
        }

        private static void ChangeCurrentShu(Account User, long characterItemID, out long beforeCharacterItemID, out DBShuInfo infos)
        {
            infos = new DBShuInfo();
            beforeCharacterItemID = -1;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_changeCurrentShu";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("characterItemID", MySqlDbType.Int64).Value = characterItemID;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                beforeCharacterItemID = Convert.ToInt64(reader["beforeCharacterItemID"]);
                                //nextCheckTime
                            }
                            reader.NextResult();

                            while (reader.Read())
                            {
                                ShuCharInfo item = new ShuCharInfo
                                {
                                    avatarItemNum = Convert.ToInt32(reader["avatarItemNum"]),
                                    Name = reader["name"].ToString(),
                                    state = Convert.ToInt32(reader["state"]),
                                    MotionList = Convert.ToInt64(reader["motionList"]),
                                    PurchaseMotionList = Convert.ToInt64(reader["purchaseMotionList"]),
                                };
                                long charid = Convert.ToInt64(reader["characterItemID"]);
                                infos.shuchars.TryAdd(charid, item);
                                infos.characterItemID.Add(charid);
                            }
                            reader.NextResult();

                            while (reader.Read())
                            {
                                ShuAvatarInfo item = new ShuAvatarInfo
                                {
                                    Position = Convert.ToInt32(reader["position"]),
                                    itemID = Convert.ToInt64(reader["itemID"]),
                                    avatarItemNum = Convert.ToInt32(reader["avatarItemNum"])
                                };
                                long charid = Convert.ToInt64(reader["characterItemID"]);
                                infos.shuavatars.AddOrUpdate(charid, new List<ShuAvatarInfo> { item }, (k, v) => { v.Add(item); return v; });
                            }
                            foreach (var av in infos.shuavatars)
                            {
                                for (int i = 3; i < 6; i++)
                                {
                                    if (!av.Value.Exists(e => e.Position == i))
                                    {
                                        ShuAvatarInfo item = new ShuAvatarInfo
                                        {
                                            Position = i,
                                            itemID = -1,
                                            avatarItemNum = -1
                                        };
                                        av.Value.Add(item);
                                    }
                                }
                            }
                            reader.NextResult();

                            while (reader.Read())
                            {
                                ShuStatusInfo item = new ShuStatusInfo
                                {
                                    statustype = Convert.ToInt32(reader["statusType"]),
                                    value = Convert.ToInt32(reader["value"])
                                };
                                long charid = Convert.ToInt64(reader["characterItemID"]);
                                infos.shustatus.AddOrUpdate(charid, new List<ShuStatusInfo> { item }, (k, v) => { v.Add(item); return v; });
                            }

                        }
                    }
                }
            }
        }

        private static bool ManagerAction(Account User, long characterItemID, int actionType, out DBShuActionInfo infos)
        {
            infos = new DBShuActionInfo();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_managerAction";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("characterItemID", MySqlDbType.Int64).Value = characterItemID;
                    cmd.Parameters.Add("actionType", MySqlDbType.Int32).Value = actionType;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ShuActionResultInfo item = new ShuActionResultInfo
                                {
                                    statusType = Convert.ToInt32(reader["statusType"]),
                                    giveValue = Convert.ToInt32(reader["giveValue"])
                                };
                                infos.remainMP = Convert.ToInt32(reader["remainMP"]);
                                //int statusType = Convert.ToInt32(reader["statusType"]);
                                infos.ActionResult.Add(item);
                            }
                            reader.NextResult();

                            while (reader.Read())
                            {
                                ShuStatusInfo item = new ShuStatusInfo
                                {
                                    statustype = Convert.ToInt32(reader["statusType"]),
                                    value = Convert.ToInt32(reader["value"])
                                };
                                long charid = Convert.ToInt64(reader["characterItemID"]);
                                infos.shustatus.AddOrUpdate(charid, new List<ShuStatusInfo> { item }, (k, v) => { v.Add(item); return v; });
                            }
                            reader.NextResult();

                            while (reader.Read())
                            {
                                infos.beforeLevel = Convert.ToInt32(reader["beforeLevel"]);
                                infos.afterLevel = Convert.ToInt32(reader["afterLevel"]);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static void ChangeAvatarInfo(Account User, long characterItemID, string strPosition, string strItemID, out DBShuChangeAVInfo infos)
        {
            infos = new DBShuChangeAVInfo();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_changeAvatarInfo";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("characterItemID", MySqlDbType.Int64).Value = characterItemID;
                    cmd.Parameters.Add("strPosition", MySqlDbType.VarString).Value = strPosition;
                    cmd.Parameters.Add("strItemID", MySqlDbType.VarString).Value = strItemID;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {

                            while (reader.Read())
                            {
                                ShuAvatarState item = new ShuAvatarState
                                {
                                    itemID = Convert.ToInt32(reader["itemID"]),
                                    state = Convert.ToInt32(reader["state"])
                                };
                                infos.AvatarState.Add(item);
                            }
                            reader.NextResult();
                            //int x = 0;
                            while (reader.Read())
                            {
                                long itemid = Convert.ToInt64(reader["itemID"]);
                                ShuAvatarInfo item = new ShuAvatarInfo
                                {
                                    Position = Convert.ToInt32(reader["position"]),
                                    itemID = itemid,
                                    avatarItemNum = Convert.ToInt32(reader["avatarItemNum"])
                                };
                                long charid = Convert.ToInt64(reader["characterItemID"]);
                                var find = infos.AvatarState.Find(f => f.itemID == itemid);
                                infos.shuavatars.AddOrUpdate(charid, new List<ShuAvatarInfo> { item }, (k, v) => { v.Add(item); return v; });
                                /*if (x < 3)
                                    infos.shuavatars.AddOrUpdate(charid, new List<ShuAvatarInfo> { item }, (k, v) => { v.Add(item); return v; });
                                else if (x > 2 && find.state == 1)
                                    infos.shuavatars.AddOrUpdate(charid, new List<ShuAvatarInfo> { item }, (k, v) => { v.Add(item); return v; });
                                x++;*/
                            }
                            foreach (var av in infos.shuavatars)
                            {
                                for (int i = 3; i < 6; i++)
                                {
                                    if (!av.Value.Exists(e => e.Position == i))
                                    {
                                        ShuAvatarInfo item = new ShuAvatarInfo
                                        {
                                            Position = i,
                                            itemID = -1,
                                            avatarItemNum = -1
                                        };
                                        av.Value.Add(item);
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }

        private static bool ChangeName(Account User, long characterItemID, string name, out string outname)
        {
            outname = string.Empty;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_changeName";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("characterItemID", MySqlDbType.Int64).Value = characterItemID;
                    cmd.Parameters.Add("changeName", MySqlDbType.VarString).Value = name;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            outname = reader["changeName"].ToString();
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private static bool ExploreStart(Account User, byte zoneid, long characterID, out ExploreInfo info)
        {
            info = new ExploreInfo();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_exploreStart";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("zoneNum", MySqlDbType.Int16).Value = zoneid;
                    cmd.Parameters.Add("characterItemID", MySqlDbType.Int64).Value = characterID;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            info.zoneNum = Convert.ToByte(reader["zoneNum"]);
                            info.characterItemID = Convert.ToInt64(reader["characterItemID"]);
                            info.endDateTime = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["endDateTime"]));
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private static bool ExploreStop(Account User, byte zoneid, out long characterItemID)
        {
            characterItemID = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_exploreStop";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("zoneNum", MySqlDbType.Int16).Value = zoneid;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            characterItemID = Convert.ToInt64(reader["characterItemID"]);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private static void ExploreCheck(Account User, byte zonenum, out List<ExploreInfo> infos)
        {
            infos = new List<ExploreInfo>();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_exploreCheck";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("zoneNum", MySqlDbType.Int16).Value = zonenum;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ExploreInfo info = new ExploreInfo
                                {
                                    zoneNum = Convert.ToByte(reader["zoneNum"]),
                                    characterItemID = Convert.ToInt64(reader["characterItemID"]),
                                    endDateTime = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["endDateTime"]))
                                };
                                infos.Add(info);
                            }
                        }
                    }
                }
            }
        }
        private static bool ExploreReward(Account User, byte zonenum, out long characterItemID, out List<ShuRewardInfo> infos)
        {
            characterItemID = -1;
            infos = new List<ShuRewardInfo>();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_exploreReward";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("zoneNum", MySqlDbType.Int16).Value = zonenum;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ShuRewardInfo info = new ShuRewardInfo
                                {
                                    rewardType = Convert.ToInt32(reader["rewardType"]),
                                    rewardItem = Convert.ToInt32(reader["rewardItem"]),
                                    rewardCount = Convert.ToInt32(reader["rewardCount"])
                                };
                                characterItemID = Convert.ToInt64(reader["characterItemID"]);
                                infos.Add(info);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private static bool GetGift(Account User, long characterItemID, out int exp, out List<ShuRewardInfo> infos)
        {
            exp = -1;
            infos = new List<ShuRewardInfo>();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_gift";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("characterItemID", MySqlDbType.Int64).Value = characterItemID;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ShuRewardInfo info = new ShuRewardInfo
                                {
                                    rewardType = Convert.ToInt32(reader["rewardType"]),
                                    rewardItem = Convert.ToInt32(reader["rewardItem"]),
                                    rewardCount = Convert.ToInt32(reader["rewardCount"])
                                };
                                exp = Convert.ToInt32(reader["exp"]);
                                infos.Add(info);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool UseItem(Account User, int position, long characterItemID, long itemID, int itemNum, int useCount, out DBShuUseItemInfo infos)
        {
            infos = new DBShuUseItemInfo();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_useItem";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("characterItemID", MySqlDbType.Int64).Value = characterItemID;
                    cmd.Parameters.Add("itemID", MySqlDbType.Int64).Value = itemID;
                    cmd.Parameters.Add("itemNum", MySqlDbType.Int32).Value = itemNum;
                    cmd.Parameters.Add("useCount", MySqlDbType.Int32).Value = useCount;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        /*if (reader.HasRows)
                        {*/
                            while (reader.Read())
                            {
                                ShuItemInfo item = new ShuItemInfo
                                {
                                    itemdescnum = Convert.ToInt32(reader["avatarItemNum"]),
                                    itemID = Convert.ToInt64(reader["itemID"]),
                                    gotDateTime = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["gotDateTime"])),
                                    count = Convert.ToInt32(reader["count"]),
                                    state = Convert.ToInt32(reader["state"])
                                };
                                infos.ItemInfos.Add(item);
                            }
                            reader.NextResult();

                        if (reader.HasRows)
                        {
                            if (position == 2001)
                            {
                                while (reader.Read())
                                {
                                    infos.beforeLevel = Convert.ToInt32(reader["beforeLevel"]);
                                    infos.afterLevel = Convert.ToInt32(reader["afterLevel"]);                               
                                }
                                reader.NextResult();
                                while (reader.Read())
                                {
                                    ShuStatusInfo item = new ShuStatusInfo
                                    {
                                        statustype = Convert.ToInt32(reader["statusType"]),
                                        value = Convert.ToInt32(reader["value"])
                                    };
                                    long charid = Convert.ToInt64(reader["characterItemID"]);
                                    infos.shustatus.AddOrUpdate(charid, new List<ShuStatusInfo> { item }, (k, v) => { v.Add(item); return v; });
                                }
                            }
                            else if (position == 2002)
                            {
                                while (reader.Read())
                                {
                                    infos.remainMP = Convert.ToInt32(reader["remainMP"]);
                                }
                            }
                            else if (position == 2003)
                            {
                                while (reader.Read())
                                {
                                    ShuCharInfo item = new ShuCharInfo
                                    {
                                        avatarItemNum = Convert.ToInt32(reader["avatarItemNum"]),
                                        Name = reader["name"].ToString(),
                                        state = Convert.ToInt32(reader["state"]),
                                        MotionList = Convert.ToInt64(reader["motionList"]),
                                        PurchaseMotionList = Convert.ToInt64(reader["purchaseMotionList"]),
                                    };
                                    long charid = Convert.ToInt64(reader["characterItemID"]);
                                    infos.shuchars.TryAdd(charid, item);
                                    infos.characterItemID.Add(charid);
                                    //infos.shuchars.AddOrUpdate(charid, new List<ShuCharInfo> { item }, (k, v) => { v.Add(item); return v; });
                                }
                                reader.NextResult();

                                while (reader.Read())
                                {
                                    ShuAvatarInfo item = new ShuAvatarInfo
                                    {
                                        Position = Convert.ToInt32(reader["position"]),
                                        itemID = Convert.ToInt64(reader["itemID"]),
                                        avatarItemNum = Convert.ToInt32(reader["avatarItemNum"])
                                    };
                                    long charid = Convert.ToInt64(reader["characterItemID"]);
                                    infos.shuavatars.AddOrUpdate(charid, new List<ShuAvatarInfo> { item }, (k, v) => { v.Add(item); return v; });
                                }
                                foreach (var av in infos.shuavatars)
                                {
                                    for (int i = 3; i < 6; i++)
                                    {
                                        if (!av.Value.Exists(e => e.Position == i))
                                        {
                                            ShuAvatarInfo item = new ShuAvatarInfo
                                            {
                                                Position = i,
                                                itemID = -1,
                                                avatarItemNum = -1
                                            };
                                            av.Value.Add(item);
                                        }
                                    }
                                }
                                reader.NextResult();

                                while (reader.Read())
                                {
                                    ShuStatusInfo item = new ShuStatusInfo
                                    {
                                        statustype = Convert.ToInt32(reader["statusType"]),
                                        value = Convert.ToInt32(reader["value"])
                                    };
                                    long charid = Convert.ToInt64(reader["characterItemID"]);
                                    infos.shustatus.AddOrUpdate(charid, new List<ShuStatusInfo> { item }, (k, v) => { v.Add(item); return v; });
                                }

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
