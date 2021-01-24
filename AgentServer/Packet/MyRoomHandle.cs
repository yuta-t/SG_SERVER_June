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
using LocalCommons.Utilities;
using System.Threading.Tasks;
using AgentServer.Structuring.Item;
using System.Collections.Generic;
using LocalCommons.Logging;
using System.Collections.Concurrent;

namespace AgentServer.Packet
{
    public class MyRoomHandle
    {
        public static void Handle_MyroomGetCharacterAvatarItem(ClientConnection Client, PacketReader reader, byte last)
        {
            //PacketReader preader = new PacketReader(Packet, 0);
            /*reader.ReadByte(); //op code 0xFF
            reader.ReadLEInt16(); //op code 0x141*/
            Account User = Client.CurrentAccount;
            int charid = reader.ReadLEInt16();
            int position = reader.ReadLEInt32();
            //GetCharacterAvatarItem(last, charid, position);
            //Console.WriteLine("GetCharacterAvatarItem CharID: {0}", charid);
            Client.SendAsync(new MyroomGetCharacterAvatarItem_0X142(User, charid, last));
        }
        public static void Handle_MyroomGetAllItem(ClientConnection Client, PacketReader reader,byte last)
        {
            Account User = Client.CurrentAccount;
            //Console.WriteLine("MyRoomHandle_Handle_MyroomGetAllItem_6400");
            getActiveFuncItem(User);
            //Client.SendAsync(new MyroomGetAllItem_0X67_64(Client.CurrentAccount, last));
            Client.SendAsync(new MyroomGetAllItem(User, last));
        }
        public static void Handle_MyRoomGetCharacterList(ClientConnection Client, byte last)
        {
            Client.SendAsync(new MyRoomGetCharacterList_0X1D0_03(Client.CurrentAccount, last));
        }
        public static void Handle_MyRoomGetMyCards(ClientConnection Client, byte last)
        {
            Client.SendAsync(new MyRoomGetMyCards_0X1D0_06(Client.CurrentAccount, last));
        }

        public static void Handle_MyRoomSetDefaultCharacter(ClientConnection Client, PacketReader reader, byte last)
        {
            /*preader.ReadByte(); //op code 0xFF
            preader.ReadLEInt16(); //op code 0x1CF*/
            reader.ReadByte(); //sub op code 0x08
            int charid = reader.ReadLEInt16();
            Account User = Client.CurrentAccount;
            Client.SendAsync(new MyRoomSetDefaultCharacter_0X1D0_09(User, charid, last));

        }

        public static void Handle_MyroomSetCharSetting(ClientConnection Client, PacketReader preader, byte last)
        {
            /*preader.ReadByte(); //0xFF
            preader.ReadLEInt16(); //op code 0x1CF*/
            Account User = Client.CurrentAccount;
            preader.ReadByte(); //sub op code 0x0B
            preader.ReadLEInt64(); //time
            int charid = preader.ReadLEUInt16();
            int head = preader.ReadLEUInt16();
            int topbody = preader.ReadLEUInt16();
            int downbody = preader.ReadLEUInt16();
            int foot = preader.ReadLEUInt16();
            int acHead = preader.ReadLEUInt16();
            int acHand = preader.ReadLEUInt16();
            int acFace = preader.ReadLEUInt16();
            int acBack = preader.ReadLEUInt16();
            int acNeck = preader.ReadLEUInt16();
            int pet = preader.ReadLEUInt16();
            int expansion = preader.ReadLEUInt16();
            int acWrist = preader.ReadLEUInt16();
            int acBooster = preader.ReadLEUInt16();
            int acTail = preader.ReadLEUInt16();
            preader.Seek(178, SeekOrigin.Begin);
            int cos_charid = preader.ReadLEUInt16();
            int cos_head = preader.ReadLEUInt16();
            int cos_topbody = preader.ReadLEUInt16();
            int cos_downbody = preader.ReadLEUInt16();
            int cos_foot = preader.ReadLEUInt16();
            int cos_acHead = preader.ReadLEUInt16();
            int cos_acHand = preader.ReadLEUInt16();
            int cos_acFace = preader.ReadLEUInt16();
            int cos_acBack = preader.ReadLEUInt16();
            int cos_acNeck = preader.ReadLEUInt16();
            int cos_pet = preader.ReadLEUInt16();
            int cos_expansion = preader.ReadLEUInt16();
            int cos_acWrist = preader.ReadLEUInt16();
            int cos_acBooster = preader.ReadLEUInt16();
            int cos_acTail = preader.ReadLEUInt16();
            preader.Seek(344, SeekOrigin.Begin);
            byte costumeMode = preader.ReadByte(); //old?
            preader.Offset += 3;
            if (User.isFashionModeOn)
                costumeMode = preader.ReadByte(); //new?

            if (!User.isFashionModeOn)
            {
                bool SetCharSettingDone = MyRoomSetCharacterSetting(User, charid, head, topbody, downbody, foot, acHead, acFace, acHand, acBack, acNeck, pet, expansion, acWrist, acBooster, acTail);
                bool SetCostumeCharSettingDone = MyRoomSetCostumeCharacterSetting(User, cos_charid, cos_head, cos_topbody, cos_downbody, cos_foot, cos_acHead, cos_acFace, cos_acHand, cos_acBack, cos_acNeck, cos_pet, cos_expansion, cos_acWrist, cos_acBooster, cos_acTail);
                bool SetCostumeModeSettingDone = MyRoomSetCostumeModeSetting(User, costumeMode);

                if (SetCharSettingDone && SetCostumeCharSettingDone && SetCostumeModeSettingDone)
                {
                    Client.SendAsync(new MyroomSetCharSettingOK_0X1D0_12(User, last));
                }
            }
            else
            {
                bool SetFashionCharSettingDone = MyRoomSetFashionCharacterSetting(User, charid, head, topbody, downbody, foot, acHead, acFace, acHand, acBack, acNeck, pet, expansion, acWrist, acBooster, acTail);
                if (SetFashionCharSettingDone)
                {
                    Client.SendAsync(new MyroomSetCharSettingOK_0X1D0_12(User, last));
                }
            }


        }

