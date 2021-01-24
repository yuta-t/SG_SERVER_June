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
using LocalCommons.Logging;
using AgentServer.Structuring.Item;
using System.Collections.Concurrent;

namespace AgentServer.Packet.Send
{

    public sealed class Room_5B_0X5C : NetPacket
    {
        public Room_5B_0X5C(Account User, byte last)
        {
            ns.Write((byte)92); //0x5C  op code
            ns.Write(0L);
            ns.Write(last); //end
        }
    }

    public sealed class ExpiredItemMsgPop : NetPacket
    {
        public ExpiredItemMsgPop(Account User, int itemtype, byte last)
        {
            /*5C 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 
             * C1 6B 00 00 60 36 2A 15 00 00 00 00 00 00 00 00 
             * C2 A9 00 00 60 E0 CB 17 80*/

            ns.Write((byte)0x5C); //0x5C  op code
            ns.Write(0);
            int countpos = (int)ns.Position;
            int count = 0;
            ns.Write(count);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_itemMsgPop";
                    cmd.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("itemMsgType", MySqlDbType.Int32).Value = itemtype;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ns.Write(Convert.ToInt32(reader["fdType"]));
                                ns.Write(Convert.ToInt32(reader["fdSubType"]));
                                ns.Write(Convert.ToInt32(reader["fdItemNum"]));
                                ns.Fill(0xC);
                                ns.Write(0xA9C2);
                                ns.Write(0);
                                count++;
                            }
                        }
                    }
                }
            }
            ns.Write(last); //end
            ns.Seek(countpos, SeekOrigin.Begin);
            ns.Write(count);
        }
    }

    public sealed class MyroomGetCharacterAvatarItem_0X142 : NetPacket
    {
        public MyroomGetCharacterAvatarItem_0X142(Account User, int charid, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)322); //0x142  op code
            ns.Write((short)charid); //character
            ns.Write(-1); //FF FF FF FF position
            ns.Fill(3);
            int countpos = (int)ns.Position;
            ns.Write((short)0); //count
            short count = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getCharacterAvatarItem";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("pcharacter", MySqlDbType.Int32).Value = charid;
                cmd.Parameters.Add("position", MySqlDbType.Int32).Value = -1;
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ns.Write(0x9CC1D0); //D0 C1 9C 00
                    ns.Write(Convert.ToInt16(reader["character"]));
                    ns.Write(Convert.ToUInt16(reader["position"]));
                    ns.Write(Convert.ToUInt16(reader["kind"]));
                    ns.Write(Convert.ToInt32(reader["itemdescnum"])); //4 bytes
                    ns.Write(Convert.IsDBNull(reader["expireTime"]) ? 0L : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["expireTime"])));
                    ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["gotDateTime"])));
                    ns.Write(Convert.ToInt32(reader["count"])); //4 bytes
                    ns.Write(Convert.ToInt32(reader["exp"]));
                    ns.Write((byte)0);
                    ns.Write(Convert.ToBoolean(reader["using"]));
                    count++;
                }

                //ns.Write((short)0);//  daily buff count?
                ns.Write(last);  //end
                ns.Seek(countpos, SeekOrigin.Begin);
                ns.Write(count);

                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
    }

    public sealed class MyroomGetAllItem_0X67_64 : NetPacket
    {
        public MyroomGetAllItem_0X67_64(Account User, byte last)
        {
            ns.Write((byte)103); //0x67  op code
            ns.Write(0);
            ns.Write((byte)0x64); //0x64 sub op code?
            ns.Fill(6);
            int countpos = (int)ns.Position;
            ns.Write((short)0); //count
            short count = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getActiveFuncItem";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("position", MySqlDbType.Int32).Value = -1;
                cmd.Parameters.Add("expiredcheck", MySqlDbType.Int32).Value = 0;
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ns.Write(0x9CC1D0); //D0 C1 9C 00
                    ns.Write(Convert.ToInt16(reader["character"]));
                    ns.Write(Convert.ToUInt16(reader["position"]));
                    ns.Write(Convert.ToUInt16(reader["kind"]));
                    ns.Write(Convert.ToInt32(reader["itemdescnum"])); //4 bytes
                    ns.Write(Convert.IsDBNull(reader["expireTime"]) ? 0L : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["expireTime"])));
                    ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["gotDateTime"])));
                    ns.Write(Convert.ToInt32(reader["count"])); //4 bytes
                    ns.Write(Convert.ToInt32(reader["exp"]));
                    ns.Write((byte)0);
                    ns.Write(Convert.ToBoolean(reader["using"]));
                    //ns.Write((short)0x100); //unknown?? 00 01 / 01 00 / 01 01 / 00 00
                    count++;
                }
                ns.Write((short)0);//  daily buff count?
                ns.Write(last);  //end
                ns.Seek(countpos, SeekOrigin.Begin);
                ns.Write(count);

                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
    }

    public sealed class MyroomGetAllItem : NetPacket
    {
        public MyroomGetAllItem(Account User, byte last)
        {
            ns.Write((byte)0x67); //0x67  op code
            ns.Write(0);
            ns.Write((byte)0x64); //0x64 sub op code?
            ns.Fill(6);
            ns.Write((short)User.AvatarItems.Count); //count
            foreach (var item in User.AvatarItems.Values)
            {
                ns.Write(0x9CC1D0); //D0 C1 9C 00
                ns.Write(item.character);
                ns.Write(item.position);
                ns.Write(item.kind);
                ns.Write(item.itemdescnum);
                ns.Write(item.expireTime);
                ns.Write(item.gotDateTime);
                ns.Write(item.count);
                ns.Write(item.exp);
                ns.Write((byte)0);
                ns.Write(item.use);
            }
            ns.Write((short)0); //  daily buff count?
            ns.Write(last);  //end
        }
    }

    public sealed class MyRoomGetCharacterList_0X1D0_03 : NetPacket
    {
        public MyRoomGetCharacterList_0X1D0_03(Account User, byte last)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_myRoomGetCharacterList";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader();

                ns.Write((byte)0xFF);
                ns.Write((short)464); //0x1D0  op code
                ns.Write((byte)3); //sub opcode
                int countpos = (int)ns.Position;
                ns.Write((byte)0); //count
                byte count = 0;
                while (reader.Read())
                {
                    ns.Write(Convert.ToInt16(reader["character"]));
                    count++;
                }
                ns.Write(last);  //end
                ns.Seek(countpos, SeekOrigin.Begin);
                ns.Write(count);

                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
    }

    public sealed class MyRoomGetMyCards_0X1D0_06 : NetPacket
    {
        public MyRoomGetMyCards_0X1D0_06(Account User, byte last)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_myRoomGetMyCards";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("requestCardItemNums", MySqlDbType.VarChar).Value = "";
                MySqlDataReader reader = cmd.ExecuteReader();

                ns.Write((byte)0xFF);
                ns.Write((short)0x1D0); //0x1D0  op code
                ns.Write((short)6); //sub opcode
                if (reader.HasRows)
                {
                    int countpos = (int)ns.Position;
                    ns.Write(0); //count
                    int count = 0;
                    while (reader.Read())
                    {
                        ns.Write(Convert.ToInt32(reader["cardnum"]));
                        ns.Write(Convert.ToInt32(reader["cardcount"]));
                        count++;
                    }
                    ns.Write((byte)0);
                    ns.Write(last);  //end
                    ns.Seek(countpos, SeekOrigin.Begin);
                    ns.Write(count);
                }
                else
                {
                    ns.Write(0); //count
                    ns.Write((byte)0);
                    ns.Write(last); //end
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
    }

    public sealed class MyRoomSetDefaultCharacter_0X1D0_09 : NetPacket
    {
        public MyRoomSetDefaultCharacter_0X1D0_09(Account User, int charid, byte last)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_myRoomSetDefaultCharacter";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("pcharacter", MySqlDbType.Int32).Value = charid;
                MySqlDataReader reader = cmd.ExecuteReader();

                ns.Write((byte)0xFF);
                ns.Write((short)464); //0x1D0  op code
                ns.Write((byte)9); //sub opcode

                while (reader.Read())
                {
                    for (int i = 0; i < 15; i++)
                    {
                        ns.Write(Convert.ToInt16(reader.GetValue(i)));
                        //avatar.Add(Convert.ToInt16(reader.GetValue(i)));
                    }
                    //count++;
                }

                ns.Fill(136);
                ns.Write(last);  //end
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
    }

    public sealed class MyroomSetCharSettingOK_0X1D0_12 : NetPacket
    {
        public MyroomSetCharSettingOK_0X1D0_12(Account User, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)464); //0x1D0  op code
            ns.Write((byte)12); //0x0C  sub op code
            ns.Write(last);  //end
        }
    }

    public sealed class MyroomGetFashionMode : NetPacket
    {
        public MyroomGetFashionMode(Account User, byte last)
        {
            /*FF A7 05 00 00 00 00 04 00 00 00 11 00 00 00 
             69 97 00 00 1E 9C 00 00 90 BE 00 00 03 00 00
             00 11 00 00 00 D9 E5 00 00 8F 2C 01 00 01 01 04*/
            /*FF A7 05 00 00 00 00 00 00 00 00 02 00 00 00 11
             00 00 00 90 BE 00 00 00 00 10*/
           /*FF A7 05 00 00 00 00 00 00 00 00 00 00 00 00
            00 00 80*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x5A7);
            ns.Write(0);
            if (User.isFashionModeOn)
            {
                ns.Write(User.WearAvatarItem.Count);
                foreach (var i in User.WearAvatarItem)
                {
                    ns.Write(i);
                }
                ns.Write(User.WearCosAvatarItem.Count);
                foreach (var i in User.WearCosAvatarItem)
                {
                    ns.Write(i);
                }
            }
            else
            {
                ns.Write(0);
                ns.Write(User.WearFashionItem.Count);
                foreach (var i in User.WearFashionItem)
                {
                    ns.Write(i);
                }
            }
            ns.Write(User.isFashionModeOn ? (byte)User.costumeMode : (byte)0);
            ns.Write(User.isFashionModeOn);
            ns.Write(last);  //end
        }
    }
    public sealed class MyroomSetFashionMode : NetPacket
    {
        public MyroomSetFashionMode(Account User, byte last)
        {
            /*FF A5 05 00 00 00 00 02 00 00 00 11 00 00 00 69 97 00
              00 01 00 00 00 11 00 00 00 00 01 04 00 00 00 11 00 00
              00 69 97 00 00 1E 9C 00 00 90 BE 00 00 02*/

            /*FF A5 05 00 00 00 00 00 00 00 00 04 00 00 00 11 00 00
             00 90 BE 00 00 69 97 00 00 1E 9C 00 00 00 00 00 00 00 00 02*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x5A5);
            ns.Write(0);
            if (User.isFashionModeOn)
            {
                ns.Write(User.WearAvatarItem.Count);
                foreach (var i in User.WearAvatarItem)
                {
                    ns.Write(i);
                }
                ns.Write(User.WearCosAvatarItem.Count);
                foreach (var i in User.WearCosAvatarItem)
                {
                    ns.Write(i);
                }
                ns.Write((byte)User.costumeMode);//User.costumeMode?
                ns.Write(User.isFashionModeOn);//isfashionModeOn?
            }
            else
            {
                ns.Write(0);
            }
            ns.Write(User.WearFashionItem.Count);
            foreach (var i in User.WearFashionItem)
            {
                ns.Write(i);
            }
            if (!User.isFashionModeOn)
            {
                ns.Fill(6);
            }
            ns.Write(last);  //end
        }
    }
    public sealed class MyroomSetFashionMode_GameRoom : NetPacket
    {
        public MyroomSetFashionMode_GameRoom(Account User, byte last)
        {
            /*FF C6 02 00 04 00 00 00 11 00 00 00 90 BE 00 00 69 97
             00 00 1E 9C 00 00 03 00 00 00 11 00 00 00 D9 E5 00 00
             8F 2C 01 00 01 01 04*/
            /*FF C6 02 00 00 00 00 00 02 00 00 00 11 00 00 00 90 BE 
             00 00 00 00 10*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x2C6);
            ns.Write(User.RoomPos);
            if (User.isFashionModeOn)
            {
                ns.Write(User.WearAvatarItem.Count);
                foreach (var i in User.WearAvatarItem)
                {
                    ns.Write(i);
                }
                ns.Write(User.WearCosAvatarItem.Count);
                foreach (var i in User.WearCosAvatarItem)
                {
                    ns.Write(i);
                }
            }
            else
            {
                ns.Write(0);
                ns.Write(User.WearFashionItem.Count);
                foreach (var i in User.WearFashionItem)
                {
                    ns.Write(i);
                }
            }
            ns.Write(User.isFashionModeOn ? (byte)User.costumeMode : (byte)0);
            ns.Write(User.isFashionModeOn);
            ns.Write(last);  //end
        }
    }

    public sealed class MyroomExitGetCurrentCharSetting_0X1D0_15 : NetPacket
    {
        public MyroomExitGetCurrentCharSetting_0X1D0_15(Account User, byte last)
        {
            //FFD00115
            ns.Write((byte)0xFF);
            ns.Write((short)464); //0x1D0  op code
            ns.Write((byte)21); //0x15  sub op code
            ns.Write(last);  //end
        }
    }

    public sealed class MyroomExitGetCurrentCharSetting_GameRoom : NetPacket
    {
        public MyroomExitGetCurrentCharSetting_GameRoom(Account User, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x38E);
            ns.Write(User.RoomPos);
            if (!User.isFashionModeOn)
            {
                for (int i = 0; i < 15; i++)
                {
                    ns.Write(User.CurrentAvatarInfo[i]);
                }
                ns.Fill(0x88); //0x88
                for (int i = 15; i < 30; i++)
                {
                    ns.Write(User.CurrentAvatarInfo[i]);
                }
                ns.Fill(0x88); //0x88
                ns.Write(User.costumeMode);
            }
            else
            {
                for (int i = 30; i < 45; i++)
                {
                    ns.Write(User.CurrentAvatarInfo[i]);
                }
                ns.Fill(0x88); //0x88
                ns.Write(User.CurrentAvatarInfo[30]);
                ns.Fill(0xA4);
                ns.Write((short)0); //costumeMode
            }
            ns.Write(User.UserItemAttr.Count);
            foreach (var item in User.UserItemAttr)
            {
                ns.Write(item.Key);
                ns.Write(item.Value.Count);
                foreach (var attr in item.Value)
                {
                    ns.Write(attr.Attr);
                    ns.Write(attr.AttrValue);
                }
            }
            //ns.Fill(8);
            ns.Write(0);
            ns.Write(last);  //end
        }
    }

    public sealed class MyroomGetUserItemAttr : NetPacket
    {
        public MyroomGetUserItemAttr(Account User, byte last)
        {
            /*69 00 00 00 00 02 00 00 00 35 02 00 00 04 00 00 00 
              02 00 EC 51 B8 3D 13 00 00 00 80 3F 2C 00 00 00 48
              43 32 00 29 5C 0F 3E DC 1C 00 00 04 00 00 00 01 00
              29 5C 0F 3E 07 00 EC 51 B8 3D 09 00 00 00 80 3E 2C
              00 00 00 C8 42 00 00 00 00 20*/
            ns.Write((byte)0x69);
            ns.Write(0);
            ns.Write(User.UserItemAttr.Count);
            foreach (var item in User.UserItemAttr)
            {
                ns.Write(item.Key);
                ns.Write(item.Value.Count);
                foreach (var attr in item.Value)
                {
                    ns.Write(attr.Attr);
                    ns.Write(attr.AttrValue);
                }
            }
            ns.Write(0);
            ns.Write(last);  //end
        }
    }
    public sealed class MyroomGetUserItemAttr_GameRoom : NetPacket
    {
        public MyroomGetUserItemAttr_GameRoom(Account User, byte last)
        {
            /*FF C4 02 00 02 00 00 00 35 02 00 00 04 00 00 00 02
             00 EC 51 B8 3D 13 00 00 00 80 3F 2C 00 00 00 48 43
             32 00 29 5C 0F 3E DC 1C 00 00 04 00 00 00 01 00 29
             5C 0F 3E 07 00 EC 51 B8 3D 09 00 00 00 80 3E 2C 00
             00 00 C8 42 00 00 00 00 20*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x2C4);
            ns.Write(User.RoomPos);
            ns.Write(User.UserItemAttr.Count);
            foreach (var item in User.UserItemAttr)
            {
                ns.Write(item.Key);
                ns.Write(item.Value.Count);
                foreach (var attr in item.Value)
                {
                    ns.Write(attr.Attr);
                    ns.Write(attr.AttrValue);
                }
            }
            ns.Write(0);
            ns.Write(last);  //end
        }
    }

    public sealed class Myroom_FFCF0100 : NetPacket
    {
        public Myroom_FFCF0100(Account User, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)464); //0x1D0  op code
            ns.Write((byte)1); //sub op code?
            ns.Write((byte)1); 
            ns.Write(last);
            
        }
    }

    public sealed class Myroom_GetGiftList : NetPacket
    {
        public Myroom_GetGiftList(Account User, short startindex, short lastindex, byte last)
        {
            /*FF 73 01 00 00 00 00 01 00 08 00 02 00 02 00 00 00 
            8D 11 2E 01 0C 00 00 00 A9 78 A4 E8 AC F5 A6 E2 A4 70 A4 A1 
            04 68 00 00 50 A5 7C E6 3C 01 00 00 
            12 00 00 00 B7 73 AC 4B B6 67 A5 BD BD E0 B5 6E A4 4A BC FA AB 7E 
            FF FF FF FF FF FF FF FF 00 00 00 00 8C 11 2E 01 0C 00 00 00 A9 78
                A4 E8 AC F5 A6 E2 A4 70 A4 A1 FA 67 00 00 50 A5 7C E6 3C 01 00
                00 12 00 00 00 B7 73 AC 4B B6 67 A5 BD BD E0 B5 6E A4 4A BC FA
                AB 7E FF FF FF FF FF FF FF FF 00 00 00 00 20*/

            ns.Write((byte)0xFF);
            ns.Write((short)0x173); // op code
            ns.Write(0);
            ns.Write(startindex);
            ns.Write(lastindex);
            int countpos = (int)ns.Position;
            short count = 0;
            ns.Write(count);
            int totalitemcountpos = (int)ns.Position;
            int totalitemcount = 0;
            ns.Write(totalitemcount);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_shopGetGiftAcceptWaitList";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("startIndex", MySqlDbType.Int32).Value = startindex;
                cmd.Parameters.Add("lastIndex", MySqlDbType.Int32).Value = lastindex;
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ns.Write(Convert.ToInt32(reader["uniNum"])); //uniNum 0x12E118D
                        ns.WriteBIG5Fixed_intSize(reader["sendNickname"].ToString());
                        ns.Write(Convert.ToInt32(reader["itemDescNum"]));
                        ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["sendDateTime"])));
                        ns.WriteBIG5Fixed_intSize(reader["memo"].ToString());
                        ns.Write(Convert.IsDBNull(reader["Expire"]) ? -1 : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["Expire"])));
                        ns.Write(0);
                        totalitemcount = Convert.ToInt16(reader["Itemcount"]);
                        count++;
                    }
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write(last);  //end
            ns.Seek(totalitemcountpos, SeekOrigin.Begin);
            ns.Write(totalitemcount);
            ns.Seek(countpos, SeekOrigin.Begin);
            ns.Write(count);

        }
    }

    public sealed class Myroom_AcceptGiftOK : NetPacket
    {
        public Myroom_AcceptGiftOK(Account User, int itemid, byte last)
        {
            //FF 75 01 00 00 00 00 47 96 00 00 01
            ns.Write((byte)0xFF);
            ns.Write((short)0x175); // op code
            ns.Write(0);
            ns.Write(itemid);
            ns.Write(last);  //end

        }
    }

    public sealed class Myroom_AcceptGiftFail : NetPacket
    {
        public Myroom_AcceptGiftFail(byte last)
        {
            //FF 75 01 4B 00 00 00 00 80
            ns.Write((byte)0xFF);
            ns.Write((short)0x175);
            ns.Write(0x4B);
            ns.Write((byte)0);
            ns.Write(last);
        }
    }

    public sealed class Myroom_GiftItemInfo_ACK : NetPacket
    {
        public Myroom_GiftItemInfo_ACK(Account User, ConcurrentBag<AvatarItemInfo> iteminfos, byte last)
        {
            //FF 45 01 B8 C1 9B 00 01 00 05 00 05 00 45 96 00 00 
            //90 CA 38 FE 67 01 00 00 90 46 2C DA 67 01 00 00 
            //01 00 00 00 00 00 00 00 01 01 04
            ns.Write((byte)0xFF);
            ns.Write((short)0x145); // op code
            foreach (var item in iteminfos)
            {
                ns.Write(0x9CC1D0); //D0 C1 9C 00
                ns.Write(item.character);
                ns.Write(item.position);
                ns.Write(item.kind);
                ns.Write(item.itemdescnum);
                ns.Write(item.expireTime);
                ns.Write(item.gotDateTime);
                ns.Write(item.count);
                ns.Write(item.exp);
                ns.Write((byte)0);
                ns.Write(item.use);
            }
            /*using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getAvatarItem";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("itemnums", MySqlDbType.VarChar).Value = itemid;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

                while (reader.Read())
                {
                    ns.Write(0x9CC1D0); //D0 C1 9C 00
                    ns.Write(Convert.ToInt16(reader["character"]));
                    ns.Write(Convert.ToUInt16(reader["position"]));
                    ns.Write(Convert.ToInt16(reader["kind"]));
                    ns.Write(Convert.ToInt32(reader["itemdescnum"])); //4 bytes
                    ns.Write(Convert.IsDBNull(reader["expireTime"]) ? 0 : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["expireTime"])));
                    ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["gotDateTime"])));
                    ns.Write(Convert.ToInt32(reader["count"])); //4 bytes
                    ns.Write(Convert.ToInt32(reader["exp"]));//ns.Write(0); exp
                    ns.Write((byte)0);
                    ns.Write(Convert.ToBoolean(reader["using"]));
                    //ns.Write((short)0x100); //unknown?? 00 01 / 01 00 / 01 01 / 00 00
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }*/
            ns.Write(last);  //end
        }
    }
    public sealed class Myroom_CheckExistNewGift : NetPacket
    {
        public Myroom_CheckExistNewGift(Account User, byte last)
        {
            //FF6701000000000100000002
            ns.Write((byte)0xFF);
            ns.Write((short)0x167); // op code
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_shopCheckExistNewGift";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                if(Convert.ToBoolean(reader["newMyroomGift"]) || Convert.ToBoolean(reader["newStorageGift"]))
                    ns.Write(true);
                else
                    ns.Write(false);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Fill(7);
            ns.Write(last);  //end
        }
    }
    public sealed class Myroom_GetStorageKeepingList : NetPacket
    {
        public Myroom_GetStorageKeepingList(Account User, byte last)
        {
            /*FF 4A 05 00 00 00 00 00 00 00 00 0A 00 00 00 28 42 09
             00 00 00 00 00 40 72 00 00 20 39 2D 1F 68 01 00 00
             00 00 00 00 29 42 09 00 00 00 00 00 0D 0D 00 00 A0
             77 2D 1F 68 01 00 00 00 00 00 00 2A 42 09 00 00 00
             00 00 40 72 00 00 C8 9A 2D 1F 68 01 00 00 00 00 00
             00 2B 42 09 00 00 00 00 00 40 72 00 00 A8 C9 2D 1F
             68 01 00 00 00 00 00 00 2C 42 09 00 00 00 00 00 40
             72 00 00 D0 EC 2D 1F 68 01 00 00 00 00 00 00 2E 42
             09 00 00 00 00 00 40 72 00 00 E0 13 2E 1F 68 01 00
             00 00 00 00 00 2F 42 09 00 00 00 00 00 A0 64 00 00
             A0 EE 2E 1F 68 01 00 00 00 00 00 00 30 42 09 00 00
             00 00 00 2A 70 00 00 40 7B 2F 1F 68 01 00 00 00 00
             00 00 32 42 09 00 00 00 00 00 A0 64 00 00 C0 36 30
             1F 68 01 00 00 00 00 00 00 33 42 09 00 00 00 00 00
             0D 0D 00 00 E8 59 30 1F 68 01 00 00 00 00 00 00 04*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x54A); // op code
            ns.Write(0);
            ns.Write(0); //type
            int countpos = (int)ns.Position;
            int count = 0;
            ns.Write(count);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_storage_getKeepingItemList";
                cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ns.Write(Convert.ToInt64(reader["uniqueNum"]));
                        ns.Write(Convert.ToInt32(reader["itemNum"]));
                        ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["dateTime"])));
                        ns.Write(0);
                        count++;
                    }
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write(last);  //end
            ns.Seek(countpos, SeekOrigin.Begin);
            ns.Write(count);

        }
    }
    public sealed class Myroom_GetStorageGiftList : NetPacket
    {
        public Myroom_GetStorageGiftList(Account User, byte last)
        {
            //FF 4A 05 00 00 00 00 01 00 00 00 00 00 00 00 08
            /*FF 4A 05 00 00 00 00 01 00 00 00 01 00 00 00 86
             41 09 00 00 00 00 00 ED A3 00 00 C0 7E D0 2C 68
             01 00 00 00 00 00 00 0C 00 00 00 C2 66 C2 63 A9
             CE A6 A8 C4 B9 AE 61 06 00 00 00 31 32 33 34 35
             68 01*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x54A); // op code
            ns.Write(0);
            ns.Write(1); //type
            int countpos = (int)ns.Position;
            int count = 0;
            ns.Write(count);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_storage_getGiftList";
                cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ns.Write(Convert.ToInt64(reader["uniqueNum"]));
                        ns.Write(Convert.ToInt32(reader["itemNum"]));
                        ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["dateTime"])));
                        ns.Write(0);
                        ns.WriteBIG5Fixed_intSize(reader["sendNickname"].ToString());
                        ns.WriteBIG5Fixed_intSize(reader["memo"].ToString());
                        count++;
                    }

                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write(last);  //end
            ns.Seek(countpos, SeekOrigin.Begin);
            ns.Write(count);

        }
    }

    public sealed class Myroom_StorageReceiveOK : NetPacket
    {
        public Myroom_StorageReceiveOK(Account User, int type, long UniqueNum, byte last)
        {
            //FF 4E 05 00 00 00 00 00 00 00 00 26 02 09 00 00 00 00 00 10
            ns.Write((byte)0xFF);
            ns.Write((short)0x54E); // op code
            ns.Write(0);
            ns.Write(type); //type
            ns.Write(UniqueNum);
            ns.Write(last);  //end

        }
    }
    public sealed class Myroom_StorageGiftACK : NetPacket
    {
        public Myroom_StorageGiftACK(Account User, int error, long UniqueNum, string nickname, byte last)
        {
            //FF 4C 05 00 00 00 00 86 41 09 00 00 00 00 00 07 00 00 00 70 6F 70 6F 36 37 38 08
            ns.Write((byte)0xFF);
            ns.Write((short)0x54C); // op code
            ns.Write(error); //errorcode
            ns.Write(UniqueNum);
            ns.WriteBIG5Fixed_intSize(nickname);
            ns.Write(last);  //end

        }
    }

    public sealed class Myroom_ItemOn : NetPacket
    {
        public Myroom_ItemOn(Account User, int itemnum, int OnOffType, int Position, byte last)
        {
            //3E 00 00 00 00 EA A5 00 00 02 00 00 00 77 00 00 00 01 40
            ns.Write((byte)0x3E);
            ns.Write(0);
            ns.Write(itemnum);
            ns.Write(OnOffType);
            ns.Write(Position);
            /*using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_item_On";
                cmd.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("ItemDescNum", MySqlDbType.Int32).Value = itemnum;
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                ns.Write(Convert.ToInt32(reader["OnOffType"]));
                ns.Write(Convert.ToInt32(reader["Position"]));
                cmd.Dispose();
                reader.Close();
                con.Close();
            }*/
            ns.Write(true);
            ns.Write(last);  //end
        }
    }
    public sealed class Myroom_ItemOn_GameRoom : NetPacket
    {
        public Myroom_ItemOn_GameRoom(byte pos, int itemnum, int OnOffType, int Position, AvatarItemInfo offitem, AvatarItemInfo onitem, byte last)
        {
            /*FF 8F 03 00 0C 8F 00 00 02 00 00 00 66 00 00 00
              01 F0 11 9E 00 00 00 66 00 C0 00 FA 2D 01 00 20 F0
              9D 59 6A 01 00 00 20 00 A2 56 68 01 00 00 01 00
              00 00 00 00 00 00 01 01 F0 11 9E 00 00 00 66 00
              99 00 0C 8F 00 00 C8 84 B6 6A 68 01 00 00 D0 33
              BC 00 66 01 00 00 01 00 00 00 00 00 00 00 01 00 80*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x38F);
            ns.Write(pos); //pos?
            ns.Write(itemnum);
            ns.Write(OnOffType);
            ns.Write(Position);
            ns.Write(true);
            if(offitem == null)
            {
                ns.Write(0x9CC1D0); //D0 C1 9C 00
                ns.Fill(0x24);
            }
            else
            {
                ns.Write(0x9CC1D0); //D0 C1 9C 00
                ns.Write(offitem.character);
                ns.Write(offitem.position);
                ns.Write(offitem.kind);
                ns.Write(offitem.itemdescnum);
                ns.Write(offitem.expireTime);
                ns.Write(offitem.gotDateTime);
                ns.Write(offitem.count);
                ns.Write(offitem.exp);
                ns.Write((byte)0);
                ns.Write(offitem.use);
            }
            ns.Write(0x9CC1D0); //D0 C1 9C 00
            ns.Write(onitem.character);
            ns.Write(onitem.position);
            ns.Write(onitem.kind);
            ns.Write(onitem.itemdescnum);
            ns.Write(onitem.expireTime);
            ns.Write(onitem.gotDateTime);
            ns.Write(onitem.count);
            ns.Write(onitem.exp);
            ns.Write((byte)0);
            ns.Write(onitem.use);

            ns.Write(last);  //end
        }
    }
    public sealed class Myroom_ItemOff : NetPacket
    {
        public Myroom_ItemOff(Account User, int itemnum, int OnOffType, int Position, byte last)
        {
            //3E 00 00 00 00 EA A5 00 00 02 00 00 00 77 00 00 00 01 40
            ns.Write((byte)0x3E);
            ns.Write(0);
            ns.Write(itemnum);
            ns.Write(OnOffType);
            ns.Write(Position);
            /*using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_item_Off";
                cmd.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("ItemDescNum", MySqlDbType.Int32).Value = itemnum;
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                ns.Write(Convert.ToInt32(reader["OnOffType"]));
                ns.Write(Convert.ToInt32(reader["Position"]));
                cmd.Dispose();
                reader.Close();
                con.Close();
            }*/
            ns.Write(false);
            ns.Write(last);  //end
        }
    }
    public sealed class Myroom_ItemOff_GameRoom : NetPacket
    {
        public Myroom_ItemOff_GameRoom(byte pos, int itemnum, int OnOffType, int Position, AvatarItemInfo offitem, byte last)
        {
            /*FF 8F 03 00 FA 2D 01 00 02 00 00 00 66 00 00
             00 00 F0 11 9E 00 00 00 66 00 C0 00 FA 2D 01
             00 20 F0 9D 59 6A 01 00 00 20 00 A2 56 68 01
             00 00 01 00 00 00 00 00 00 00 01 01 F0 11 9E
             00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
             00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
             00 00 00 00 00 00 00 10*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x38F);
            ns.Write(pos); //pos?
            ns.Write(itemnum);
            ns.Write(OnOffType);
            ns.Write(Position);
            ns.Write(false);

            ns.Write(0x9CC1D0); //D0 C1 9C 00
            ns.Write(offitem.character);
            ns.Write(offitem.position);
            ns.Write(offitem.kind);
            ns.Write(offitem.itemdescnum);
            ns.Write(offitem.expireTime);
            ns.Write(offitem.gotDateTime);
            ns.Write(offitem.count);
            ns.Write(offitem.exp);
            ns.Write((byte)0);
            ns.Write(offitem.use);

            ns.Write(0x9CC1D0); //D0 C1 9C 00
            ns.Fill(0x24);

            ns.Write(last);  //end
        }
    }

    public sealed class Myroom_ActiveFuncItemOne : NetPacket
    {
        public Myroom_ActiveFuncItemOne(Account User, int itemnum, byte last)
        {
            /*67 00 00 00 00 66 00 00 00 01 00 00 00 B8 C1 9B
             00 00 00 02 00 D8 09 68 91 00 00 10 4C 3F C8 68
             01 00 00 F0 B9 A3 55 65 01 00 00 01 00 00 00 00
             00 00 00 01 01 80*/
            ns.Write((byte)0x67);
            ns.Write(0);
            ns.Write(0x66); // op code
            int countpos = (int)ns.Position;
            int count = 0;
            ns.Write(count); //count
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_getActiveFuncItemOne";
                    cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("itemnum", MySqlDbType.Int32).Value = itemnum;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ns.Write(0x9CC1D0); //D0 C1 9C 00
                                ns.Write(Convert.ToInt16(reader["character"]));
                                ns.Write(Convert.ToUInt16(reader["position"]));
                                ns.Write(Convert.ToInt16(reader["kind"]));
                                ns.Write(Convert.ToInt32(reader["itemdescnum"])); //4 bytes
                                ns.Write(Convert.IsDBNull(reader["expireTime"]) ? 0L : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["expireTime"])));
                                ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["gotDateTime"])));
                                ns.Write(Convert.ToInt32(reader["count"])); //4 bytes
                                ns.Write(Convert.ToInt32(reader["exp"]));
                                ns.Write((byte)0);
                                ns.Write(Convert.ToBoolean(reader["using"]));
                                count++;
                            }
                        }
                    }
                }
            }
            ns.Write(last);  //end
            ns.Seek(countpos, SeekOrigin.Begin);
            ns.Write(count);
        }
    }

    public sealed class Myroom_RepairItemOK : NetPacket
    {
        public Myroom_RepairItemOK(Account User, int itemnum, int repairitemnum, byte last)
        {
            //FF D0 01 2A 68 91 00 00 C4 02 00 00 80
            ns.Write((byte)0xFF);
            ns.Write((short)0x1D0); // op code
            ns.Write((byte)0x2A); //subop
            ns.Write(itemnum); //type
            ns.Write(repairitemnum);
            ns.Write(last);  //end
        }
    }
    public sealed class Myroom_PetRebirth : NetPacket
    {
        public Myroom_PetRebirth(Account User, int petitemnum, int rebirthitemnum, byte last)
        {
            //FF D0 01 24 04 00 00 00 C2 14 00 00 14 8B 00 00 F9 DE 00 00 D6 E1 00 00 2E 52 00 00 08
            ns.Write((byte)0xFF);
            ns.Write((short)0x1D0); // op code
            ns.Write((byte)0x24); //subop
            int countpos = (int)ns.Position;
            int count = 0;
            ns.Write(count);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_myRoomPetRebirth";
                    cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("petItemNum", MySqlDbType.Int32).Value = petitemnum;
                    cmd.Parameters.Add("petRebirthItemNum", MySqlDbType.Int32).Value = rebirthitemnum;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ns.Write(Convert.ToInt32(reader["fdPetItemNum"]));
                                count++;
                            }
                            ns.Write(rebirthitemnum);
                        }
                    }
                }
            }
            ns.Write(last);  //end
            ns.Seek(countpos, SeekOrigin.Begin);
            ns.Write(count);
        }
    }
    public sealed class Myroom_FeedPetOK : NetPacket
    {
        public Myroom_FeedPetOK(Account User, int petitemnum, int feeditemnum, byte last)
        {
            //FF D0 01 21 C2 14 00 00 52 5E 00 00 80
            ns.Write((byte)0xFF);
            ns.Write((short)0x1D0); // op code
            ns.Write((byte)0x21); //subop
            ns.Write(petitemnum); //type
            ns.Write(feeditemnum);
            ns.Write(last);  //end
        }
    }
    public sealed class Myroom_PetUpgradeOK : NetPacket
    {
        public Myroom_PetUpgradeOK(Account User, byte last)
        {
            //FF D0 01 27 08
            ns.Write((byte)0xFF);
            ns.Write((short)0x1D0); // op code
            ns.Write((byte)0x27); //subop
            ns.Write(last);  //end
        }
    }

    public sealed class Myroom_UseLuckyBagOK : NetPacket
    {
        public Myroom_UseLuckyBagOK(Account User, int itemnum, ConcurrentBag<int> itemlist, byte last)
        {
            //FF D0 01 1B 00 00 00 00 01 00 00 00 B4 94 00 00 F4 B7 00 00 01
            //FF D0 01 1B 00 00 00 00 02 00 00 00 A4 E0 00 00 E8 DF 00 00 9D E0 00 00 40
            ns.Write((byte)0xFF);
            ns.Write((short)0x1D0); // op code
            ns.Write((byte)0x1B); //subop
            ns.Write(0);
            ns.Write(itemlist.Count);
            foreach(var i in itemlist)
            {
                ns.Write(i);
            }
            ns.Write(itemnum);
            ns.Write(last);  //end
        }
    }
    public sealed class Myroom_UseLuckyBagFail : NetPacket
    {
        public Myroom_UseLuckyBagFail(int itemnum, byte last)
        {
            //FF D0 01 1B A9 00 00 00 FA 2C 01 00 10
            ns.Write((byte)0xFF);
            ns.Write((short)0x1D0);
            ns.Write((byte)0x1B);
            ns.Write(0xA9);
            ns.Write(itemnum);
            ns.Write(last);
        }
    }

    public sealed class Myroom_SetSlotItemSettingOK : NetPacket
    {
        public Myroom_SetSlotItemSettingOK(int slotNum, string SlotName, ushort charid, ushort head, ushort topbody, ushort downbody, ushort foot, ushort acHead, ushort acFace, ushort acHand, ushort acBack, ushort acNeck, ushort pet, ushort expansion, ushort acWrist, ushort acBooster, ushort acTail, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x1D0); //0x1D0  op code
            ns.Write((byte)0x43); //sub op code
            ns.Write(slotNum);
            ns.WriteBIG5Fixed_intSize(SlotName);
            ns.Write(charid);
            ns.Write(head);
            ns.Write(topbody);
            ns.Write(downbody);
            ns.Write(foot);
            ns.Write(acHead);
            ns.Write(acHand);
            ns.Write(acFace);
            ns.Write(acBack);
            ns.Write(acNeck);
            ns.Write(pet);
            ns.Write(expansion);
            ns.Write(acWrist);
            ns.Write(acBooster);
            ns.Write(acTail);
            ns.Fill(0x88);
            ns.Write(last);  //end
        }
    }
    public sealed class Myroom_GetSlotInfoOK : NetPacket
    {
        public Myroom_GetSlotInfoOK(List<UserSlotItemKindInfo> slotinfos, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x1D0); //0x1D0  op code
            ns.Write((byte)0x46); //0x46 sub opcode
            ns.Write(slotinfos.Count);
            foreach (var i in slotinfos)
            {
                ns.Write(i.SlotNum);
                ns.WriteBIG5Fixed_intSize(i.SlotName);
                ns.Write(i.charid);
                ns.Write(i.head);
                ns.Write(i.topbody);
                ns.Write(i.downbody);
                ns.Write(i.foot);
                ns.Write(i.acHead);
                ns.Write(i.acHand);
                ns.Write(i.acFace);
                ns.Write(i.acBack);
                ns.Write(i.acNeck);
                ns.Write(i.pet);
                ns.Write(i.expansion);
                ns.Write(i.acWrist);
                ns.Write(i.acBooster);
                ns.Write(i.acTail);
                ns.Fill(0x88);
            }
            ns.Write(last);  //end

        }
    }
    public sealed class Myroom_BuyMyRoomSlotOK : NetPacket
    {
        public Myroom_BuyMyRoomSlotOK(int SlotNum, byte last)
        {
            //FF D0 01 49 00 00 00 00 01 00 00 00 02
            ns.Write((byte)0xFF);
            ns.Write((short)0x1D0); //0x1D0  op code
            ns.Write((byte)0x49); //0x46 sub opcode
            ns.Write(0);
            ns.Write(SlotNum);
            ns.Write(last);  //end
        }
    }
}
