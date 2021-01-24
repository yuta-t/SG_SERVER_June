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
using System.Reflection;
using System.Security.Cryptography;

namespace AgentServer.Packet
{
    public class LoginHandle
    {
        public static void Handle_LoginCheck(ClientConnection Client, PacketReader reader)
        {
            //int packetlength = reader.ReadLEInt16();
            //reader.ReadByte(); //opcode
            try
            {
                if (!ServerStatus.isReady)
                {
                    Log.Info("connection blocked by current setting ServerReady off");
                    Client.SendAsync(new ServerNotReady());
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
                    Client.CurrentAccount = nCurrent;
                    //ClientConnection.CurrentAccounts.TryAdd(cookie, Client.CurrentAccount);
                    ClientConnection.CurrentAccounts[Client.session] = nCurrent;

                }
                else
                {
                    Console.WriteLine("Login Fail!");
                }
                Client.SendAsync(new LoginServerTime_0X41());
                Client.SendAsync(new LoginCheck_0X10(userid, UserCheckOK));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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
                Client.SendAsync(new LoginGenKey_0X12(key));

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
            bool logined = ClientConnection.CurrentAccounts.ToList().FindAll(players => players.Value.UserID == User.UserID).Count > 1;
            if (logined)
            {
                Log.Info("User [{0}]  has already logged in!", User.UserID);
                Client.SendAsync(new LoginError(User, 0x6, last));
                return;
            }
            int onlinecount = ClientConnection.CurrentAccounts.Count;
            if (onlinecount >= Conf.MaxUserCount)
            {
                Log.Error("User [{0}] can't login because server full!", User.UserID);
                Client.SendAsync(new LoginError(User, 0x7, last));
                return;
            }
            if(CheckBlackList(User.UserID, out long startdate, out long enddate))
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
            Client.SendAsync(new NP_Hex(User, "FF2E050200000000000000000000000000000000000000000000000000000002")); //shu info?
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

            Client.SendAsync(new NP_Hex(User, "FF2E0508000000000000000000000004"));
            getUserItemAttr(User);
            Client.SendAsync(new GetUserItemAttr(User, last)); //鍊金能力

        }

        public static void Handle_GetCurrentAvatarInfo(ClientConnection Client, PacketReader preader, byte last)
        {
            Account User = Client.CurrentAccount;
            getCurrentAvatarInfo(User);
            //Client.SendAsync(new GetCurrentAvatarInfo_0X6D(User, last));
            Client.SendAsync(new GetCurrentAvatarInfo(User, last));
           MyRoomHandle.UpdateUserLuck(User);
        }

        public static void Handle_UDP(ClientConnection Client, byte last)
        {
            Client.SendAsync(new LoginUDP_0X09(Client.CurrentAccount, last));
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

        public static void Handle_FF2E0505(ClientConnection Client, byte last)
        {
            //SendNoOpCodeString("FF2E050500000000000000000000000000000040");
            Client.SendAsync(new Login_FF2E0505(Client.CurrentAccount, last));
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
        public static void getUserCash(Account User)
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

        public static void getCurrentAvatarInfo(Account User)
        {
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
                    User.haveAvatar = true;
                    reader.Read();
                    byte charid = Convert.ToByte(reader.GetValue(0));
                    for (int i = 0; i < 30; i++)
                    {
                        //byte charid = Convert.ToByte(reader.GetValue(0));
                        short itemkind = Convert.ToInt16(reader.GetValue(i));
                        User.CurrentAvatarInfo.Add(itemkind);
                        if (itemkind > 0)
                        {
                            if (i > 0 && i < 15)
                            {
                                //User.CurrentAvatarItemNum.Add(ItemHolder.ItemCPKInfos.FirstOrDefault(k => (k.Value.ItemChar == charid || k.Value.ItemChar == 0) && k.Value.ItemPosition == i && k.Value.ItemKind == Convert.ToInt16(reader.GetValue(i))).Key);
                                int itemnum = ItemHolder.ItemCPKInfos.FirstOrDefault(k => (k.Value.ItemChar == charid || k.Value.ItemChar == 0)
                                     && k.Value.ItemPosition == i && k.Value.ItemKind == Convert.ToInt16(reader.GetValue(i))).Key;
                                if (ItemHolder.ItemAttrCollections.TryGetValue(itemnum, out var attr) && !attr.Exists(a => a.Attr == 5001))
                                    User.WearAvatarItemAttr.AddRange(attr);
                                else if (User.UserItemAttr.TryGetValue(itemnum, out var attr2) && !attr2.Exists(a => a.Attr == 5001))
                                    User.WearAvatarItemAttr.AddRange(attr2);
                            }
                            else if (i > 15 && i < 30)
                            {
                                int j = i - 15;
                                //User.CurrentCosAvatarItemNum.Add(ItemHolder.ItemCPKInfos.FirstOrDefault(k => (k.Value.ItemChar == charid || k.Value.ItemChar == 0) && k.Value.ItemPosition == j && k.Value.ItemKind == Convert.ToInt16(reader.GetValue(i))).Key);
                                int itemnum = ItemHolder.ItemCPKInfos.FirstOrDefault(k => (k.Value.ItemChar == charid || k.Value.ItemChar == 0)
                                    && k.Value.ItemPosition == j && k.Value.ItemKind == Convert.ToInt16(reader.GetValue(i))).Key;
                                if (ItemHolder.ItemAttrCollections.TryGetValue(itemnum, out var attr) && attr.Exists(a => a.Attr == 5001))
                                    User.WearCosAvatarItemAttr.AddRange(attr);
                                else if (User.UserItemAttr.TryGetValue(itemnum, out var attr2) && attr2.Exists(a => a.Attr == 5001))
                                    User.WearCosAvatarItemAttr.AddRange(attr2);
                            }
                        }
                    }          
                    User.costumeMode = Convert.ToInt16(reader["costumeMode"]);
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
            /*User.CurrentAvatarItemNum.Remove(0);
            User.CurrentCosAvatarItemNum.Remove(0);*/
        }
        public static void updateCurrentAvatarInfo(Account User)
        {
            //User.CurrentAvatarItemNum.Clear();
            //User.CurrentCosAvatarItemNum.Clear();
            User.WearAvatarItemAttr.Clear();
            User.WearCosAvatarItemAttr.Clear();
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
                    User.haveAvatar = true;
                    reader.Read();
                    byte charid = Convert.ToByte(reader.GetValue(0));
                    for (int i = 0; i < 30; i++)
                    {
                        short itemkind = Convert.ToInt16(reader.GetValue(i));
                        User.CurrentAvatarInfo[i] = itemkind;
                        if (itemkind > 0)
                        {
                            if (i > 0 && i < 15)
                            {
                                //User.CurrentAvatarItemNum.Add(ItemHolder.ItemCPKInfos.FirstOrDefault(k => (k.Value.ItemChar == charid || k.Value.ItemChar == 0) && k.Value.ItemPosition == i && k.Value.ItemKind == Convert.ToInt16(reader.GetValue(i))).Key);
                                int itemnum = ItemHolder.ItemCPKInfos.FirstOrDefault(k => (k.Value.ItemChar == charid || k.Value.ItemChar == 0)
                                     && k.Value.ItemPosition == i && k.Value.ItemKind == Convert.ToInt16(reader.GetValue(i))).Key;
                                if (ItemHolder.ItemAttrCollections.TryGetValue(itemnum, out var attr) && !attr.Exists(a => a.Attr == 5001))
                                    User.WearAvatarItemAttr.AddRange(attr);
                                else if (User.UserItemAttr.TryGetValue(itemnum, out var attr2) && !attr2.Exists(a => a.Attr == 5001))
                                    User.WearAvatarItemAttr.AddRange(attr2);
                            }
                            else if (i > 15 && i < 30)
                            {
                                int j = i - 15;
                                //User.CurrentCosAvatarItemNum.Add(ItemHolder.ItemCPKInfos.FirstOrDefault(k => (k.Value.ItemChar == charid || k.Value.ItemChar == 0) && k.Value.ItemPosition == j && k.Value.ItemKind == Convert.ToInt16(reader.GetValue(i))).Key);
                                int itemnum = ItemHolder.ItemCPKInfos.FirstOrDefault(k => (k.Value.ItemChar == charid || k.Value.ItemChar == 0)
                                    && k.Value.ItemPosition == j && k.Value.ItemKind == Convert.ToInt16(reader.GetValue(i))).Key;
                                if (ItemHolder.ItemAttrCollections.TryGetValue(itemnum, out var attr) && attr.Exists(a => a.Attr == 5001))
                                    User.WearCosAvatarItemAttr.AddRange(attr);
                                else if (User.UserItemAttr.TryGetValue(itemnum, out var attr2) && attr2.Exists(a => a.Attr == 5001))
                                    User.WearCosAvatarItemAttr.AddRange(attr2);
                            }
                        }
                    }
                    User.costumeMode = Convert.ToInt16(reader["costumeMode"]);
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

    }
}
