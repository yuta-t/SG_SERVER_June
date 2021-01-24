using AgentServer.Packet.Send;
using AgentServer.Structuring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LocalCommons.Utilities;
using AgentServer.Holders;
using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;
using LocalCommons.Network;
using LocalCommons.Logging;
using LocalCommons.Cryptography;
using AgentServer.Network.Connections;

namespace AgentServer.Packet
{
    public class GameRoomEvent
    {
        public static async Task Execute_GameEnd(NormalRoom room, long EndTime, byte last)
        {
            room.addMatchTime();
            while (Utility.CurrentTimeMilliseconds() < EndTime)
            {
                if (room.Players.Where(p => p.Attribute != 3).All(p => p.GameEndType > 0))
                {
                    foreach (Account player in room.Players.Where(p => p.GameEndType > 2 && p.Attribute != 3)
                                                            .OrderBy(o => o.GameEndType).ThenByDescending(o => o.RaceDistance))
                    {
                        if(player.GameEndType == 4)
                            player.Rank = 98;
                        else
                            player.Rank = 99;
                        Calc_DropItem(player, room, player.Rank, last);
                        //await Task.Run(() => Calc_DropItem(player, room, player.Rank, last));
                    }
                    break;
                }
 
                await Task.Delay(500);
            };
            await Task.Delay(4000);
            room.Result = GenResult(room, last);
            room.RegisterItem(-1, -1, 2, 0x1049F00C, true);
            try
            {
                foreach (Account RoomPlayer in room.Players) //更新自己EXP
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_GameUpdateEXP(RoomPlayer, last));
                }
                foreach (Account RoomPlayer in room.Players)
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_GameResult2(RoomPlayer, room.Result));
                    RoomPlayer.Connection.SendAsync(new GameRoom_Hex(RoomPlayer, "FF0102", last));
                    foreach (Account Player in room.Players)
                    {
                        RoomPlayer.Connection.SendAsync(new GameRoom_RoomPosReady(RoomPlayer, Player.RoomPos, false, last));
                    }
                    RoomPlayer.Connection.SendAsync(new GameRoom_GoodsInfo(RoomPlayer, room, last));
                }
                foreach (Account RoomPlayer in room.Players)
                {
                    //RoomPlayer.Connection.SendAsync(new GameRoom_Hex(RoomPlayer, "FF2D02000000000000000000000000000800000000000000000000", last)); //FF2D02A3220000C4C11400CB110000DB0800000E0C00001E0F0000
                    RoomPlayer.Connection.SendAsync(new GameRoom_UpdateIndividualGameRecord(RoomPlayer, last));
                }
                await Task.Delay(6000);
                foreach (Account RoomPlayer in room.Players)
                {
                    //MoveToGameRoom
                    RoomPlayer.Connection.SendAsync(new GameRoom_Hex(RoomPlayer, "FF9503", last)); //9704
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            //reset
            room.Rank = 1;
            room.Result = null;
            room.DropItem.Clear();
            room.isPlaying = false;
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.IsReady = false;
                RoomPlayer.EndLoading = false;
                RoomPlayer.GameEndType = 0;
            }
        }

        public static void DisconnectRoom(Account User)
        {
            if (User.CurrentRoomId != 0 && User.InGame)
            {
                byte roompos = User.RoomPos;
                NormalRoom normalRoom = Rooms.NormalRoomList.Find(room => room.ID == User.CurrentRoomId);
                bool isRemoved = true;
                if (normalRoom.Players.Count == 1)
                {
                    if (normalRoom.ItemNum != -1)
                    {
                        User.Connection.SendAsync(new GameRoom_LockKeepItem(User, normalRoom, true, 0x1));//解鎖之前保管了的物品
                    }
                    Rooms.NormalRoomList.Remove(normalRoom);
                }
                else
                {
                    //加回位置id
                    normalRoom.Players.Remove(User);
                    normalRoom.PosList.Add(roompos);
                    isRemoved = false;

                    if (normalRoom.isPlaying)
                    {
                        normalRoom.Survival -= 1;
                        if (normalRoom.Survival == 1 && normalRoom.RuleType == 2) //生存mode
                        {
                            byte alivepos = normalRoom.Players.Find(f => f.GameEndType == 0 && f.Attribute != 3).RoomPos;
                            foreach (Account RoomPlayer in normalRoom.Players)
                            {
                                RoomPlayer.Connection.SendAsync(new GameRoom_alive(RoomPlayer, alivepos, 0x01));
                            }
                        }
                        else if (normalRoom.Survival == 0 && normalRoom.RuleType == 3) //Hardcore
                        {
                            long EndTime = Utility.CurrentTimeMilliseconds() + 2000;
                            Task.Run(() => Execute_GameEnd(normalRoom, EndTime, 0x1));
                        }
                    }
                }
                byte RoomMasterIndex = normalRoom.RoomMasterIndex;
                if (!isRemoved)
                {
                    foreach (Account RoomPlayer in normalRoom.Players)
                    {
                        RoomPlayer.Connection.SendAsync(new GameRoom_RemoveRoomUser(RoomPlayer, roompos, 0x1));
                    }
                    if (User.RoomPos == RoomMasterIndex)
                    {
                        if (normalRoom.ItemNum != -1)
                        {
                            User.Connection.SendAsync(new GameRoom_LockKeepItem(User, normalRoom, true, 0x1));//解鎖之前保管了的物品
                            normalRoom.RegisterItem(-1, -1, 0, 0, false);

                            foreach (Account RoomPlayer in normalRoom.Players)
                            {
                                //RoomPlayer.Connection.SendAsync(new GameRoom_RegisterSuccess(User, -1, -1, 0x1));
                                RoomPlayer.Connection.SendAsync(new GameRoom_GoodsInfo(User, normalRoom, 0x1));
                            }
                        }
                        Account newRoomMaster = normalRoom.Players.FirstOrDefault();
                        normalRoom.RoomMasterIndex = newRoomMaster.RoomPos;
                        foreach (Account RoomPlayer in normalRoom.Players)
                        {
                            RoomPlayer.Connection.SendAsync(new GameRoom_GetRoomMaster(RoomPlayer, newRoomMaster.RoomPos, 0x1));
                        }

                    }
                }
            }
        }
        public static async Task TimeoutLoading(NormalRoom room, int matchtime)
        {
            await Task.Delay(60000);
            if (matchtime == room.MatchTime)
            {
                bool isAllSync = room.Players.All(player => player.EndLoading);
                if (!isAllSync && room.isPlaying)
                {
                    Account[] Users = room.Players.FindAll(player => !player.EndLoading).ToArray();
                    foreach (Account KickedPlayer in Users)
                    {
                        KickPlayer(KickedPlayer, room, 0x1);
                        byte RoomMasterIndex = room.RoomMasterIndex;
                        if (KickedPlayer.RoomPos == RoomMasterIndex)
                        {
                            if (room.ItemNum != -1)
                            {
                                KickedPlayer.Connection.SendAsync(new GameRoom_LockKeepItem(KickedPlayer, room, true, 0x1));//解鎖之前保管了的物品
                                room.RegisterItem(-1, -1, 0, 0, false);
                            }
                            Account newRoomMaster = room.Players.FirstOrDefault();
                            room.RoomMasterIndex = newRoomMaster.RoomPos;
                            foreach (Account RoomPlayer in room.Players)
                            {
                                RoomPlayer.Connection.SendAsync(new GameRoom_GetRoomMaster(RoomPlayer, newRoomMaster.RoomPos, 0x1));
                            }
                        }
                    }
                    if (room.Players.Count == 0)
                    {
                        Rooms.NormalRoomList.Remove(room);
                    }
                    else
                    {
                        foreach (Account RoomPlayer in room.Players)
                        {
                            RoomPlayer.Connection.SendAsync(new GameRoom_Hex(RoomPlayer, "FF1203", 0x1));
                        }
                    }
                }
            }
        }

        public static void Calc_DropItem(Account User, NormalRoom room, byte rank, byte last)
        {
            if (User.Attribute != 3)
            {
                int PlayerCount = room.Players.FindAll(p => p.Attribute != 3).Count;
                int TR = 0;
                int EXP = 0;
                //rank = rank > 10 ? (byte)10 : rank;

                TR = Calc_TR(User, room, rank > 10 ? (byte)10 : rank, out short BounsTR);
                EXP = Calc_EXP(User, room, rank > 10 ? (byte)10 : rank, out short BounsEXP);
                /*Console.WriteLine("BounsTR: {0}", BounsTR);
                 Console.WriteLine("BounsEXP: {0}", BounsEXP);*/
                /*if (User.GameEndType == 1)
                {
                    TR = Calc_TR(User, room, rank, out short initTR);
                    EXP = Calc_EXP(User, room, rank, out short initEXP);
                }
                else
                {
                    TR = 1 * PlayerCount;
                    EXP = 1 * PlayerCount;
                }*/

                addEXP(User.UserNum, EXP);
                addTR(User.UserNum, TR);
                User.Exp += EXP;
                User.TR += TR;
                Random rand = new Random();
                var takecard = MapCardHolder.MapCardInfos.Any(c => c.Value.MapNum == room.PlayingMapNum) ? MapCardHolder.MapCardInfos.Where(c => c.Value.MapNum == room.PlayingMapNum).Select(c => c.Value.CardNum).OrderBy(x => rand.Next()).Take(1).ToList() : new List<int> { 0 };
                //Console.WriteLine(takecard[0]);
                foreach (var card in takecard)
                {
                    if (card != 0)
                        giveCard(User.UserNum, card);
                    //Console.WriteLine("giveCard");
                }
                DropList dropList = new DropList
                {
                    UserNum = User.UserNum,
                    BounsTR = BounsTR,
                    BounsEXP = BounsEXP,
                    TR = TR,
                    EXP = EXP,
                    Rank = rank,
                    CardID = takecard
                };
                Log.Info("Drop Item - Nickname: {0}, TR: {1}, EXP: {2}, Rank: {3}", User.NickName, TR, EXP, rank);
                room.DropItem.Add(dropList);
            }
        }

        public static void addEXP(int usernum, int exp)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_addExpByusernum";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = usernum;
                cmd.Parameters.Add("exp", MySqlDbType.Int32).Value = exp;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
        public static void addTR(int usernum, int tr)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_addmoneybyusernum";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = usernum;
                cmd.Parameters.Add("gamemoney", MySqlDbType.Int32).Value = tr;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
        public static void giveCard(int usernum, int itemdescnum)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_alchemist_giveCard";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = usernum;
                cmd.Parameters.Add("itemdescnum", MySqlDbType.Int32).Value = itemdescnum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }

        public static int Calc_TR(Account User, NormalRoom room, byte rank, out short BounsTR)
        {
            int PlayerCount = room.Players.Count(p => p.Attribute != 3); //room.Players.FindAll(p => p.Attribute != 3).Count;
            //float inittr = 12 * PlayerCount * ((110 - (rank * 10)) / 100);
            int inittr = (int)Math.Round(12 * PlayerCount * (float)(1 - (((decimal)rank / 10) - (decimal)0.1)), MidpointRounding.AwayFromZero);
            int trtotal = 0;
            float trpercent = 0;
            float trnormal = 0;
            Random rnd = new Random();
            foreach (var a in User.WearAvatarItemAttr.Where(w => w.Attr == 1 || w.Attr == 37 || w.Attr == 51))
            {
                switch (a.Attr)
                {
                    case 1:
                        trpercent += a.AttrValue;
                        break;
                    case 37:
                        trnormal += a.AttrValue;
                        break;
                    case 51:
                        trnormal += rnd.Next(Convert.ToInt32(a.AttrValue));
                        break;
                    default:
                        break;
                }
            }
            //avon
            if (User.costumeMode == 1)
            {
                foreach (var a in User.WearCosAvatarItemAttr.Where(w => w.Attr == 1 || w.Attr == 37 || w.Attr == 51))
                {
                    switch (a.Attr)
                    {
                        case 1:
                            trpercent += a.AttrValue;
                            break;
                        case 37:
                            trnormal += a.AttrValue;
                            break;
                        case 51:
                            trnormal += rnd.Next(Convert.ToInt32(a.AttrValue));
                            break;
                        default:
                            break;
                    }
                }
            }
            if (User.AvatarItems.Any(i => i.itemdescnum == 47635)) //優越童話通行証BUFF
            {
                trpercent += 1;
            }
            trtotal  = (int)Math.Round((inittr + (inittr * trpercent) + trnormal), MidpointRounding.AwayFromZero);
            //trtotal = inittr + (inittr * trpercent) + trnormal;
            BounsTR = (short)(trtotal - inittr);
            return trtotal;
        }    
        public static int Calc_EXP(Account User, NormalRoom room, byte rank, out short BounsEXP)
        {
            int PlayerCount = room.Players.Count(p => p.Attribute != 3); //room.Players.FindAll(p => p.Attribute != 3).Count;
            int initexp = (int)Math.Round(12 * PlayerCount * (float)(1 - (((decimal)rank / 10) - (decimal)0.1)), MidpointRounding.AwayFromZero);
            int  exptotal = 0;
            float exppercent = 0;
            float expnormal = 0;
            Random rnd = new Random();
            foreach (var a in User.WearAvatarItemAttr.Where(w => w.Attr == 2 || w.Attr == 38 || w.Attr == 52))
            {
                switch (a.Attr)
                {
                    case 2:
                        exppercent += a.AttrValue;
                        break;
                    case 38:
                        expnormal += a.AttrValue;
                        break;
                    case 52:
                        expnormal += rnd.Next(Convert.ToInt32(a.AttrValue));
                        break;
                    default:
                        break;
                }
            }
            //avon
            if (User.costumeMode == 1)
            {
                foreach (var a in User.WearCosAvatarItemAttr.Where(w => w.Attr == 2 || w.Attr == 38 || w.Attr == 52))
                {
                    switch (a.Attr)
                    {
                        case 2:
                            exppercent += a.AttrValue;
                            break;
                        case 38:
                            expnormal += a.AttrValue;
                            break;
                        case 52:
                            expnormal += rnd.Next(Convert.ToInt32(a.AttrValue));
                            break;
                        default:
                            break;
                    }
                }
            }
            if (User.AvatarItems.Any(i => i.itemdescnum == 47635)) //優越童話通行証BUFF
            {
                exppercent += (float)1.5;
            }
            //exptotal = initexp + (initexp * exppercent) + expnormal;
            exptotal = (int)Math.Round(initexp + (initexp * exppercent) + expnormal, MidpointRounding.AwayFromZero);
            BounsEXP = (short)(exptotal - initexp);
            return exptotal;
        }

        private static byte[] GenResult(NormalRoom room, byte last)
        {
            if (room.Players.Exists(p => p.GameEndType == 0 && p.Attribute != 3))
            {
                foreach (Account player in room.Players.Where(p => p.GameEndType == 0 && p.Attribute != 3).OrderBy(o => o.RaceDistance))
                {
                    long CurrentTime = Utility.CurrentTimeMilliseconds();
                    player.LapTime = (int)(CurrentTime + 200000 - room.StartTime);
                    player.ServerLapTime = player.LapTime;
                    player.Rank = 99;//99 = TIME OVER, 98 = GAME OVER
                    Calc_DropItem(player, room, player.Rank, last);
                    //await Task.Run(() => Calc_DropItem(player, room, player.Rank, last));
                }
            }

            PacketWriter ns = new PacketWriter();
            ns = PacketWriter.CreateInstance(16, true);
            ns.Write((byte)0xFF);
            ns.Write((short)0x381);
            ns.Write((byte)room.RoomKindID);
            ns.Fill(0xB);
            int playercount = room.Players.Count(p => p.Attribute != 3);
            ns.Write((byte)playercount); //count?
            ns.Fill(0x6);
            foreach (var p in room.Players.Where(p => p.Attribute != 3)
                            .Join(room.DropItem, p => p.UserNum, d => d.UserNum, (p, d) => new { player = p, droplist = d })
                                 .OrderBy(o => o.droplist.Rank).ThenByDescending(o => o.player.RaceDistance).ThenBy(o => o.player.RoomPos))
            {
                ns.Write(p.player.ServerLapTime);
                ns.Write(p.player.LapTime);
                ns.Write(p.droplist.Rank);
                ns.Write(p.player.RoomPos);
                ns.Write(p.droplist.EXP);
                ns.Write(0);
                ns.Write(p.droplist.TR);
                ns.Write(0);
                ns.Fill(0xC);
                ns.Write(p.droplist.BounsEXP);
                ns.Write(p.droplist.BounsTR);
                int bounsweight = 0;
                if (p.droplist.BounsTR > 0)
                    bounsweight += 4;
                if (p.droplist.BounsEXP > 0)
                    bounsweight += 8;
                ns.Write(bounsweight); // 0C 00 00 00
                ns.Write(playercount); //02 00 00 00 player count?
                int count = p.droplist.CardID.Count;
                ns.Write(count); //card count

                foreach (var card in p.droplist.CardID)
                {
                    ns.Write(card);
                }
                ns.WriteHex("00BA8B19000000000000000000000000000000000000000000C5B01B000000000000");
            }

            bool hasgavegoods = SendRegisterGoods(room, out byte[] getgoodsnickname, last);
            if (hasgavegoods)
            {
                Account roomMaster = room.Players.Find(p => p.RoomPos == room.RoomMasterIndex);
                roomMaster.Connection.SendAsync(new GameRoom_DeleteKeepItem(roomMaster, room, last));
            }

            ns.Write((short)0);
            ns.Write(2);
            ns.Write(0x3EA);
            ns.Write(!hasgavegoods ? 0x11 : 0x11 + getgoodsnickname.Length);
            ns.Write(room.ItemNum);

            if (!hasgavegoods)
            {
                ns.Write(-1L);
                ns.Write((byte)0);
                ns.Write(1);
            }
            else
            {
                ns.Write(0x3b1c32);//32 1C 3B 00
                ns.Write(2);
                ns.Write(getgoodsnickname, 0, getgoodsnickname.Length);//B5 A3 B8 DC B6 5D A4 48
                ns.Write((byte)0);
                ns.Write(getgoodsnickname.Length + 1);
            }
            ns.Write(0x3F6);
            ns.Write(4);
            ns.Write(0);
            //ns.WriteHex("000002000000EA03000011000000FFFFFFFFFFFFFFFFFFFFFFFF0001000000F60300000400000000000000");
            ns.Write(last);
            byte[] ret = ns.ToArray();
            PacketWriter.ReleaseInstance(ns);
            ns = null;
            //Console.WriteLine("Result packet: " + Utility.ByteArrayToString(ret));
            return ret;
        }
        public static bool SendRegisterGoods(NormalRoom room, out byte[] nickname, byte last)
        {
            if (room.ItemNum != -1)
            {
                if (room.isOrderBy == 1)
                {
                    foreach (Account RoomUser in room.Players)
                    {
                        if (RoomUser.Rank == room.SendRank)
                        {
                            if (RoomUser.RoomPos == room.RoomMasterIndex)
                            {
                                RoomUser.Connection.SendAsync(new GameRoom_LockKeepItem(RoomUser, room, true, last));
                                nickname = null;
                                return false;
                            }
                            else
                            {
                                //TODO: 派野比人
                                StorageGiveReward(RoomUser.UserNum, room.ItemNum, room.Storage_Id);
                                nickname = Encoding.GetEncoding("Big5").GetBytes(RoomUser.NickName);
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    Random random = new Random();
                    Account RoomUser = room.Players[random.Next(room.Players.Count)];
                    if (RoomUser.RoomPos == room.RoomMasterIndex)
                    {
                        RoomUser.Connection.SendAsync(new GameRoom_LockKeepItem(RoomUser, room, true, last));
                        nickname = null;
                        return false;
                    }
                    else
                    {
                        //TODO: 派野比人
                        StorageGiveReward(RoomUser.UserNum, room.ItemNum, room.Storage_Id);
                        nickname = Encoding.GetEncoding("Big5").GetBytes(RoomUser.NickName);
                        return true;
                    }
                }
            }
            nickname = null;
            return false;
        }
        private static bool StorageGiveReward(int recvUserNum, int ItemNum, long uniqueNum)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_storage_giveroomreward";
                cmd.Parameters.Add("recvUserNum", MySqlDbType.Int32).Value = recvUserNum;
                cmd.Parameters.Add("ItemNum", MySqlDbType.Int32).Value = ItemNum;
                cmd.Parameters.Add("uniqueNum", MySqlDbType.Int64).Value = uniqueNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                byte ret = Convert.ToByte(reader["retval"]);
                if (ret == 0)
                    return true;
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            return false;
        }
        public static void EnterRoom(ClientConnection Client, NormalRoom room, string pw, byte last)
        {
            Account User = Client.CurrentAccount;
            int MaxPlayer;
            if (room.is8Player)
                MaxPlayer = room.SlotCount;
            else
                MaxPlayer = room.MaxPlayersCount;
            if (room.isPlaying)
            {
                Client.SendAsync(new GameRoom_EnterRoomError(User, 5, (byte)room.RoomKindID, last));
                return;
            }
            if (room.Players.Count(p => p.Attribute != 3) < MaxPlayer || User.Attribute == 3)
            {
                //room.Players.Add(User);
                // User.IsRoomMaster = false;                
                if (room.Players.Exists(p => p.Attribute == 3) && User.Attribute == 3)
                {
                    Client.SendAsync(new GameRoom_EnterRoomError(User, 9, (byte)room.RoomKindID, last));
                    return;
                }
                if (room.Password != pw && User.Attribute == 0)
                {
                    Client.SendAsync(new GameRoom_EnterRoomError(User, 7, (byte)room.RoomKindID, last));
                    return;
                }
                DateTime kickedtime;
                if (room.KickedList.TryGetValue(User, out kickedtime))
                {
                    bool cantenter = DateTime.Compare(DateTime.Now, kickedtime.AddSeconds(60)) < 0;
                    if (cantenter)
                    {
                        Client.SendAsync(new GameRoom_EnterRoomError(User, 6, (byte)room.RoomKindID, last));
                        return;
                    }
                    room.KickedList.Remove(User);
                }
                User.InGame = true;
                User.CurrentRoomId = room.ID;
                User.IsReady = false;

                //取得當前第一個位置id
                User.RoomPos = (byte)(User.Attribute == 3 ? 100 : room.PosList.FirstOrDefault());
                //Console.WriteLine("User.RoomPos: "+ User.RoomPos);
                room.PosList.Remove(User.RoomPos);

                //Client.SendAsync(new GameRoom_RegisterSuccess(User, room.ItemNum, room.Storage_Id, last));
                Client.SendAsync(new GameRoom_GoodsInfo(User, room, last));
                Client.SendAsync(new GameRoom_SendRoomInfo(User, room, last, User.RoomPos));

                //Send自己info俾其他roomuser
                foreach (Account roomUser in room.Players)
                {
                    roomUser.Connection.SendAsync(new GameRoom_SendPlayerInfo(User, last));
                    if (room.IsTeamPlay == 2)
                    {
                        User.Team = 1;
                        roomUser.Connection.SendAsync(new GameRoom_RoomPosTeam(User, last));
                    }
                }

                room.Players.Add(User);

                //Send roomuser info俾自己
                foreach (Account roomUser in room.Players)
                {
                    Client.SendAsync(new GameRoom_SendPlayerInfo(roomUser, last));
                    Client.SendAsync(new GameRoom_RoomPosReady(roomUser, roomUser.RoomPos, roomUser.IsReady, last));

                    if (room.IsTeamPlay == 2)
                    {
                        Client.SendAsync(new GameRoom_RoomPosTeam(roomUser, last));
                    }
                    if (User.Attribute == 3)
                    {
                        room.setRoomMasterIndex(100);
                        byte africapos = room.RoomMasterIndex; //room.Players.Find(player => player.IsRoomMaster).RoomPos;
                        roomUser.Connection.SendAsync(new GameRoom_GetRoomMaster(roomUser, africapos, last));
                    }
                }

                if (User.Attribute != 3)
                {
                    byte roommasterpos = room.RoomMasterIndex; //room.Players.Find(player => player.IsRoomMaster).RoomPos;
                    Client.SendAsync(new GameRoom_GetRoomMaster(User, roommasterpos, last));
                }
            }
            else
            {
                Client.SendAsync(new GameRoom_EnterRoomError(User, 1, (byte)room.RoomKindID, last));
            }
        }
        public static void KickPlayer(Account KickedPlayer, NormalRoom room, byte last)
        {
            byte KickedPlayerIndex = KickedPlayer.RoomPos;
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_KickPlayer(RoomPlayer, KickedPlayerIndex, last));
            }
            room.Players.Remove(KickedPlayer);
            room.PosList.Add(KickedPlayerIndex);
            KickedPlayer.RoomPos = 0;
            KickedPlayer.InGame = false;
            KickedPlayer.CurrentRoomId = 0;
            KickedPlayer.Connection.SendAsync(new GameRoom_Hex(KickedPlayer, "FF6405F703000002000000", last));
            KickedPlayer.Connection.SendAsync(new GameRoom_LeaveRoomUser_0XA9(KickedPlayer, KickedPlayerIndex, last));
            room.addKickedPlayer(KickedPlayer);
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_RemoveRoomUser(RoomPlayer, KickedPlayerIndex, last));
                RoomPlayer.Connection.SendAsync(new GameRoom_Hex(RoomPlayer, "FFA80500000000", last));
            }
            KickedPlayer.Connection.SendAsync(new GameRoom_Hex(KickedPlayer, "FFA80500000000", last));
            KickedPlayer.Connection.SendAsync(new GameRoom_KickPlayer(KickedPlayer, KickedPlayerIndex, last));
        }
    }
}