        public static void Handle_MyroomGetFashionMode(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF A6 05 01 40
            Account User = Client.CurrentAccount;
            //LoginHandle.getCurrentAvatarInfo(User);
            //Console.WriteLine("MyroomGetFashionMode");
            //Client.SendAsync(new NP_Hex(User, "FFA705000000000000000000000000000080"));
            Client.SendAsync(new MyroomGetFashionMode(User, last));
        }
        public static void Handle_MyroomSetFashionMode(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF A4 05 01 40
            Account User = Client.CurrentAccount;
            bool fsahionModeOn = reader.ReadBoolean();
            User.isFashionModeOn = fsahionModeOn;
            if(MyRoomSetFashionModeSetting(User, fsahionModeOn))
            {
                Client.SendAsync(new MyroomSetFashionMode(User, last));
                if (User.InGame)
                {
                    NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
                    room.BroadcastToAll(new MyroomSetFashionMode_GameRoom(User, last));
                }
            }
        }

        public static void Handle_ExitGetCurrentCharSetting(ClientConnection Client, byte last)
        {
            Account User = Client.CurrentAccount;
            //Client.SendAsync(new GetCurrentAvatarInfo_0X6D(User, last));
            LoginHandle.updateCurrentAvatarInfo(User);
            Client.SendAsync(new GetCurrentAvatarInfo(User, last));
            Client.SendAsync(new MyroomExitGetCurrentCharSetting_0X1D0_15(User, last));
            if (User.InGame)
            {
                NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
                room.BroadcastToAll(new MyroomExitGetCurrentCharSetting_GameRoom(User, last));
            }
            User.AllItemAttr.Clear();
            UpdateSetItemAttrInfo(User);
            UpdateUserTotalAttr(User);
            UpdateUserLuck(User);
            UpdateAttackInfo(User);
        }

        public static void Handle_ItemMsgPop(ClientConnection Client, PacketReader reader, byte last)
        {
            //5B 03 00 00 00 40
            // Console.WriteLine("itemMsgPop");
            Account User = Client.CurrentAccount;
            int itemtype = reader.ReadLEInt32();
            //Console.WriteLine(Utility.ByteArrayToString(reader.Buffer));
            //Client.SendAsync(new NP_Hex(User, "5C00000000010000000000000000000000C16B000060362A150000000000000000C2A9000060E0CB1780"));
            if(itemtype == 0)
            {
                Client.SendAsync(new ExpiredItemMsgPop(User, itemtype, last));
            }
        }

        public static void Handle_RepairItem(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF CF 01 29 68 91 00 00 C4 02 00 00 80
            Account User = Client.CurrentAccount;
            reader.ReadByte(); //sub op code 0x29
            int itemnum = reader.ReadLEInt32();
            int RepairItemNum = reader.ReadLEInt32();
            byte ret = RepairItem(User, itemnum, RepairItemNum);

            if (ret == 0)
            {
                Client.SendAsync(new Myroom_RepairItemOK(User, itemnum, RepairItemNum, last));
                Client.SendAsync(new Myroom_ActiveFuncItemOne(User, itemnum, last));
            }
        }
        public static void Handle_PetRebirth(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF CF 01 23 FF FF FF FF 2E 52 00 00 08
            Account User = Client.CurrentAccount;
            reader.ReadByte(); //sub op code
            int petitemnum = reader.ReadLEInt32();
            int petRebirthItemNum = reader.ReadLEInt32();
            Client.SendAsync(new Myroom_PetRebirth(User, petitemnum, petRebirthItemNum, last));
        }
        public static void Handle_FeedPet(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            reader.ReadByte(); //sub op code
            int feeditemnum = reader.ReadLEInt32();
            int petItemNum = reader.ReadLEInt32();
            int feedcount = reader.ReadLEInt32();
            if (feedcount < 1)
                return;

            int addexp = 0;
            int addday = 0;
            ItemHolder.ItemAttrCollections.TryGetValue(feeditemnum, out List<ItemAttr> feedinfo);
            //60 = type
            //35 = exp
            //36 = date
            ItemAttr typeattr = feedinfo.FirstOrDefault(f => f.Attr == 60);
            ItemAttr expattr = feedinfo.FirstOrDefault(f => f.Attr == 35);
            ItemAttr dayattr = feedinfo.FirstOrDefault(f => f.Attr == 36);
            if (expattr != null)
                addexp = (int)expattr.AttrValue;
            if (dayattr != null)
                addday = (int)dayattr.AttrValue;
            if ((addexp == 0 && addday == 0) || typeattr == null)
                return;
            if (typeattr.AttrValue == 1)
            {
                ////FF CF 01 20 52 5E 00 00 C2 14 00 00 02 00 00 00 80
                byte ret = FeedPet(User, petItemNum, feeditemnum, addday, addexp, feedcount);

                if (ret == 0)
                    Client.SendAsync(new Myroom_FeedPetOK(User, petItemNum, feeditemnum, last));
            }
            else
            {
                //FF CF 01 20 51 64 00 00 00 00 00 00 03 00 00 00 20
                byte ret = FeedAllPet(User, feeditemnum, addday, addexp, feedcount);

                if (ret == 0)
                    Client.SendAsync(new Myroom_FeedPetOK(User, petItemNum, feeditemnum, last));
            }
        }
        public static void Handle_PetUpgrade(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF CF 01 26 C2 14 00 00 08
            Account User = Client.CurrentAccount;
            reader.ReadByte(); //sub op code
            int petItemNum = reader.ReadLEInt32();
            ItemHolder.ItemCPKInfos.TryGetValue(petItemNum, out ItemCPK petcpk);
            int level = petcpk.ItemKind % 10;
            if (level <= 0 || level >= 3)  //wrong level
                return;

            int maxexp;
            if (!ItemHolder.PetMaxEXP.TryGetValue(petcpk.ItemKind, out maxexp))
            {
                if (level == 1)
                    maxexp = 50000;
                else if (level == 2)
                    maxexp = 100000;
                else if (level == 3)
                    maxexp = 1000000;
            }

            byte ret = PetUpgrade(User, petItemNum, maxexp);

            if (ret == 0)
                Client.SendAsync(new Myroom_PetUpgradeOK(User, last));
        }

