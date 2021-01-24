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
using AgentServer.Structuring.User;
using System.Collections.Generic;
using LocalCommons.Logging;

namespace AgentServer.Packet
{
    public class LobbyHandle
    {
        public static void Handle_ShowPage(ClientConnection Client, PacketReader reader, byte last)
        {
            byte type = reader.ReadByte();
            Client.SendAsync(new ShowPage(Client.CurrentAccount, type, last));
        }

        public static void HandlePingTime(ClientConnection Client, byte last)
        {
            Account User = Client.CurrentAccount;
            long CurrentTime = Utility.CurrentTimeMilliseconds();
            Client.SendAsync(new PingTime_0X41(CurrentTime, last));
            if (User.isLogin && CurrentTime > User.LastCheckTime + 60000) //60s
            {
                User.LastCheckTime = CurrentTime;
                var expireitem = User.AvatarItems.Where(w => w.Value.expireTime != 0 && w.Value.expireTime < CurrentTime).ToDictionary(d => d.Key, d => d.Value);
                if (expireitem.Count > 0)
                {
                    foreach (var item in expireitem)
                    {
                        ItemHolder.ItemShopInfos.TryGetValue(item.Key, out var info);
                        if (!info.NotDeleteWhenExpired)
                            User.AvatarItems.TryRemove(item.Key, out _);
                    }
                    MyRoomHandle.getActiveFuncItem(User);
                    Client.SendAsync(new ExpiredItemInfo(expireitem.Values.FirstOrDefault(), last));
                }
            }
        }

        public static void HandleSinglePlay(ClientConnection Client, PacketReader reader, byte last)
        {
            int mapnum = reader.ReadLEInt32();
            //Console.WriteLine("mapnum: {0}", mapnum);
            MapHolder.MapInfos.TryGetValue(mapnum, out MapInfo mapinfo);
            if (mapinfo.CanTimeAttack)
                Client.SendAsync(new SinglePlay_0X1D4(Client.CurrentAccount, mapnum, last));
        }

        public static void Handle_GetUserInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);

