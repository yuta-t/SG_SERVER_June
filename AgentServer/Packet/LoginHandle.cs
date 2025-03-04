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
using System.Collections.Generic;
using System.IO;
using LocalCommons.Logging;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Map;
using System.Reflection;
using System.Security.Cryptography;
using AgentServer.Structuring.Shu;
using System.Diagnostics;

namespace AgentServer.Packet
{
    public class LoginHandle
    {
        public static void Handle_LoginCheck(ClientConnection Client, PacketReader reader)
        {
            try
            {
                if (!ServerStatus.isReady)
                {
                    Log.Info("connection blocked by current setting ServerReady off");
                    return;
                }
                byte unk1 = reader.ReadByte();
                ushort unk2 = reader.ReadLEUInt16();
                int unk3 = reader.ReadLEInt32();
                int useridlen = reader.ReadByte();
                string userid = reader.ReadStringSafe(useridlen);
                int passwordlen = reader.ReadByte();
                string password = reader.ReadStringSafe(passwordlen);
                //reader.Clear();
                //Console.WriteLine("unk1: {0}, unk2: {1}, unk3: {2}, useridlen: {3}, userid: {4}, passwordlen: {5}, password: {6}", unk1, unk2, unk3, useridlen, userid, passwordlen, password);
                bool UserCheckOK = checkUserAccount(userid, password);

                if (UserCheckOK)
                {
                    Account nCurrent = new Account
                    {
                        UserID = userid,
                        Connection = Client,
                        LastIp = Client.ToString(),
                        Port = (short)((IPEndPoint)Client.CurrentChannel.RemoteEndPoint).Port,
                        Session = Client.session,
                        isLogin = false
                    };
                    Client.CurrentAccount = nCurrent;
                    ClientConnection.CurrentAccounts[Client.session] = nCurrent;

                    Console.WriteLine("Login OK!");
                    nCurrent.isLogin = true;

                    bool logined = ClientConnection.CurrentAccounts.Count(players => players.Value.UserID == nCurrent.UserID) > 1;
                    if (logined)
                    {
                        Log.Info("User [{0}]  has already logged in!", nCurrent.UserID);
                        Client.SendAsync(new NP_Hex(nCurrent, "00AA03000001"));

                        /*List<Account> objects = ClientConnection.CurrentAccounts.Values.ToList().FindAll(players => players.UserID == nCurrent.UserID);
                        foreach (Account obj in objects)
                        {
                            if (obj.Session != nCurrent.Session)
                            {
                                obj.Connection.SendAsync(new DisconnectPacket(obj, 257, last));
                            }
                        }*/
                        return;
                    }

                    CheckGameID(nCurrent);
                    Client.SendAsync(new AccountResult_0x371_00(true));
                    Client.SendAsync(new LoginGameID_0x371_01(nCurrent));
                    getUserType(nCurrent);
                }
                else
                {
                    Console.WriteLine("Login Fail!");
                    Client.SendAsync(new AccountResult_0x371_00(false));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            /*int packetlength = reader.ReadLEInt16();
            reader.ReadByte(); //opcode
            try
            {
                if (!ServerStatus.isReady)
                {
                    Log.Info("connection blocked by current setting ServerReady off");
                    //Client.SendAsync(new ServerNotReady());
                    return;
                }
                int useridlen = reader.ReadLEInt32();
                string userid = reader.ReadStringSafe(useridlen);
                int passwordlen = reader.ReadLEInt32();
                string password = pwdecode(reader.ReadByteArray(passwordlen));
                reader.Clear();
               // Console.WriteLine("User Login ( Account: {0} ,Password: {1} )", userid, password);

                bool UserCheckOK = checkUserAccount(userid, password);
                if (UserCheckOK)
                {
                    Console.WriteLine("Login Success!");
                    //int cookie = LocalCommons.Cookie.Cookie.Generate();
                    //byte[] key = { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff, 0x00 };
                    byte[] randomkey = new byte[16];
                    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                    rng.GetBytes(randomkey);
                    byte[] xorkey = Encrypt.EncryptKey(randomkey, randomkey);
                    //Console.WriteLine("rand key {0}", Utility.ByteArrayToString(randomkey));
                    //Console.WriteLine("xor key {0}", Utility.ByteArrayToString(xorkey));
                    Account nCurrent = new Account
                    {
                        UserID = userid,
                        Connection = Client,
                        LastIp = Client.ToString(),
                        Port = (short)((IPEndPoint)Client.CurrentChannel.RemoteEndPoint).Port,
                        EncryptKey = randomkey,
                        XorKey = xorkey,
                        Session = Client.session,
                        isLogin = false
                    };
                    Log.Debug("AES Key: {0}, XOR Key: {1}", BitConverter.ToString(randomkey).Replace("-", " "), BitConverter.ToString(xorkey).Replace("-", " "));
                    Client.CurrentAccount = nCurrent;
                    //ClientConnection.CurrentAccounts.TryAdd(cookie, Client.CurrentAccount);
                    ClientConnection.CurrentAccounts[Client.session] = nCurrent;

                }
                else
                {
                    Console.WriteLine("Login Fail!");
                }
                Client.SendAsync(new LoginServerTime_0X41(Client.CurrentAccount));
                Client.SendAsync(new LoginCheck_0X10(Client.CurrentAccount, userid, UserCheckOK));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            */
        }
        public static void Handle_Logout(ClientConnection Client, PacketReader reader)
        {
            try
            {
                Account User = Client.CurrentAccount;
                GameRoomHandle.LeaveRoom(User);
                Client.SendAsync(new NP_Hex(User, "00AA03000001"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void Handle_FirstLoginCheck(ClientConnection Client, PacketReader reader)
        {
            try
            {
                Account User = Client.CurrentAccount;
                int unk1 = reader.ReadLEInt32();
                ushort unk2 = reader.ReadLEUInt16();
                byte unk3 = reader.ReadByte();
                int gameidlen = reader.ReadByte();
                //string gameid = reader.ReadStringSafe(gameidlen);
                string gameid = reader.ReadUTF8StringSafe(gameidlen);

                bool AccountCheckOK = checkNewAccount(User, gameid);
                if (AccountCheckOK)
                {
                    User.GameID = gameid;
                    Client.SendAsync(new FirstLoginResult_0x371_02(User, true));
                }
                else
                {
                    Client.SendAsync(new FirstLoginResult_0x371_02(User, false));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void Handle_LoginDisplayCharacter(ClientConnection Client, PacketReader reader)
        {
            try
            {
                Account User = Client.CurrentAccount;
                if (User.GameID == null)
                    return;

                getCharNumber(User);
                Console.WriteLine("Character count: {0}", User.CharacterCount);
                getCharBasicInfo(User);
                getEquItemInfo(User);
                getFoodItemInfo(User);
                getCharEncodedColor(User);
                getCharDecodedColor(User);

                //CheckGlobalID(User);
                //if (User.GlobalID == 0)
                //return;
                Console.WriteLine("Global ID: {0}", User.GlobalID);
                /*if (User.CharacterCount > 0)
                {
                    Console.WriteLine("Get Head From DB: {0}, Get Face From DB: {1}", User.CharacterOneEquipment1, User.CharacterOneEquipment2);
                }*/
                //Console.WriteLine("Char1 Nation: {0}, Job: {1}, Level: {2}, Nickname: {3}, Equipment1: {4}", User.CharacterNation[0], User.CharacterJob[0], User.CharacterLevel[0], User.CharacterNickname[0], User.CharacterEquipment[0][0]);
                //Console.WriteLine("Char2 Nation: {0}, Job: {1}, Level: {2}, Nickname: {3}, Equipment1: {4}", User.CharacterNation[1], User.CharacterJob[1], User.CharacterLevel[1], User.CharacterNickname[1], User.CharacterEquipment[1][0]);
                Client.SendAsync(new LoginDisplayCharacter_0x371_03(User, User.CharacterCount));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void Handle_LoginGetNewCharacterNum(ClientConnection Client, PacketReader reader)
        {
            try
            {
                //TO DO STRUGARDEN
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void Handle_LoginNewCharacter(ClientConnection Client, PacketReader reader)
        {
            try
            {
                Account User = Client.CurrentAccount;
                getCharNumber(User);
                if (User.CharacterCount > 0)
                {
                    return;
                }
                string hairClump1 = "FFFFFFFF";
                string hairClump2 = "FFFFFFFF";
                string hairClump3 = "FFFFFFFF";
                string hairClump4 = "FFFFFFFF";
                byte unk1 = reader.ReadByte();
                ushort unk2 = reader.ReadLEUInt16();
                byte unk3 = reader.ReadByte();
                byte unk4 = reader.ReadByte();
                ushort unk5 = reader.ReadLEUInt16();
                byte unk6 = reader.ReadByte();
                int face = reader.ReadByte();
                int hair = reader.ReadByte();
                int head = 1;
                int gender = 0;

                byte[] clothColor = getMaterialColor(reader);
                byte[] skinColor = getMaterialColor(reader);
                byte[] hairColor = getMaterialColor(reader);

                int charidlen = reader.ReadByte();
                string charid = reader.ReadUTF8StringSafe(charidlen);

                switch (hair)
                {
                    case 0:
                        if (face == 0x9 | face == 0xA | face == 0xB | face == 0xC | face == 0xD | face == 0xE | face == 0xF | face == 0x10 | face == 0x11)
                        {
                            hairClump1 = "0000010A";
                            hairClump2 = "0100010A";
                            hairClump3 = "0200010A";
                            hairClump4 = "0300010A";
                            gender = 1;
                            //face = 65539;
                            head = 65537;
                        }
                        else
                        {
                            hairClump1 = "0000000A";
                            hairClump2 = "0100000A";
                            hairClump3 = "0200000A";
                            hairClump4 = "0300000A";
                            head = 1;
                        }
                        break;
                    case 1:
                        if (face == 0x9 | face == 0xA | face == 0xB | face == 0xC | face == 0xD | face == 0xE | face == 0xF | face == 0x10 | face == 0x11)
                        {
                            hairClump1 = "0A00010A";
                            hairClump2 = "0B00010A";
                            hairClump3 = "0C00010A";
                            hairClump4 = "0D00010A";
                            gender = 1;
                            //face = 65549;
                            head = 65547;
                        }
                        else
                        {
                            hairClump1 = "0A00000A";
                            hairClump2 = "0B00000A";
                            hairClump3 = "0C00000A";
                            hairClump4 = "0D00000A";
                            head = 11;
                        }
                        break;
                    case 2:
                        if (face == 0x9 | face == 0xA | face == 0xB | face == 0xC | face == 0xD | face == 0xE | face == 0xF | face == 0x10 | face == 0x11)
                        {
                            hairClump1 = "1400010A";
                            hairClump2 = "1500010A";
                            hairClump3 = "1600010A";
                            hairClump4 = "1700010A";
                            gender = 1;
                            //face = 65559;
                            head = 65557;
                        }
                        else
                        {
                            hairClump1 = "1400000A";
                            hairClump2 = "1500000A";
                            hairClump3 = "1600000A";
                            hairClump4 = "1700000A";
                            head = 21;
                        }
                        break;
                    case 3:
                        if (face == 0x9 | face == 0xA | face == 0xB | face == 0xC | face == 0xD | face == 0xE | face == 0xF | face == 0x10 | face == 0x11)
                        {
                            hairClump1 = "1E00010A";
                            hairClump2 = "1F00010A";
                            hairClump3 = "2000010A";
                            hairClump4 = "2100010A";
                            gender = 1;
                            //face = 65569;
                            head = 65567;
                        }
                        else
                        {
                            hairClump1 = "1E00000A";
                            hairClump2 = "1F00000A";
                            hairClump3 = "2000000A";
                            hairClump4 = "2100000A";
                            head = 31;
                        }
                        break;
                    case 4:
                        if (face == 0x9 | face == 0xA | face == 0xB | face == 0xC | face == 0xD | face == 0xE | face == 0xF | face == 0x10 | face == 0x11)
                        {
                            hairClump1 = "2800010A";
                            hairClump2 = "2900010A";
                            hairClump3 = "2A00010A";
                            hairClump4 = "2B00010A";
                            gender = 1;
                            //face = 65579;
                            head = 65577;
                        }
                        else
                        {
                            hairClump1 = "2800000A";
                            hairClump2 = "2900000A";
                            hairClump3 = "2A00000A";
                            hairClump4 = "2B00000A";
                            head = 41;
                        }
                        break;
                    case 5:
                        if (face == 0x9 | face == 0xA | face == 0xB | face == 0xC | face == 0xD | face == 0xE | face == 0xF | face == 0x10 | face == 0x11)
                        {
                            hairClump1 = "3200010A";
                            hairClump2 = "3300010A";
                            hairClump3 = "3400010A";
                            hairClump4 = "3500010A";
                            gender = 1;
                            //face = 65599;
                            head = 65587;
                        }
                        else
                        {
                            hairClump1 = "3200000A";
                            hairClump2 = "3300000A";
                            hairClump3 = "3400000A";
                            hairClump4 = "3500000A";
                            head = 51;
                        }
                        break;
                    case 6:
                        if (face == 0x9 | face == 0xA | face == 0xB | face == 0xC | face == 0xD | face == 0xE | face == 0xF | face == 0x10 | face == 0x11)
                        {
                            hairClump1 = "3C00010A";
                            hairClump2 = "3D00010A";
                            hairClump3 = "3E00010A";
                            hairClump4 = "3F00010A";
                            gender = 1;
                            //face = 65589;
                            head = 65597;
                        }
                        else
                        {
                            hairClump1 = "3C00000A";
                            hairClump2 = "3D00000A";
                            hairClump3 = "3E00000A";
                            hairClump4 = "3F00000A";
                            head = 61;
                        }
                        break;
                    case 7:
                        if (face == 0x9 | face == 0xA | face == 0xB | face == 0xC | face == 0xD | face == 0xE | face == 0xF | face == 0x10 | face == 0x11)
                        {
                            hairClump1 = "4600010A";
                            hairClump2 = "4700010A";
                            hairClump3 = "4800010A";
                            hairClump4 = "4900010A";
                            gender = 1;
                            //face = 65609;
                            head = 65607;
                        }
                        else
                        {
                            hairClump1 = "4600000A";
                            hairClump2 = "4700000A";
                            hairClump3 = "4800000A";
                            hairClump4 = "4900000A";
                            head = 71;
                        }
                        break;
                    case 8:
                        if (face == 0x9 | face == 0xA | face == 0xB | face == 0xC | face == 0xD | face == 0xE | face == 0xF | face == 0x10 | face == 0x11)
                        {
                            hairClump1 = "5000010A";
                            hairClump2 = "5100010A";
                            hairClump3 = "5200010A";
                            hairClump4 = "5300010A";
                            gender = 1;
                            //face = 65619;
                            head = 65617;
                        }
                        else
                        {
                            hairClump1 = "5000000A";
                            hairClump2 = "5100000A";
                            hairClump3 = "5200000A";
                            hairClump4 = "5300000A";
                            head = 81;
                        }
                        break;
                }
                if (gender == 0)
                {
                    face = (10 * (face + 1)) - 7;
                }
                else
                {
                    /*switch (face)
                    {
                        case 0x9:
                            face = 65539;
                            break;
                        case 0xA:
                            face = 65549;
                            break;
                        case 0xB:
                            face = 65559;
                            break;
                        case 0xC:
                            face = 65569;
                            break;
                        case 0xD:
                            face = 65579;
                            break;
                        case 0xE:
                            face = 65589;
                            break;
                        case 0xF:
                            face = 65599;
                            break;
                        case 0x10:
                            face = 65609;
                            break;
                        case 0x11:
                            face = 65619;
                            break;
                    }*/
                    face = (10 * (face + 6545)) - 1;
                }

                bool NewCharCheckOK = checkNewCharacter(User, charid, face, head, gender, clothColor, skinColor, hairColor, hairClump1, hairClump2, hairClump3, hairClump4);
                //Console.WriteLine("New Char Head: {0}, Face: {1}", head, face);

                if (NewCharCheckOK)
                {
                    Client.SendAsync(new LoginNewCharacter_0x371_04(User, true));
                }
                else
                {
                    Client.SendAsync(new LoginNewCharacter_0x371_04(User, false));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void Handle_LoginDeleteCharacter(ClientConnection Client, PacketReader reader)
        {
            try
            {
                Account User = Client.CurrentAccount;
                getCharNumber(User);
                if (User.CharacterCount == 0)
                {
                    return;
                }

                bool DeleteCharCheckOK = checkDeleteCharacter(User);

                if (DeleteCharCheckOK)
                {
                    Client.SendAsync(new LoginDeleteCharacter_0x371_05(User));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void Handle_LoginGetCharacterNum(ClientConnection Client, PacketReader reader)
        {
            try
            {
                //TO DO STRUGARDEN
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void Handle_LoginUnknown1(ClientConnection Client, PacketReader reader)
        {
            try
            {
                Account User = Client.CurrentAccount;
                Client.SendAsync(new NP_Hex(User, "004903000001681C080600000084DB81D672020000"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void Handle_LoginCheckGlobalID(ClientConnection Client, PacketReader reader)
        {
            try
            {
                Account User = Client.CurrentAccount;
                User.CharacterPos = 0;
                Client.SendAsync(new LoginGlobalID_0x34F_00(User));
                //Client.SendAsync(new NP_Hex(User, "004F030000BA020100000000000000"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void Handle_LoginUnknown2(ClientConnection Client, PacketReader reader)
        {
            try
            {
                Account User = Client.CurrentAccount;
                //Client.SendAsync(new NP_Hex(User, "00EC010000000012001D00000032000000FFFFFFFF50000000A000000018000000260000003200000040000000C5642408000000000000000000000000000000006B0000004153453A31305F365F383A335F31355F31392F4153453A31305F365F31323A335F31395F39352F4153453A31305F365F323A335F3235345F31303930322D31303935312F4153453A31305F365F373A335F38395F302D332F4153453A31305F365F3100"));
                //Client.SendAsync(new NP_Hex(User, "00EC01000000000002AF0000006B000000FFFFFFFFDC000000DC00000018000000260000003200000040000000D2E72408000000000000000000000000000000006B0000004153453A31305F365F383A335F31355F31392F4153453A31305F365F31323A335F31395F39352F4153453A31305F365F323A335F3235345F31303930322D31303935312F4153453A31305F365F373A335F38395F302D332F4153453A31305F365F3100"));
                getMapInfo(User);
                getDecodedMapID(User);
                Client.SendAsync(new LoginMapInfo_0x1EC(User));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void Handle_LoginUnknown3(ClientConnection Client, PacketReader reader)
        {
            try
            {
                Account User = Client.CurrentAccount;
                Client.SendAsync(new LoginMapMusicInfo_0x31B(User));
                //Client.SendAsync(new NP_Hex(User, "001B0300000000000280000000020708C1B0C0FEB5F2C5C01B0201030586A084801304020805098FFCB1DC6E06098FFFC3914807098FFBE2D55C08098FFC89B45A1C098FFDD1F0781D098FFFFF91481E098FFCD9E6731F098FFCD9E673090264200464210481020B098FFFFFE1700C098FFFFEE9340D098FFBE3917A0E048FFBE180400F030010030022050000000000"));
                //Client.SendAsync(new NP_Hex(User, "001B0300000000120080000000020708C1B0C0FEB5F2C5C01B0201030586A084801304020805098FFCB1DC6E06098FFFC3914807098FFBE2D55C08098FFC89B45A1C098FFDD1F0781D098FFFFF91481E098FFCD9E6731F098FFCD9E673090264200464210481020B098FFFFFE1700C098FFFFEE9340D098FFBE3917A0E048FFBE180400F030010030022050000000000"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void Handle_LoginUnknown4(ClientConnection Client, PacketReader reader)
        {
            try
            {
                Account User = Client.CurrentAccount;
                Client.SendAsync(new NP_Hex(User, "00CB02000000000000080000000000058FFFFFFF7F000000"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void Handle_LoginUnknown5(ClientConnection Client, PacketReader reader)
        {
            try
            {
                Account User = Client.CurrentAccount;
                //Client.SendAsync(new NP_Hex(User, "00A0030000F700000002048442030490988003040710A5B6A5EBA5D6A5EAA5B9A4CEC7DBB2BC05048163060707616c616e6c65690704AE35080485300904030A04680B047F1C04030C0484B7D9CB330D071CB0ADCBE2A4CEA5D5A5EAA4B7A4C6C0DAA4EACEF6A4A4A4BFB2CEA4F20E04030F0710A5DFA5AFA5ECA5F3A5B7A5A2C4EBB9F110048B7611048B761204824F1304810614040015040C0C0C0A0A0A0A0A0A0A16000000"));
                Client.SendAsync(new LoginCharParam_0x3A0(User));
                if (Rooms.ExistRoom(User.UserMap.MapGlobalID))
                {
                    NormalRoom room = Rooms.GetRoom(User.UserMap.MapGlobalID);
                    room.EnterRoom(Client, "", 0x00);
                }
                else
                {
                    NormalRoom room = new NormalRoom();
                    room.setID(User.UserMap.MapGlobalID);
                    Rooms.AddRoom(room.ID, room);

                    room.EnterRoom(Client, "", 0x00);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void Handle_LoginReadDataCompleted(ClientConnection Client, PacketReader reader)
        {
            try
            {
                Account User = Client.CurrentAccount;
                Client.SendAsync(new NP_Hex(User, "002607000000000000000000000000"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void Handle_LoadAllItem(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;
            getEncodedItem(User);
            Client.SendAsync(new LoginLoadAllItem(User));
        }
        public static void Handle_MoveItem(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;
            ushort unk1 = reader.ReadLEUInt16();
            byte unk2 = reader.ReadByte();
            byte destItemPos = reader.ReadByte();
            byte movedItemPos = reader.ReadByte();

            int removeOldItem = 0;
            //check all item position is same as src and dest pos
            for (int i = 0; i < User.UserItem.Count; i++)
            {
                if (User.UserItem[i].ItemPos == movedItemPos)
                {
                    moveCharItem(User, User.UserItem[i], destItemPos);
                    //Console.WriteLine("Move From Position {0} to {1}", movedItemPos, destItemPos);
                    User.UserItem[i].ItemPos = destItemPos;
                    Client.SendAsync(new LoginMoveItem(User, User.UserItem[i], 0));
                }
                else if (User.UserItem[i].ItemPos == destItemPos)
                {
                    moveCharItem(User, User.UserItem[i], movedItemPos);
                    //Console.WriteLine("Move From Position {0} to {1}", movedItemPos, destItemPos);
                    User.UserItem[i].ItemPos = movedItemPos;
                    Client.SendAsync(new LoginMoveItem(User, User.UserItem[i], 0));
                    removeOldItem = 1;
                }
            }
            if (removeOldItem == 0)
            {
                Client.SendAsync(new LoginMovedItem(User, movedItemPos));
            }
        }
        public static void Handle_GetClientKey(ClientConnection Client, PacketReader reader)
        {
            try
            {
                byte[] clientpubkey = reader.ReadByteArray(255);
                var mahByteArray = new List<byte>();
                mahByteArray.AddRange(clientpubkey);
                mahByteArray.Add(0x00);
                //byte[] inkey = { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff, 0x00 };
                byte[] key = Encrypt.LoginGenKey(mahByteArray.ToArray(), Client.CurrentAccount.EncryptKey);
                Client.SendAsync(new LoginGenKey_0X12(Client.CurrentAccount, key));


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void Handle_LoginSuccess(ClientConnection Client, PacketReader reader, byte last)
        {
            int unknow = reader.ReadLEInt16();
            int clientVer = reader.ReadLEInt16();
            int iplen = reader.ReadLEInt32();
            string userip = reader.ReadStringSafe(iplen);
            int verlen = reader.ReadLEInt32();
            string version = reader.ReadStringSafe(verlen);

            //Console.WriteLine("Client IP: " + userip);
            //Console.WriteLine("Client Version: " + version);
            Account User = Client.CurrentAccount;
            if (Conf.HashCheck)
            {
                if (!CheckHashIsValid(version))
                {
                    Log.Info("InCorrect hash. {0} : {1}", User.UserID, version);
                    Client.SendAsync(new LoginError(User, 8, last));
                    return;
                }
            }
            bool logined = ClientConnection.CurrentAccounts.Count(players => players.Value.UserID == User.UserID) > 1;
            if (logined)
            {
                Log.Info("User [{0}]  has already logged in!", User.UserID);
                Client.SendAsync(new LoginError(User, 0x6, last));

                List<Account> objects = ClientConnection.CurrentAccounts.Values.ToList().FindAll(players => players.UserID == User.UserID);
                foreach (Account obj in objects)
                {
                    if (obj.Session != User.Session)
                    {
                        obj.Connection.SendAsync(new DisconnectPacket(obj, 257, last));
                    }
                }
                return;
            }
            int onlinecount = ClientConnection.CurrentAccounts.Count;
            if (onlinecount > Conf.MaxUserCount)
            {
                Log.Error("User [{0}] can't login because server full!", User.UserID);
                Client.SendAsync(new LoginError(User, 0x7, last));
                return;
            }
            if (CheckBlackList(User.UserID, out long startdate, out long enddate))
            {
                Client.SendAsync(new LoginBlackList(User, startdate, enddate, last));
                return;
            }

            Client.SendAsync(new NP_Byte(User, DBInit.Levels));
            Client.SendAsync(new LoginUserInfo_0X02(User, last)); //op 0x02

            //SendNoOpCode(StringToByteArray("02A800313DFFFF57640000E10C00000000000075030000000000000000000061AC0000FFFFFFFF0000000000000000CB781167B68B0600CB781167B68B0600CB781167B68B0600FFFFFFFFFFFFFFFFFFFF2836FFFFFFFF000000000000000000000000000000000000004F624C2667010000020023C3C0A801042C5E4878000000000100000000010000000000000000000000000000000020000000590200000000000000000000050000000400000000000000000000001414000000000000000000000000000000000002"));
            Client.SendAsync(new NP_Byte(User, DBInit.HackTools));
            Client.SendAsync(new NP_Hex(User, "06000000000002"));
            Client.SendAsync(new NP_Hex(User, "070000000002"));
            Client.SendAsync(new NP_Hex(User, "130000000002"));

            Client.SendAsync(new NP_Byte(User, DBInit.GameServerSetting));
            Client.SendAsync(new NP_Byte(User, DBInit.SmartChannelModeInfo)); //op 1297
            Client.SendAsync(new NP_Byte(User, DBInit.SmartChannelScheduleInfo)); //op 1298
            Client.SendAsync(new NP_Byte(User, DBInit.RoomKindPenaltyInfo)); //op 1301

            Client.SendAsync(new NP_Hex(User, "FF4205000000000000000002"));
            Client.SendAsync(new NP_Hex(User, "FF88010100000002"));

            //xtrap??
            //Client.SendAsync(new NP_Hex(User, "4201000000000000009744573A1B23AE0AA0122B0E8842434C621C9C4D232EDC72342AC06AC556E309A64AEC19990FF9220000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000006400000097003BB70030951A00200000ECFB87064EB3827C0000F1010030951A0030951A0000F101000052170000521700005C0834FC870641B8827C0020000000309501000000000000F1010030951A02"));
            Client.SendAsync(new NP_Hex(User, "FFF10500000000000000000000000002"));
            Client.SendAsync(new Myroom_CheckExistNewGift(User, last)); //new gift
            Client.SendAsync(new NP_Hex(User, "FF430601000000000000000000000000000000000000000000000000000002"));

            //SendNoOpCode(StringToByteArray("FF2D02872100005E8C0600C310000061080000BC0B0000160F000002")); //usp_IndividualRecordGetGame
            Client.SendAsync(new LoginUserIndividualRecordGameRecord_FF2D02(User, last)); //opcode 557
            //SendNoOpCode(StringToByteArray("FF2E02000074530000000013000000000000000000000000000000000000000000000002")); //usp_IndividualRecordGetMiscellaneous
            Client.SendAsync(new LoginUserIndividualRecordMiscellaneous_FF2E02(User, last)); //opcode 558

            MyRoomHandle.ExpiredItemCheck(User);
            //ShuSystemHandle.Shu_GetUserCharacterItemList(User, out var iteminfos, out var shuinfo, out var shuavatarinfo, out var shustatus);
            ShuSystemHandle.Shu_GetUserCharacterItemList(User, out var iteminfos);
            Client.SendAsync(new Shu_GetUserCharacterItemList(iteminfos, last));
            //Client.SendAsync(new NP_Hex(User, "FF2E050200000000000000000000000000000000000000000000000000000002")); //usp_shu_getUserCharacterItemList
            Client.SendAsync(new NP_Hex(User, "FFB8040000000002")); //usp_anubisExpeditionGetUserInfo
            Client.SendAsync(new NP_Hex(User, "6A000000000000000002"));

            getNickname(User);
            getUserCash(User);
            //SendNoOpCode(StringToByteArray("FFD2011800000000D8F7110000000000000000000100000015000000577269746520796F7572206661726D206E616D652E07000000706F706F424242C0F63AB612040000C0266DF93301000001000200000000000000000000000000000000000000000000000002"));
            Client.SendAsync(new LoginFarm_GetMyFarmInfo_FF1D02(User, last));//opcode 466

            Client.SendAsync(new NP_Hex(User, "FFB10400000000000000573A0000000000000100C0644C266701000002"));//time??

            Client.SendAsync(new NP_Hex(User, "FF9B0400000000000000000002"));
            Client.SendAsync(new NP_Hex(User, "FFA3040104000000500E0300510E0300520E0300530E030002")); //onRecvQuestEventNotify?
            Client.SendAsync(new NP_Hex(User, "FFA40400000000000000000002")); //onRecvLobbyQuestUserInfo?
            Client.SendAsync(new NP_Hex(User, "FFAC0401040000000100000002000000030000000400000002")); //onRecvLobbyQuestEventNotify?

            if (!User.noNickName)
            {
                Client.SendAsync(new LoginNickName_0X20(User, last)); //opcode 32
            }
            ShuSystemHandle.Shu_UserStatusInfo(User, out var shuststus);
            Client.SendAsync(new Shu_GetUserStatusInfo(shuststus, last));
            //Client.SendAsync(new NP_Hex(User, "FF2E0508000000000000000000000004")); //usp_shu_getUserStatusInfo
            getUserItemAttr(User);
            Client.SendAsync(new MyroomGetUserItemAttr(User, last)); //鍊金能力
            User.GetMyLevel();
            iteminfos.shuavatars.TryGetValue(User.CurrentShuID, out var avatarinfos);
            iteminfos.shuchars.TryGetValue(User.CurrentShuID, out var charinfo);
            iteminfos.shustatus.TryGetValue(User.CurrentShuID, out var statusinfo);
            ShuSystemHandle.UpdateUserShuInfo(User, User.CurrentShuID, avatarinfos, charinfo, statusinfo);
        }

        public static void Handle_GetCurrentAvatarInfo(ClientConnection Client, PacketReader preader, byte last)
        {
            Account User = Client.CurrentAccount;
            //getCurrentAvatarInfo(User);
            User.CurrentAvatarInfo.AddRange(Enumerable.Repeat((ushort)0, 45));
            updateCurrentAvatarInfo(User);
            //Client.SendAsync(new GetCurrentAvatarInfo_0X6D(User, last));
            Client.SendAsync(new GetCurrentAvatarInfo(User, last));
            User.AllItemAttr.Clear();
            MyRoomHandle.UpdateSetItemAttrInfo(User);
            MyRoomHandle.UpdateUserTotalAttr(User);
            MyRoomHandle.UpdateUserLuck(User);
            MyRoomHandle.UpdateAttackInfo(User);
        }

        public static void Handle_NOTIFY_MY_UDP(ClientConnection Client, PacketReader reader, byte last)
        {
            //Console.WriteLine("eServer_NOTIFY_MY_UDP_ACK");
            /*08 02 00 26 84 C0 A8 01 02 00 00 00 00 00 00 00 00 02 00
             26 84 C0 A8 01 02 00 00 00 00 00 00 00 00 02 00 23 C3
             C0 A8 01 04 2C 5E 48 78 00 00 00 00 D0*/
            Account User = Client.CurrentAccount;
            User.UDPInfo = reader.ReadByteArray(0x30);
            Client.SendAsync(new Login_NOTIFY_MY_UDP(last));
        }

        public static void Handle_GetNickName(ClientConnection Client, PacketReader reader, byte last)
        {
            Client.SendAsync(new LoginGetNickName_0X1C(Client.CurrentAccount, last));
        }

        public static void Handle_GetUserCash(ClientConnection Client, byte last)
        {
            //SendNoOpCodeString("FF7901000000000200000004");
            Client.SendAsync(new LoginGetUserCash_0X179(Client.CurrentAccount, last));
        }

        public static void Handle_FF4D01(ClientConnection Client, byte last)
        {
            //SendNoOpCodeString("FF4E010004");
            Client.SendAsync(new Login_FF4D01_0X14E(Client.CurrentAccount, last));
        }

        public static void Handle_6E(ClientConnection Client, byte last)
        {
            Client.SendAsync(new Login_6E_0X6F(Client.CurrentAccount, last));
        }

        public static void Handle_GetCommunityAgentServer(ClientConnection Client, byte last)
        {
            Client.SendAsync(new GetCommunityAgentServer_0X74(Client.CurrentAccount, last));
        }
        private static bool checkNewAccount(Account User, string gameid)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_checkNewAccount";
                cmd.Parameters.Add("userid", MySqlDbType.VarChar).Value = User.UserID;
                cmd.Parameters.Add("gameid", MySqlDbType.VarChar).Value = gameid;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                string result = reader["result"].ToString();
                cmd.Dispose();
                reader.Close();
                con.Close();
                if (result == "1")
                {
                    Console.WriteLine("GameID {0} is available", gameid);
                    return true;
                }
                else
                {
                    Console.WriteLine("GameID {0} is already used", gameid);
                    return false;
                }
            }
        }
        private static bool checkNewCharacter(Account User, string charid, int face, int head, int gender, byte[] clothColor, byte[] skinColor, byte[] hairColor, string hairClump1, string hairClump2, string hairClump3, string hairClump4)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_checkNewCharacter";
                cmd.Parameters.Add("gameid", MySqlDbType.VarChar).Value = User.GameID;
                if (User.UserType == 1)
                {
                    cmd.Parameters.Add("charid", MySqlDbType.VarChar).Value = charid;
                }
                else if(User.UserType == 0)
                {
                    cmd.Parameters.Add("charid", MySqlDbType.VarChar).Value = "免費玩家-" + charid;
                }
                //cmd.Parameters.Add("charChooseNum", MySqlDbType.Int32).Value = User.charChooseNum;
                cmd.Parameters.Add("face", MySqlDbType.Int32).Value = face;
                cmd.Parameters.Add("head", MySqlDbType.Int32).Value = head;
                cmd.Parameters.Add("gender", MySqlDbType.Int32).Value = gender;
                cmd.Parameters.Add("clothColor", MySqlDbType.VarChar).Value = Utility.ByteArrayToString(clothColor).ToLower();
                cmd.Parameters.Add("skinColor", MySqlDbType.VarChar).Value = Utility.ByteArrayToString(skinColor).ToLower();
                cmd.Parameters.Add("hairColor", MySqlDbType.VarChar).Value = Utility.ByteArrayToString(hairColor).ToLower();
                cmd.Parameters.Add("hairClump1", MySqlDbType.VarChar).Value = hairClump1;
                cmd.Parameters.Add("hairClump2", MySqlDbType.VarChar).Value = hairClump2;
                cmd.Parameters.Add("hairClump3", MySqlDbType.VarChar).Value = hairClump3;
                cmd.Parameters.Add("hairClump4", MySqlDbType.VarChar).Value = hairClump4;
                cmd.Parameters.Add("usertype", MySqlDbType.Int32).Value = User.UserType;

                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                string result = reader["result"].ToString();
                cmd.Dispose();
                reader.Close();
                con.Close();
                if (result == "1")
                {
                    Console.WriteLine("CharID {0} is available", charid);
                    return true;
                }
                else
                {
                    Console.WriteLine("CharID {0} is already used", charid);
                    return false;
                }
            }
        }
        private static bool checkDeleteCharacter(Account User)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_checkDeleteCharacter";
                cmd.Parameters.Add("gameid", MySqlDbType.VarChar).Value = User.GameID;
                cmd.Parameters.Add("globalid", MySqlDbType.Int32).Value = User.GlobalID;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                string result = reader["result"].ToString();
                cmd.Dispose();
                reader.Close();
                con.Close();
                if (result == "1")
                {
                    Console.WriteLine("Char {0} is deleted", User.CharacterNickname1);
                    return true;
                }
                else
                {
                    Console.WriteLine("Char {0} is unable to delete", User.CharacterNickname1);
                    return false;
                }
            }
        }
        private static int getCharNumber(Account User)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getCharCount";
                cmd.Parameters.Add("gameid", MySqlDbType.VarChar).Value = User.GameID;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.HasRows)
                {
                    reader.Read();
                    User.CharacterCount = Convert.ToInt32(reader["charCount"]);
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            return User.CharacterCount;
        }
        private static byte[] decodedMapID(string srcID)
        {
            byte[] mapID = new byte[4];
            string [] map = srcID.Split('_');

            byte[] mapByte = Encrypt.encodeMultiBytes(Convert.ToInt32(map[2]));
            mapID[0] = mapByte[0];
            if (mapByte.Length > 1)
            {
                mapID[1] = mapByte[1];
            }
            else
            {
                mapID[1] = 0x00;
            }
            mapID[2] = (byte)Convert.ToInt32(map[1]);
            mapID[3] = (byte)Convert.ToInt32(map[0]);
            return mapID;
        }
        private static byte[] decodedMaterialColor(byte[] encodedColor, Account User)
        {
            byte[] color = new byte[4];
            if (User.CharacterCount > 0)
            {
                int count = 0;
                int saveCount = 0;
                for (int i = 0; i < 3; i++)
                {
                    byte[] inChar = new byte[2];
                    byte outChar;


                    for (int j = 0; j < 2; j++)
                    {
                        inChar[j] = encodedColor[count]; //inChar[0] = 0x81, inChar[0] = 0x58
                        //Console.WriteLine("inChar Color: {0}", Utility.ByteArrayToString(inChar));
                        //Console.WriteLine(" ");
                        if (j == 1)
                        {
                            inChar[j] = (byte)(Convert.ToInt32(inChar[j]) & 0x7F);
                            inChar[0] = (byte)(Convert.ToInt32(inChar[0]) << 7);
                            outChar = (byte)Convert.ToInt32(inChar[0] + inChar[1]);
                            color[saveCount] = outChar;
                            saveCount++;
                        }
                        else
                        {
                            if (inChar[0] != 0x81)
                            {
                                inChar[j] = (byte)(Convert.ToInt32(inChar[j]) & 0x7F);
                                outChar = (byte)Convert.ToInt32(inChar[0]);
                                color[saveCount] = outChar;
                                saveCount++;
                                count++;
                                break;
                            }
                        }
                        count++;
                    }
                    //Console.WriteLine("Decoded Color: {0}", Utility.ByteArrayToString(color));
                    //Console.WriteLine(" ");
                }
            }
            return color;
        }
        private static byte[] getMaterialColor(PacketReader reader)
        {
            byte[] color = new byte[6];
            int count = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    byte inChar = reader.ReadByte();
                    color[count] = inChar;
                    count++;
                    if (inChar != 0x81)
                    {
                        break;
                    }
                }
            }
            byte[] newColor = new byte[count];
            Array.Copy(color, newColor, count);
            //Console.WriteLine("Color test: {0}", Utility.ByteArrayToString(newColor));
            //Console.WriteLine(" ");
            return newColor;
        }
        private static void getCharDecodedColor(Account User)
        {
            if (User.CharacterCount == 0)
            {
                return;
            }
            User.CharacterDecodedCloth = decodedMaterialColor(User.CharacterEncodedCloth, User);
            User.CharacterDecodedSkin = decodedMaterialColor(User.CharacterEncodedSkin, User);
            User.CharacterDecodedHair = decodedMaterialColor(User.CharacterEncodedHair, User);
        }
        private static void getCharEncodedColor(Account User)
        {
            if (User.CharacterCount == 0)
            {
                return;
            }
            User.CharacterEncodedCloth = StringToByteArrayFastest(User.CharacterEncodedClothColor);
            User.CharacterEncodedHair = StringToByteArrayFastest(User.CharacterEncodedHairColor);
            User.CharacterEncodedSkin = StringToByteArrayFastest(User.CharacterEncodedSkinColor);
        }
        public static void getDecodedMapID(Account User)
        {
            User.UserMap.decodedMapID = decodedMapID(User.UserMap.MapID);
        }
        private static void getEncodedItem(Account User)
        {
            if (User.CharacterCount == 0)
            {
                return;
            }
            for (int i = 0; i < User.UserItem.Count; i++)
            {
                User.UserItem[i].ItemEncodedID = Encrypt.encodeMultiBytes(User.UserItem[i].ItemDecodedID);
            }

        }
        private static void decodedMultiBytes(byte[] src)
        {
            //Console.WriteLine(Utility.ByteArrayToString(User.CharacterDecodedHairClump1));
            //Console.WriteLine(" ");
            int result = 0;
            for (int i = 0; i < 5; i++)
            {
                if (i == 0)
                {
                    result = ((src[i] - 0x80) << 7);
                }
                else if (i == 1)
                {
                    result = (((src[i] - 0x80) + result) << 7);
                }
                else if (i == 2)
                {
                    result = (((src[i] - 0x80) + result) << 7);
                }
                else if (i == 3)
                {
                    result = (((src[i] - 0x80) + result) << 7);
                }
                else if (i == 4)
                {
                    result = result + src[i];
                }
                Console.WriteLine("decodedMultiBytes: {0}", result);
            }
        }
        private static int decodedDynamicBytes(byte[] src)
        {
            int result = 0;
            for (int i = 0; i < src.Length; i++)
            {
                if (i == 0)
                {
                    if (src[i] > 0x7F)
                    {
                        result = ((src[i] - 0x80) << 7);
                    }
                    else
                    {
                        result = src[i];
                        break;
                    }
                }
                else if (i == 1)
                {
                    if (src[i] > 0x7F)
                    {
                        result = (((src[i] - 0x80) + result) << 7);
                    }
                    else
                    {
                        result = result + src[i];
                        break;
                    }
                }
                else if (i == 2)
                {
                    if (src[i] > 0x7F)
                    {
                        result = (((src[i] - 0x80) + result) << 7);
                    }
                    else
                    {
                        result = result + src[i];
                        break;
                    }
                }
                else if (i == 3)
                {
                    if (src[i] > 0x7F)
                    {
                        result = (((src[i] - 0x80) + result) << 7);
                    }
                    else
                    {
                        result = result + src[i];
                        break;
                    }
                }
                else if (i == 4)
                {
                    result = result + src[i];
                }
            }
            Console.WriteLine("decodedMultiBytes: {0}", result);
            return result;
        }
        /*private static byte[] encodeMultiBytes(int src)
        {
            int result = 0;
            int remainder = 0;
            int count = 0;

            if (src > 8000000)
            {
                byte[] cal = new byte[5];
                for (int i = 0; i < 5; i++)
                {
                    if (i == 0)
                    {
                        cal[4] = (byte)(src & 0x7F);
                        result = src - cal[4];
                    }
                    else if (i == 1)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[3] = (byte)(remainder + 0x80);
                        result = src - remainder;
                    }
                    else if (i == 2)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[2] = (byte)(remainder + 0x80);
                        result = src - remainder;
                    }
                    else if (i == 3)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[1] = (byte)(remainder + 0x80);
                        result = src - remainder;
                    }
                    else if (i == 4)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[0] = (byte)(remainder + 0x80);
                    }
                }
                //Console.WriteLine("encodeMultiBytes: {0}", Utility.ByteArrayToString(cal));
                //Console.WriteLine(" ");
                return cal;
            }
            else if (src > 60000)
            {
                byte[] cal = new byte[3];
                for (int i = 0; i < 3; i++)
                {
                    if (i == 0)
                    {
                        cal[2] = (byte)(src & 0x7F);
                        result = src - cal[2];
                    }
                    else if (i == 1)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[1] = (byte)(remainder + 0x80);
                        result = src - remainder;
                    }
                    else if (i == 2)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[0] = (byte)(remainder + 0x80);
                    }
                }
                Console.WriteLine("encodeMultiBytes: {0}", Utility.ByteArrayToString(cal));
                Console.WriteLine(" ");
                return cal;
            }
            else if (src > 127)
            {
                byte[] cal = new byte[2];
                for (int i = 0; i < 2; i++)
                {
                    if (i == 0)
                    {
                        cal[1] = (byte)(src & 0x7F);
                        result = src - cal[1];
                    }
                    else if (i == 1)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[0] = (byte)(remainder + 0x80);
                    }
                }
                Console.WriteLine("encodeMultiBytes: {0}", Utility.ByteArrayToString(cal));
                Console.WriteLine(" ");
                return cal;
            }
            else
            {
                byte[] cal = new byte[1];
                cal[0] = (byte)src;
                //Console.WriteLine("encodeMultiBytes: {0}", Utility.ByteArrayToString(cal));
                //Console.WriteLine(" ");
                return cal;
            }
        }*/
        private static byte[] encodedHairClump(byte[] clump)
        {
            //Console.WriteLine(BitConverter.ToInt32(User.CharacterDecodedHairClump1, 0));
            //Console.WriteLine(" ");
            byte[] cal = new byte[5];
            int result = 0;
            int src = 0;
            int remainder = 0;

            if (clump[0] == 0xFF)
            {
                byte[] noCal = new byte[1];
                noCal[0] = 0x00;
                return noCal;
            }
            for (int i = 0; i < 5; i++)
            {
                if (i == 0)
                {
                    src = BitConverter.ToInt32(clump, 0);
                    cal[4] = (byte)(src & 0x7F);
                    result = src - cal[4];
                }
                else if (i == 1)
                {
                    src = result >> 7;
                    remainder = (byte)(src & 0x7F);
                    cal[3] = (byte)(remainder + 0x80);
                    result = src - remainder;
                }
                else if (i == 2)
                {
                    src = result >> 7;
                    remainder = (byte)(src & 0x7F);
                    cal[2] = (byte)(remainder + 0x80);
                    result = src - remainder;
                }
                else if (i == 3)
                {
                    src = result >> 7;
                    remainder = (byte)(src & 0x7F);
                    cal[1] = (byte)(remainder + 0x80);
                    result = src - remainder;
                }
                else if (i == 4)
                {
                    //cal[0] = 0x80;
                    src = result >> 7;
                    remainder = (byte)(src & 0x7F);
                    cal[0] = (byte)(remainder + 0x80);
                }
            }
            Console.WriteLine("Encoded Hair Clump: {0}", Utility.ByteArrayToString(cal));
            Console.WriteLine(" ");
            return cal;
        }
        private static void getFoodItemInfo(Account User)
        {
            if (User.CharacterCount == 0)
            {
                return;
            }
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_getItemFoodInfo";
                    cmd.Parameters.Add("globalid", MySqlDbType.Int32).Value = User.GlobalID;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ItemAttr item = new ItemAttr
                                {
                                    ItemGlobalID = Convert.ToInt32(reader["fdGlobalItemID"]),
                                    ItemDecodedID = Convert.ToInt32(reader["fdItemID"]),
                                    ItemPos = Convert.ToInt32(reader["fdItemPos"]) - 1,
                                    ItemCount = Convert.ToInt32(reader["fdItemCount"]),
                                    ItemTypeNum = 1,
                                    ItemWear = 0,
                                    ItemName = reader["fdItemName"].ToString(),
                                    ItemDesc = reader["fdItemDesc"].ToString(),
                                    ItemType = reader["fdItemType"].ToString(),
                                    ItemValidDate = reader["fdItemValidDate"].ToString(),
                                    ItemUsageLimit = reader["fdItemUsageLimit"].ToString(),
                                    ItemCombinable = reader["fdItemCombinable"].ToString(),
                                };
                                for (int i = 0; i < 15; i++)
                                {
                                    item.ItemFoodAttackEffect[i] = Convert.ToInt32(reader["fdItemAttackEffect" + (i + 1).ToString()]);
                                }
                                for (int i = 0; i < 15; i++)
                                {
                                    item.ItemFoodDefenceEffect[i] = Convert.ToInt32(reader["fdItemDefenceEffect" + (i + 1).ToString()]);
                                }
                                User.UserItem.Add(item);
                            }
                        }
                        cmd.Dispose();
                        reader.Close();
                        con.Close();
                    }
                }
            }
        }
        private static void getEquItemInfo(Account User)
        {
            int RemainGender = 0;
            if (User.CharacterCount == 0)
            {
                return;
            }
            User.UserItem.Clear();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_getItemEquInfo";
                    cmd.Parameters.Add("globalid", MySqlDbType.Int32).Value = User.GlobalID;
                    cmd.Parameters.Add("gender", MySqlDbType.Int32).Value = User.Gender;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ItemAttr item = new ItemAttr
                                {
                                    ItemGlobalID = Convert.ToInt32(reader["fdGlobalItemID"]),
                                    ItemDecodedID = Convert.ToInt32(reader["fdItemID"]),
                                    ItemPos = Convert.ToInt32(reader["fdItemPos"]) - 1,
                                    ItemCount = Convert.ToInt32(reader["fdItemCount"]),
                                    ItemTypeNum = Convert.ToInt32(reader["fdItemTypeNum"]),
                                    ItemCorrect = Convert.ToInt32(reader["fdItemCorrect"]),
                                    ItemWear = Convert.ToInt32(reader["fdItemWear"]),
                                    ItemName = reader["fdItemName"].ToString(),
                                    ItemDesc = reader["fdItemDesc"].ToString(),
                                    ItemType = reader["fdItemType"].ToString(),
                                    ItemValidDate = reader["fdItemValidDate"].ToString(),
                                    ItemUsageLimit = reader["fdItemUsageLimit"].ToString(),
                                    ItemCombinable = reader["fdItemCombinable"].ToString(),
                                    ItemAppearance = Convert.ToInt32(reader["fdEqu"]),
                                    ItemWeight = Convert.ToInt32(reader["fdItemWeight"]),
                                    ItemDurability = Convert.ToInt32(reader["fdItemDurability"]),
                                    ItemPhysicalDamage = Convert.ToInt32(reader["fdItemPhysicalDamage"]),
                                    ItemMagicDamage = Convert.ToInt32(reader["fdItemMagicDamage"]),
                                };
                                for (int i = 0; i < 10; i++)
                                {
                                    item.ItemEquipmentAttackEffect[i] = Convert.ToInt32(reader["fdItemAttackEffect" + (i + 1).ToString()]);
                                }
                                for (int i = 0; i < 10; i++)
                                {
                                    item.ItemEquipmentDefenceEffect[i] = Convert.ToInt32(reader["fdItemDefenceEffect" + (i + 1).ToString()]);
                                }
                                User.UserItem.Add(item);
                            }
                        }
                        cmd.Dispose();
                        reader.Close();
                        con.Close();
                    }
                }
            }
            if(User.Gender == 1)
            {
                RemainGender = 65536;
            }
            for (int i = 0; i < 6; i++)
            {
                User.CharacterOneEquipment[i + 2] = (User.CharacterOneRawEquipment + i);
            }

            if (User.UserItem.Count(w => w.ItemWear == 1 && w.ItemTypeNum == 2) > 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    User.CharacterOneEquipment[i + 2] = (User.UserItem.Find(w => w.ItemWear == 1 && w.ItemTypeNum == 2).ItemAppearance + i + RemainGender);
                }
            }
            if (User.UserItem.Count(w => w.ItemWear == 1 && w.ItemTypeNum == 3) > 0) //上半身下半身
            {
                User.CharacterOneEquipment[2] = (User.UserItem.Find(w => w.ItemWear == 1 && w.ItemTypeNum == 3).ItemAppearance + RemainGender);
                User.CharacterOneEquipment[5] = (User.UserItem.Find(w => w.ItemWear == 1 && w.ItemTypeNum == 3).ItemAppearance + 3 + RemainGender);
            }
            if (User.UserItem.Count(w => w.ItemWear == 1 && w.ItemTypeNum == 4) > 0) //上半身
            {
                User.CharacterOneEquipment[2] = (User.UserItem.Find(w => w.ItemWear == 1 && w.ItemTypeNum == 4).ItemAppearance + RemainGender);
            }
            if (User.UserItem.Count(w => w.ItemWear == 1 && w.ItemTypeNum == 5) > 0) //下半身
            {
                User.CharacterOneEquipment[5] = (User.UserItem.Find(w => w.ItemWear == 1 && w.ItemTypeNum == 5).ItemAppearance + RemainGender);
            }
            if (User.UserItem.Count(w => w.ItemWear == 1 && w.ItemTypeNum == 6) > 0) //手
            {
                User.CharacterOneEquipment[3] = (User.UserItem.Find(w => w.ItemWear == 1 && w.ItemTypeNum == 6).ItemAppearance + RemainGender);
                User.CharacterOneEquipment[4] = (User.UserItem.Find(w => w.ItemWear == 1 && w.ItemTypeNum == 6).ItemAppearance + 1 + RemainGender);
            }
            if (User.UserItem.Count(w => w.ItemWear == 1 && w.ItemTypeNum == 7) > 0) //鞋
            {
                for (int i = 0; i < 2; i++)
                {
                    User.CharacterOneEquipment[i + 6] = (User.UserItem.Find(w => w.ItemWear == 1 && w.ItemTypeNum == 7).ItemAppearance + i + RemainGender);
                }
            }
        }
        private static bool moveCharItem(Account User, ItemAttr UserItem, int destItemPos)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_moveCharItem";
                cmd.Parameters.Add("globalid", MySqlDbType.Int32).Value = User.GlobalID;
                cmd.Parameters.Add("itemid", MySqlDbType.Int32).Value = UserItem.ItemDecodedID;
                cmd.Parameters.Add("globalitemid", MySqlDbType.Int32).Value = UserItem.ItemGlobalID;
                cmd.Parameters.Add("destItemPos", MySqlDbType.Int32).Value = (destItemPos + 1);
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                string result = reader["result"].ToString();
                cmd.Dispose();
                reader.Close();
                con.Close();
                if (result == "1")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private static void getCharBasicInfo(Account User)
        {
            if (User.CharacterCount == 0)
            {
                User.GlobalID = 0;
                return;
            }
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_getCharBasicInfo";
                    cmd.Parameters.Add("gameid", MySqlDbType.VarChar).Value = User.GameID;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (User.CharacterCount < 2)
                                {
                                    User.CharacterPos = 1;
                                }
                                else
                                {
                                    User.CharacterPos = Convert.ToInt32(reader["fdStoredPos"]);
                                }
                                User.GlobalID = Convert.ToInt32(reader["fdGlobalID"]);

                                User.CharacterEncodedClothColor = reader["fdEncodedCloth"].ToString();
                                User.CharacterEncodedHairColor = reader["fdEncodedHair"].ToString();
                                User.CharacterEncodedSkinColor = reader["fdEncodedSkin"].ToString();
                                User.CharacterDecodedHairClump1 = StringToByteArrayFastest(reader["fdHairClump1"].ToString());
                                User.CharacterDecodedHairClump2 = StringToByteArrayFastest(reader["fdHairClump2"].ToString());
                                User.CharacterDecodedHairClump3 = StringToByteArrayFastest(reader["fdHairClump3"].ToString());
                                User.CharacterDecodedHairClump4 = StringToByteArrayFastest(reader["fdHairClump4"].ToString());
                                User.UserMap.MapGlobalID = Convert.ToInt32(reader["fdMapID"]);
                                User.GamePosX = (byte)Convert.ToInt32(reader["fdMapX"]);
                                User.GamePosY = (byte)Convert.ToInt32(reader["fdMapY"]);
                                User.Gender = Convert.ToInt32(reader["fdGender"]);
                                if (User.Gender == 1)
                                {
                                    User.CharacterOneRawEquipment = 65540;
                                }
                                else
                                {
                                    User.CharacterOneRawEquipment = 4;
                                }

                                //1. decode hair color
                                //decodedMaterialColor(StringToByteArrayFastest("81483840"));
                                //2. decode hair clump & item
                                //decodedMultiBytes(StringToByteArrayFastest("86A880845F"));     //86A880845F->6500025F
                                //decodedMultiBytes(StringToByteArrayFastest("80D0848032"));     //80D0848032->0A010032
                                //decodedMultiBytes(StringToByteArrayFastest("86B08C802B"));
                                //decodedDynamicBytes(StringToByteArrayFastest("86A0808836"));
                                //1. encode hair clump & item & char equip
                                //encodeMultiBytes(167837746);                                   //0A010032->167837746 ->80D0848032
                                //encodeMultiBytes(1694499423);                                  //6500025F->1694499423->86A880845F
                                //encodeMultiBytes(5941);
                                //2. encode hair clump
                                //encodedHairClump(StringToByteArrayFastest("3200010A"));        //3200010A->0A010032->80D0848032

                                if (User.CharacterPos == 1)
                                {
                                    User.CharacterZula1 = Convert.ToInt32(reader["fdZula"]);
                                    User.CharacterNation1 = Convert.ToInt32(reader["fdNation"]);
                                    User.CharacterJob1 = Convert.ToInt32(reader["fdJob"]);
                                    User.CharacterLevel1 = Convert.ToInt32(reader["fdLevel"]);
                                    User.CharacterNickname1 = reader["fdCharName"].ToString();
                                    for (int i = 0; i < 2; i++)
                                    {
                                        User.CharacterOneEquipment[i] = Convert.ToInt32(reader["fdEqu" + (i+1).ToString()]);
                                    }
                                }
                                else
                                {
                                    User.CharacterZula2 = Convert.ToInt32(reader["fdZula"]);
                                    User.CharacterNation2 = Convert.ToInt32(reader["fdNation"]);
                                    User.CharacterJob2 = Convert.ToInt32(reader["fdJob"]);
                                    User.CharacterLevel2 = Convert.ToInt32(reader["fdLevel"]);
                                    User.CharacterNickname2 = reader["fdCharName"].ToString();
                                }
                                User.CharacterPos = Convert.ToInt32(reader["fdStoredPos"]);
                            }
                        }
                        cmd.Dispose();
                        reader.Close();
                        con.Close();
                    }
                }
            }
        }
        public static void getMapInfo(Account User)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_getMapInfo";
                    cmd.Parameters.Add("mapid", MySqlDbType.VarChar).Value = User.UserMap.MapGlobalID;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                User.UserMap.MapID = reader["fdMapID"].ToString();
                                User.UserMap.MapName = reader["fdMapName"].ToString();
                                User.UserMap.MapMusic = reader["fdMapMusic"].ToString();
                                User.UserMap.MapWidth = Convert.ToInt32(reader["fdMapWidth"]);
                                User.UserMap.MapHeight = Convert.ToInt32(reader["fdMapHeight"]);
                                User.UserMap.MapCRC = reader["fdMapCRC"].ToString();
                            }
                        }
                        cmd.Dispose();
                        reader.Close();
                        con.Close();
                    }
                }
            }
            if (Map.SubTeleportList.Count(t => t.MapGlobalID == User.UserMap.MapGlobalID) > 0)
            {
                User.UserMap.TeleportList = Map.SubTeleportList.Find(t => t.MapGlobalID == User.UserMap.MapGlobalID).SubList;
            }
            else
            {
                User.UserMap.TeleportList = null;
            }
        }
        private static void CheckGlobalID(Account User)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getGlobalID";
                cmd.Parameters.Add("gameid", MySqlDbType.VarChar).Value = User.GameID;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.HasRows)
                {
                    reader.Read();
                    User.GlobalID = Convert.ToInt32(reader["fdGlobalID"]);
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
        private static void CheckGameID(Account User)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getGameID";
                cmd.Parameters.Add("userid", MySqlDbType.VarChar).Value = User.UserID;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.HasRows)
                {
                    reader.Read();
                    User.GameID = reader["fdGameID"].ToString();
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
        private static void getUserType(Account User)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getUserType";
                cmd.Parameters.Add("userid", MySqlDbType.VarChar).Value = User.UserID;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.HasRows)
                {
                    reader.Read();
                    User.UserType = Convert.ToInt32(reader["fdUserType"]);
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
        private static bool checkUserAccount(string userid, string password)
        {

            try
            {
                using (var con = new MySqlConnection(Conf.Connstr))
                {

                    con.Open();
                    var cmd = new MySqlCommand(string.Empty, con);
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_checkUserAccount";
                    cmd.Parameters.Add("userid", MySqlDbType.VarChar).Value = userid;
                    cmd.Parameters.Add("password", MySqlDbType.VarChar).Value = password;
                    MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

                    reader.Read();
                    string result = reader["result"].ToString();
                    cmd.Dispose();
                    reader.Close();
                    con.Close();
                    if (result == "1")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        private static void getNickname(Account User)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getNickname";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.HasRows)
                {
                    reader.Read();
                    if (Convert.IsDBNull(reader["fdNickname"]))
                    {
                        User.noNickName = true;
                    }
                    else
                    {
                        User.NickName = reader["fdNickname"].ToString();
                        User.noNickName = false;
                    }
                }
                else
                {
                    User.noNickName = true;
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
        private static void getUserCash(Account User)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getUserCash";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.HasRows)
                {
                    reader.Read();
                    User.Cash = Convert.ToInt32(reader["cash"]);
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
        private static void getUserItemAttr(Account User)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_getUserItemAttr";
                    cmd.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                int resultitem = Convert.ToInt32(reader["ItemDescNum"]);
                                ItemAttr attr = new ItemAttr
                                {
                                    Attr = Convert.ToUInt16(reader["AttrType"]),
                                    AttrValue = Convert.ToSingle(reader["AttrValue"])
                                };
                                User.UserItemAttr.AddOrUpdate(resultitem, new List<ItemAttr> { attr }, (k, v) => { v.Add(attr); return v; });
                            }
                        }
                    }
                }
            }
        }
        /*
        public static void getCurrentAvatarInfo(Account User)
        {
            User.WearAvatarItem.Clear();
            User.WearCosAvatarItem.Clear();
            User.WearFashionItem.Clear();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getCurrentAvatarInfo";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.HasRows)
                {
                    Stopwatch runTime = new Stopwatch();
                    runTime.Start();
                    User.haveAvatar = true;
                    reader.Read();
                    byte charid = Convert.ToByte(reader.GetValue(0));
                    for (int i = 0; i < 45; i++)
                    {
                        ushort itemkind = Convert.ToUInt16(reader.GetValue(i));
                        ushort itemkind2 = (i == 0 || i == 15 || i == 30) ? (ushort)0 : itemkind;
                        User.CurrentAvatarInfo.Add(itemkind);
                        if (itemkind > 0)
                        {
                            if (i >= 0 && i < 15)
                            {
                                int itemnum = ItemHolder.ItemCPKInfos.FirstOrDefault(k => (k.Value.ItemChar == charid || k.Value.ItemChar == 0)
                                     && k.Value.ItemPosition == i && k.Value.ItemKind == itemkind2).Key;
                                User.WearAvatarItem.Add(itemnum);
                                int itemnum2 = 0;
                                if (ItemHolder.ItemPCKDict.TryGetValue((ushort)i, 0, itemkind2, out itemnum2))
                                    User.WearAvatarItem.Add(itemnum2);
                                else if (ItemHolder.ItemPCKDict.TryGetValue((ushort)i, charid, itemkind2, out itemnum2))
                                    User.WearAvatarItem.Add(itemnum2);
                            }
                            else if (i >= 15 && i < 30)
                            {
                                int j = i - 15;
                                int itemnum = ItemHolder.ItemCPKInfos.FirstOrDefault(k => (k.Value.ItemChar == charid || k.Value.ItemChar == 0)
                                    && k.Value.ItemPosition == j && k.Value.ItemKind == itemkind2).Key;
                                User.WearCosAvatarItem.Add(itemnum);
                                int itemnum2 = 0;
                                if (ItemHolder.ItemPCKDict.TryGetValue((ushort)j, 0, itemkind2, out itemnum2))
                                    User.WearCosAvatarItem.Add(itemnum2);
                                else if (ItemHolder.ItemPCKDict.TryGetValue((ushort)j, charid, itemkind2, out itemnum2))
                                    User.WearCosAvatarItem.Add(itemnum2);
                            }
                            else if (i >= 30 && i < 45)
                            {
                                int x = i - 30;
                                int itemnum = ItemHolder.ItemCPKInfos.FirstOrDefault(k => (k.Value.ItemChar == charid || k.Value.ItemChar == 0)
                                    && k.Value.ItemPosition == x && k.Value.ItemKind == itemkind2).Key;
                                User.WearFashionItem.Add(itemnum);
                                int itemnum2 = 0;
                                if (ItemHolder.ItemPCKDict.TryGetValue((ushort)x, 0, itemkind2, out itemnum2))
                                    User.WearFashionItem.Add(itemnum2);
                                else if (ItemHolder.ItemPCKDict.TryGetValue((ushort)x, charid, itemkind2, out itemnum2))
                                    User.WearFashionItem.Add(itemnum2);
                            }
                        }
                    }
                    //runTime.Stop();
                    Log.Debug("getCurrentAvatarInfo time: {0}ms", runTime.Elapsed.TotalMilliseconds);
                    runTime.Reset();
                    User.costumeMode = Convert.ToInt16(reader["costumeMode"]);
                    User.isFashionModeOn = Convert.ToBoolean(reader["fashionMode"]);
                }
                else
                {
                    //ns.Write(0x3D); //3D 00 00 00 new ac?
                    User.haveAvatar = false;
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
        */
        public static void updateCurrentAvatarInfo(Account User)
        {
            User.WearAvatarItem.Clear();
            User.WearCosAvatarItem.Clear();
            User.WearFashionItem.Clear();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_getCurrentAvatarInfo";
                    cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.HasRows)
                        {
                            User.haveAvatar = true;
                            reader.Read();
                            byte charid = Convert.ToByte(reader.GetValue(0));
                            for (int i = 0; i < 45; i++)
                            {
                                ushort itemkind = Convert.ToUInt16(reader.GetValue(i));
                                ushort itemkind2 = (i == 0 || i == 15 || i == 30) ? (ushort)0 : itemkind;
                                User.CurrentAvatarInfo[i] = itemkind;
                                if (itemkind > 0)
                                {
                                    if (i >= 0 && i < 15)
                                    {
                                        /*int itemnum = ItemHolder.ItemCPKInfos.FirstOrDefault(k => (k.Value.ItemChar == charid || k.Value.ItemChar == 0)
                                             && k.Value.ItemPosition == i && k.Value.ItemKind == itemkind2).Key;*/
                                        int itemnum = 0;
                                        if (ItemHolder.ItemPCKDict.TryGetValue((ushort)i, 0, itemkind2, out itemnum))
                                            User.WearAvatarItem.Add(itemnum);
                                        else if (ItemHolder.ItemPCKDict.TryGetValue((ushort)i, charid, itemkind2, out itemnum))
                                            User.WearAvatarItem.Add(itemnum);
                                    }
                                    else if (i >= 15 && i < 30)
                                    {
                                        int j = i - 15;
                                        /*int itemnum = ItemHolder.ItemCPKInfos.FirstOrDefault(k => (k.Value.ItemChar == charid || k.Value.ItemChar == 0)
                                            && k.Value.ItemPosition == j && k.Value.ItemKind == itemkind2).Key;*/
                                        int itemnum = 0;
                                        if (ItemHolder.ItemPCKDict.TryGetValue((ushort)j, 0, itemkind2, out itemnum))
                                            User.WearCosAvatarItem.Add(itemnum);
                                        else if (ItemHolder.ItemPCKDict.TryGetValue((ushort)j, charid, itemkind2, out itemnum))
                                            User.WearCosAvatarItem.Add(itemnum);
                                    }
                                    else if (i >= 30 && i < 45)
                                    {
                                        int x = i - 30;
                                        /* int itemnum = ItemHolder.ItemCPKInfos.FirstOrDefault(k => (k.Value.ItemChar == charid || k.Value.ItemChar == 0)
                                             && k.Value.ItemPosition == x && k.Value.ItemKind == itemkind2).Key;*/
                                        int itemnum = 0;
                                        if (ItemHolder.ItemPCKDict.TryGetValue((ushort)x, 0, itemkind2, out itemnum))
                                            User.WearFashionItem.Add(itemnum);
                                        else if (ItemHolder.ItemPCKDict.TryGetValue((ushort)x, charid, itemkind2, out itemnum))
                                            User.WearFashionItem.Add(itemnum);
                                    }
                                }
                            }
                            User.costumeMode = Convert.ToInt16(reader["costumeMode"]);
                            User.isFashionModeOn = Convert.ToBoolean(reader["fashionMode"]);
                        }
                        else
                        {
                            //ns.Write(0x3D); //3D 00 00 00 new ac?
                            User.haveAvatar = false;
                        }
                        cmd.Dispose();
                        reader.Close();
                    }
                }
            }
        }

        private static bool CheckBlackList(string userid, out long startdate, out long enddate)
        {
            startdate = 0;
            enddate = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_login_blacklistcheck";
                    cmd.Parameters.Add("userid", MySqlDbType.VarString).Value = userid;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            startdate = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["blackliststart"]));
                            enddate = Utility.ConvertToTimestamp(Convert.ToDateTime(reader["blacklistend"]));
                            return true;
                        }
                        else
                            return false;
                    }
                }
            }
        }

        private static bool CheckHashIsValid(string hash)
        {
            List<string> allhashes = File.ReadAllLines("hash.ini").ToList();
            return allhashes.Exists(serverhash => serverhash == hash);
        }
        private static string pwdecode(byte[] pw)
        {
            int pwlen = pw.Length / 4;
            int first, second, sbefore = 0, final;
            string pwstr = "";

            for (int i = 0; i < pwlen; i++)
            {
                if (pw[0 + i * 4] < pw[1 + i * 4])
                {
                    //first = Convert.ToInt32(pw[1 + i * 4]) - Convert.ToInt32(pw[0 + i * 4]);
                    first = pw[1 + i * 4] - pw[0 + i * 4];
                }
                else
                {
                    //first = ((0x100 + Convert.ToInt32(pw[1 + i * 4])) - Convert.ToInt32(pw[0 + i * 4]));
                    first = (0x100 + pw[1 + i * 4]) - pw[0 + i * 4];
                }
                if (pw[2 + i * 4] == 0xFF)
                {
                    //second = ((Convert.ToInt32(pw[2 + i * 4]) + 1) - (Convert.ToInt32(pw[2 + i * 4]) - Convert.ToInt32(pw[3 + i * 4]))) * 0x100;
                    sbefore = second = ((pw[2 + i * 4] + 1) - (pw[2 + i * 4] - pw[3 + i * 4])) * 0x100;
                    second = (second >> 0xB) << 0xB;
                    sbefore -= second;
                }
                else
                {
                    //second = (Convert.ToInt32(pw[3 + i * 4]) - Convert.ToInt32(pw[2 + i * 4])) * 0x100;
                    sbefore = second = (pw[3 + i * 4] - pw[2 + i * 4]) * 0x100;
                    second = (second >> 0xB) << 0xB;
                    sbefore -= second;
                }
                final = (first + sbefore) >> 4;
                pwstr += (char)Convert.ToByte(final);
            }
            return pwstr;
        }
        private static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        public static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }
        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}