        public static void Handle_ActiveFuncItemOne(ClientConnection Client, PacketReader reader, byte last)
        {
            //66 01 00 00 00 0D 29 00 00 10
            Account User = Client.CurrentAccount;
            int count = reader.ReadLEInt32(); //count??
            int itemnum = reader.ReadLEInt32();
            Client.SendAsync(new Myroom_ActiveFuncItemOne(User, itemnum, last));
        }

        public static void Handle_FFCF0100(ClientConnection Client, byte last)
        {
            Client.SendAsync(new Myroom_FFCF0100(Client.CurrentAccount, last));
        }

        public static void Handle_FFCF014A(ClientConnection Client, byte last)
        {
            Client.SendAsync(new NP_Hex(Client.CurrentAccount, "FFD0014B000000000000000001"));
        }

        public static void Handle_GetGiftList(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 72 01 01 00 08 00 20
            Account User = Client.CurrentAccount;
            short startindex = reader.ReadLEInt16();
            short lastindex = reader.ReadLEInt16();
            Client.SendAsync(new Myroom_GetGiftList(User, startindex, lastindex, last));

            //gift item pop
            /*5D 00 00 00 00 04 00 00 00 03 00 00 00 00 00 00 00 
                 48 A1 00 00 00 00 00 00 00 00 00 00 00 00 00 00
                 B2 A9 00 00 D0 2B 4D 18 03 00 00 00 00 00 00 00 
                 48 A1 00 00 00 00 00 00 00 00 00 00 00 00 00 00 
                 B2 A9 00 00 D0 2B 4D 18 03 00 00 00 00 00 00 00 
                 48 A1 00 00 00 00 00 00 00 00 00 00 00 00 00 00 
                 B2 A9 00 00 D0 2B 4D 18 03 00 00 00 00 00 00 00 
                 48 A1 00 00 00 00 00 00 00 00 00 00 00 00 00 00 
                 B2 A9 00 00 D0 2B 4D 18 40*/
            //5D 00 00 00 00 00 00 00 00 01
        }
        public static void Handle_AcceptGift(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 74 01 08 34 CA 06 01
            Account User = Client.CurrentAccount;
            int giftnum = reader.ReadLEInt32();

            byte ret = AcceptGiftCheck(User, giftnum, out int itemid);

            if (ret == 0 && itemid != 0)
            {
                Client.SendAsync(new Myroom_AcceptGiftOK(User, itemid, last));
            }
            else
            {
                Client.SendAsync(new Myroom_AcceptGiftFail(last));
            }
            //Client.SendAsync(new Myroom_GetGiftList(User, startindex, lastindex, last));

        }
        public static void Handle_GetGiftItemInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            // FF 44 01 45 96 00 00 02
            Account User = Client.CurrentAccount;
            int realitemid = reader.ReadLEInt32();
            string itemnum = string.Format("{0},", realitemid);

            GetGiftItemInfo(User.UserNum, itemnum, out var iteminfos);
            Client.SendAsync(new Myroom_GiftItemInfo_ACK(User, iteminfos, last));
            foreach (var item in iteminfos)
            {
                //User.AvatarItems.TryGetValue(item.itemdescnum, out var uitem);
                if (User.AvatarItems.ContainsKey(item.itemdescnum))
                {
                    if (item.count > 0)
                    {
                        User.AvatarItems[item.itemdescnum].UpdateItemInfo(item.count, item.expireTime, item.gotDateTime);
                    }
                    else
                    {
                        User.AvatarItems.TryRemove(item.itemdescnum, out _);
                    }
                }
                else
                {
                    User.AvatarItems.TryAdd(item.itemdescnum, item);
                }
            }
        }

        public static void Handle_MyroomGetUserItemAttr(ClientConnection Client, PacketReader reader, byte last)
        {
            //68 01 20
            //Console.WriteLine(Utility.ByteArrayToString(reader.Buffer));
            Account User = Client.CurrentAccount;
            //ExpiredItemCheck(User);
            //LoginHandle.updateCurrentAvatarInfo(User);
            Client.SendAsync(new MyroomGetUserItemAttr(User, last));
            if (User.InGame)
            {
                NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
                room.BroadcastToAll(new MyroomGetUserItemAttr_GameRoom(User, last));
            }
        }

