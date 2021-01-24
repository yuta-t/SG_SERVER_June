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
using LocalCommons.Logging;

namespace AgentServer.Packet
{
    public class ParkHandle
    {
        public static void Handle_GetMachineInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 73 05 7C DF 00 00 62 00 00 00 40
            Account User = Client.CurrentAccount;
            int MachineItemNum = reader.ReadLEInt32();
            int MachineID = reader.ReadLEInt32();
            //Console.WriteLine("MachineItemNum: {0}", MachineItemNum);
            //Console.WriteLine("MachineID: {0}", MachineID);

            if (CapsuleMachineHolder.CapsuleMachineInfos.TryGetValue(MachineItemNum, out CapsuleMachineInfo MachineInfos))
            {
                if (MachineInfos.LastResetTime < DateTime.Now)
                {
                    Client.SendAsync(new GetMachineInfo(User, MachineItemNum, MachineInfos, last));
                }
                else
                {
                    Client.SendAsync(new GetMachineInfoResetting(User, MachineItemNum, MachineInfos, last));
                }
            }
            else
            {
                Client.SendAsync(new GetMachineInfoFail(User, MachineItemNum, MachineID, last));
            }
        }

        public static void Handle_Alchemist_MachineSelect(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            int type = reader.ReadLEInt32(); //04 00 00 00
            if (type == 4) //4 = 抽蛋
            {
                //FF 97 01 04 00 00 00 05 00 00 00 36 31 37 39 38 03 00 00 00 31 31 32 00 00 00 00 01
                int itemnumlen = reader.ReadLEInt32();
                int MachineItemNum = Convert.ToInt32(reader.ReadBig5StringSafe(itemnumlen));
                int itemkindlen = reader.ReadLEInt32();
                int MachineKindID = Convert.ToInt32(reader.ReadBig5StringSafe(itemkindlen));
                //00 00 00 00
                if (MachineKindID > 1000)
                    MachineItemNum = 43845;

                CapsuleMachineHolder.CapsuleMachineItems.TryGetValue(MachineItemNum, out List<CapsuleMachineItem> MachineItems);
                CapsuleMachineHolder.CapsuleMachineInfos.TryGetValue(MachineItemNum, out CapsuleMachineInfo MachineInfos);
                if (MachineInfos.LastResetTime < DateTime.Now)
                {
                    int GetItem = CapsuleMachineGetItem(User, MachineItemNum, MachineKindID, MachineInfos, MachineItems, out byte ret);
                    //SelectMachineItem(User, MachineItemNum, MachineKindID,MachineKindID MachineInfos, GetItem);
                    if (ret == 0)
                        Client.SendAsync(new GetMachineSelectItem(User, MachineInfos.RealMachineNum, GetItem, last));
                    else
                        Client.SendAsync(new GetMachineSelectItemFail(MachineInfos.RealMachineNum, last));
                    return;
                }
                Client.SendAsync(new GetMachineSelectItemFail(MachineInfos.RealMachineNum, last));
            }
            else if (type == 3) //3 = 鍊金合成
            {
                //FF 97 01 03 00 00 00 01 00 00 00 31 03 00 00 00 35 37 31 01 00 00 00 30 02 00 00 00 38 30 80
                //FF 97 01 03 00 00 00 01 00 00 00 30 04 00 00 00 37 34 33 36 01 00 00 00 30 02 00 00 00 38 30 80
                //Console.WriteLine(Utility.ByteArrayToString(reader.Buffer));
                int CouponConsumelen = reader.ReadLEInt32();
                int CouponConsume = Convert.ToInt32(reader.ReadBig5StringSafe(CouponConsumelen)); //CouponConsume?
                int RecipeItemNumlen = reader.ReadLEInt32();
                int RecipeItemNum = Convert.ToInt32(reader.ReadBig5StringSafe(RecipeItemNumlen));
                int unk2len = reader.ReadLEInt32();
                int unk2 = Convert.ToInt32(reader.ReadBig5StringSafe(unk2len)); //pAlchemistSetItemNum??
                int luckylen = reader.ReadLEInt32();
                int lucky = Convert.ToInt32(reader.ReadBig5StringSafe(luckylen));

                int AlchemistMethod;
                if (CouponConsume == 1)
                    AlchemistMethod = 1;
                else
                    AlchemistMethod = 2;

                int ItemClass = GetAlchemistMixGrade((float)User.Luck);
                lucky = (int)Math.Round(User.Luck, MidpointRounding.AwayFromZero);
                if (AlchemistMix(User, CouponConsume, RecipeItemNum, lucky, ItemClass, AlchemistMethod, out int resultitem, out List<ItemAttr> Attrs))
                {
                    Client.SendAsync(new AlchemistMix(User, resultitem, Attrs, last));
                    User.UserItemAttr.TryAdd(resultitem, Attrs);
                }

            }
            else if (type == 5) //5 = 鍊金升級
            {
                //FF 97 01 05 00 00 00 03 00 00 00 35 36 35 03 00 00 00 31 33 30 03 00 00 00 35 37 31 01 00 00 00 31 40
                //FF 97 01 05 00 00 00 03 00 00 00 35 36 35 03 00 00 00 31 33 30 03 00 00 00 35 37 31 01 00 00 00 30 02
                int ItemNumlen = reader.ReadLEInt32();
                int ItemNum = Convert.ToInt32(reader.ReadBig5StringSafe(ItemNumlen));
                int luckylen = reader.ReadLEInt32();
                int lucky = Convert.ToInt32(reader.ReadBig5StringSafe(luckylen));
                int RecipeItemNumlen = reader.ReadLEInt32();
                int RecipeItemNum = Convert.ToInt32(reader.ReadBig5StringSafe(RecipeItemNumlen));
                int AlchemistMethodlen = reader.ReadLEInt32();
                int AlchemistMethod = Convert.ToInt32(reader.ReadBig5StringSafe(AlchemistMethodlen)); //pAlchemistSetItemNum??

                if (User.UserItemAttr.ContainsKey(ItemNum))
                {
                    int ItemClass = GetAlchemistMixGrade((float)User.Luck);
                    lucky = (int)Math.Round(User.Luck, MidpointRounding.AwayFromZero);
                    if (AlchemistUpgrade(User.UserNum, ItemNum, RecipeItemNum, lucky, ItemClass, AlchemistMethod, out int resultitem, out List<ItemAttr> Attrs))
                    {
                        Client.SendAsync(new AlchemistUpgrade(User, resultitem, Attrs, last));
                        User.UserItemAttr.TryRemove(ItemNum, out _);
                        User.UserItemAttr.TryAdd(resultitem, Attrs);
                    }
                }

            }
            else if (type == 7) //7 = BuyMyRoomSlot
            {
                //FF 97 01 07 00 00 00 05 00 00 00 32 39 36 31 33 00 00 00 00 00 00 00 00 10
                int len = reader.ReadInt32();
                int slotitemnum = Convert.ToInt32(reader.ReadStringSafe(len));
                if (MyRoomGiveUserMyRoomSlot(User, slotitemnum, out int SlotNum))
                    Client.SendAsync(new Myroom_BuyMyRoomSlotOK(SlotNum, last));
            }
        }

