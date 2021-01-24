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
using AgentServer.Structuring.Item;

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
                    /*foreach (Account player in room.Players.Where(p => p.GameEndType > 2 && p.GameEndType < 5 && p.Attribute != 3)
                                                            .OrderByDescending(o => o.RaceDistance).ThenByDescending(o => o.GameEndType))
                    {
                        player.LapTime = room.GetCurrentTime() + 240000;
                        player.ServerLapTime = player.LapTime;
                        if (player.GameEndType == 4)//gameover
                        {
                            switch (room.RuleType)
                            {
                                case 4:
                                    player.Rank = 99;
                                    break;
                                default:
                                    player.Rank = 98;
                                    break;
                            }
                        }
                        else if (player.GameEndType == 3)//timeover
                        {
                            player.Rank = 99;
                        }
                        Calc_DropItem(player, room, player.Rank, last);
                    }*/
                    if (room.GameMode != 3)
                    {
                        foreach (var player in room.Players.OrderBy(o => o.ServerLapTime).ThenByDescending(o => o.RaceDistance))
                        {
                            if (player.GameEndType == 1)//goalin
                            {
                                player.Rank = room.Rank++;
                            }
                            if (player.GameEndType == 2)//alive
                            {
                                player.Rank = room.Rank++;
                            }
                            else if (player.GameEndType == 3)//timeover
                            {
                                player.LapTime = room.GetCurrentTime() + 220000;
                                player.ServerLapTime = player.LapTime;
                                player.Rank = 99;
                            }
                            else if (player.GameEndType == 4)//gameover
                            {
                                player.LapTime = room.GetCurrentTime() + 240000;
                                player.ServerLapTime = player.LapTime;
                                switch (room.RuleType)
                                {
                                    case 4:
                                        player.Rank = 99;
                                        break;
                                    default:
                                        player.Rank = 98;
                                        break;
                                }
                            }
                            else if (player.GameEndType == 5)//footstep
                            {
                                player.Rank = room.Rank++;
                            }
                            Calc_DropItem(player, room, player.Rank, last);
                            await Task.Delay(100);
                        }
                    }
                    else
                    {
                        foreach (var player in room.Players.Where(w => w.RelayTeamPos == 5 || w.RelayTeamPos == 8 || w.RelayTeamPos == 11 || w.RelayTeamPos == 14 || w.RelayTeamPos == 17 || w.RelayTeamPos == 20)
                            .OrderBy(o => o.ServerLapTime).ThenByDescending(o => o.RaceDistance))
                        {
                            if (player.GameEndType == 1)//goalin
                            {
                                player.Rank = room.Rank++;
                                int i = 1;
                                foreach (var teamplayer in room.Players.Where(w => w.RelayTeam == player.RelayTeam && w.UserNum != player.UserNum))
                                {
                                    teamplayer.LapTime = player.LapTime + i;
                                    teamplayer.ServerLapTime = player.ServerLapTime + i;
                                    teamplayer.Rank = room.Rank++;
                                    i++;
                                    Calc_DropItem(teamplayer, room, teamplayer.Rank, last);
                                }
                            }
                            if (player.GameEndType == 2)//alive
                            {
                                player.Rank = room.Rank++;
                                int i = 1;
                                foreach (var teamplayer in room.Players.Where(w => w.RelayTeam == player.RelayTeam && w.UserNum != player.UserNum))
                                {
                                    teamplayer.LapTime = player.LapTime + i;
                                    teamplayer.ServerLapTime = player.ServerLapTime + i;
                                    teamplayer.Rank = room.Rank++;
                                    i++;
                                    Calc_DropItem(teamplayer, room, teamplayer.Rank, last);
                                }
                            }
                            else if (player.GameEndType == 3)//timeover
                            {
                                player.LapTime = room.GetCurrentTime() + 220000;
                                player.ServerLapTime = player.LapTime;
                                player.Rank = 99;
                                foreach (var teamplayer in room.Players.Where(w => w.RelayTeam == player.RelayTeam && w.UserNum != player.UserNum))
                                {
                                    teamplayer.LapTime = player.LapTime;
                                    teamplayer.ServerLapTime = player.ServerLapTime;
                                    teamplayer.Rank = player.Rank;
                                    Calc_DropItem(teamplayer, room, teamplayer.Rank, last);
                                }
                            }
                            else if (player.GameEndType == 4)//gameover
                            {
                                player.LapTime = room.GetCurrentTime() + 240000;
                                player.ServerLapTime = player.LapTime;
                                switch (room.RuleType)
                                {
                                    case 4:
                                        player.Rank = 99;
                                        break;
                                    default:
                                        player.Rank = 98;
                                        break;
                                }
                                foreach (var teamplayer in room.Players.Where(w => w.RelayTeam == player.RelayTeam && w.UserNum != player.UserNum))
                                {
                                    teamplayer.LapTime = player.LapTime;
                                    teamplayer.ServerLapTime = player.ServerLapTime;
                                    teamplayer.Rank = player.Rank;
                                    Calc_DropItem(teamplayer, room, teamplayer.Rank, last);
                                }
                            }
                            else if (player.GameEndType == 5)//footstep
                            {
                                player.Rank = room.Rank++;
                                int i = 1;
                                foreach (var teamplayer in room.Players.Where(w => w.RelayTeam == player.RelayTeam && w.UserNum != player.UserNum))
                                {
                                    teamplayer.LapTime = player.LapTime + i;
                                    teamplayer.ServerLapTime = player.ServerLapTime + i;
                                    teamplayer.Rank = room.Rank++;
                                    i++;
                                    Calc_DropItem(teamplayer, room, teamplayer.Rank, last);
                                }
                            }
                            Calc_DropItem(player, room, player.Rank, last);
                            await Task.Delay(100);
                        }
                    }
                    break;
                }
 
                await Task.Delay(1000);
            };
            await Task.Delay(4000);
            room.Result = GenResult(room, last);
            room.RegisterItem(-1, -1, 2, 0x1049F00C, true);
            List<Account> playerlist = room.PlayerList();
            try
            {
                foreach (Account RoomPlayer in playerlist) //更新自己EXP
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_GameUpdateEXP(RoomPlayer, last));
                }
                foreach (Account RoomPlayer in playerlist)
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_GameResult2(RoomPlayer, room.Result));
                    RoomPlayer.Connection.SendAsync(new GameRoom_Hex("FF0102", last));
                    foreach (Account Player in room.Players)
                    {
                        RoomPlayer.Connection.SendAsync(new GameRoom_RoomPosReady(Player.RoomPos, false, last));
                        if (room.GameMode == 3)
                        {
                            Player.SelectRelayTeam((byte)new Random().Next(1, 3));
                            RoomPlayer.Connection.SendAsync(new GameRoom_RoomPosRelayTeam(Player, last));
                        }
                    }
                    RoomPlayer.Connection.SendAsync(new GameRoom_GoodsInfo(room, last));
                    //RoomPlayer.Connection.SendAsync(new GameRoom_Hex(RoomPlayer, "FF860301000B0100000000000001", last)); levelup
                }
                if(room.DropItem.Count(w => w.isLevelUP) > 0)
                {
                    foreach (Account RoomPlayer in playerlist)
                    {
                        foreach (var lvupplayer in room.DropItem.Where(w => w.isLevelUP))
                        {
                            RoomPlayer.Connection.SendAsync(new GameRoom_LevelUP(lvupplayer.TotalEXP, lvupplayer.Pos, last));
                        }
                    }
                }
                foreach (Account RoomPlayer in playerlist)
                {
                    //RoomPlayer.Connection.SendAsync(new GameRoom_Hex(RoomPlayer, "FF2D02000000000000000000000000000800000000000000000000", last)); //FF2D02A3220000C4C11400CB110000DB0800000E0C00001E0F0000
                    RoomPlayer.Connection.SendAsync(new GameRoom_UpdateIndividualGameRecord(RoomPlayer, last));
                }
                await Task.Delay(6000);
                foreach (Account RoomPlayer in playerlist)
                {
                    //MoveToGameRoom
                    RoomPlayer.Connection.SendAsync(new MoveToGameRoom(last)); //9704
                }

                //重新設定房主
                if (!room.Players.Exists(p => p.Attribute == 3))
                {
                    Account newRoomMaster = room.Players.Exists(p => p.Attribute == 1) ? room.Players.FirstOrDefault(p => p.Attribute == 1) : room.Players.OrderBy(p => p.Rank).FirstOrDefault();
                    room.RoomMasterIndex = newRoomMaster.RoomPos;
                    foreach (Account RoomPlayer in playerlist)
                    {
                        RoomPlayer.Connection.SendAsync(new GameRoom_GetRoomMaster(RoomPlayer, newRoomMaster.RoomPos, last));
                    }
                }

                room.StartAutoChangeRoomMaster();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            //reset
            room.GameEndReset();
        }

        /*public static void DisconnectRoom(Account User)
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
                        switch (normalRoom.RuleType)
                        {
                            case 2: //生存
                                if (normalRoom.Survival == 1)
                                {
                                    byte alivepos = normalRoom.Players.Find(f => f.GameEndType == 0 && f.Attribute != 3).RoomPos;
                                    foreach (Account RoomPlayer in normalRoom.Players.OrderBy(o => o.GameEndType))
                                    {
                                        RoomPlayer.Connection.SendAsync(new GameRoom_Alive(alivepos, 0x1));
                                    }
                                }
                                break;
                            case 3: //Hardcore
                            case 4: //跑步吧
                            case 8: //衝呀!!
                                if (normalRoom.Survival == 0)
                                {
                                    long EndTime = Utility.CurrentTimeMilliseconds() + 5000;
                                    Task.Run(() => GameRoomEvent.Execute_GameEnd(normalRoom, EndTime, 0x1));
                                }
                                break;
                            default:
                                break;
                        }
                        normalRoom.DisconnectDropList(User);
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
                else
                {
                    normalRoom.Dispose();
                }
            }
        }*/

        public static void Calc_DropItem(Account User, NormalRoom room, byte rank, byte last)
        {
            bool isExists = room.DropItem.Exists(e => e.UserNum == User.UserNum);
            if (User.Attribute != 3 && !isExists)
            {
                int PlayerCount = room.Players.FindAll(p => p.Attribute != 3).Count;
                int TR = 0;
                int EXP = 0;
                //rank = rank > 10 ? (byte)10 : rank;

                TR = Calc_TR(User, room, rank > 10 ? (byte)10 : rank, out short BounsTR);
                EXP = Calc_EXP(User, room, rank > 10 ? (byte)10 : rank, out short BounsEXP);
                int oldlevel = User.Level;
                addEXP(User.UserNum, EXP);
                addTR(User.UserNum, TR);
                User.Exp += EXP;
                User.TR += TR;
                User.GetMyLevel();
                  
                GetCard(room.PlayingMapNum, out var takecard);

                foreach (var card in takecard)
                {
                    if (card != 0)
                        giveCard(User.UserNum, card);
                }

                DropList dropList = new DropList
                {
                    UserNum = User.UserNum,
                    TotalEXP = User.Exp,
                    RaceDistance = User.RaceDistance,
                    ServerLapTime = User.ServerLapTime,
                    LapTime = User.LapTime,
                    Pos = User.RoomPos,
                    Team = User.Team,
                    RelayTeam = User.RelayTeam,
                    RelayTeamPos = User.RelayTeamPos,
                    BounsTR = BounsTR,
                    BounsEXP = BounsEXP,
                    TR = TR,
                    EXP = EXP,
                    Rank = User.Rank,
                    isLevelUP = User.Level != oldlevel,
                    CardID = takecard,
                    MiniGamePoint = 0,
                    MiniGameStarPoint = 0
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
            int PlayerCount = room.PlayerCount();
            float ServerMultiplyTR = ServerSettingHolder.ServerSettings.MultiplyTR;
            int inittr = (int)Math.Round(12 * PlayerCount * (float)(1 - (((decimal)rank / 10) - (decimal)0.1)), MidpointRounding.AwayFromZero);
            int trtotal = 0;
            float trpercent = 0, trnormal = 0;
            int MaxTR = 20000;
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            foreach (var a in User.AllItemAttr) 
            {
                switch (a.Key)
                {
                    case 1:
                        trpercent += a.Value;
                        break;
                    case 14://得到個人賽 前3名時 TR，經驗值+%
                        if (room.IsTeamPlay == 0 && rank <= 3)
                            trpercent += a.Value;
                        break;
                    case 30://個人賽 TR 隨機獎勵 0~N%
                        if (room.IsTeamPlay == 0 && room.GameMode != 3)
                            trpercent += (float)rnd.Next(Convert.ToInt32(a.Value)) / 100;
                        break;
                    case 32://團體賽，接力賽 TR 隨機獎勵 0~N%
                        if (room.IsTeamPlay != 0 || room.GameMode == 3)
                            trpercent += (float)rnd.Next(Convert.ToInt32(a.Value)) / 100;
                        break;
                    case 37:
                        trnormal += a.Value;
                        break;
                    case 51:
                        trnormal += rnd.Next(Convert.ToInt32(a.Value));
                        break;
                    case 91://簡易頻道 +N%
                        if (room.Channel == 2)
                            trpercent += a.Value;
                        break;
                    case 93://普通頻道 +N%
                        if (room.Channel == 3)
                            trpercent += a.Value;
                        break;
                    case 103://簡易頻道 +N
                        if (room.Channel == 2)
                            trnormal += a.Value;
                        break;
                    case 105://普通頻道 +N
                        if (room.Channel == 3)
                            trnormal += a.Value;
                        break;
                    case 131://個人戰頭3名以內過關時TR+%
                        if (room.IsTeamPlay == 0 && rank <= 3 && room.GameMode != 3)
                            trpercent += a.Value;
                        break;
                    default:
                        break;
                }
            }

            #region old
            /*
            foreach (var a in User.WearAvatarItemAttr.Where(w => w.Attr < 140))
            {
                switch (a.Attr)
                {
                    case 1:
                        trpercent += a.AttrValue;
                        break;
                    case 14://得到個人賽 前3名時 TR，經驗值+%
                        if (room.IsTeamPlay == 0 && rank <= 3)
                            trpercent += a.AttrValue;
                        break;
                    case 30://個人賽 TR 隨機獎勵 0~N%
                        if (room.IsTeamPlay == 0 && room.GameMode != 3)
                            trpercent += (float)rnd.Next(Convert.ToInt32(a.AttrValue)) / 100;
                        break;
                    case 32://團體賽，接力賽 TR 隨機獎勵 0~N%
                        if (room.IsTeamPlay != 0 || room.GameMode == 3)
                            trpercent += (float)rnd.Next(Convert.ToInt32(a.AttrValue)) / 100;
                        break;
                    case 37:
                        trnormal += a.AttrValue;
                        break;
                    case 51:
                        trnormal += rnd.Next(Convert.ToInt32(a.AttrValue));
                        break;
                    case 91://簡易頻道 +N%
                        if (room.Channel == 2)
                            trpercent += a.AttrValue;
                        break;
                    case 93://普通頻道 +N%
                        if (room.Channel == 3)
                            trpercent += a.AttrValue;
                        break;
                    case 103://簡易頻道 +N
                        if (room.Channel == 2)
                            trnormal += a.AttrValue;
                        break;
                    case 105://普通頻道 +N
                        if (room.Channel == 3)
                            trnormal += a.AttrValue;
                        break;
                    case 131://個人戰頭3名以內過關時TR+%
                        if (room.IsTeamPlay == 0 && rank <= 3 && room.GameMode != 3)
                            trpercent += a.AttrValue;
                        break;
                    default:
                        break;
                }
            }
            foreach (var a in User.WearItemSetAttr.Where(w => w.Attr < 140 && w.ApplyTarget == 1))
            {
                switch (a.Attr)
                {
                    case 1:
                        trpercent += a.AttrValue;
                        break;
                    case 14://得到個人賽 前3名時 TR，經驗值+%
                        if (room.IsTeamPlay == 0 && rank <= 3)
                            trpercent += a.AttrValue;
                        break;
                    case 30://個人賽 TR 隨機獎勵 0~N%
                        if (room.IsTeamPlay == 0 && room.GameMode != 3)
                            trpercent += (float)rnd.Next(Convert.ToInt32(a.AttrValue)) / 100;
                        break;
                    case 32://團體賽，接力賽 TR 隨機獎勵 0~N%
                        if (room.IsTeamPlay != 0 || room.GameMode == 3)
                            trpercent += (float)rnd.Next(Convert.ToInt32(a.AttrValue)) / 100;
                        break;
                    case 37:
                        trnormal += a.AttrValue;
                        break;
                    case 51:
                        trnormal += rnd.Next(Convert.ToInt32(a.AttrValue));
                        break;
                    case 91://簡易頻道 +N%
                        if (room.Channel == 2)
                            trpercent += a.AttrValue;
                        break;
                    case 93://普通頻道 +N%
                        if (room.Channel == 3)
                            trpercent += a.AttrValue;
                        break;
                    case 103://簡易頻道 +N
                        if (room.Channel == 2)
                            trnormal += a.AttrValue;
                        break;
                    case 105://普通頻道 +N
                        if (room.Channel == 3)
                            trnormal += a.AttrValue;
                        break;
                    case 131://個人戰頭3名以內過關時TR+%
                        if (room.IsTeamPlay == 0 && rank <= 3 && room.GameMode != 3)
                            trpercent += a.AttrValue;
                        break;
                    default:
                        break;
                }
            }
            //avon
            if (User.costumeMode == 1)
            {
                foreach (var a in User.WearCosAvatarItemAttr.Where(w => w.Attr < 140))
                {
                    switch (a.Attr)
                    {
                        case 1:
                            trpercent += a.AttrValue;
                            break;
                        case 14:
                            if (room.IsTeamPlay == 0 && rank <= 3)
                                trpercent += a.AttrValue;
                            break;
                        case 30://個人賽 TR 隨機獎勵 0~N%
                            if (room.IsTeamPlay == 0 && room.GameMode != 3)
                                trpercent += (float)rnd.Next(Convert.ToInt32(a.AttrValue)) / 100;
                            break;
                        case 32://團體賽，接力賽 TR 隨機獎勵 0~N%
                            if (room.IsTeamPlay != 0 || room.GameMode == 3)
                                trpercent += (float)rnd.Next(Convert.ToInt32(a.AttrValue)) / 100;
                            break;
                        case 37:
                            trnormal += a.AttrValue;
                            break;
                        case 51:
                            trnormal += rnd.Next(Convert.ToInt32(a.AttrValue));
                            break;
                        case 91://簡易頻道 +N%
                            if (room.Channel == 2)
                                trpercent += a.AttrValue;
                            break;
                        case 93://普通頻道 +N%
                            if (room.Channel == 3)
                                trpercent += a.AttrValue;
                            break;
                        case 103://簡易頻道 +N
                            if (room.Channel == 2)
                                trnormal += a.AttrValue;
                            break;
                        case 105://普通頻道 +N
                            if (room.Channel == 3)
                                trnormal += a.AttrValue;
                            break;
                        case 131://個人戰頭3名以內過關時TR+%
                            if (room.IsTeamPlay == 0 && rank <= 3 && room.GameMode != 3)
                                trpercent += a.AttrValue;
                            break;
                        default:
                            break;
                    }
                }
                foreach (var a in User.WearItemSetAttr.Where(w => w.Attr < 140 && w.ApplyTarget == 2))
                {
                    switch (a.Attr)
                    {
                        case 1:
                            trpercent += a.AttrValue;
                            break;
                        case 14://得到個人賽 前3名時 TR，經驗值+%
                            if (room.IsTeamPlay == 0 && rank <= 3)
                                trpercent += a.AttrValue;
                            break;
                        case 30://個人賽 TR 隨機獎勵 0~N%
                            if (room.IsTeamPlay == 0 && room.GameMode != 3)
                                trpercent += (float)rnd.Next(Convert.ToInt32(a.AttrValue)) / 100;
                            break;
                        case 32://團體賽，接力賽 TR 隨機獎勵 0~N%
                            if (room.IsTeamPlay != 0 || room.GameMode == 3)
                                trpercent += (float)rnd.Next(Convert.ToInt32(a.AttrValue)) / 100;
                            break;
                        case 37:
                            trnormal += a.AttrValue;
                            break;
                        case 51:
                            trnormal += rnd.Next(Convert.ToInt32(a.AttrValue));
                            break;
                        case 91://簡易頻道 +N%
                            if (room.Channel == 2)
                                trpercent += a.AttrValue;
                            break;
                        case 93://普通頻道 +N%
                            if (room.Channel == 3)
                                trpercent += a.AttrValue;
                            break;
                        case 103://簡易頻道 +N
                            if (room.Channel == 2)
                                trnormal += a.AttrValue;
                            break;
                        case 105://普通頻道 +N
                            if (room.Channel == 3)
                                trnormal += a.AttrValue;
                            break;
                        case 131://個人戰頭3名以內過關時TR+%
                            if (room.IsTeamPlay == 0 && rank <= 3 && room.GameMode != 3)
                                trpercent += a.AttrValue;
                            break;
                        default:
                            break;
                    }
                }
            }
            */
            #endregion
            if (User.AvatarItems.Any(i => i.itemdescnum == 47635)) //優越童話通行証BUFF
            {
                trpercent += 1;
            }
            trtotal  = (int)Math.Round((inittr + (inittr * trpercent) + trnormal) * ServerMultiplyTR, MidpointRounding.AwayFromZero);
            trtotal = trtotal > MaxTR ? MaxTR : trtotal;
            BounsTR = (short)(trtotal - inittr);
            return trtotal;
        }    
        public static int Calc_EXP(Account User, NormalRoom room, byte rank, out short BounsEXP)
        {
            int PlayerCount = room.PlayerCount();
            float ServerMultiplyEXP = ServerSettingHolder.ServerSettings.MultiplyEXP;
            int initexp = (int)Math.Round(12 * PlayerCount * (float)(1 - (((decimal)rank / 10) - (decimal)0.1)), MidpointRounding.AwayFromZero);
            int  exptotal = 0;
            float exppercent = 0, expnormal = 0;
            int MaxEXP = 20000;
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            foreach (var a in User.AllItemAttr)
            {
                switch (a.Key)
                {
                    case 2:
                        exppercent += a.Value;
                        break;
                    case 14://得到個人賽 前3名時 TR，經驗值+%
                        if (room.IsTeamPlay == 0 && rank <= 3)
                            exppercent += a.Value;
                        break;
                    case 31://個人賽 經驗值 隨機獎勵 0~N%
                        if (room.IsTeamPlay == 0 && room.GameMode != 3)
                            exppercent += (float)rnd.Next(Convert.ToInt32(a.Value)) / 100;
                        break;
                    case 33://團體賽，接力賽 經驗值 隨機獎勵 0~N%
                        if (room.IsTeamPlay != 0 || room.GameMode == 3)
                            exppercent += (float)rnd.Next(Convert.ToInt32(a.Value)) / 100;
                        break;
                    case 38:
                        expnormal += a.Value;
                        break;
                    case 52:
                        expnormal += rnd.Next(Convert.ToInt32(a.Value));
                        break;
                    case 92://簡易頻道 +N%
                        if (room.Channel == 2)
                            exppercent += a.Value;
                        break;
                    case 94://普通頻道 +N%
                        if (room.Channel == 3)
                            exppercent += a.Value;
                        break;
                    case 104://簡易頻道 +N
                        if (room.Channel == 2)
                            expnormal += a.Value;
                        break;
                    case 106://普通頻道 +N
                        if (room.Channel == 3)
                            expnormal += a.Value;
                        break;
                    case 132://個人戰頭3名以內過關時EXP+%
                        if (room.IsTeamPlay == 0 && rank <= 3 && room.GameMode != 3)
                            exppercent += a.Value;
                        break;
                    case 264://最大獲得經驗值增加 +
                        MaxEXP += (int)a.Value;
                        break;
                    default:
                        break;
                }
            }

            #region old
            /*
            foreach (var a in User.WearAvatarItemAttr.Where(w => w.Attr < 270))
            {
                switch (a.Attr)
                {
                    case 2:
                        exppercent += a.AttrValue;
                        break;
                    case 14://得到個人賽 前3名時 TR，經驗值+%
                        if (room.IsTeamPlay == 0 && rank <= 3)
                            exppercent += a.AttrValue;
                        break;
                    case 31://個人賽 經驗值 隨機獎勵 0~N%
                        if (room.IsTeamPlay == 0 && room.GameMode != 3)
                            exppercent += (float)rnd.Next(Convert.ToInt32(a.AttrValue)) / 100;
                        break;
                    case 33://團體賽，接力賽 經驗值 隨機獎勵 0~N%
                        if (room.IsTeamPlay != 0 || room.GameMode == 3)
                            exppercent += (float)rnd.Next(Convert.ToInt32(a.AttrValue)) / 100;
                        break;
                    case 38:
                        expnormal += a.AttrValue;
                        break;
                    case 52:
                        expnormal += rnd.Next(Convert.ToInt32(a.AttrValue));
                        break;
                    case 92://簡易頻道 +N%
                        if (room.Channel == 2)
                            exppercent += a.AttrValue;
                        break;
                    case 94://普通頻道 +N%
                        if (room.Channel == 3)
                            exppercent += a.AttrValue;
                        break;
                    case 104://簡易頻道 +N
                        if (room.Channel == 2)
                            expnormal += a.AttrValue;
                        break;
                    case 106://普通頻道 +N
                        if (room.Channel == 3)
                            expnormal += a.AttrValue;
                        break;
                    case 132://個人戰頭3名以內過關時EXP+%
                        if (room.IsTeamPlay == 0 && rank <= 3 && room.GameMode != 3)
                            exppercent += a.AttrValue;
                        break;
                    case 264://最大獲得經驗值增加 +
                        MaxEXP += (int)a.AttrValue;
                        break;
                    default:
                        break;
                }
            }
            foreach (var a in User.WearItemSetAttr.Where(w => w.Attr < 270 && w.ApplyTarget == 1))
            {
                switch (a.Attr)
                {
                    case 2:
                        exppercent += a.AttrValue;
                        break;
                    case 14:
                        if (room.IsTeamPlay == 0 && rank <= 3)
                            exppercent += a.AttrValue;
                        break;
                    case 31://個人賽 經驗值 隨機獎勵 0~N%
                        if (room.IsTeamPlay == 0 && room.GameMode != 3)
                            exppercent += (float)rnd.Next(Convert.ToInt32(a.AttrValue)) / 100;
                        break;
                    case 33://團體賽，接力賽 經驗值 隨機獎勵 0~N%
                        if (room.IsTeamPlay != 0 || room.GameMode == 3)
                            exppercent += (float)rnd.Next(Convert.ToInt32(a.AttrValue)) / 100;
                        break;
                    case 38:
                        expnormal += a.AttrValue;
                        break;
                    case 52:
                        expnormal += rnd.Next(Convert.ToInt32(a.AttrValue));
                        break;
                    case 92://簡易頻道 +N%
                        if (room.Channel == 2)
                            exppercent += a.AttrValue;
                        break;
                    case 94://普通頻道 +N%
                        if (room.Channel == 3)
                            exppercent += a.AttrValue;
                        break;
                    case 104://簡易頻道 +N
                        if (room.Channel == 2)
                            expnormal += a.AttrValue;
                        break;
                    case 106://普通頻道 +N
                        if (room.Channel == 3)
                            expnormal += a.AttrValue;
                        break;
                    case 132://個人戰頭3名以內過關時EXP+%
                        if (room.IsTeamPlay == 0 && rank <= 3 && room.GameMode != 3)
                            exppercent += a.AttrValue;
                        break;
                    case 264://最大獲得經驗值增加 +
                        MaxEXP += (int)a.AttrValue;
                        break;
                    default:
                        break;
                }
            }
            //avon
            if (User.costumeMode == 1)
            {
                foreach (var a in User.WearCosAvatarItemAttr.Where(w => w.Attr < 270))
                {
                    switch (a.Attr)
                    {
                        case 2:
                            exppercent += a.AttrValue;
                            break;
                        case 14:
                            if (room.IsTeamPlay == 0 && rank <= 3)
                                exppercent += a.AttrValue;
                            break;
                        case 31://個人賽 經驗值 隨機獎勵 0~N%
                            if (room.IsTeamPlay == 0 && room.GameMode != 3)
                                exppercent += (float)rnd.Next(Convert.ToInt32(a.AttrValue)) / 100;
                            break;
                        case 33://團體賽，接力賽 經驗值 隨機獎勵 0~N%
                            if (room.IsTeamPlay != 0 || room.GameMode == 3)
                                exppercent += (float)rnd.Next(Convert.ToInt32(a.AttrValue)) / 100;
                            break;
                        case 38:
                            expnormal += a.AttrValue;
                            break;
                        case 52:
                            expnormal += rnd.Next(Convert.ToInt32(a.AttrValue));
                            break;
                        case 92://簡易頻道 +N%
                            if (room.Channel == 2)
                                exppercent += a.AttrValue;
                            break;
                        case 94://普通頻道 +N%
                            if (room.Channel == 3)
                                exppercent += a.AttrValue;
                            break;
                        case 104://簡易頻道 +N
                            if (room.Channel == 2)
                                expnormal += a.AttrValue;
                            break;
                        case 106://普通頻道 +N
                            if (room.Channel == 3)
                                expnormal += a.AttrValue;
                            break;
                        case 132://個人戰頭3名以內過關時EXP+%
                            if (room.IsTeamPlay == 0 && rank <= 3 && room.GameMode != 3)
                                exppercent += a.AttrValue;
                            break;
                        case 264://最大獲得經驗值增加 +
                            MaxEXP += (int)a.AttrValue;
                            break;
                        default:
                            break;
                    }
                }
                foreach (var a in User.WearItemSetAttr.Where(w => w.Attr < 270 && w.ApplyTarget == 2))
                {
                    switch (a.Attr)
                    {
                        case 2:
                            exppercent += a.AttrValue;
                            break;
                        case 14:
                            if (room.IsTeamPlay == 0 && rank <= 3)
                                exppercent += a.AttrValue;
                            break;
                        case 31://個人賽 經驗值 隨機獎勵 0~N%
                            if (room.IsTeamPlay == 0 && room.GameMode != 3)
                                exppercent += (float)rnd.Next(Convert.ToInt32(a.AttrValue)) / 100;
                            break;
                        case 33://團體賽，接力賽 經驗值 隨機獎勵 0~N%
                            if (room.IsTeamPlay != 0 || room.GameMode == 3)
                                exppercent += (float)rnd.Next(Convert.ToInt32(a.AttrValue)) / 100;
                            break;
                        case 38:
                            expnormal += a.AttrValue;
                            break;
                        case 52:
                            expnormal += rnd.Next(Convert.ToInt32(a.AttrValue));
                            break;
                        case 92://簡易頻道 +N%
                            if (room.Channel == 2)
                                exppercent += a.AttrValue;
                            break;
                        case 94://普通頻道 +N%
                            if (room.Channel == 3)
                                exppercent += a.AttrValue;
                            break;
                        case 104://簡易頻道 +N
                            if (room.Channel == 2)
                                expnormal += a.AttrValue;
                            break;
                        case 106://普通頻道 +N
                            if (room.Channel == 3)
                                expnormal += a.AttrValue;
                            break;
                        case 132://個人戰頭3名以內過關時EXP+%
                            if (room.IsTeamPlay == 0 && rank <= 3 && room.GameMode != 3)
                                exppercent += a.AttrValue;
                            break;
                        case 264://最大獲得經驗值增加 +
                            MaxEXP += (int)a.AttrValue;
                            break;
                        default:
                            break;
                    }
                }
            } 
            */
            #endregion

            if (User.AvatarItems.Any(i => i.itemdescnum == 47635)) //優越童話通行証BUFF
            {
                exppercent += (float)1.5;
            }
            exptotal = (int)Math.Round((initexp + (initexp * exppercent) + expnormal) * ServerMultiplyEXP, MidpointRounding.AwayFromZero);
            exptotal = exptotal > MaxEXP ? MaxEXP : exptotal;
            BounsEXP = (short)(exptotal - initexp);
            return exptotal;
        }
        private static void GetCard(int mapnum, out List<int> takecard)
        {
            takecard = new List<int>();
            if (MapCardHolder.MapCardRateInfos.TryGetValue(mapnum, out var cardrateinfo))
            {
                Random rand = new Random(Guid.NewGuid().GetHashCode());
                double r = rand.NextDouble() * MapCardHolder.GetSumWeight(cardrateinfo);
                double min = 0, max = 0;
                foreach (var card in cardrateinfo.OrderBy(o => o.RateKind))
                {
                    max += MapCardHolder.GetWeight(card.RateKind);
                    if (min <= r && r < max)
                    {
                        takecard.Add(card.CardNum);
                        break;
                    }
                    min = max;
                }
            }
            else
                takecard.Add(0);
        }

        private static byte[] GenResult(NormalRoom room, byte last)
        {
            if (room.Players.Exists(p => p.GameEndType == 0 && p.Attribute != 3))
            {
                foreach (Account player in room.Players.Where(p => p.GameEndType == 0 && p.Attribute != 3).OrderBy(o => o.RaceDistance))
                {
                    long CurrentTime = Utility.CurrentTimeMilliseconds();
                    player.LapTime = room.GetCurrentTime() + 200000;//(int)(CurrentTime + 200000 - room.StartTime);
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
            ns.Fill(room.IsTeamPlay == 0 ? 0xB : 0xA);
            if (room.IsTeamPlay != 0)
            {
                int winteam = room.Players.OrderBy(p => p.Rank).ThenByDescending(p => p.RaceDistance).FirstOrDefault().Team;
                int loseteam = winteam == 1 ? 2 : 1;
                ns.Write((byte)0x1);
                ns.Write(2);
                ns.Write(winteam);
                ns.Write(loseteam);
                ns.Write(winteam);
            }

            int playercount = room.DropItem.Count;
            ns.Write((byte)playercount); //count?
            //ns.Fill(0x6);
            ns.Write(0);
            var rankplayer = room.DropItem.OrderBy(p => p.Rank).ThenBy(p => p.ServerLapTime).ThenBy(o => o.Pos);
            var lastrankplayer = rankplayer.Last();
            foreach (var p in rankplayer)
            {
                ns.Write((short)0);
                ns.Write(p.ServerLapTime);
                ns.Write(p.LapTime);
                ns.Write(p.Rank);
                ns.Write(p.Pos);
                ns.Write(p.EXP);
                ns.Write(p.MiniGameStarPoint);
                ns.Write(p.TR);
                ns.Write(0);
                ns.Write(0L);
                if (room.IsTeamPlay != 0)
                    ns.Write((int)p.Team);
                ns.Write((int)p.RelayTeamPos);
                ns.Write(p.BounsEXP);
                ns.Write(p.BounsTR);
                short bounsweight = 0;
                if (p.BounsTR > 0)
                    bounsweight += 4;
                if (p.BounsEXP > 0)
                    bounsweight += 8;
                ns.Write(bounsweight); // 0C 00 / 1C 00 bouns
                ns.Write((short)0); //20 00  02=情侶? 0x2 =通行證?
                ns.Write(playercount); //02 00 00 00 player count?
                int count = p.CardID.Count;
                ns.Write(count); //card count
                foreach (var card in p.CardID)
                {
                    ns.Write(card);
                } 
                ns.WriteHex("00BA8B19000000000000000000000000000000000000000000C5B01B");
                //ns.WriteHex("00BA8B19000000000000000000000000000000000000000000C5B01B00000000");
                if (!p.Equals(lastrankplayer))
                    ns.Write(0);
            }
            /*foreach (var p in room.Players.Where(p => p.Attribute != 3)
                            .Join(room.DropItem, p => p.UserNum, d => d.UserNum, (p, d) => new { player = p, droplist = d })
                                 .OrderByDescending(o => o.player.RaceDistance).ThenBy(o => o.droplist.Rank).ThenBy(o => o.player.RoomPos))
            {
                ns.Write(p.player.ServerLapTime);
                ns.Write(p.player.LapTime);
                ns.Write(p.droplist.Rank);
                ns.Write(p.player.RoomPos);
                ns.Write(p.droplist.EXP);
                ns.Write(0);
                ns.Write(p.droplist.TR);
                ns.Write(0);
                ns.Write(0L);
                if (room.IsTeamPlay != 0)
                    ns.Write((int)p.player.Team);
                ns.Write(0);
                ns.Write(p.droplist.BounsEXP);
                ns.Write(p.droplist.BounsTR);
                short bounsweight = 0;
                if (p.droplist.BounsTR > 0)
                    bounsweight += 4;
                if (p.droplist.BounsEXP > 0)
                    bounsweight += 8;
                ns.Write(bounsweight); // 0C 00 00 00
                ns.Write((short)0x20); //1C 00 20 00
                ns.Write(playercount); //02 00 00 00 player count?
                int count = p.droplist.CardID.Count;
                ns.Write(count); //card count

                foreach (var card in p.droplist.CardID)
                {
                    ns.Write(card);
                }
                ns.WriteHex("00BA8B19000000000000000000000000000000000000000000C5B01B000000000000");
            }*/

            bool hasgavegoods = SendRegisterGoods(room, out byte[] getgoodsnickname, last);
            if (hasgavegoods)
            {
                Account roomMaster = room.Players.Find(p => p.RoomPos == room.RoomMasterIndex);
                roomMaster.Connection.SendAsync(new GameRoom_DeleteKeepItem(roomMaster, room, last));
            }

            //ns.Write((short)0);
            //04 02 00 00 00 03 00 00 00 00
            //03 02 00 00 00 05 03 00 00 00 04 00 00 00 00 C E D
            //01 00 00 00 02 02 00 00 00 04 03 00 00 00 03 04 00 00 00 06 05 00 00 00 01 06 00 00 00 05 00 00 00 00  B D C F A E
            if (room.GameMode == 3)
            {
                int rank = 1;//startrank?
                var relayteamrank = room.DropItem.OrderBy(o => o.Rank).Select(s => s.RelayTeam).Distinct();
                ns.Write(relayteamrank.Count());
                foreach (var relayteam in relayteamrank)
                {
                    ns.Write(rank);
                    ns.Write(relayteam);
                    rank++;
                }
            }
            else
                ns.Write(0);
            ns.Write(0);
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
                                RoomUser.Connection.SendAsync(new GameRoom_LockKeepItem(room, true, last));
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
                        RoomUser.Connection.SendAsync(new GameRoom_LockKeepItem(room, true, last));
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
            if (room.isPlaying)
            {
                Client.SendAsync(new GameRoom_EnterRoomError(User, 5, (byte)room.RoomKindID, last));
                return;
            }
            //RoomHolder.RoomKindInfos.TryGetValue(room.RoomKindID, out var roomkindinfo);
            if (room.Channel == 1)
            {
                if (User.Exp > ServerSettingHolder.ServerSettings.NewbieOnlyChannelLimitExp && User.Attribute == 0)
                {
                    Client.SendAsync(new GameRoom_EnterRoomError(User, 12, (byte)room.RoomKindID, last));
                    return;
                }
            }
            if (room.PlayerCount() < room.SlotCount || User.Attribute == 3)
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
                Client.SendAsync(new GameRoom_GoodsInfo(room, last));
                Client.SendAsync(new GameRoom_SendRoomInfo(User, room, last, User.RoomPos));

                //Send自己info俾其他roomuser
                foreach (Account roomUser in room.Players)
                {
                    roomUser.Connection.SendAsync(new GameRoom_SendPlayerInfo(User, last));
                    if (room.IsTeamPlay == 2)
                    {
                        int redcount = room.Players.Count(p => p.Team == 1);
                        int blueteam = room.Players.Count(p => p.Team == 2);
                        User.Team = (byte)(redcount <= blueteam ? 1 : 2);
                        roomUser.Connection.SendAsync(new GameRoom_RoomPosTeam(User, last));
                    }
                    if (room.GameMode == 3)
                    {
                        User.SelectRelayTeam((byte)new Random().Next(1, 3));
                        roomUser.Connection.SendAsync(new GameRoom_RoomPosRelayTeam(User, last));
                    }
                }

                room.Players.Add(User);

                //Send roomuser info俾自己
                foreach (Account roomUser in room.Players)
                {
                    Client.SendAsync(new GameRoom_SendPlayerInfo(roomUser, last));
                    Client.SendAsync(new GameRoom_RoomPosReady(roomUser.RoomPos, roomUser.IsReady, last));
                    if (room.GameMode == 3)
                    {
                        Client.SendAsync(new GameRoom_RoomPosRelayTeam(roomUser, last));
                    }
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
                RoomPlayer.Connection.SendAsync(new GameRoom_KickPlayer(KickedPlayerIndex, last));
            }
            room.Players.Remove(KickedPlayer);
            room.PosList.Add(KickedPlayerIndex);
            KickedPlayer.LeaveRoomReset();
            KickedPlayer.Connection.SendAsync(new GameRoom_Hex("FF6405F703000002000000", last));
            KickedPlayer.Connection.SendAsync(new GameRoom_LeaveRoomUser_0XA9(KickedPlayer, KickedPlayerIndex, last));
            if (KickedPlayer.Attribute == 0)
            {
                room.addKickedPlayer(KickedPlayer);
            }
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_RemoveRoomUser(RoomPlayer, KickedPlayerIndex, last));
                RoomPlayer.Connection.SendAsync(new GameRoom_Hex("FFA80500000000", last));
            }
            KickedPlayer.Connection.SendAsync(new GameRoom_Hex("FFA80500000000", last));
            KickedPlayer.Connection.SendAsync(new GameRoom_KickPlayer(KickedPlayerIndex, last));
        }
        public static void LeaveRoom(ClientConnection Client, bool isDisconnect, byte last)
        {
            Account User = Client.CurrentAccount;
            if (User.CurrentRoomId != 0 && User.InGame)
            {
                byte roompos = User.RoomPos;
                //NormalRoom normalRoom = Rooms.NormalRoomList.Find(room => room.ID == User.CurrentRoomId);
                NormalRoom normalRoom = Rooms.GetRoom(User.CurrentRoomId);
                bool isRemoved = true;
                if (normalRoom.Players.Count == 1)
                {
                    if (normalRoom.ItemNum != -1 && !isDisconnect)
                    {
                        Client.SendAsync(new GameRoom_LockKeepItem(normalRoom, true, last));//解鎖之前保管了的物品
                    }
                    //Rooms.NormalRoomList.Remove(normalRoom);
                    Rooms.RemoveRoom(User.CurrentRoomId);
                }
                else
                {
                    //加回位置id
                    normalRoom.Players.Remove(User);
                    normalRoom.PosList.Add(roompos);
                    isRemoved = false;
                    if (normalRoom.isPlaying)
                    {
                        normalRoom.DisconnectDropList(User);
                        normalRoom.Survival -= 1;
                        switch (normalRoom.RuleType)
                        {
                            case 2: //生存
                                if (normalRoom.Survival == 1)
                                {
                                    byte alivepos = normalRoom.Players.Find(f => f.GameEndType == 0 && f.Attribute != 3).RoomPos;
                                    foreach (Account RoomPlayer in normalRoom.Players.OrderBy(o => o.GameEndType))
                                    {
                                        RoomPlayer.Connection.SendAsync(new GameRoom_Alive(alivepos, last));
                                    }
                                }
                                break;
                            case 3: //Hardcore
                            case 4: //跑步吧
                            case 8: //衝呀!!
                                if (normalRoom.Survival == 0)
                                {
                                    long EndTime = Utility.CurrentTimeMilliseconds() + 5000;
                                    Task.Run(() => Execute_GameEnd(normalRoom, EndTime, last));
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }

                byte RoomMasterIndex = normalRoom.RoomMasterIndex;
                if (!isDisconnect)
                {
                    User.LeaveRoomReset();
                    Client.SendAsync(new GameRoom_LeaveRoomUser_0XA9(User, roompos, last));
                    Client.SendAsync(new GameRoom_Hex("FFA80500000000", last));
                }

                //Client.SendAsync(new GameRoom_Hex(User, "FF6405F703000002000000", last));
                if (!isRemoved)
                {
                    foreach (Account RoomPlayer in normalRoom.Players)
                    {
                        RoomPlayer.Connection.SendAsync(new GameRoom_RemoveRoomUser(RoomPlayer, roompos, last));
                    }

                    if (roompos == RoomMasterIndex)
                    {
                        if (normalRoom.ItemNum != -1)
                        {
                            if (!isDisconnect)
                                Client.SendAsync(new GameRoom_LockKeepItem(normalRoom, true, last));//解鎖之前保管了的物品
                            normalRoom.RegisterItem(-1, -1, 0, 0, false);
                            foreach (Account RoomPlayer in normalRoom.Players)
                            {
                                RoomPlayer.Connection.SendAsync(new GameRoom_GoodsInfo(normalRoom, last));
                            }
                        }
                        Account newRoomMaster = normalRoom.Players.Exists(p => p.Attribute == 1) ? normalRoom.Players.FindAll(p => p.Attribute == 1).FirstOrDefault() : normalRoom.Players.FirstOrDefault();
                        normalRoom.RoomMasterIndex = newRoomMaster.RoomPos;
                        foreach (Account RoomPlayer in normalRoom.Players)
                        {
                            RoomPlayer.Connection.SendAsync(new GameRoom_GetRoomMaster(RoomPlayer, newRoomMaster.RoomPos, last));
                        }
                    }
                }
                else
                {
                    normalRoom.Dispose();
                }
            }
        }
    }
}