        public static void Handle_GetStorageItemList(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 49 05 01 00 00 00 08 gift
            //FF 49 05 00 00 00 00 20 keep
            Account User = Client.CurrentAccount;
            int type = reader.ReadLEInt32();
            if (type == 0)
                Client.SendAsync(new Myroom_GetStorageKeepingList(User, last));
            else if (type == 1)
                Client.SendAsync(new Myroom_GetStorageGiftList(User, last));
        }
        public static void Handle_StorageItemGift(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 4B 05 07 00 00 00 70 6F 70 6F 36 37 38 06 00 00 00 31 32 33 34 35 68 86 41 09 00 00 00 00 00 08
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            int msglen = reader.ReadLEInt32();
            string msg = string.Empty;
            if(msglen > 0)
                msg = reader.ReadBig5StringSafe(msglen);
            long UniqueNum = reader.ReadLEInt64();
            if(User.NickName == nickname)
            {
                Client.SendAsync(new Myroom_StorageGiftACK(User, 4, UniqueNum, nickname, last));
                return;
            }

            byte ret = StorageGift(User, nickname, msg, UniqueNum);
            Client.SendAsync(new Myroom_StorageGiftACK(User, ret, UniqueNum, nickname, last));
            //66 01 00 00 00 0D 29 00 00 20
            //67 00 00 00 00 66 00 00 00 00 00 00 00 20
        }
        public static void Handle_StorageItemReceive(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 4D 05 00 00 00 00 26 02 09 00 00 00 00 00 10
            Account User = Client.CurrentAccount;
            int type = reader.ReadLEInt32();
            long UniqueNum = reader.ReadLEInt64();
            byte ret = StorageReceive(User, type, UniqueNum);
            if (ret == 0)
                Client.SendAsync(new Myroom_StorageReceiveOK(User, type, UniqueNum, last));
        }

        public static void Handle_ItemOnOff(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            int itemnum = reader.ReadLEInt32();
            bool On = reader.ReadBoolean();
            //Console.WriteLine("ITEM: {0},{1}", itemnum, On);
            if (On)
            {
                if (ItemOn(User, itemnum, out int OnOffType, out int Position))
                {
                    Client.SendAsync(new Myroom_ItemOn(User, itemnum, OnOffType, Position, last));
                    AvatarItemInfo offitem = null, onitem = null;
                    if (OnOffType == 2)
                    {
                        offitem = User.AvatarItems.Values.FirstOrDefault(f => f.position == Position && f.use == true);
                    }
                    User.AvatarItems.TryGetValue(itemnum, out onitem);
                    if (User.InGame && User.CurrentRoomId != 0)
                    {
                        NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
                        room.BroadcastToAll(new Myroom_ItemOn_GameRoom(User.RoomPos, itemnum, OnOffType, Position, offitem, onitem, last));
                    }
                    if (offitem != null)
                        offitem.use = false;
                    if (onitem != null)
                        onitem.use = true;
                }
            }
            else
            {
                if (ItemOff(User, itemnum, out int OnOffType, out int Position))
                {
                    Client.SendAsync(new Myroom_ItemOff(User, itemnum, OnOffType, Position, last));
                    AvatarItemInfo offitem = null;
                    User.AvatarItems.TryGetValue(itemnum, out offitem);
                    if (User.InGame && User.CurrentRoomId != 0)
                    {
                        NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
                        room.BroadcastToAll(new Myroom_ItemOff_GameRoom(User.RoomPos, itemnum, OnOffType, Position, offitem, last));
                    }
                    offitem.use = false;
                }
            }
            //User.AvatarItems.FirstOrDefault(f => f.itemdescnum == itemnum).use = On;
        }

        public static void Handle_UseLuckyBag(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF CF 01 1A F4 B7 00 00 01
            Account User = Client.CurrentAccount;
            reader.Offset += 1; //sub op code
            int itemnum = reader.ReadLEInt32();
            if (UseLuckyBag(User.UserNum, itemnum, out var itemlist))
            {
                Client.SendAsync(new Myroom_UseLuckyBagOK(User, itemnum, itemlist, last));
            }
            else
            {
                Client.SendAsync(new Myroom_UseLuckyBagFail(itemnum, last));
            }
        }

        public static void Handle_SetSlotItemSetting(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF CF 01 42 01 00 00 00 A5 09 00 00 00 00 00 00
            //00 00 00 00 00 00 00 04 00 00 00 31 34 35 33 20
            Account User = Client.CurrentAccount;
            reader.Offset +=1; //sub op code
            ushort charid = reader.ReadLEUInt16();
            ushort head = reader.ReadLEUInt16();
            ushort topbody = reader.ReadLEUInt16();
            ushort downbody = reader.ReadLEUInt16();
            ushort foot = reader.ReadLEUInt16();
            ushort acHead = reader.ReadLEUInt16();
            ushort acHand = reader.ReadLEUInt16();
            ushort acFace = reader.ReadLEUInt16();
            ushort acBack = reader.ReadLEUInt16();
            ushort acNeck = reader.ReadLEUInt16();
            ushort pet = reader.ReadLEUInt16();
            ushort expansion = reader.ReadLEUInt16();
            ushort acWrist = reader.ReadLEUInt16();
            ushort acBooster = reader.ReadLEUInt16();
            ushort acTail = reader.ReadLEUInt16();
            reader.Offset += 0x88;
            int slotnum = reader.ReadLEInt32();
            int len = reader.ReadLEInt32();
            string slotname = string.Empty;
            slotname = reader.ReadBig5StringSafe(len);

            bool MyRoomSetSlotItemSettingOK = MyRoomSetSlotItemSetting(User.UserNum, slotnum, slotname, charid, head, topbody, downbody, foot, acHead, acFace, acHand, acBack, acNeck, pet, expansion, acWrist, acBooster, acTail);
            if (MyRoomSetSlotItemSettingOK)
                Client.SendAsync(new Myroom_SetSlotItemSettingOK(slotnum, slotname, charid, head, topbody, downbody, foot, acHead, acFace, acHand, acBack, acNeck, pet, expansion, acWrist, acBooster, acTail, last));

        }
        public static void Handle_GetUserSlotInfo(ClientConnection Client, byte last)
        {
            Account User = Client.CurrentAccount;
            MyRoomGetUserSlotInfo(User.UserNum, out List<UserSlotItemKindInfo> slotinfos);
            Client.SendAsync(new Myroom_GetSlotInfoOK(slotinfos, last));
        }

