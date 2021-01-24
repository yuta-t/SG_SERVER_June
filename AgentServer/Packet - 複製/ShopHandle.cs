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
using System.IO;
using AgentServer.Structuring.Item;
using System.Collections.Generic;
using LocalCommons.Utilities;

namespace AgentServer.Packet
{
    public class ShopHandle
    {
        public static void Handle_BuyItem(ClientConnection Client, PacketReader reader, byte last)
        {
            /*FF 69 01 B4 0F 00 00 01 00 00 00 27 BA 00 00 
             70 D8 E9 1E FF FF FF FF FF FF FF FF 01 00 00
             00 00 01 00 00 4E 13 A8 00 FF FF FF FF 00 00
             00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
             00 00 00 93 08 00 00 00 00 00 00 40*/
            Account User = Client.CurrentAccount;
            int unk1 = reader.ReadLEInt32();
            int unk2 = reader.ReadLEInt32();
            int itemid = reader.ReadLEInt32();
            int unk3 = reader.ReadLEInt32();
            reader.ReadLEInt64(); //FF FF FF FF FF FF FF FF
            reader.ReadLEInt32(); //00 00 00 00
            int unk4 = reader.ReadLEInt32();
            reader.ReadLEInt64(); //FF FF FF FF FF FF FF FF
            reader.Offset += 0x14;
            int unk5 = reader.ReadLEInt32();
            bool check = ItemHolder.ItemShopInfos.TryGetValue(itemid, out ItemShopInfo info);
            if (check && info.CanBuy)
            {
                bool BuyCheckDone = BuyItemCheck(User, itemid);
                LoginHandle.getUserCash(User);
                if (BuyCheckDone)
                {
                    Client.SendAsync(new ShopBuyItem_ACK(User, itemid, unk3, unk4, unk5, last));
                    if (info.ItemPosition == 115 || info.ItemPosition == 116)
                    {
                        Client.SendAsync(new ShopBuyFreePassUpdate(User, last));
                        Client.SendAsync(new ShopBuyFreePassUpdate2(User, last));
                    }
                    Client.SendAsync(new MyroomGetAllItem_0X67_64(User, last));
                    //Client.SendAsync(new MyroomGetAllItem(User, last));
                }
            }
            else
            {
                Client.SendAsync(new ShopBuyItemFail(User, itemid, unk3, unk4, unk5, last));
            }
        }

        public static void Handle_GiftItem(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            int itemid = reader.ReadLEInt32();
            int memolen = reader.ReadLEInt32();
            string memo = string.Empty;
            if (memolen > 0)
                memo = reader.ReadBig5StringSafe(memolen);      
            reader.ReadLEInt32(); //00 00 00 00
            int unk1 = reader.ReadLEInt32();
            int unk2 = reader.ReadLEInt32();

            byte ret = GiftItemCheck(User, itemid, nickname, memo);
            //CAN_NOT_GIVE_TO_SELFT
            if (ret == 0)
                Client.SendAsync(new ShopGiftItemOK_ACK(User, itemid, nickname, last));
            else
                Client.SendAsync(new ShopGiftItemError_ACK(User, ret, last));


        }

        public static void Handle_GetItemInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 47 01 01 00 00 00 45 96 00 00 04
            Console.WriteLine("GetItemInfo");
            Account User = Client.CurrentAccount;
            int count = reader.ReadLEInt32(); //count?
            string itemnums = string.Empty;
            for (int i = 0; i < count; i++)
            {
                int itemid = reader.ReadLEInt32();
                itemnums += string.Format("{0},", itemid);
                //itemnums += itemid +",";
            }
            GetAvatarItemInfo(User.UserNum, itemnums, out var iteminfos);

            Client.SendAsync(new ShopBuyItemInfo_ACK(User, iteminfos, last));
            foreach (var item in iteminfos)
            {
                var uitem = User.AvatarItems.Find(f => f.itemdescnum == item.itemdescnum);
                if (uitem != null)
                {
                    if (item.count > 0)
                    {
                        uitem.count = item.count;
                        uitem.expireTime = item.expireTime;
                        uitem.gotDateTime = item.gotDateTime;
                    }
                    else
                    {
                        //User.AvatarItems.RemoveWhere(w => w.itemdescnum == item.itemdescnum);
                        User.AvatarItems.RemoveAll(w => w.itemdescnum == item.itemdescnum);
                    }
                }
                else
                {
                    User.AvatarItems.Add(item);
                }
            }

        }

        public static void Handle_GetCurrentGameMoney(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            GetCurrentGameMoney(User);
            Client.SendAsync(new CurrentGameMoney_ACK(User, last));
        }

        private static bool BuyItemCheck(Account User, int itemid)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_shopBuyProductItemDescNum";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("itemdescnum", MySqlDbType.Int16).Value = itemid;
                cmd.Parameters.Add("paymentType", MySqlDbType.Int16).Value = 1;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                if(Convert.ToInt32(reader["ret"]) == 0)
                {
                    return true;
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            return false;
        }
        private static byte GiftItemCheck(Account User, int itemid, string nickname, string memo)
        {
            byte ret = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_shopGiftItem";
                cmd.Parameters.Add("sendUserNum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("sendNickname", MySqlDbType.VarChar).Value = User.NickName;
                cmd.Parameters.Add("receiveNickname", MySqlDbType.VarChar).Value = nickname;
                cmd.Parameters.Add("itemDescNum", MySqlDbType.Int32).Value = itemid;
                cmd.Parameters.Add("memo", MySqlDbType.VarString).Value = memo;
                cmd.Parameters.Add("paymentType", MySqlDbType.Int32).Value = 1;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                ret = Convert.ToByte(reader["ret"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            return ret;
        }

        private static bool GetAvatarItemInfo(int Usernum, string itemnums, out List<AvatarItemInfo> iteminfo)
        {
            iteminfo = new List<AvatarItemInfo>();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_getAvatarItem";
                    cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = Usernum;
                    cmd.Parameters.Add("itemnums", MySqlDbType.VarChar).Value = itemnums;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AvatarItemInfo item = new AvatarItemInfo
                            {
                                character = Convert.ToInt16(reader["character"]),
                                position = Convert.ToUInt16(reader["position"]),
                                kind = Convert.ToInt16(reader["kind"]),
                                itemdescnum = Convert.ToInt32(reader["itemdescnum"]),
                                expireTime = Convert.IsDBNull(reader["expireTime"]) ? 0L : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["expireTime"])),
                                gotDateTime = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["gotDateTime"])),
                                count = Convert.ToInt32(reader["count"]),
                                exp = Convert.ToInt32(reader["exp"]),
                                use = Convert.ToBoolean(reader["using"])
                            };
                            iteminfo.Add(item);
                        }
                        return true;
                    }
                }
            }
        }

        public static void GetCurrentGameMoney(Account User)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getCurrentGameMoney";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                User.TR = Convert.ToInt64(reader["fdGameMoney"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
    }
}