            Client.SendAsync(new GetUserInfo(User, nickname, last));
        }

        public static void Handle_RightClickItemInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 98 05 F8 64 00 00 01
            Account User = Client.CurrentAccount;
            int ItemNum = reader.ReadLEInt32();
            Client.SendAsync(new RightClickItemInfo(ItemNum, last));
        }

        public static void Handle_GetUserPoint(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            int pointtype = reader.ReadLEInt32();
            //2200
            //2300
            //2400 = 小遊戲
            //Console.WriteLine("UserPoint Type: {0}", pointtype);
            requestUserPoint(User.UserNum, pointtype, out int totlapoint, out int currentpoint);
            //Client.SendAsync(new NP_Hex(User, "FF3A05600900005F0100005F02000010"));
            Client.SendAsync(new GetUserPoint(pointtype, totlapoint, currentpoint, last));
        }

        public static void Handle_GetUserItemCollectionPointInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            //1F 04 00 00 00 B9 DC AB D3 08
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            short bOtherUser = User.NickName == nickname ? (short)0 : (short)1;
            //type 1
            if (itemCollection_GetUserInfo(nickname, bOtherUser, 1, out var info, out var itemnums))
                Client.SendAsync(new GetUserItemCollectionPointInfo(nickname, info, last));
        }
        public static void Handle_GetUserItemCollectionItemInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            //21 04 00 00 00 B9 DC AB D3 04
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            short bOtherUser = User.NickName == nickname ? (short)0 : (short)1;
            //type 2
            if (itemCollection_GetUserInfo(nickname, bOtherUser, 2, out var info, out var itemnums))
                Client.SendAsync(new GetUserItemCollectionItemInfo(nickname, itemnums, last));
        }
        public static void Handle_SetItemCollectionShowItem(ClientConnection Client, PacketReader reader, byte last)
        {
            //23 0C 00 00 00 32 39 36 39 39 2C 34 32 34 38 34 2C 02
            Account User = Client.CurrentAccount;
            int len = reader.ReadLEInt32();
            string val = reader.ReadBig5StringSafe(len);

            if (itemCollection_UdateUserInfo(User.UserNum, 2, val))
                Client.SendAsync(new SetItemCollectionShowItemOK(last));
        }
        public static void Handle_SetGameOption(ClientConnection Client, PacketReader reader, byte last)
        {
            //92 01 00 00 00 01 20
            Account User = Client.CurrentAccount;
            int value = reader.ReadLEInt32();
            bool isAdd = reader.ReadBoolean();
            Console.WriteLine("GameOption Value:{0} isAdd:{1}", value, isAdd);
            if (isAdd)
                User.GameOption += value;
            else
                User.GameOption -= value;
            setGameOption(User.UserNum, User.GameOption);
            Client.SendAsync(new SetGameOption(User.GameOption, last));
        }

        public static void Handle_GetUserEXPInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            //1D 01 00 04
            Account User = Client.CurrentAccount;
            short type = reader.ReadLEInt16(); //type???
            long value = 0;
            if (type == 1)
                value = User.Exp;
            else
                Log.Debug("GetUserEXPInfo type {0}", type);

            Client.SendAsync(new GetUserEXPInfo(type, value, last));
        }

        public static void Handle_GetEventPickBoard(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            int pickboardnum = reader.ReadLEInt32();
            Client.SendAsync(new GetEventPickBoard(pickboardnum, last));
        }

        private static bool itemCollection_GetUserInfo(string UserName, short bOtherUser, short reqType, out UserItemCollectionInfo info, out List<int> itemnums)
        {
            info = new UserItemCollectionInfo();
            itemnums = new List<int>();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_itemCollection_GetUserInfo";
                    cmd.Parameters.Add("nickName", MySqlDbType.VarString).Value = UserName;
                    cmd.Parameters.Add("bOtherUser", MySqlDbType.Int16).Value = bOtherUser;
                    cmd.Parameters.Add("reqType", MySqlDbType.Int16).Value = reqType;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reqType == 1)
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                info.point = Convert.ToInt32(reader["point"]);
                                info.rank = Convert.ToInt32(reader["rank"]);
                                info.noticedLevel = Convert.ToByte(reader["noticedLevel"]);
                                return true;
                            }
                            else
                            {
                                info.point = 0;
                                info.rank = 0;
                                info.noticedLevel = 0;
                                return true;
                            }
                        }
                        else if (reqType == 2 && reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                itemnums.Add(Convert.ToInt32(reader["itemNum"]));
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private static bool itemCollection_UdateUserInfo(int UserNun, short reqType, string val)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_itemCollection_UpdateUserInfo";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNun;
                    cmd.Parameters.Add("reqType", MySqlDbType.Int16).Value = reqType;
                    cmd.Parameters.Add("val", MySqlDbType.VarString).Value = val;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        reader.Read();
                        if (Convert.ToInt32(reader["ret"]) == 0)
                            return true;
                        else
                            return false;
                    }
                }
            }
        }

        private static void setGameOption(int UserNun, int optionvalue)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_setGameOption";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNun;
                    cmd.Parameters.Add("option", MySqlDbType.Int32).Value = optionvalue;
                    cmd.ExecuteNonQuery();
                    /*using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                    }*/
                }
            }
        }
        private static void requestUserPoint(int UserNum, int rewardGroup, out int totlapoint, out int currentpoint)
        {
            totlapoint = 0;
            currentpoint = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_requestUserPoint";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
                    cmd.Parameters.Add("rewardGroup", MySqlDbType.Int32).Value = rewardGroup;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        reader.Read();
                        totlapoint = Convert.ToInt32(reader["pointAccumulated"]);
                        currentpoint = Convert.ToInt32(reader["pointCurrent"]);
                    }
                }
            }
        }
    }
}