        private static bool MyRoomSetCharacterSetting(Account User, int charid, int head, int topbody, int downbody, int foot, int acHead, int acFace, int acHand, int acBack, int acNeck, int pet, int expansion, int acWrist, int acBooster, int acTail)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_myRoomSetCharacterSetting";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("pcharacter", MySqlDbType.Int32).Value = charid;
                cmd.Parameters.Add("head", MySqlDbType.Int32).Value = head;
                cmd.Parameters.Add("topbody", MySqlDbType.Int32).Value = topbody;
                cmd.Parameters.Add("downbody", MySqlDbType.Int32).Value = downbody;
                cmd.Parameters.Add("foot", MySqlDbType.Int32).Value = foot;
                cmd.Parameters.Add("acHead", MySqlDbType.Int32).Value = acHead;
                cmd.Parameters.Add("acFace", MySqlDbType.Int32).Value = acFace;
                cmd.Parameters.Add("acHand", MySqlDbType.Int32).Value = acHand;
                cmd.Parameters.Add("acBack", MySqlDbType.Int32).Value = acBack;
                cmd.Parameters.Add("acNeck", MySqlDbType.Int32).Value = acNeck;
                cmd.Parameters.Add("pet", MySqlDbType.Int32).Value = pet;
                cmd.Parameters.Add("expansion", MySqlDbType.Int32).Value = expansion;
                cmd.Parameters.Add("acWrist", MySqlDbType.Int32).Value = acWrist;
                cmd.Parameters.Add("acBooster", MySqlDbType.Int32).Value = acBooster;
                cmd.Parameters.Add("acTail", MySqlDbType.Int32).Value = acTail;
                MySqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            return true;
        }
        private static bool MyRoomSetCostumeCharacterSetting(Account User, int charid, int head, int topbody, int downbody, int foot, int acHead, int acFace, int acHand, int acBack, int acNeck, int pet, int expansion, int acWrist, int acBooster, int acTail)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_myRoomSetCostumeCharacterSetting";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("pcharacter", MySqlDbType.Int32).Value = charid;
                cmd.Parameters.Add("head", MySqlDbType.Int32).Value = head;
                cmd.Parameters.Add("topbody", MySqlDbType.Int32).Value = topbody;
                cmd.Parameters.Add("downbody", MySqlDbType.Int32).Value = downbody;
                cmd.Parameters.Add("foot", MySqlDbType.Int32).Value = foot;
                cmd.Parameters.Add("acHead", MySqlDbType.Int32).Value = acHead;
                cmd.Parameters.Add("acFace", MySqlDbType.Int32).Value = acFace;
                cmd.Parameters.Add("acHand", MySqlDbType.Int32).Value = acHand;
                cmd.Parameters.Add("acBack", MySqlDbType.Int32).Value = acBack;
                cmd.Parameters.Add("acNeck", MySqlDbType.Int32).Value = acNeck;
                cmd.Parameters.Add("pet", MySqlDbType.Int32).Value = pet;
                cmd.Parameters.Add("expansion", MySqlDbType.Int32).Value = expansion;
                cmd.Parameters.Add("acWrist", MySqlDbType.Int32).Value = acWrist;
                cmd.Parameters.Add("acBooster", MySqlDbType.Int32).Value = acBooster;
                cmd.Parameters.Add("acTail", MySqlDbType.Int32).Value = acTail;
                MySqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            return true;
        }
        private static bool MyRoomSetCostumeModeSetting(Account User, byte costumeMode)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_myRoomSetCostumeModeSetting";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("costumeMode", MySqlDbType.Int16).Value = costumeMode;
                MySqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            return true;
        }
        private static bool MyRoomSetFashionCharacterSetting(Account User, int charid, int head, int topbody, int downbody, int foot, int acHead, int acFace, int acHand, int acBack, int acNeck, int pet, int expansion, int acWrist, int acBooster, int acTail)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_myRoomSetFashionCharacterSetting";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("pcharacter", MySqlDbType.Int32).Value = charid;
                cmd.Parameters.Add("head", MySqlDbType.Int32).Value = head;
                cmd.Parameters.Add("topbody", MySqlDbType.Int32).Value = topbody;
                cmd.Parameters.Add("downbody", MySqlDbType.Int32).Value = downbody;
                cmd.Parameters.Add("foot", MySqlDbType.Int32).Value = foot;
                cmd.Parameters.Add("acHead", MySqlDbType.Int32).Value = acHead;
                cmd.Parameters.Add("acFace", MySqlDbType.Int32).Value = acFace;
                cmd.Parameters.Add("acHand", MySqlDbType.Int32).Value = acHand;
                cmd.Parameters.Add("acBack", MySqlDbType.Int32).Value = acBack;
                cmd.Parameters.Add("acNeck", MySqlDbType.Int32).Value = acNeck;
                cmd.Parameters.Add("pet", MySqlDbType.Int32).Value = pet;
                cmd.Parameters.Add("expansion", MySqlDbType.Int32).Value = expansion;
                cmd.Parameters.Add("acWrist", MySqlDbType.Int32).Value = acWrist;
                cmd.Parameters.Add("acBooster", MySqlDbType.Int32).Value = acBooster;
                cmd.Parameters.Add("acTail", MySqlDbType.Int32).Value = acTail;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                //reader.Close();
                con.Close();
            }
            return true;
        }
        private static bool MyRoomSetFashionModeSetting(Account User, bool fashionMode)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_myRoomSetFashionModeSetting";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("fashionMode", MySqlDbType.Int16).Value = Convert.ToInt16(fashionMode);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                //reader.Close();
                con.Close();
            }
            return true;
        }

        private static bool MyRoomSetSlotItemSetting(int UserNum, int slotNum, string SlotName, int charid, int head, int topbody, int downbody, int foot, int acHead, int acFace, int acHand, int acBack, int acNeck, int pet, int expansion, int acWrist, int acBooster, int acTail)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_myRoomSetSlotItemSetting";
                    cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = UserNum;
                    cmd.Parameters.Add("slotNum", MySqlDbType.Int32).Value = slotNum;
                    cmd.Parameters.Add("pSlotName", MySqlDbType.VarString).Value = SlotName;
                    cmd.Parameters.Add("charact", MySqlDbType.Int32).Value = charid;
                    cmd.Parameters.Add("head", MySqlDbType.Int32).Value = head;
                    cmd.Parameters.Add("topbody", MySqlDbType.Int32).Value = topbody;
                    cmd.Parameters.Add("downbody", MySqlDbType.Int32).Value = downbody;
                    cmd.Parameters.Add("foot", MySqlDbType.Int32).Value = foot;
                    cmd.Parameters.Add("acHead", MySqlDbType.Int32).Value = acHead;
                    cmd.Parameters.Add("acFace", MySqlDbType.Int32).Value = acFace;
                    cmd.Parameters.Add("acHand", MySqlDbType.Int32).Value = acHand;
                    cmd.Parameters.Add("acBack", MySqlDbType.Int32).Value = acBack;
                    cmd.Parameters.Add("acNeck", MySqlDbType.Int32).Value = acNeck;
                    cmd.Parameters.Add("pet", MySqlDbType.Int32).Value = pet;
                    cmd.Parameters.Add("expansion", MySqlDbType.Int32).Value = expansion;
                    cmd.Parameters.Add("acWrist", MySqlDbType.Int32).Value = acWrist;
                    cmd.Parameters.Add("acBooster", MySqlDbType.Int32).Value = acBooster;
                    cmd.Parameters.Add("acTail", MySqlDbType.Int32).Value = acTail;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        reader.Read();
                        if (reader.GetInt32("ret") == 0)
                            return true;
                    }
                }
            }       
            return false;
        }
        private static void MyRoomGetUserSlotInfo(int UserNum, out List<UserSlotItemKindInfo> slotinfos)
        {
            slotinfos = new List<UserSlotItemKindInfo>();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_myRoomGetUserSlotInfo";
                    cmd.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = UserNum;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            UserSlotItemKindInfo slotinfo = new UserSlotItemKindInfo
                            {
                                SlotNum = Convert.ToInt32(reader["fdSlotNum"]),
                                SlotName = reader["fdSlotName"].ToString(),
                                charid = Convert.ToUInt16(reader["character"]),
                                head = Convert.ToUInt16(reader["head"]),
                                topbody = Convert.ToUInt16(reader["topbody"]),
                                downbody = Convert.ToUInt16(reader["downbody"]),
                                foot = Convert.ToUInt16(reader["foot"]),
                                acHead = Convert.ToUInt16(reader["acHead"]),
                                acFace = Convert.ToUInt16(reader["acFace"]),
                                acHand = Convert.ToUInt16(reader["acHand"]),
                                acBack = Convert.ToUInt16(reader["acBack"]),
                                acNeck = Convert.ToUInt16(reader["acNeck"]),
                                pet = Convert.ToUInt16(reader["pet"]),
                                expansion = Convert.ToUInt16(reader["expansion"]),
                                acWrist = Convert.ToUInt16(reader["acWrist"]),
                                acBooster = Convert.ToUInt16(reader["acBooster"]),
                                acTail = Convert.ToUInt16(reader["acTail"])
                            };
                            slotinfos.Add(slotinfo);
                        }
                    }
                }
            }
        }

        private static byte AcceptGiftCheck(Account User, int giftnum, out int itemid)
        {
            itemid = 0;
            byte ret = 0;
            try
            {
                using (var con = new MySqlConnection(Conf.Connstr))
                {
                    con.Open();
                    var cmd = new MySqlCommand(string.Empty, con);
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shopAcceptGift";
                    cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("giftnum", MySqlDbType.Int32).Value = giftnum;
                    MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                    reader.Read();
                    ret = Convert.ToByte(reader["ret"]);
                    itemid = Convert.ToInt32(reader["itemdescnum"]);
                    cmd.Dispose();
                    reader.Close();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error on accept gift:\r\n{0}", ex.Message);
                ret = 1;
            }
            return ret;
        }

        public static void ExpiredItemCheck(Account User)
        {

            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getActiveFuncItem";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("position", MySqlDbType.Int32).Value = 0;
                cmd.Parameters.Add("expiredcheck", MySqlDbType.Int32).Value = 1;
                MySqlDataReader reader = cmd.ExecuteReader();
                //reader.Read();
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }

        public static void UpdateUserTotalAttr(Account User)
        {
            foreach (var itemnum in User.WearAvatarItem)
            {
                if (ItemHolder.ItemAttrCollections.TryGetValue(itemnum, out var attr) && !attr.Exists(a => a.Attr == 5001))
                    attr.ForEach((x) =>
                    {
                        User.AllItemAttr.AddOrUpdate(x.Attr, x.AttrValue, (k, v) => { v += x.AttrValue; return v; });
                    });
                else if (User.UserItemAttr.TryGetValue(itemnum, out var attr2) && !attr2.Exists(a => a.Attr == 5001))
                    attr2.ForEach((x) =>
                    {
                        User.AllItemAttr.AddOrUpdate(x.Attr, x.AttrValue, (k, v) => { v += x.AttrValue; return v; });
                    });
            }
            if (User.costumeMode == 1) //avon
            {
                foreach (var itemnum in User.WearCosAvatarItem)
                {
                    if (ItemHolder.ItemAttrCollections.TryGetValue(itemnum, out var attr) && attr.Exists(a => a.Attr == 5001))
                        attr.ForEach((x) =>
                        {
                            User.AllItemAttr.AddOrUpdate(x.Attr, x.AttrValue, (k, v) => { v += x.AttrValue; return v; });
                        });
                    else if (User.UserItemAttr.TryGetValue(itemnum, out var attr2) && attr2.Exists(a => a.Attr == 5001))
                        attr2.ForEach((x) =>
                        {
                            User.AllItemAttr.AddOrUpdate(x.Attr, x.AttrValue, (k, v) => { v += x.AttrValue; return v; });
                        });
                }

            }
            //User.AllItemAttr.TryGetValue(1, out var tr);
            //Console.WriteLine("tr:{0}", tr*100);
        }
        public static void UpdateUserLuck(Account User)
        {
            float luckpercent = User.AvatarItems.ContainsKey(47635) ? 100 : 0; //優越童話通行証BUFF

            if (User.AllItemAttr.TryGetValue(9, out var totalucky))
                User.Luck = (decimal)totalucky * 100 + (decimal)luckpercent;
            else
                User.Luck = 0 + (decimal)luckpercent;
            //Console.WriteLine("Luck:{0}", User.Luck);
        }
        public static void UpdateAttackInfo(Account User)
        {
            int baseAP = 0;
            int baseHP = 100;
            int baseDP = 0;
            //攻擊力
            if (User.AllItemAttr.TryGetValue(214, out var totalap))
                baseAP += (int)totalap;
            //防禦力
            if (User.AllItemAttr.TryGetValue(215, out var totaldp))
                baseDP += (int)totaldp;
            //生命力
            if (User.AllItemAttr.TryGetValue(216, out var totalhp))
                baseHP += (int)totalhp;
            User.AttackPoint = baseAP;
            User.DefensePoint = baseDP;
            User.HealthPoint = baseHP;
            //Console.WriteLine("Luck:{0}", User.Luck);
        }
        public static void UpdateSetItemAttrInfo(Account User)
        {
            List<int> applygroup = new List<int>();
            //User.WearItemSetAttr.Clear();
            foreach (var item in User.WearAvatarItem)
            {
                var findgroup = ItemHolder.ItemSetDesc.Where(w => w.Value.Find(f => f == item) == item).FirstOrDefault();
                if (findgroup.Key != 0)
                {
                    var setitemlist = findgroup.Value;
                    var isHaveAllSetItem = setitemlist.Intersect(User.WearAvatarItem).Count() == setitemlist.Count;
                    if (isHaveAllSetItem)
                    {
                        var isalreadyadd = applygroup.Exists(e => e == findgroup.Key);
                        if (!isalreadyadd) {
                            ItemHolder.ItemSetAttr.TryGetValue(findgroup.Key, out var setitemattr);
                            if (setitemattr.Exists(e => e.ApplyTarget == 1))
                            {
                                applygroup.Add(findgroup.Key);
                                //User.WearItemSetAttr.AddRange(setitemattr);
                                setitemattr.ForEach((x) =>
                                {
                                    User.AllItemAttr.AddOrUpdate(x.Attr, x.AttrValue, (k, v) => { v += x.AttrValue; return v; });
                                });
                            }
                        }
                    }
                }
            }
            if (User.costumeMode == 1)
            {
                foreach (var item in User.WearCosAvatarItem)
                {
                    var findgroup = ItemHolder.ItemSetDesc.Where(w => w.Value.Find(f => f == item) == item).FirstOrDefault();
                    if (findgroup.Key != 0)
                    {
                        var setitemlist = findgroup.Value;
                        var isHaveAllSetItem = setitemlist.Intersect(User.WearCosAvatarItem).Count() == setitemlist.Count;
                        if (isHaveAllSetItem)
                        {
                            var isalreadyadd = applygroup.Exists(e => e == findgroup.Key);
                            if (!isalreadyadd)
                            {
                                ItemHolder.ItemSetAttr.TryGetValue(findgroup.Key, out var setitemattr);
                                if (setitemattr.Exists(e => e.ApplyTarget == 2))
                                {
                                    applygroup.Add(findgroup.Key);
                                    //User.WearItemSetAttr.AddRange(setitemattr);
                                    setitemattr.ForEach((x) =>
                                    {
                                        User.AllItemAttr.AddOrUpdate(x.Attr, x.AttrValue, (k, v) => { v += x.AttrValue; return v; });
                                    });
                                }
                            }
                        }
                    }
                }
            }
            applygroup = null;
        }

        private static byte StorageReceive(Account User, int type, long UniqueNum)
        {
            byte ret = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_storage_receive";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("type", MySqlDbType.Int32).Value = type;
                cmd.Parameters.Add("uniqueNum", MySqlDbType.Int32).Value = UniqueNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                ret = Convert.ToByte(reader["retval"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            return ret;
        }
        private static byte StorageGift(Account User, string nickname, string msg, long UniqueNum)
        {
            byte ret = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_storage_gift";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("sendNickname", MySqlDbType.VarString).Value = User.NickName;
                    cmd.Parameters.Add("targetNickname", MySqlDbType.VarString).Value = nickname;
                    cmd.Parameters.Add("uniqueNum", MySqlDbType.Int64).Value = UniqueNum;
                    cmd.Parameters.Add("memo", MySqlDbType.VarString).Value = msg;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        reader.Read();
                        ret = Convert.ToByte(reader["retval"]);
                    }
                }
            }
            return ret;
        }

        private static byte RepairItem(Account User, int itemnum, int repairitemnum)
        {
            byte ret = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_alchemist_repairItem";
                    cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("repairTargetItemdescnum", MySqlDbType.Int32).Value = itemnum;
                    cmd.Parameters.Add("repairItemDescNum", MySqlDbType.Int32).Value = repairitemnum;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        reader.Read();
                        ret = Convert.ToByte(reader["retval"]);
                    }
                }
            }
            return ret;
        }

        private static byte FeedPet(Account User, int petitemnum, int feeditemnum, int addDays, int addExp, int feedcount)
        {
            byte ret = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_myRoomPetFeed";
                    cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("petItemDescNum", MySqlDbType.Int32).Value = petitemnum;
                    cmd.Parameters.Add("petFeedItemDescNum", MySqlDbType.Int32).Value = feeditemnum;
                    cmd.Parameters.Add("addDays", MySqlDbType.Int32).Value = addDays;
                    cmd.Parameters.Add("addExp", MySqlDbType.Int32).Value = addExp;
                    cmd.Parameters.Add("feedcount", MySqlDbType.Int32).Value = feedcount;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        reader.Read();
                        ret = Convert.ToByte(reader["retval"]);
                    }
                }
            }
            return ret;
        }
        private static byte FeedAllPet(Account User, int feeditemnum, int addDays, int addExp, int feedcount)
        {
            byte ret = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_myRoomPetFeedToAll";
                    cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("petFeedItemDescNum", MySqlDbType.Int32).Value = feeditemnum;
                    cmd.Parameters.Add("addDays", MySqlDbType.Int32).Value = addDays;
                    cmd.Parameters.Add("addExp", MySqlDbType.Int32).Value = addExp;
                    cmd.Parameters.Add("feedcount", MySqlDbType.Int32).Value = feedcount;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        reader.Read();
                        ret = Convert.ToByte(reader["retval"]);
                    }
                }
            }
            return ret;
        }
        private static byte PetUpgrade(Account User, int petitemnum, int needexp)
        {
            byte ret = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_myRoomPetUpgrade";
                    cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("petitemdescnum", MySqlDbType.Int32).Value = petitemnum;
                    cmd.Parameters.Add("needExp", MySqlDbType.Int32).Value = needexp;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        reader.Read();
                        ret = Convert.ToByte(reader["retval"]);
                    }
                }
            }
            return ret;
        }

        public static void getActiveFuncItem(Account User)
        {
            //HashSet<AvatarItemInfo> AvatarItem = new HashSet<AvatarItemInfo>();
            //ConcurrentBag<AvatarItemInfo> AvatarItem2 = new ConcurrentBag<AvatarItemInfo>();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_getActiveFuncItem";
                    cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("position", MySqlDbType.Int32).Value = -1;
                    cmd.Parameters.Add("expiredcheck", MySqlDbType.Int32).Value = 0;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                int itemdescnum = Convert.ToInt32(reader["itemdescnum"]);
                                AvatarItemInfo item = new AvatarItemInfo
                                {
                                    character = Convert.ToInt16(reader["character"]),
                                    position = Convert.ToUInt16(reader["position"]),
                                    kind = Convert.ToUInt16(reader["kind"]),
                                    itemdescnum = Convert.ToInt32(reader["itemdescnum"]),
                                    expireTime = Convert.IsDBNull(reader["expireTime"]) ? 0L : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["expireTime"])),
                                    gotDateTime = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["gotDateTime"])),
                                    count = Convert.ToInt32(reader["count"]),
                                    exp = Convert.ToInt32(reader["exp"]),
                                    use = Convert.ToBoolean(reader["using"])
                                };
                                //AvatarItem2.Add(item);
                                User.AvatarItems.AddOrUpdate(itemdescnum, item, (k, v) => { return item; });
                            }
                        }
                    }
                }
            }
            //User.AvatarItems.Clear();
            //User.AvatarItems.AddRange(AvatarItem2);
            //AvatarItem2.Clear();
        }

        private static bool ItemOn(Account User, int itemnum, out int OnOffType, out int Position)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                OnOffType = 0;
                Position = 0;
                try
                {
                    con.Open();
                    using (var cmd = new MySqlCommand(string.Empty, con))
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "usp_item_On";
                        cmd.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
                        cmd.Parameters.Add("ItemDescNum", MySqlDbType.Int32).Value = itemnum;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                OnOffType = Convert.ToInt32(reader["OnOffType"]);
                                Position = Convert.ToInt32(reader["Position"]);
                                return true;
                            }
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
        private static bool ItemOff(Account User, int itemnum, out int OnOffType, out int Position)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                OnOffType = 0;
                Position = 0;
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_item_Off";
                    cmd.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("ItemDescNum", MySqlDbType.Int32).Value = itemnum;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            OnOffType = Convert.ToInt32(reader["OnOffType"]);
                            Position = Convert.ToInt32(reader["Position"]);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool GetGiftItemInfo(int Usernum, string itemnums, out ConcurrentBag<AvatarItemInfo> iteminfo)
        {
            iteminfo = new ConcurrentBag<AvatarItemInfo>();
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
                                kind = Convert.ToUInt16(reader["kind"]),
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

        private static bool UseLuckyBag(int Usernum, int itemnum, out ConcurrentBag<int> itemlist)
        {
            itemlist = new ConcurrentBag<int>();
            try
            {
                using (var con = new MySqlConnection(Conf.Connstr))
                {
                    con.Open();
                    using (var cmd = new MySqlCommand(string.Empty, con))
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "usp_myRoomUseLuckyBag_New";
                        cmd.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = Usernum;
                        cmd.Parameters.Add("pItemDescNum", MySqlDbType.Int32).Value = itemnum;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    itemlist.Add(Convert.ToInt32(reader["resultItem"]));
                                }
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Error("Error on using lucky bag:\r\n{0}", ex.Message);
                return false;
            }
        }

    }
}