        public static void Handle_MachineReceiveORGiftItem(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 37 02 08 00 00 00 70 6F 70 6F 34 35 36 49 00 00 00 00 80
            //FF 37 02 06 00 00 00 41 6C 74 4B 6F 59 04 00 00 00 33 34 35 33 80
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            int memolen = reader.ReadLEInt32();
            string memo = string.Empty;
            if (memolen > 0)
                memo = reader.ReadBig5StringSafe(memolen);
            int itemnum = GiveItem(User, nickname, memo, out bool isGift, out byte ret);
            if (ret == 0)
                Client.SendAsync(new MachineGiveItem(User, itemnum, isGift, nickname, last));
            else
                Client.SendAsync(new MachineGiveItemFail(last));
        }
        public static void Handle_MachineKeepItem(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 47 05 01 00 00 00 2E 1A 00 00 20
            Account User = Client.CurrentAccount;
            int type = reader.ReadLEInt32();
            int ItemNum = reader.ReadLEInt32();
            bool isSuccess = KeepItem(User, type, ItemNum, out long uniqueNum, out int itemNum, out long dateTime);
            Client.SendAsync(new MachineKeepItem(User, isSuccess, uniqueNum, itemNum, dateTime, last));
        }

        private static int CapsuleMachineGetItem(Account User, int MachineItemNum, int MachineKindID, CapsuleMachineInfo infos, List<CapsuleMachineItem> CapsuleMachineItem, out byte ret)
        {
            int TotalCount = infos.TotalItemCount;
            //int itemnum = 0;
            CapsuleMachineItem ItemInfo = new CapsuleMachineItem();
            //ItemInfo = CapsuleMachineItem.Where(w => w.Level == 1).FirstOrDefault();

            double UserLucky = (double)User.Luck;
            double CapsuleMachineRateLuckyDiv = 200; //DB
            int CapsuleMachineMaxDice = 10; //DB

            int ItemLevel = 6;
            int ItemLevelBefore = 0;
            int DrawTime = 1;
            DrawTime = (int)(UserLucky / CapsuleMachineRateLuckyDiv + 1.0);
            if (DrawTime > CapsuleMachineMaxDice)
                DrawTime = CapsuleMachineMaxDice;

            while (DrawTime > 0)
            {
                drawtart:
                Random rnd = new Random(Guid.NewGuid().GetHashCode());
                int rndnum = rnd.Next(1, TotalCount + 1);
                int min = 1, max = 1;
                foreach (var item in CapsuleMachineItem.OrderBy(o => o.ItemMax))
                {
                    max += item.ItemMax;
                    if (min <= rndnum && rndnum < max)
                    {
                        if (item.ItemCount <= 0)
                            goto drawtart; //redraw

                        ItemLevelBefore = ItemLevel;
                        if (ItemLevelBefore >= item.Level)
                        {
                            ItemLevel = item.Level;
                            ItemInfo = item;
                        }
                        break;
                    }
                    min = max;
                }

                DrawTime--;
            }

            ret = 0;
            bool isReset = false;
            int resultItemNum = 0;
            int level = 0;
            int PriceType = 0, cost = 0;
            try
            {
                using (var con = new MySqlConnection(Conf.Connstr))
                {
                    con.Open();
                    var cmd = new MySqlCommand(string.Empty, con);
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_capsuleMachineSelect";
                    cmd.Parameters.Add("machineNum", MySqlDbType.Int32).Value = infos.RealMachineNum;
                    cmd.Parameters.Add("machineKind", MySqlDbType.Int32).Value = infos.RealMachineNumKind;
                    cmd.Parameters.Add("itemNum", MySqlDbType.Int32).Value = ItemInfo.ItemNum;
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("selectTime", MySqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("resetCount", MySqlDbType.Int32).Value = infos.ResetCount;
                    MySqlDataReader reader = cmd.ExecuteReader();
                    reader.Read();
                    ret = (byte)reader.GetInt32("retval");
                    if (ret == 0)
                    {
                        resultItemNum = reader.GetInt32("resultItemNum");
                        level = reader.GetInt32("level");
                        isReset = reader.GetBoolean("isReset");
                        cost = reader.GetInt32("cost");
                        //myAsset = reader.GetInt32("myAsset");
                        PriceType = Convert.ToInt32(reader["PriceType"]);
                    }
                    else
                        Console.WriteLine("RET: {0}", ret);
                    cmd.Dispose();
                    reader.Close();
                    con.Close();
                }
                if (ret == 0)
                {
                    if (level < 3)
                    {
                        Task.Run(() =>
                        {
                            foreach (var ac in ClientConnection.CurrentAccounts.Values)
                            {
                                ac.Connection.SendAsync(new CapsuleMachineNotice(ac, (byte)level, MachineItemNum, resultItemNum, User.NickName));
                            }
                        });
                    }

                    if (isReset)
                    {
                        string MachineNum = CapsuleMachineHolder.UpdateCapsuleMachineInfo(MachineItemNum, infos.RealMachineNum, infos.isRotate);
                        if (infos.isRotate)
                        {
                            Task.Run(() =>
                            {
                                foreach (var ac in ClientConnection.CurrentAccounts.Values)
                                {
                                    ac.Connection.SendAsync(new RoatateMachineNotice(ac, MachineNum));
                                }
                            });
                        }
                    }
                    else
                        CapsuleMachineHolder.CapsuleMachineItems[MachineItemNum].Where(w => w.ItemNum == ItemInfo.ItemNum).ToList()
                                                                                .ForEach(f => f.ItemCount -= 1);

                    if (PriceType == 0) //TR
                        User.TR -= cost;
                    else if (PriceType == 1) //點數
                        User.Cash -= cost;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error on capsule machine get item: {0}", ex.Message);
                ret = 1;
            }
            return resultItemNum;
        }
        private static int GiveItem(Account User, string NickName, string memo, out bool isGift, out byte ret)
        {
            ret = 0;
            isGift = false;
            int resultItemNum = 0;
            try
            {
                using (var con = new MySqlConnection(Conf.Connstr))
                {
                    con.Open();
                    var cmd = new MySqlCommand(string.Empty, con);
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_capsuleMachineGiveItem";
                    cmd.Parameters.Add("sendUserNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("sendNickname", MySqlDbType.VarString).Value = User.NickName;
                    cmd.Parameters.Add("receiveNickname", MySqlDbType.VarString).Value = NickName;
                    cmd.Parameters.Add("memo", MySqlDbType.VarString).Value = memo;
                    MySqlDataReader reader = cmd.ExecuteReader();
                    reader.Read();
                    ret = (byte)reader.GetInt32("retval");
                    if (ret == 0)
                    {
                        resultItemNum = reader.GetInt32("itemNum");
                        isGift = reader.GetBoolean("isGift");
                        Convert.ToInt64(reader["reamainGameMoney"]);
                    }
                    cmd.Dispose();
                    reader.Close();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error on machine give item: {0}", ex.Message);
                ret = 1;
            }
            return resultItemNum;
        }
        private static bool KeepItem(Account User, int type, int itemnum, out long uniqueNum, out int itemNum, out long dateTime)
        {
            uniqueNum = 0;
            itemNum = 0;
            dateTime = 0;
            try
            {
                using (var con = new MySqlConnection(Conf.Connstr))
                {
                    con.Open();
                    var cmd = new MySqlCommand(string.Empty, con);
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_storage_save";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("type", MySqlDbType.Int32).Value = type;
                    cmd.Parameters.Add("itemNum", MySqlDbType.Int32).Value = itemnum;
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        uniqueNum = Convert.ToInt64(reader["uniqueNum"]);
                        itemNum = Convert.ToInt32(reader["itemNum"]);
                        dateTime = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["dateTime"]));
                        return true;
                    }
                    cmd.Dispose();
                    reader.Close();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error on keep item: {0}", ex.Message);
                return false;
            }
            return false;
        }

        public static int GetAlchemistMixGrade(float luck)
        {
            //100 = S
            //200 = A
            //300 = B
            //400 = C
            int ItemClass = 0;
            int randpoint = 0;
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            if (luck <= 0) //noluck
            {
                int AlchemistMixNoLuckUserMaxLimitPoint = 81;
                randpoint = rnd.Next() % AlchemistMixNoLuckUserMaxLimitPoint;
            }
            else
            {
                int AlchemistMixDiceDivPoint = 70;
                int maxdice = (int)(luck / AlchemistMixDiceDivPoint + 1);
                maxdice = maxdice > 10 ? 10 : maxdice;
                for (int i = 0; i < maxdice; i++)
                {
                    int AlchemistMixLuckUserBonusPoint = 30;
                    int rndnum = AlchemistMixLuckUserBonusPoint + rnd.Next() % (101 - AlchemistMixLuckUserBonusPoint);
                    if (randpoint < rndnum)
                        randpoint = rndnum;
                }
            }
            int AlchemistMixItemClassPointS = 98;
            int AlchemistMixItemClassPointA = 75;
            int AlchemistMixItemClassPointB = 40;
            if (randpoint < AlchemistMixItemClassPointS)
            {
                if (randpoint < AlchemistMixItemClassPointA)
                {
                    if (randpoint < AlchemistMixItemClassPointB)
                        ItemClass = 400;
                    else
                        ItemClass = 300;
                }
                else
                {
                    ItemClass = 200;
                }
            }
            else
            {
                ItemClass = 100;
            }
            return ItemClass;
        }

        private static bool AlchemistMix(Account User, int CouponConsume, int RecipeItemNum, int UserLuck, int ItemClass, int AlchemistMethod, out int resultitem, out List<ItemAttr> Attrs)
        {
            resultitem = 0;
            Attrs = new List<ItemAttr>();
            int tr = 0;
            int cash = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_alchemist_mix";
                    cmd.Parameters.Add("pUsernum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("pRecipeItemNum", MySqlDbType.Int32).Value = RecipeItemNum;
                    cmd.Parameters.Add("pAlchemistSetItemNum", MySqlDbType.Int32).Value = 0;
                    cmd.Parameters.Add("pUserLuck", MySqlDbType.Int32).Value = UserLuck;
                    cmd.Parameters.Add("pItemClass", MySqlDbType.Int32).Value = ItemClass;
                    cmd.Parameters.Add("pAlchemistMethod", MySqlDbType.Int16).Value = AlchemistMethod;
                    cmd.Parameters.Add("pCouponConsume", MySqlDbType.Int16).Value = CouponConsume;
                    cmd.Parameters.Add("pCardConsume", MySqlDbType.Int32).Value = 1;
                    cmd.Parameters.Add("pCheckHasItem", MySqlDbType.Int32).Value = 1;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                resultitem = Convert.ToInt32(reader["ResultItemDescNum"]);
                                ItemAttr attr = new ItemAttr
                                {
                                    Attr = Convert.ToUInt16(reader["AttrType"]),
                                    AttrValue = Convert.ToSingle(reader["AttrValue"])
                                };
                                tr = Convert.ToInt32(reader["cost"]);
                                cash = Convert.ToInt32(reader["cash"]);
                                Attrs.Add(attr);
                            }
                            User.TR -= tr;
                            User.Cash -= cash;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private static bool AlchemistUpgrade(int Usernum, int ItemNum, int RecipeItemNum, int UserLuck, int ItemClass, int AlchemistMethod, out int resultitem, out List<ItemAttr> Attrs)
        {
            resultitem = 0;
            Attrs = new List<ItemAttr>();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_alchemist_enchantGrade";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = Usernum;
                    cmd.Parameters.Add("enchantReqItemNum", MySqlDbType.Int32).Value = ItemNum;
                    cmd.Parameters.Add("userLuck", MySqlDbType.Int32).Value = UserLuck;
                    cmd.Parameters.Add("itemClass", MySqlDbType.Int32).Value = ItemClass;
                    cmd.Parameters.Add("pRecipeItemNum", MySqlDbType.Int16).Value = RecipeItemNum;
                    cmd.Parameters.Add("pAlchemistMethod", MySqlDbType.Int16).Value = AlchemistMethod;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                resultitem = Convert.ToInt32(reader["ResultItemDescNum"]);
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
        private static bool MyRoomGiveUserMyRoomSlot(Account User, int itemnum, out int SlotNum)
        {
            SlotNum = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_myRoom_GiveUserMyRoomSlot";
                    cmd.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("pMyRoomSlotItemNum", MySqlDbType.Int32).Value = itemnum;
                    cmd.Parameters.Add("pMyRoomSlotMethod", MySqlDbType.Int32).Value = 2;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            SlotNum = Convert.ToInt32(reader["fdSlotNum"]);
                            User.TR -= Convert.ToInt32(reader["TRPrice"]);
                            User.Cash -= Convert.ToInt32(reader["CashPrice"]);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
